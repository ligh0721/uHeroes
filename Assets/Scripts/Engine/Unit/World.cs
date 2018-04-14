using UnityEngine;
using System.Collections.Generic;
using cca;

public class World : MonoBehaviour, INetworkable<GamePlayerController> {
    static World _main;

    //public Dictionary<string, GameObject> dbgPosPrefabs = new Dictionary<string, GameObject>();
    public List<GameObject> dbgPos = new List<GameObject>();

    // 用于对象池分配Unit对象
    public GameObject unitPrefab;
    // 用于对象池分配Projectile对象
    public GameObject projectilePrefab;
    // 用于对象池分配UnitHUD对象
    public GameObject unitHUDPrefab;
    // 用于控制摄像机跟踪当前玩家操控的单位
    public CameraFollowPlayer cameraCtrl;
    // Unit HUD parent
    public GameObject hudCanvas;

    bool shutdown = false;
    Dictionary<Unit, int> units = new Dictionary<Unit, int>();
    Dictionary<Unit, int> unitsToRevive = new Dictionary<Unit, int>();
    Dictionary<Projectile, int> projectiles = new Dictionary<Projectile, int>();
    Dictionary<int, Unit> unitsIndex = new Dictionary<int, Unit>();
    //protected Dictionary<int, Projectile> projectilesIndex = new Dictionary<int, Projectile>();

    public static World Current {
        get { return _main; }
    }

    void Awake() {
        if (_main == null) {
            _main = this;
        }
    }

    void OnDestroy() {
        if (_main == this) {
            _main = null;
        }
    }

    void Start() {
        Debug.Assert(unitPrefab != null);
        Debug.Assert(projectilePrefab != null);
        Debug.Assert(unitHUDPrefab != null);
        Debug.Assert(cameraCtrl != null);
        Debug.Assert(hudCanvas != null);

        // unit pool
        GameObjectPool.ResetFunction unitReset = delegate (GameObject obj) {
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            UnitNode node = obj.GetComponent<UnitNode>();
            node.init();
            node.enabled = true;
            Unit unit = obj.GetComponent<Unit>();
            unit.enabled = true;
        };
        GameObjectPool.DestroyFunction unitDestroy = delegate(GameObject obj) {
            Unit unit = obj.GetComponent<Unit>();
            unit.enabled = false;
            unit.Cleanup();
            UnitNode node = obj.GetComponent<UnitNode>();
            node.enabled = false;
            node.cleanup();
        };
        GameObjectPool.instance.Alloc(unitPrefab, 50, unitReset, unitDestroy);

        // projectile pool
        GameObjectPool.ResetFunction projectileReset = delegate (GameObject obj) {
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            ProjectileNode node = obj.GetComponent<ProjectileNode>();
            node.init();
            node.enabled = true;
            Projectile projectile = obj.GetComponent<Projectile>();
            projectile.enabled = true;
        };
        GameObjectPool.DestroyFunction projectileDestroy = delegate (GameObject obj) {
            Projectile projectile = obj.GetComponent<Projectile>();
            projectile.enabled = false;
            //projectile.Cleanup();
            ProjectileNode node = obj.GetComponent<ProjectileNode>();
            node.enabled = false;
            node.cleanup();
        };
        GameObjectPool.instance.Alloc(projectilePrefab, 50, projectileReset, projectileDestroy);

        // unit hud pool
        GameObjectPool.ResetFunction unitHUDReset = delegate (GameObject obj) {
            UnitHUD unitHUD = obj.GetComponent<UnitHUD>();
            unitHUD.enabled = true;
        };
        GameObjectPool.DestroyFunction unitHUDDestroy = delegate(GameObject obj) {
            UnitHUD unitHUD = obj.GetComponent<UnitHUD>();
            unitHUD.enabled = false;
            unitHUD.m_unit. Set(null);
        };
        GameObjectPool.instance.Alloc(unitHUDPrefab, 50, unitHUDReset, unitHUDDestroy);
    }

    void FixedUpdate() {
        Step(Time.fixedDeltaTime);
    }

