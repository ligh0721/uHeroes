using UnityEngine;
using System.Collections.Generic;
using cca;

public class World {
    public WorldController Controller {
        get {
            return m_ctrl;
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
        unit.World = this;
        m_units.Add(unit, unit.Id);
        m_unitsIndex.Add(unit.Id, unit);
    }

    public void AddProjectile(Projectile projectile) {
        projectile.World = this;
        m_projectiles.Add(projectile, projectile.Id);
        //m_projectilesIndex.Add(projectile.Id, projectile);
    }

    /// <summary>
    /// Server发起
    /// </summary>
    /// <param name="syncInfo"></param>
    /// <param name="playerId"></param>
    public void CreateUnit(SyncUnitInfo syncInfo, int playerId = 0) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncCreateUnit(syncInfo, playerId));

        GamePlayerController client;
        if (GameController.AllPlayers.TryGetValue(playerId, out client)) {
            // 玩家单位
            UnitController unitCtrl = UnitController.Create(syncInfo, client);
            client.unitCtrl = unitCtrl;
            Debug.LogFormat("CreateUnit, unitId({0}) <-> playerId({1}).", unitCtrl.unit.Id, client.playerId);
            if (client == GamePlayerController.localClient) {
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

    public void RemoveUnit(Unit unit, bool revivalbe = false) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncRemoveUnit(unit.Id, revivalbe));
        if (!m_units.ContainsKey(unit)) {
            return;
        }

        int id = m_units[unit];

        OnDelUnit(unit);

        if (revivalbe) {
            // 如果单位可以复活，拖进灵魂域
            m_unitsToRevive.Add(unit, id);
        } else {
            // 如果不可以复活，该单位将不再拥有世界，清除该单位的所有CD中的技能
            unit.World = null;
            CleanAbilitiesCD(unit);
        }

        m_unitsIndex.Remove(id);
        m_units.Remove(unit);
        unit.Renderer.Node.destroy();
    }

    protected void ReviveUnit(Unit unit, float hp) {
        if (!m_unitsToRevive.ContainsKey(unit)) {
            return;
        }

        if (!unit.Valid) {
            return;
        }

        AddUnit(unit);
        unit.Revive(hp);

        m_unitsToRevive.Remove(unit);
    }

    public void RemoveProjectile(Projectile projectile) {
        if (!m_projectiles.ContainsKey(projectile)) {
            return;
        }

        OnDelProjectile(projectile);

        projectile.World = null;
        //int id = m_projectiles[projectile];
        //m_projectilesIndex.Remove(id);
        projectile.World = null;
        m_projectiles.Remove(projectile);
        projectile.Renderer.Node.destroy();
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
        foreach (var skill in unit.ActiveSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }

        foreach (var skill in unit.PassiveSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }

        foreach (var skill in unit.BuffSkills) {
            if (skill.coolingDown) {
                m_skillsCD.Remove(skill);
            }
        }
    }

    protected HashSet<Skill> m_skillsCD = new HashSet<Skill>();

    protected void SkillReady(Skill skill) {
        // 由于技能的所有者可能在等待重生，所以主世界可能不存在该单位，但是单位仍未被释放
        Unit o = skill.owner;
        if (o != null && o.Valid) {
            // 存在于主世界中，则触发事件
            o.OnSkillReady(skill);
        }

        // 防止BUFF更改CD而导技能在致脱离CD管理器后CD大于Elapsed
        skill.coolingDownElapsed = float.MaxValue;
    }

    public void Step(float dt) {
        if (m_shutdown) {
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

        foreach (var kv in m_units) {
            var unit = kv.Key;
            unit.Step(dt);

            if (unit.Dead && !unit.IsDoingOr(Unit.kDoingDying))  // terrible code
            {
                // 刚死，计划最后移除该单位
                unit.OnDying();
            }
        }

        foreach (var kv in m_projectiles) {
            var projectile = kv.Key;
            projectile.Step(dt);
        }

        OnTick(dt);
    }

    public void Start() {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncStartWorld());
        m_shutdown = false;
    }

    public void Shutdown()  // 为避免CUnitEventAdapter(CWorld).CUnit.CWorld的retain cycle，在这里主动销毁一些子节点
    {
        OnShutDown();
        m_shutdown = true;
        m_units.Clear();
        m_unitsToRevive.Clear();
        m_projectiles.Clear();
        m_unitsIndex.Clear();
        //m_projectilesIndex.Clear();
        m_skillsCD.Clear();

    }

    protected void OnShutDown() {
    }

    public Unit GetUnit(int id) {
        if (id == 0) {
            return null;
        }

        Unit ret;
        m_unitsIndex.TryGetValue(id, out ret);
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
        get {
            return m_units;
        }
    }

    protected bool m_shutdown;

    protected Dictionary<Unit, int> m_units = new Dictionary<Unit, int>();
    protected Dictionary<Unit, int> m_unitsToRevive = new Dictionary<Unit, int>();
    protected Dictionary<Projectile, int> m_projectiles = new Dictionary<Projectile, int>();
    protected Dictionary<int, Unit> m_unitsIndex = new Dictionary<int, Unit>();
    //protected Dictionary<int, Projectile> m_projectilesIndex = new Dictionary<int, Projectile>();
    protected internal WorldController m_ctrl;
}
