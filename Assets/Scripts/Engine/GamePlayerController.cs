using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitJson;


public struct PlayerInfo {
    public bool initialized {
        get {
            return id != 0;
        }
    }

    public int id;
    public string name;
    public int force;
    public string heroData;
}

/// <summary>
/// 网络玩家控制器，发送请求，储存玩家信息
/// </summary>
public class GamePlayerController : NetworkBehaviour {
    public List<TextAsset> m_heroes;

    public List<TextAsset> m_testUnits;
    [Range(0, 8)]
    public int m_testPlayerCount = 1;
    [Range(0.02f, 10.0f)]
    public float m_testCreateRate = 10.0f;
    public int m_testMax = 50;
    int m_testCount = 0;

    internal static GamePlayerController s_localClient;
    public static GamePlayerController localClient {
        get {
            return s_localClient;
        }
    }

    public int playerId {
        get {
            return m_playerInfo.id;
        }
    }

    [SyncVar]
    internal PlayerInfo m_playerInfo = new PlayerInfo();

    public PlayerInfo playerInfo {
        get {
            return m_playerInfo;
        }
    }

    UnitController m_unitCtrl;
    public UnitController unitCtrl {
        get {
            return m_unitCtrl;
        }

        set {
            m_unitCtrl = value;
        }
    }

    

    


    void Start() {
        DontDestroyOnLoad(gameObject);
        if (isLocalPlayer) {
            // 创建新接入的可控本地，具备发送Cmd能力
            s_localClient = this;

            PlayerInfo playerInfo;
            ClientLoadUserData(out playerInfo);
            CmdAddPlayer(playerInfo);
        } else if (!m_playerInfo.initialized) {
            // 创建新接入的远程
            Debug.LogFormat("New Client Connected.");
        } else /*if (m_playerInfo.Initialized)*/
          {
            // 创建已接入的远程，需要同步到本地
            ClientAddPlayerToSlot();
        }
    }

    void ClientLoadUserData(out PlayerInfo playerInfo) {
        playerInfo = new PlayerInfo();
        playerInfo.name = PlayerPrefs.GetString("name");
        playerInfo.force = 0;
        playerInfo.heroData = PlayerPrefs.GetString("hero");
        if (playerInfo.heroData == "") {
            playerInfo.heroData = m_heroes[Utils.Random.Next(m_heroes.Count)].text;
        }
    }

    /// <summary>
    /// 添加同步动作
    /// </summary>
    /// <param name="sync"></param>
    //[Server]  // 防止warning，注释掉此处attr
    public void ServerAddSyncAction(SyncGameAction sync) {
        if (!isServer || !sync.valid) {
            // sync不合法
            return;
        }
        GameController.syncActionSender.Add(sync);
    }

    /// <summary>
    /// 同步动作（定时被调用）
    /// </summary>
    [Server]
    public void ServerSyncActions() {
        var data = GameController.syncActionSender.Serialize();
        if (data != null) {
            for (int i = 0; i < data.Length; ++i) {
                localClient.RpcSyncActions(data[i], i + 1 == data.Length);
            }
        }
    }

    [ClientRpc]
    public void RpcSyncActions(byte[] data, bool end) {
        if (isServer) {
            return;
        }

        //Debug.LogFormat("ServerSyncActions|Recv: {0}", data.Length);
        SyncGameAction[] syncs = GameController.syncActionReceiver.Deserialize(data, end);
        if (syncs != null) {
            for (int i = 0; i < syncs.Length; ++i) {
                syncs[i].Play();
            }
        }
    }





    // =======================================================

    [Command]
    void CmdAddPlayer(PlayerInfo playerInfo) {
        // 服务器补全信息
        playerInfo.id = Utils.IdGenerator.nextId;
        playerInfo.force = GameController.ServerAddNewForce();

        // 下发到所有客户端
        RpcAddPlayer(playerInfo);
    }

    [ClientRpc]
    void RpcAddPlayer(PlayerInfo playerInfo) {
        // 客户端接收到服务器的同步信息
        m_playerInfo = playerInfo;
        ClientAddPlayerToSlot();
        if (localClient == this) {
            Debug.LogFormat("LocalPlayer, Id({0}).", m_playerInfo.id);
        }
        Debug.LogFormat("Rpc AddPlayer, Id({0}).", m_playerInfo.id);
    }