    void Update() {
        if (localClient != null && isServer) {
            localClient.ServerSyncActions();
        }
    }

    protected void OnTick(float dt) {
    }

    protected void OnAddUnit(Unit unit) {
    }

    protected void OnDelUnit(Unit unit) {
    }

    protected void OnAddProjectile(Projectile projectile) {
    }

    protected void OnDelProjectile(Projectile projectile) {
    }

    public void OnAddNormalAttributes(Unit unit) {
    }

    public void OnDelNormalAttributes(Unit unit) {
    }

    public void AddUnit(Unit unit) {
        unit.m_world = this;
        units.Add(unit, unit.Id);
        unitsIndex.Add(unit.Id, unit);
    }

    public void AddProjectile(Projectile projectile) {
        projectile.m_world = this;
        projectiles.Add(projectile, projectile.Id);
        //m_projectilesIndex.Add(projectile.Id, projectile);
    }

    public void SetCameraFollowed(GameObject obj) {
        cameraCtrl.followed = obj;
    }

    public void SetCameraFollowedEnabled(bool enabled) {
        cameraCtrl.enabled = enabled;
    }

    /// <summary>
    /// Server发起
    /// </summary>
    /// <param name="syncInfo"></param>
    /// <param name="playerId"></param>
    public Unit CreateUnit(SyncUnitInfo syncInfo, int playerId = 0) {
        localClient.ServerAddSyncAction(new SyncCreateUnit(syncInfo, playerId));

        GamePlayerController player;
        GameManager.AllPlayers.TryGetValue(playerId, out player);

        GameObject obj = GameObjectPool.instance.Instantiate(unitPrefab);
        UnitNode node = obj.GetComponent<UnitNode>();
        Unit unit = obj.GetComponent<Unit>();

        ResourceManager.instance.LoadUnitModel(syncInfo.baseInfo.model);  // high time cost
        ResourceManager.instance.AssignModelToUnitNode(syncInfo.baseInfo.model, node);

        unit.m_id = syncInfo.id;
        node.m_id = syncInfo.id;
        AddUnit(unit);

        unit.m_model = syncInfo.baseInfo.model;
        if (isServer) {
            unit.AI = UnitAI.instance;
        }

        unit.Name = syncInfo.baseInfo.name;
        unit.InitHp(syncInfo.hp, (float)syncInfo.baseInfo.maxHp);
        if (syncInfo.baseInfo.attackSkill.valid) {
            AttackAct atk = new AttackAct(syncInfo.baseInfo.attackSkill.name, (float)syncInfo.baseInfo.attackSkill.cd, new AttackValue(AttackValue.NameToType(syncInfo.baseInfo.attackSkill.type), (float)syncInfo.baseInfo.attackSkill.value), (float)syncInfo.baseInfo.attackSkill.vrange);
            atk.CastRange = (float)syncInfo.baseInfo.attackSkill.range;
            atk.CastHorizontal = syncInfo.baseInfo.attackSkill.horizontal;
            foreach (string ani in syncInfo.baseInfo.attackSkill.animations) {
                atk.AddCastAnimation(ModelNode.NameToId(ani));
            }
            atk.ProjectileTemplate = ResourceManager.instance.LoadProjectile(syncInfo.baseInfo.attackSkill.projectile);
            unit.AddActiveSkill(atk);
        }
        node.position = syncInfo.position;
        node.SetFlippedX(syncInfo.flippedX);
        unit.force.Force = syncInfo.force;
        unit.MoveSpeedBase = (float)syncInfo.baseInfo.move;
        unit.Revivable = syncInfo.baseInfo.revivable;
        unit.Fixed = syncInfo.baseInfo.isfixed;

        if (player != null) {
            // 玩家单位
            Debug.LogFormat("CreateUnit, unitId({0}) <-> playerId({1}).", unit.Id, player.playerId);
            if (player == localClient) {
                Debug.LogFormat("That's Me, {0}.", unit.Name);
            }

            // TEST !!!!
            unit.InitHp(1000, 1000);
            unit.AttackSkill.coolDownBase = 2.0f;
            unit.AttackSkill.coolDownSpeedCoeff = 2;
            unit.CriticalRateBase = 0.2f;
            unit.CriticalDamageBase = 20.0f;

            SplashPas splash = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
            unit.AddPassiveSkill(splash);

            if (player == localClient) {
                BattleWorldUI.Current.portraitGroup.AddPortrait(unit);
            }
        }

        node.SetFrame(ModelNode.kFrameDefault);
        CreateUnitHUD(unit);
        return unit;
    }

