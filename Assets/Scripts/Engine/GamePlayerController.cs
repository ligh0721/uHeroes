#define _UHEROES_

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitJson;
using UnityEditor;


/// <summary>
/// 网络玩家控制器，发送请求，储存玩家信息
/// </summary>
public class GamePlayerController : NetworkBehaviour {
    public List<TextAsset> m_heroes;

    public List<TextAsset> m_testUnits;
    string[] m_testProjectilesDatas =
    {
        "Projectiles/ArcaneRay",
        "Projectiles/ArcherArrow",
        "Projectiles/Lightning",
        "Projectiles/MageBolt",
        "Projectiles/TeslaRay"
    };
    string[] m_testUnitsDatas =
    {
        "Units/Arcane",
        "Units/Archer",
        "Units/Barracks",
        "Units/Mage",
        "Units/Malik",
        "Units/Tesla"
    };
    [Range(0, 8)]
    public int m_testPlayerCount = 1;
    [Range(0.02f, 10.0f)]
    public float m_testCreateRate = 10.0f;
    public int m_testMax = 50;

    internal static GamePlayerController s_localClient;

    /// <summary>
    /// 发送Command的一定是localClient
    /// </summary>
    /// <value>The local client.</value>
    public static GamePlayerController localClient {
        get {
#if UNITY_EDITOR
            fakeClientForEditor();
#endif
            return s_localClient;
        }
    }

    static void fakeClientForEditor() {
        if (s_localClient != null) {
            return;
        }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/Engine/GamePlayer.prefab");
        var gamePlayer = Instantiate(prefab);
        s_localClient = gamePlayer.GetComponent<GamePlayerController>();
    }

    public int playerId {
        get { return m_player.id; }
    }

    [SyncVar]
    internal Player m_player = new Player();

    public Player Player {
        get { return m_player; }
    }

    void Start() {
        DontDestroyOnLoad(gameObject);
        var canvas = GameObject.Find("Canvas");
        m_roomui = canvas.GetComponent<RoomUI>();
        if (isLocalPlayer) {
            // 创建新接入的可控本地，具备发送Cmd能力
            s_localClient = this;

            Player playerInfo;
            ClientLoadUserData(out playerInfo);
            CmdAddPlayer(playerInfo);
        } else if (!m_player.Initialized) {
                // 创建新接入的远程
                Debug.LogFormat("New Client Connected.");
            } else { /*if (m_playerInfo.Initialized)*/
                // 创建已接入的远程，需要同步到本地
                ClientAddPlayerToSlot();
            }
    }

    void ClientLoadUserData(out Player playerInfo) {
        playerInfo = new Player();
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
        GameManager.syncActionSender.Add(sync);
    }

    /// <summary>
    /// 同步动作（定时被调用）
    /// </summary>
    [Server]
    public void ServerSyncActions() {
        var data = GameManager.syncActionSender.Serialize();
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
        SyncGameAction[] syncs = GameManager.syncActionReceiver.Deserialize(data, end);
        if (syncs != null) {
            for (int i = 0; i < syncs.Length; ++i) {
                syncs[i].Play();
            }
        }
    }





    // =======================================================
    RoomUI m_roomui;

    [Command]
    void CmdAddPlayer(Player playerInfo) {
        // 服务器补全信息
        playerInfo.id = Utils.IdGenerator.nextId;
        playerInfo.force = GameManager.ServerAddNewForce();

        // 下发到所有客户端
        RpcAddPlayer(playerInfo);
    }

    [ClientRpc]
    void RpcAddPlayer(Player playerInfo) {
        // 客户端接收到服务器的同步信息
        m_player = playerInfo;
        ClientAddPlayerToSlot();
        if (localClient == this) {
            Debug.LogFormat("LocalPlayer, Id({0}).", m_player.id);
        }
        Debug.LogFormat("Rpc AddPlayer, Id({0}).", m_player.id);
    }

    void ClientAddPlayerToSlot() {
        GameManager.ClientAddPlayer(m_player.id, gameObject);

        var roomPlayerUI = m_roomui.m_playerUIs[m_player.force - 1];
        var baseInfo = JsonMapper.ToObject<UnitInfo>(m_player.heroData);
        roomPlayerUI.Portrait = Resources.Load<Sprite>(string.Format("{0}/portrait_sel", baseInfo.model));
        roomPlayerUI.Name = m_player.name;
    }

    [Command]
    public void CmdPlayerReady() {
        RpcPlayerReady();
    }

    [ClientRpc]
    void RpcPlayerReady() {
        GameManager.PlayerReady(m_player.id);
        Debug.LogFormat("Player({0}) is Ready.", m_player.id);
        bool allReady = GameManager.AllPlayersReady();
        if (allReady) {
            Debug.LogFormat("All Players are Ready.");
        }
    }


    [Command]
    void CmdClientLoadProgress(float value) {
        RpcClientLoadProgress(value);
    }

    [ClientRpc]
    void RpcClientLoadProgress(float value) {
        var slot = m_roomui.m_playerUIs[m_player.force - 1];
        slot.Progress = value;
    }

