using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;


/// <summary>
/// 全局唯一数据在这里
/// </summary>
public class GameController {
    static bool m_isServer;
    public static bool isServer {
        get {
            return m_isServer;
        }
    }

    public static void Init(bool asHost) {
        m_isServer = asHost;

        m_allPlayers = new Dictionary<int, GamePlayerController>();
        m_playersReady = new Dictionary<int, bool>();

        if (m_isServer) {
            // Server专用数据
            m_syncActionsSend = new List<SyncGameAction>();
            m_forceIndex = 0;
        }
    }


    // ===== 全局Server专用数据 =====
    static List<SyncGameAction> m_syncActionsSend;
    static int m_forceIndex;
    // ===== 全局Server专用数据 结束 =====


    // ===== 全局玩家数据 =====
    static Dictionary<int, GamePlayerController> m_allPlayers;  // 所有玩家
    public static Dictionary<int, GamePlayerController> AllPlayers {
        get {
            return m_allPlayers;
        }
    }
    static Dictionary<int, bool> m_playersReady;  // 玩家准备状态，点击准备按钮和加载资源完成的时候会设置这个状态
    // ===== 全局玩家数据 结束 =====

    
    /// <summary>
    /// 重置数据
    /// </summary>
    public static void Reset() {
        if (m_isServer) {
            m_syncActionsSend.Clear();
            m_forceIndex = 0;
        }
        m_allPlayers.Clear();
        m_playersReady.Clear();
    }

    /// <summary>
    /// 向同步队列中追加一个待同步的动作
    /// </summary>
    /// <param name="sync"></param>
    public static void ServerAddSyncAction(SyncGameAction sync) {
        m_syncActionsSend.Add(sync);
    }

    /// <summary>
    /// 将同步动作队列中的数据串行化，返回分片数据
    /// </summary>
    /// <returns></returns>
    public static byte[][] ServerSerializeSyncActions() {
        byte[][] data = null;
        if (m_syncActionsSend.Count > 0) {
            var arr = m_syncActionsSend.ToArray();
            int total;
            data = Utils.Serialize(arr, out total);
            m_syncActionsSend.Clear();
        }
        return data;
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
    public static void ClientAddPlayer(int playerId, GameObject gameObject) {
        m_allPlayers.Add(playerId, gameObject.GetComponent<GamePlayerController>());
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