    public void CreatePlayerUnit(SyncUnitInfo syncInfo, int playerId = 0) {

    }

    public void RemoveUnit(Unit unit, bool revivalbe = false) {
        localClient.ServerAddSyncAction(new SyncRemoveUnit(unit, revivalbe));
        if (!units.ContainsKey(unit)) {
            return;
        }

        int id = units[unit];

        OnDelUnit(unit);

        if (revivalbe) {
            // 如果单位可以复活，拖进灵魂域
            unitsToRevive.Add(unit, id);
        } else {
            // 如果不可以复活，该单位将不再拥有世界，清除该单位的所有CD中的技能
            unit.m_world = null;
            CleanAbilitiesCD(unit);
            GameObjectPool.instance.Destroy(unitPrefab, unit.gameObject);
        }
        unitsIndex.Remove(id);
        units.Remove(unit);
    }

    public UnitHUD CreateUnitHUD(Unit unit) {
        GameObject obj = GameObjectPool.instance.Instantiate(unitHUDPrefab);
        obj.transform.SetParent(hudCanvas.transform);
        UnitHUD unitHUD = obj.GetComponent<UnitHUD>();
        unitHUD.m_unit.Set(unit);
        unitHUD.UpdateRectTransform();
        unit.m_unitHUD = unitHUD;
        return unitHUD;
    }

    public void RemoveUnitHUD(Unit unit) {
        localClient.ServerAddSyncAction(new SyncRemoveUnitHUD(unit));
        GameObjectPool.instance.Destroy(unitHUDPrefab, unit.m_unitHUD.gameObject);
        unit.m_unitHUD = null;
    }

    public Projectile CreateProjectile(ProjectileSyncInfo syncInfo, Skill sourceSkill = null) {
        localClient.ServerAddSyncAction(new SyncCreateProjectile(syncInfo));

        GameObject obj = GameObjectPool.instance.Instantiate(projectilePrefab);
        ProjectileNode node = obj.GetComponent<ProjectileNode>();
        Projectile projectile = obj.GetComponent<Projectile>();

        ResourceManager.instance.LoadProjectileModel(syncInfo.baseInfo.model);  // high time cost
        ResourceManager.instance.AssignModelToProjectileNode(syncInfo.baseInfo.model, node);

        projectile.MoveSpeed = (float)syncInfo.baseInfo.move;
        projectile.MaxHeightDelta = (float)syncInfo.baseInfo.height;
        projectile.TypeOfFire = Projectile.FireNameToType(syncInfo.baseInfo.fire);
        projectile.EffectFlags = (uint)syncInfo.baseInfo.effect;

        //node.position = syncInfo.position;
        //node.visible = syncInfo.visible;
        projectile.TypeOfFromTo = syncInfo.fromTo;
        projectile.UseFireOffset = syncInfo.useFireOffset;
        projectile.SourceUnit = GetUnit(syncInfo.srcUnit);
        projectile.FromUnit = GetUnit(syncInfo.fromUnit);
        projectile.ToUnit = GetUnit(syncInfo.toUnit);
        projectile.FromPosition = syncInfo.fromPos;
        projectile.ToPosition = syncInfo.toPos;
        if (sourceSkill != null) {
            projectile.SourceSkill = sourceSkill;
            projectile.EffectiveTypeFlags = sourceSkill.effectiveTypeFlags;
        }

        AddProjectile(projectile);
        node.SetFrame(ModelNode.kFrameDefault);
        projectile.Fire();
        return projectile;
    }

