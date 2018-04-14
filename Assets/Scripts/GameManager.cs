using UnityEngine;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// 同步对象发送器，服务端专用
/// </summary>
/// <typeparam name="SyncObject"></typeparam>
public class SyncObjectSender<SyncObject> {
    List<SyncObject> m_syncObjsSend;

    public SyncObjectSender() {
        m_syncObjsSend = new List<SyncObject>();
    }

    /// <summary>
    /// 向同步队列中追加一个待同步的对象
    /// </summary>
    public void Add(SyncObject sync) {
        m_syncObjsSend.Add(sync);
    }

    public void Reset() {
        m_syncObjsSend.Clear();
    }

    /// <summary>
    /// 将同步动作队列中的对象串行化，返回分片数据
    /// </summary>
    /// <returns></returns>
    public byte[][] Serialize() {
        byte[][] data = null;
        if (m_syncObjsSend.Count > 0) {
            var arr = m_syncObjsSend.ToArray();
            int total;
            data = Utils.Serialize(arr, out total);
            m_syncObjsSend.Clear();
        }
        return data;
    }
}


/// <summary>
/// 同步对象接收器，客户端专用
/// </summary>
public class SyncObjectReceiver<SyncObject> {
    MemoryStream m_syncObjsRecv;

    public SyncObjectReceiver() {
        m_syncObjsRecv = new MemoryStream(102400);
    }

    public void Reset() {
        m_syncObjsRecv.Position = 0;
    }

    /// <summary>
    /// 接收来自服务端同步数据，数据完整后，返回同步对象数组
    /// </summary>
    /// <param name="data"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public SyncObject[] Deserialize(byte[] data, bool end) {
        m_syncObjsRecv.Write(data, 0, data.Length);

        if (end) {
            long size = m_syncObjsRecv.Position;
            byte[] buf = new byte[size];
            m_syncObjsRecv.Position = 0;
            m_syncObjsRecv.Read(buf, 0, (int)size);
            m_syncObjsRecv.Position = 0;
            SyncObject[] syncs = (SyncObject[])Utils.Deserialize(buf);
            return syncs;
        }
        return null;
    }
}


/// <summary>
/// 全局唯一数据在这里
/// </summary>
public class GameManager {
    static bool m_isServer;

    public static bool isServer {
        get { return m_isServer; }
    }

    // ===== 全局Server专用数据 =====
    public static SyncObjectSender<SyncGameAction> syncActionSender;
    static int m_forceIndex;
    // ===== 全局Server专用数据 结束 =====


    // ===== 全局玩家数据 =====
    public static SyncObjectReceiver<SyncGameAction> syncActionReceiver;
    static Dictionary<int, GamePlayerController> m_allPlayers;
    // 所有玩家
    public static Dictionary<int, GamePlayerController> AllPlayers {
        get { return m_allPlayers; }
    }

    static Dictionary<int, bool> m_playersReady;
    // 玩家准备状态，点击准备按钮和加载资源完成的时候会设置这个状态                                                  // ===== 全局玩家数据 结束 =====

    // ===== 通用成员函数 =====
    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="asHost"></param>
    public static void Init(bool asHost) {
        m_isServer = asHost;

        m_allPlayers = new Dictionary<int, GamePlayerController>();
        m_playersReady = new Dictionary<int, bool>();

        if (m_isServer) {
            // Server专用数据
            syncActionSender = new SyncObjectSender<SyncGameAction>();
            m_forceIndex = 0;
        } else {
            syncActionReceiver = new SyncObjectReceiver<SyncGameAction>();
        }
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    public static void Reset() {
        if (m_isServer) {
            syncActionSender.Reset();
            m_forceIndex = 0;
        } else {
            syncActionReceiver.Reset();
        }
        
        m_allPlayers.Clear();
        m_playersReady.Clear();
    }



    /// <summary>
    /// 分配一个新的势力ID
    /// </summary>
    /// <returns></returns>
    public static int ServerAddNewForce() {
        ++m_forceIndex;
        return m_forceIndex;
    }

    /// <summary>
    /// 将一个玩家ID与一个GamePlayerController绑定
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="gameObject"></param>
    public static void ClientAddPlayer(int playerId, GameObject obj) {
        m_allPlayers.Add(playerId, obj.GetComponent<GamePlayerController>());
    }

    /// <summary>
    /// 把所有玩家准备状态设为“未准备”状态
    /// </summary>
    public static void ResetPlayersReady() {
        m_playersReady.Clear();
        foreach (var playerId in m_allPlayers.Keys) {
            m_playersReady[playerId] = false;
        }
    }

    /// <summary>
    /// 设置某玩家为“准备”状态；如果全部玩家准备就绪返回true
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public static bool PlayerReady(int playerId) {
        m_playersReady[playerId] = true;
        return AllPlayersReady();
    }

    public static bool AllPlayersReady() {
        return !m_playersReady.ContainsValue(false);
    }
}