    void ClientAddPlayerToSlot() {
        GameController.ClientAddPlayer(m_playerInfo.id, gameObject);

        var canvas = GameObject.Find("Canvas");
        var ui = canvas.GetComponent<RoomUI>();
        var slot = ui.m_playerSlots[m_playerInfo.force - 1];

        var portrait = slot.transform.Find("portrait").GetComponent<Image>();
        var baseInfo = JsonMapper.ToObject<UnitInfo>(m_playerInfo.heroData);
        portrait.sprite = Resources.Load<Sprite>(string.Format("{0}/portrait_sel", baseInfo.root));

        var name = slot.transform.Find("name").GetComponent<Text>();
        name.text = m_playerInfo.name;
    }

    [Command]
    public void CmdPlayerReady() {
        RpcPlayerReady();
    }

    [ClientRpc]
    void RpcPlayerReady() {
        GameController.PlayerReady(m_playerInfo.id);
        Debug.LogFormat("Player({0}) is Ready.", m_playerInfo.id);
        bool allReady = GameController.AllPlayersReady();
        if (allReady) {
            Debug.LogFormat("All Players are Ready.");
        }
    }

    [ClientRpc]
    public void RpcStart() {
        GameController.ResetPlayersReady();
        SceneManager.LoadScene("TestStage");
        localClient.CmdClientLoadSceneFinished();
    }

    [Command]
    void CmdClientLoadSceneFinished() {
        GameController.PlayerReady(m_playerInfo.id);
        Debug.LogFormat("Player({0}) LoadScene Finished.", m_playerInfo.id);
        bool allReady = GameController.AllPlayersReady();
        if (allReady) {
            Debug.LogFormat("All Players LoadScene Finished.");
            // 服务器创建单位
            Invoke("ServerCreateUnits", 1.0f);
        }
    }

















    // ============== Unit Game Actions ==============

    /// <summary>
    /// playerId 不为0时，为玩家单位
    /// </summary>
    /// <param name="syncInfo"></param>
    /// <param name="playerId"></param>
    public void CreateUnit(SyncUnitInfo syncInfo, int playerId = 0) {
        ServerAddSyncAction(new SyncCreateUnit(syncInfo, playerId));

        GamePlayerController client;
        if (GameController.AllPlayers.TryGetValue(playerId, out client)) {
            // 玩家单位
            UnitController unitCtrl = UnitController.Create(syncInfo, client);
            client.unitCtrl = unitCtrl;
            Debug.LogFormat("CreateUnit, unitId({0}) <-> playerId({1}).", unitCtrl.unit.Id, client.playerId);
            if (client == localClient) {
                Debug.LogFormat("That's Me, {0}.", unitCtrl.unit.Name);
            }

            // TEST !!!!
            unitCtrl.unit.MaxHpBase = 100000;  // test
            unitCtrl.unit.Hp = unitCtrl.unit.MaxHp;
            unitCtrl.unit.AttackSkill.coolDownBase = 0;
            unitCtrl.unit.AttackSkill.coolDownSpeedCoeff = 2;
            unitCtrl.unit.CriticalRateBase = 0.2f;
            unitCtrl.unit.CriticalDamageBase = 20.0f;

            SplashPas splash = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
            unitCtrl.unit.AddPassiveSkill(splash);
        } else {
            // 普通单位
            UnitController.Create(syncInfo, null);
        }
    }

    public void RemoveUnit(Unit unit, bool revivalbe) {
        ServerAddSyncAction(new SyncRemoveUnit(unit.Id, revivalbe));
    }

    public void StartWorld() {
        ServerAddSyncAction(new SyncStartWorld());
    }

    public void FireProjectile(Projectile projectile) {
        SyncProjectileInfo syncInfo = SyncProjectileInfo.Create(projectile);
        ServerAddSyncAction(new SyncFireProjectile(syncInfo));
    }


    // ======== 创建单位 ========
    [Server]
    public void ServerCreateUnits() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建玩家单位
        foreach (GamePlayerController ctrl in GameController.AllPlayers.Values) {
            PlayerInfo playerInfo = ctrl.playerInfo;
            string path = string.Format("UnitsData/[Player{0}]", ctrl.playerId);
            SyncUnitInfo syncInfo = new SyncUnitInfo();
            syncInfo.baseInfo = ResourceManager.instance.LoadUnit(path, playerInfo.heroData);
            syncInfo.id = Utils.IdGenerator.nextId;
            syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
            syncInfo.force = playerInfo.force;
            Vector2 position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
            syncInfo.positionX = position.x;
            syncInfo.positionY = position.y;
            localClient.CreateUnit(syncInfo, ctrl.playerId);
        }

        // 随即创建单位
        if (GameController.AllPlayers.Count <= m_testPlayerCount) {
            InvokeRepeating("CreateTestUnit", 0.0f, m_testCreateRate);
        }