    public Tank CreateTank(TankSyncInfo syncInfo, int playerId = 0) {
        GamePlayerController player;
        GameManager.AllPlayers.TryGetValue(playerId, out player);

        GameObject obj = GameObjectPool.instance.Instantiate(unitPrefab);
        TankNode node = obj.GetComponent<TankNode>();
        Tank unit = obj.GetComponent<Tank>();
        //TankController ctrl = obj.GetComponent<TankController>();

        ResourceManager.instance.LoadUnitModel(syncInfo.baseInfo.model);  // high time cost
        ResourceManager.instance.AssignModelToUnitNode(syncInfo.baseInfo.model, node);

        unit.m_id = syncInfo.id;
        node.m_id = syncInfo.id;
        AddUnit(unit);

        //unit.m_client = player;
        unit.m_model = syncInfo.baseInfo.model;
        if (localClient.isServer) {
            unit.AI = UnitAI.instance;
        }

        unit.Name = syncInfo.baseInfo.name;
        unit.MaxHpBase = (float)syncInfo.baseInfo.maxHp;
        if (syncInfo.baseInfo.attackSkill.valid) {
            AttackAct atk = new AttackAct(syncInfo.baseInfo.attackSkill.name, (float)syncInfo.baseInfo.attackSkill.cd, new AttackValue(AttackValue.NameToType(syncInfo.baseInfo.attackSkill.type), (float)syncInfo.baseInfo.attackSkill.value), (float)syncInfo.baseInfo.attackSkill.vrange);
            atk.CastRange = (float)syncInfo.baseInfo.attackSkill.range;
            atk.CastHorizontal = syncInfo.baseInfo.attackSkill.horizontal;
            foreach (var ani in syncInfo.baseInfo.attackSkill.animations) {
                atk.AddCastAnimation(ModelNode.NameToId(ani));
            }
            atk.ProjectileTemplate = ResourceManager.instance.LoadProjectile(syncInfo.baseInfo.attackSkill.projectile);
            atk.ProjectileTemplate.fire = "Straight";
            unit.AddActiveSkill(atk);
        }

        node.position = syncInfo.position;
        node.rotation = syncInfo.rotation;
        unit.Hp = syncInfo.hp;
        unit.force.Force = syncInfo.force;
        unit.MoveSpeedBase = (float)syncInfo.baseInfo.move;
        unit.Revivable = syncInfo.baseInfo.revivable;
        unit.Fixed = syncInfo.baseInfo.isfixed;

        for (int i = 0; i < syncInfo.guns.Count; ++i){
            unit.AddGun(i);
            unit.SetGunPosition(i, syncInfo.guns[i].position);
            unit.SetGunRotation(i, syncInfo.guns[i].rotation);
            //syncInfo.guns[i].rotateSpeed;
        }

        return unit;
    }

    protected void ReviveUnit(Unit unit, float hp) {
        Debug.Assert(unit.enabled);
        if (!unitsToRevive.ContainsKey(unit)) {
            return;
        }

        AddUnit(unit);
        unit.Revive(hp);

        unitsToRevive.Remove(unit);
    }

    public void RemoveProjectile(Projectile projectile) {
        if (!projectiles.ContainsKey(projectile)) {
            return;
        }

        OnDelProjectile(projectile);

        projectile.m_world = null;
        //int id = m_projectiles[projectile];
        //m_projectilesIndex.Remove(id);
        projectiles.Remove(projectile);
        GameObjectPool.instance.Destroy(projectilePrefab, projectile.gameObject);
    }

    public void RemovePlayerUnits(GamePlayerController player) {
#if false
        List<Unit> toDel = new List<Unit>();
        foreach (Unit unit in units.Keys) {
            if (unit.client == player) {
                toDel.Add(unit);
            }
        }
        foreach (Unit unit in toDel) {
            RemoveUnit(unit);
        }
        toDel.Clear();

        foreach (Unit unit in unitsToRevive.Keys) {
            if (unit.client == player) {
                toDel.Add(unit);
            }
        }
        foreach (Unit unit in toDel) {
            CleanAbilitiesCD(unit);
            unitsToRevive.Remove(unit);
        }
#endif
    }