    /// <summary>
    /// 开始加载
    /// </summary>
    [ClientRpc]
    public void RpcStart() {
        GameManager.ResetPlayersReady();
#if _UHEROES_
        ResourceManager.instance.AddProjectilesToLoadingQueue(m_testProjectilesDatas);
        ResourceManager.instance.AddUnitsToLoadingQueue(m_testUnitsDatas);
        //ResourceManager.instance.SetNextSceneAndStartLoadingScene("BattleWorld");
        localClient.StartLoadingWithoutLoadingScene("BattleWorld", 0.1f);
#else
        ResourceManager.instance.LoadingScene("TestTankStage");
#endif
    }

    static IEnumerator WaitForOneSeconds(ResourceManager.OnUpdateProgress onUpdate) {
        while (!localClient.m_roomui.IsAllProgressActionDone) {
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
    }

    void StartLoadingWithoutLoadingScene(string name, float sceneper) {
        float lastValue = -1.0f;
        m_roomui.ShowAllProgressText();
        ResourceManager.instance.SetNextScene(name);
        StartCoroutine(ResourceManager.instance.LoadResourcesFromQueueAndReplaceScene(delegate (ResourceManager.LoadingProgressInfo prog) {
                    float value;
                    switch (prog.type) {
                    case ResourceManager.LoadingProgressType.Resource:
                        value = prog.value / prog.max * (1.0f - sceneper);
                        if (value != lastValue) {
                            CmdClientLoadProgress(value);
                            lastValue = value;
                        }
                        break;
                    case ResourceManager.LoadingProgressType.Scene:
                        value = 1.0f - sceneper + prog.value / prog.max * sceneper;
                        if (value != lastValue) {
                            CmdClientLoadProgress(value);
                            lastValue = value;
                        }
                        break;
                    case ResourceManager.LoadingProgressType.Custom:
                        break;
                    case ResourceManager.LoadingProgressType.Done:
                        CmdClientLoadSceneFinished();
                        break;
                    }
                }, WaitForOneSeconds));
    }

    [Command]
    public void CmdClientLoadSceneFinished() {
        GameManager.PlayerReady(m_player.id);
        Debug.LogFormat("Player({0}) LoadScene Finished.", m_player.id);
        bool allReady = GameManager.AllPlayersReady();
        if (allReady) {
            Debug.LogFormat("All Players LoadScene Finished.");
            RpcStartScene();
            // 服务器创建单位
#if _UHEROES_
            Invoke("ServerCreateUnits", 1.0f);
#else
            Invoke("ServerCreateTanks", 1.0f);
#endif
        }
    }

    [ClientRpc]
    void RpcStartScene() {
        ResourceManager.instance.StartScene();
    }













    // ============== Unit Game Actions ==============


    // ======== 创建单位, 需在World实例化之后调用 ========
    [Server]
    public void ServerCreateUnits() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建玩家单位
        foreach (GamePlayerController ctrl in GameManager.AllPlayers.Values) {
            Player playerInfo = ctrl.Player;
            string path = string.Format("Units/[Player{0}]", ctrl.playerId);
            SyncUnitInfo syncInfo = new SyncUnitInfo();
            syncInfo.baseInfo = ResourceManager.instance.LoadUnit(path, playerInfo.heroData);
            syncInfo.id = Utils.IdGenerator.nextId;
            syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
            syncInfo.force = playerInfo.force;
            syncInfo.position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
            Unit unit = World.Main.CreateUnit(syncInfo, ctrl.playerId);
            if (ctrl.isLocalPlayer) {
                World.Main.SetCameraFollowed(unit.gameObject);
            }
        }

        // 随即创建单位
        StartCoroutine(RepeatCreateUnit("CreateTestUnit"));

        //CreateOneTestUnit();

        // 世界开始运转
        World.Main.StartWorld();
    }

    IEnumerator RepeatCreateUnit(string name) {
        for (int i = 0; GameManager.AllPlayers.Count <= m_testPlayerCount && i < m_testMax; i++) {
            yield return new WaitForSeconds(m_testCreateRate);
            Invoke(name, 0.0f);
        }
    }

    void CreateOneTestUnit() {
        Vector2 sz = Utils.halfCameraSize;
        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo = ResourceManager.instance.LoadUnit("Units/Arcane");
        syncInfo.position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
        syncInfo.id = Utils.IdGenerator.nextId;
        syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
        syncInfo.force = Utils.Random.Next(8);
        World.Main.CreateUnit(syncInfo);
    }