        //CreateOneTestUnit();

        // 世界开始运转
        localClient.StartWorld();
    }

    void CreateOneTestUnit() {
        Vector2 sz = Utils.halfCameraSize;
        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo = ResourceManager.instance.LoadUnit("UnitsData/Arcane");
        Vector2 position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
        syncInfo.positionX = position.x;
        syncInfo.positionY = position.y;
        syncInfo.id = Utils.IdGenerator.nextId;
        syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
        syncInfo.force = Utils.Random.Next(8);
        localClient.CreateUnit(syncInfo);
    }

    void CreateTestUnit() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建普通单位
        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo = ResourceManager.instance.LoadUnit("", m_testUnits[Utils.Random.Next(m_testUnits.Count)].text);
        Vector2 position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
        syncInfo.positionX = position.x;
        syncInfo.positionY = position.y;
        syncInfo.id = Utils.IdGenerator.nextId;
        syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
        syncInfo.force = Utils.Random.Next(8);
        localClient.CreateUnit(syncInfo);
        ++m_testCount;
        if (m_testCount > m_testMax) {
            CancelInvoke("CreateTestUnit");
            m_testCount = 0;
        }
    }


#if false
    [ClientRpc]
    public void RpcCreateUnit(SyncUnitInfo syncInfo, int playerId)
    {
        GamePlayerController client;
        if (GameController.instance.allPlayers.TryGetValue(playerId, out client))
        {
            UnitController unitCtrl = UnitController.Create(syncInfo, client);
            client.m_unitCtrl = unitCtrl;
            Debug.LogFormat("CreateUnit, unitId({0}) <-> playerId({1}).", unitCtrl.unit.Id, client.playerId);
            if (client == localClient)
            {
                Debug.LogFormat("That's Me, {0}.", unitCtrl.unit.Name);
            }

            // TEST !!!!
            unitCtrl.unit.MaxHpBase = 100000;  // test
            unitCtrl.unit.Hp = unitCtrl.unit.MaxHp;
            unitCtrl.unit.AttackSkill.CoolDownBase = 0;
            unitCtrl.unit.AttackSkill.CoolDownSpeedCoeff = 20;

            SplashPas splash = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
            unitCtrl.unit.AddPassiveSkill(splash);
        }
        else
        {
            UnitController.Create(syncInfo, null);
        }
    }

    [ClientRpc]
    void RpcStartWorld()
    {
        WorldController.instance.StartWorld();
    }
#endif
    ////////////////////////////////////////////////////
    // UnitCtrl

    [Command]
    public void CmdMove(Vector2 pos, bool obstinate) {
        //RpcMove(transform.localPosition, pos, obstinate);
        //Unit unit = WorldController.instance.World.GetUnit(id);
        //Debug.LogFormat("ServerMove, unitId({0}) <-> playerId({1}), force({2}), hp({3}/{4})", m_unitCtrl.Unit.Id, PlayerId, m_unitCtrl.Unit.Force, m_unitCtrl.Unit.Hp, m_unitCtrl.Unit.MaxHp);
        m_unitCtrl.unit.CommandMove(pos, obstinate);
    }

    [Command]
    public void CmdCastSpell(CommandTarget.Type targetType, int targetUnit, Vector2 targetPoint, string activeSkill, bool obstinate) {
        CommandTarget target;
        switch (targetType) {
        default:
            target = new CommandTarget();
            break;
        case CommandTarget.Type.kUnitTarget:
            target = new CommandTarget(m_unitCtrl.unit.World.GetUnit(targetUnit));
            break;
        case CommandTarget.Type.kPointTarget:
            target = new CommandTarget(targetPoint);
            break;
        }

        foreach (var skill in m_unitCtrl.unit.ActiveSkills) {
            if (skill.name == activeSkill) {
                m_unitCtrl.unit.CommandCastSpell(target, skill, obstinate);
                break;
            }
        }
    }
#if false
    [ClientRpc]
    public void RpcRemoveUnit(int id)
    {
        if (isServer)
        {
            return;
        }

        World world = WorldController.instance.world;
        Unit unit = world.GetUnit(id);
        if (unit != null)
        {
            world.RemoveUnit(unit, unit.Revivable);
        }
    }

    [ClientRpc]
    public void RpcFireProjectile(SyncProjectileInfo syncInfo)
    {
        if (isServer)
        {
            return;
        }

        ProjectileController projCtrl = ProjectileController.Create(syncInfo);
        Projectile projectile = projCtrl.projectile;
        projectile.Fire();
    }
#endif
    
}