    // Skill
    public void AddSkillCD(Skill skill) {
        Debug.Assert(skill.coolingDown);
        m_skillsCD.Add(skill);
        //Debug.LogFormat("{0}的{1}技能开始冷却({2}s).", skill.Owner.Name, skill.Name, skill.CoolDown);
    }

    public void RemoveSkillCD(Skill skill) {
        m_skillsCD.Remove(skill);
    }

    public bool IsSkillCD(Skill skill) {
        return m_skillsCD.Contains(skill);
    }

    public void UpdateSkillCD(Skill skill) {
        if (!m_skillsCD.Contains(skill)) {
            return;
        }

        if (skill.coolingDown) {
            return;
        }

        m_skillsCD.Remove(skill);
        SkillReady(skill);
    }

    protected void CleanAbilitiesCD(Unit unit) {
        foreach (ActiveSkill skill in unit.ActiveSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }

        foreach (PassiveSkill skill in unit.PassiveSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }

        foreach (BuffSkill skill in unit.BuffSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }
    }

    protected HashSet<Skill> m_skillsCD = new HashSet<Skill>();

    protected void SkillReady(Skill skill) {
        // 由于技能的所有者可能在等待重生，所以主世界可能不存在该单位，但是单位仍未被释放
        Unit o = skill.owner;
        if (o != null && !o.Dead) {
            // 存在于主世界中，则触发事件
            o.OnSkillReady(skill);
        }

        // 防止BUFF更改CD而导技能在致脱离CD管理器后CD大于Elapsed
        skill.coolingDownElapsed = float.MaxValue;
    }

    public void Step(float dt) {
        if (shutdown) {
            return;
        }

        // 单位死亡后技能CD独立计算，所以放在此处独立计算，不整合到单位onTick中
        List<Skill> delayToDel = new List<Skill>();
        foreach (Skill skill in m_skillsCD) {
            skill.coolingDownElapsed += dt;
            if (!skill.coolingDown) {
                // 如果技能已经就绪，从中删除
                SkillReady(skill);
                delayToDel.Add(skill);
            }
        }
        foreach (Skill toDel in delayToDel) {
            m_skillsCD.Remove(toDel);
        }
        delayToDel.Clear();

        if (isServer) {
            foreach (Unit unit in units.Keys) {
                unit.Step(dt);
                if (unit.Dead && !unit.IsDoingOr(Unit.kDoingDying)) {
                    // 刚死，计划最后移除该单位
                    unit.OnDying();
                    RemoveUnitHUD(unit);
                }
            }

            foreach (Projectile projectile in projectiles.Keys) {
                projectile.Step(dt);
            }
        }

        OnTick(dt);
    }

    public void StartWorld() {
        localClient.ServerAddSyncAction(new SyncStartWorld());
        shutdown = false;
    }

    public void StopWorld() {  // 为避免CUnitEventAdapter(CWorld).CUnit.CWorld的retain cycle，在这里主动销毁一些子节点
        // TODO: SyncStopWorld
        OnStop();
        shutdown = true;
        units.Clear();
        unitsToRevive.Clear();
        projectiles.Clear();
        unitsIndex.Clear();
        //m_projectilesIndex.Clear();
        m_skillsCD.Clear();
        // TODO: RemoveAllUnit and Projectile
    }

    protected void OnStop() {
    }

    public Unit GetUnit(int id) {
        if (id == 0) {
            return null;
        }

        Unit ret;
        unitsIndex.TryGetValue(id, out ret);
        return ret;
    }

    /*
    public Projectile GetProjectile(int id)
    {
        if (id == 0)
        {
            return null;
        }

        Projectile ret;
        m_projectilesIndex.TryGetValue(id, out ret);
        return ret;
    }
    */

    public Dictionary<Unit, int> Units {
        get { return units; }
    }

    public GamePlayerController localClient {
        get { return GamePlayerController.localClient; }
    }

    public bool isServer {
        get { return GamePlayerController.localClient.isServer; }
    }
}
