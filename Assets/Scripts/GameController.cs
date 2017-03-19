using UnityEngine;
using System.Collections.Generic;


public class GameController
{
    static GameController s_inst;
    public static GameController instance
    {
        get
        {
            return s_inst ?? (s_inst = new GameController());
        }
    }

    public bool isServer;

    int m_forceIndex;
    Dictionary<int, GamePlayerController> m_allPlayers = new Dictionary<int, GamePlayerController>();
    public Dictionary<int, GamePlayerController> allPlayers
    {
        get
        {
            return m_allPlayers;
        }
    }

    public void Reset()
    {
        GamePlayerController.s_localClient = null;
        m_forceIndex = 0;
        m_allPlayers.Clear();
    }

    public int ServerAddNewForce()
    {
        ++m_forceIndex;
        return m_forceIndex;
    }

    public void ClientAddPlayer(int playerId, GameObject gameObject)
    {
        m_allPlayers.Add(playerId, gameObject.GetComponent<GamePlayerController>());
    }

    Dictionary<int, bool> m_playerReady = new Dictionary<int, bool>();
    public void ServerResetPlayersReady()
    {
        m_playerReady.Clear();
        foreach (var playerId in m_allPlayers.Keys)
        {
            m_playerReady[playerId] = false;
        }
    }

    /// <summary>
    /// 如果全部客户端准备好则返回true
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool ServerPlayerReady(int playerId)
    {
        m_playerReady[playerId] = true;
        return !m_playerReady.ContainsValue(false);
    }

    // Sync Actions

    // 只有服务端调用时才会加入到队列
    public void AddSyncAction(SyncGameAction sync)
    {
        if (!isServer || !sync.valid)
        {
            return;
        }
        m_syncActionsSend.Add(sync);
    }

    public void SyncActions()
    {
        Debug.Assert(GamePlayerController.localClient.isServer);
        if (m_syncActionsSend.Count > 0)
        {
            var arr = m_syncActionsSend.ToArray();
            int total;
            byte[][] data = Utils.Serialize(arr, out total);
            //Debug.LogFormat("SyncActions|Send: {0}B", total);
            for (int i = 0; i < data.Length; ++i)
            {
                GamePlayerController.localClient.RpcSyncActions(data[i], i + 1 == data.Length);
            }
            m_syncActionsSend.Clear();
        }
    }

    public void PlayActions(SyncGameAction[] syncActions)
    {
        m_cacheUnit = null;
        for (int i = 0; i < syncActions.Length; ++i)
        {
            syncActions[i].Play();
        }
    }

    public Unit GetUnit(int id)
    {
        if (id == 0)
        {
            return null;
        }
        if (m_cacheUnit != null && m_cacheUnit.Id == id)
        {
            return m_cacheUnit;
        }
        return WorldController.instance.world.GetUnit(id);
    }

    Unit m_cacheUnit;
    List<SyncGameAction> m_syncActionsSend = new List<SyncGameAction>();

    // ============== Game Actions ==============

    /// <summary>
    /// playerId 不为0时，为玩家单位
    /// </summary>
    /// <param name="syncInfo"></param>
    /// <param name="playerId"></param>
    public void CreateUnit(SyncUnitInfo syncInfo, int playerId = 0)
    {
        AddSyncAction(new SyncCreateUnit(syncInfo, playerId));

        GamePlayerController client;
        if (allPlayers.TryGetValue(playerId, out client))
        {
            UnitController unitCtrl = UnitController.Create(syncInfo, client);
            client.unitCtrl = unitCtrl;
            Debug.LogFormat("CreateUnit, unitId({0}) <-> playerId({1}).", unitCtrl.unit.Id, client.playerId);
            if (client == GamePlayerController.localClient)
            {
                Debug.LogFormat("That's Me, {0}.", unitCtrl.unit.Name);
            }

            // TEST !!!!
            unitCtrl.unit.MaxHpBase = 100000;  // test
            unitCtrl.unit.Hp = unitCtrl.unit.MaxHp;
            unitCtrl.unit.AttackSkill.coolDownBase = 0;
            unitCtrl.unit.AttackSkill.coolDownSpeedCoeff = 20;

            SplashPas splash = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
            unitCtrl.unit.AddPassiveSkill(splash);
        }
        else
        {
            UnitController.Create(syncInfo, null);
        }
    }

    public void RemoveUnit(Unit unit, bool revivalbe)
    {
        AddSyncAction(new SyncRemoveUnit(unit.Id, revivalbe));
    }

    public void StartWorld()
    {
        AddSyncAction(new SyncStartWorld());
    }

    public void FireProjectile(Projectile projectile)
    {
        SyncProjectileInfo syncInfo = SyncProjectileInfo.Create(projectile);
        AddSyncAction(new SyncFireProjectile(syncInfo));
    }
}