    void CreateTestUnit() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建普通单位
        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo = ResourceManager.instance.LoadUnit("", m_testUnits[Utils.Random.Next(m_testUnits.Count)].text);
        syncInfo.position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
        syncInfo.id = Utils.IdGenerator.nextId;
        syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
        syncInfo.force = Utils.Random.Next(8);
        World.Main.CreateUnit(syncInfo);
    }

    [Command]
    public void CmdMove(int unitId, Vector2 pos, bool obstinate) {
        //RpcMove(transform.localPosition, pos, obstinate);
        //Unit unit = WorldController.instance.World.GetUnit(id);
        //Debug.LogFormat("ServerMove, unitId({0}) <-> playerId({1}), force({2}), hp({3}/{4})", m_unitCtrl.Unit.Id, PlayerId, m_unitCtrl.Unit.Force, m_unitCtrl.Unit.Hp, m_unitCtrl.Unit.MaxHp);
        Unit unit = World.Main.GetUnit(unitId);
        unit.CommandMove(pos, obstinate);
    }

    [Command]
    public void CmdCastSpell(int unitId, CommandTarget.Type targetType, int targetUnitId, Vector2 targetPoint, string activeSkill, bool obstinate) {
        Unit unit = World.Main.GetUnit(unitId);
        CommandTarget target;
        switch (targetType) {
        default:
            target = new CommandTarget();
            break;
        case CommandTarget.Type.kUnitTarget:
            target = new CommandTarget(World.Main.GetUnit(targetUnitId));
            break;
        case CommandTarget.Type.kPointTarget:
            target = new CommandTarget(targetPoint);
            break;
        }

        foreach (ActiveSkill skill in unit.ActiveSkills) {
            if (skill.name == activeSkill) {
                unit.CommandCastSpell(target, skill, obstinate);
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



    // =============== Tanks ==================
    // 创建坦克单位
    [Server]
    public void ServerCreateTanks() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建玩家单位
        foreach (GamePlayerController ctrl in GameManager.AllPlayers.Values) {
            Player playerInfo = ctrl.Player;
            //string path = string.Format("Units/[Player{0}]", ctrl.playerId);
            SyncTankInfo syncInfo = new SyncTankInfo();
            //syncInfo.baseInfo = ResourceManager.instance.LoadTank(path, playerInfo.heroData);
            syncInfo.baseInfo.model = "Player";
            syncInfo.baseInfo.maxHp = 2000;
            syncInfo.baseInfo.isfixed = false;
            syncInfo.baseInfo.name = "PlayerTank";
            syncInfo.baseInfo.revivable = true;
            syncInfo.baseInfo.move = 2;
            AttackInfo attackSkill = new AttackInfo();
            attackSkill.cd = 1.75f;
            attackSkill.type = "Physical";
            attackSkill.value = 60;
            attackSkill.vrange = 0.2f;
            attackSkill.range = 2.5f;
            attackSkill.horizontal = false;
            attackSkill.animations = new string[0];
            attackSkill.projectile = "Projectiles/MageBolt";
            syncInfo.baseInfo.attackSkill = attackSkill;
            syncInfo.id = Utils.IdGenerator.nextId;
            syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
            syncInfo.force = playerInfo.force;
           
            syncInfo.position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
            syncInfo.rotation = (float)Utils.Random.NextDouble() * 360.0f;
            SyncTankGunInfo gunInfo = new SyncTankGunInfo();
            gunInfo.position = new Vector3Serializable(0.0f, 0.0f, 0.0f);
            gunInfo.rotation = 0.0f;
            gunInfo.rotateSpeed = 1.0f;
            syncInfo.guns.Add(gunInfo);
            Tank unit = World.Main.CreateTank(syncInfo, ctrl.playerId);
            if (ctrl.isLocalPlayer) {
                World.Main.SetCameraFollowed(unit.gameObject);
            }
        }

        // 随即创建单位
        StartCoroutine(RepeatCreateUnit("CreateTestTank"));

        //CreateOneTestUnit();

        // 世界开始运转
        World.Main.StartWorld();
    }

    void CreateTestTank() {
        Vector2 sz = Utils.halfCameraSize;
        // 创建普通单位
        SyncTankInfo syncInfo = new SyncTankInfo();
        //syncInfo.baseInfo = ResourceManager.instance.LoadUnit("", m_testUnits[Utils.Random.Next(m_testUnits.Count)].text);
        syncInfo.baseInfo.model = "Test";
        syncInfo.baseInfo.maxHp = 500;
        syncInfo.baseInfo.isfixed = false;
        syncInfo.baseInfo.name = "TestTank";
        syncInfo.baseInfo.revivable = true;
        syncInfo.baseInfo.move = 2;
        syncInfo.position = new Vector2((float)(-sz.x + Utils.Random.NextDouble() * sz.x * 2), (float)(-sz.y + Utils.Random.NextDouble() * sz.y * 2));
        syncInfo.rotation = (float)Utils.Random.NextDouble() * 360.0f;
        syncInfo.id = Utils.IdGenerator.nextId;
        syncInfo.hp = (float)syncInfo.baseInfo.maxHp;
        syncInfo.force = Utils.Random.Next(8);
        SyncTankGunInfo gunInfo = new SyncTankGunInfo();
        gunInfo.position = new Vector3(0.0f, 0.0f, 0.0f);
        gunInfo.rotation = 0.0f;
        syncInfo.guns.Add(gunInfo);
        World.Main.CreateTank(syncInfo);
    }
}

public struct Player {
    public int id;
    public string name;
    public int force;
    public string heroData;

    public bool Initialized {
        get { return id != 0; }
    }
}
