using UnityEngine;
using System.Collections.Generic;


public class Unit : UnitForce, INetworkable<GamePlayerController> {
    public Unit(UnitRenderer renderer) {
        m_renderer = renderer;
        renderer.m_unit = this;
        m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
    }

    public static implicit operator bool (Unit unit) {
        return unit != null && unit.Valid;
    }

    public UnitRenderer Renderer {
        get {
            return m_renderer;
        }
    }

    public bool Valid {
        get {
            return m_renderer != null && m_renderer.Valid;
        }
    }

    public int Id {
        get {
            return m_id;
        }
    }

    public string Name {
        get {
            return m_name;
        }

        set {
            m_name = value;
        }
    }

    public World World {
        get {
            return m_world;
        }

        set {
            m_world = value;
        }
    }

    public string Root {
        get {
            return m_root;
        }
    }

    protected UnitRenderer m_renderer;
    protected World m_world;
    protected string m_name;
    protected internal int m_id;
    protected internal string m_root;

    // Networkable
    protected internal GamePlayerController m_client;
    public GamePlayerController client {
        get {
            return m_client;
        }
    }

    public GamePlayerController localClient {
        get {
            return GamePlayerController.localClient;
        }
    }

    public bool isServer {
        get {
            return localClient.isServer;
        }
    }

    // event
    // 等级变化时被通知，在通过addExp升级的时候，通常来讲changed总是为1，尽管经验有时会足够多以至于连升2级
    public void OnLevelChanged(int changed) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitLevelChanged(this, changed);
        }
    }

    // 复活时被通知
    public void OnRevive() {
        if (!isServer) {
            return;
        }

        m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
    }

    // 死亡时被通知
    public void OnDying() {
        if (!isServer) {
            return;
        }

        if (Dead) {
            Die();
        }

        TriggerOnDying();

        if (m_AI != null) {
            m_AI.OnUnitDying(this);
        }
    }

    // 死亡后被通知
    protected void OnDead() {
        if (!isServer) {
            return;
        }

        TriggerOnDead();

        if (!Dead) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitDead(this);
        }
    }

    // 血量变化时被通知
    protected void OnHpChanged(float changed) {
        if (!isServer) {
            return;
        }

        TriggerOnHpChanged(changed);

        if (m_AI != null) {
            m_AI.OnUnitHpChanged(this, changed);
        }
    }

    // 每个游戏刻被通知
    public void Step(float dt) {
        if (!isServer) {
            return;
        }

        UpdateBuffSkillElapsed(dt);

        OnTick(dt);

        // show hpChanged text
        if (m_renderer != null && m_renderer.Valid) {
            foreach (var kv in m_hpChanged) {
                int id = kv.Key;
                float val = kv.Value;
                if (val > 0) {
                    if (id == 0) {
                        // heal
                        string str = ((int)val).ToString();
                        m_renderer.AddBattleTip(str, "", 18, new Color(40 / 255.0f, 220 / 255.0f, 40 / 255.0f));
                    } else {
                        // damage
                        string str = ((int)-val).ToString();
                        m_renderer.AddBattleTip(str, "", 18, new Color(220 / 255.0f, 40 / 255.0f, 40 / 255.0f));
                    }
                }
            }
        }
        m_hpChanged.Clear();
    }

    protected Dictionary<int, float> m_hpChanged = new Dictionary<int, float>();
    // 用来累加伤害/治疗数字ff

    protected void OnTick(float dt) {
        if (!Valid) {
            return;
        }

        if (!Dead) {
            OnCommandTick(dt);
        }

        // TODO: TriggerOnTick(dt);

        if (m_AI != null && !Dead) {
            m_AI.OnUnitTick(this, dt);
        }
    }

    protected void OnCommandTick(float dt) {
        // 为了沿路径移动以及校正施法位置
        if (Suspended) {
            return;
        }

        if (m_movePath != null && !IsDoingOr(kDoingMoving | kDoingCasting)) {
            StartDoing(kDoingAlongPath);
            if (m_pathObstinate) {
                StartDoing(kDoingObstinate);
            }
        }

        // 路径逻辑
        if (m_movePath != null && IsDoingOr(kDoingAlongPath)) {
            // 正在运行路径
            Vector2 target = m_movePath.getCurTargetPoint(m_pathCurPos);
            bool bPathEnd = false;
            if (Vector2.Distance(target, m_renderer.Node.position) < m_pathBufArrive) {
                bPathEnd = m_movePath.arriveCurTargetPoint(ref m_pathCurPos);
                Vector2.Distance(target, m_renderer.Node.position);
            }

            if (bPathEnd) {
                m_movePath = null;
                EndDoing(kDoingAlongPath);
            } else if ((target != m_lastMoveTo || IsDoingOr(kDoingMoving) == false) && IsDoingOr(kDoingCasting) == false) {
                // 单位没有施法，并且当前路径目标点不是移动目标点 或 单位没在移动，则继续沿路径行进
                if (m_pathObstinate) {
                    StartDoing(kDoingObstinate);
                } else {
                    EndDoing(kDoingObstinate);
                }

                Move(target);
            }
        }

        if (IsDoingAnd(kDoingCasting) && m_renderer.IsDoingAction(m_castActionId) == false) {
            // 正在施法，且不是在施法动画中
            Unit t = null;
            UnitRenderer td = null;
            ActiveSkill skill = m_castActiveSkill;
            if (skill != null) {
                // 如果施法技能仍存在
                bool bUnitTarget = m_castTarget.TargetType == CommandTarget.Type.kUnitTarget;
                if (bUnitTarget) {
                    // 如果是以单位为目标的技能
                    t = m_castTarget.TargetUnit;
                    if (t != null && t.Valid && !t.Dead) {
                        // 单位存在且单位没有死亡
                        td = t.Renderer;
                        Debug.Assert(td != null);
                        m_castTarget.UpdateTargetPoint();
                    } else {
                        StopCast(false);
                        EndDoing(kDoingObstinate);  // 没有成功施法，需要取出固执状态
                        return;
                    }
                }

                if (CheckCastTargetDistance(skill, m_renderer.Node.position, m_castTarget, t)) {
                    // 施法
                    if (!m_fixed) {
                        m_renderer.SetFlippedX(m_castTarget.TargetPoint.x < m_renderer.Node.position.x);
                    }

                    CastSpell(skill);
                } else if (skill.coolingDown == false && (IsDoingOr(kDoingMoving) == false || CheckCastTargetDistance(skill, m_lastMoveTo, m_castTarget, t) == false)) {
                    if (!m_fixed) {
                        MoveToCastPosition(skill, t);
                    } else {
                        StopCast(false);
                        EndDoing(kDoingObstinate);
                    }
                }
                return;
            } else {
                EndDoing(kDoingObstinate);  // 没有成功施法，需要去除固执状态
            }
        }
    }
    // 攻击发出时，攻击者被通知
    void OnAttackTarget(AttackData attack, Unit target, uint triggerMask) {
        if (!isServer) {
            return;
        }

        if ((triggerMask & kTriggerOnAttackTargetTrigger) == 0) {
            TriggerOnAttackTarget(attack, target);
        }

        if (m_AI != null) {
            m_AI.OnUnitAttackTarget(this, attack, target);
        }
    }

    // 攻击抵达时，受害者被通知
    bool OnAttacked(AttackData attack, Unit source, uint triggerMask) {
        if (!isServer) {
            return true;
        }

        bool res = true;
        if ((triggerMask & kTriggerOnAttackedTrigger) == 0) {
            res = TriggerOnAttacked(attack, source);
        }

        if (m_AI != null) {
            m_AI.OnUnitAttacked(this, attack, source);
        }

        return res;
    }

    // 攻击命中时，受害者被通知
    void OnDamaged(AttackData attack, Unit source, uint triggerMask) {
        if (!isServer) {
            return;
        }

        if ((triggerMask & kTriggerOnDamagedSurfaceTrigger) == 0) {
            TriggerOnDamagedSurface(attack, source);
        }

        if ((triggerMask & kTriggerOnDamagedInnerTrigger) == 0) {
            TriggerOnDamagedInner(attack, source);
        }

        if (m_AI != null) {
            m_AI.OnUnitDamaged(this, attack, source);
        }
    }

    // 攻击命中时，受害者被通知
    void OnDamagedDone(float fDamage, Unit source, uint triggerMask) {
        if (!isServer) {
            return;
        }

        if ((triggerMask & kTriggerOnDamagedDoneTrigger) == 0) {
            TriggerOnDamagedDone(fDamage, source);
        }

        if (m_AI != null) {
            m_AI.OnUnitDamagedDone(this, fDamage, source);
        }
    }

    // 攻击命中时，攻击者被通知
    void OnDamageTargetDone(float fDamage, Unit target, uint triggerMask) {
        if (!isServer) {
            return;
        }

        if ((triggerMask & kTriggerOnDamageTargetDoneTrigger) == 0) {
            TriggerOnDamageTargetDone(fDamage, target);
        }

        TriggerOnCalcDamageTarget(fDamage, target);

        if (m_AI != null) {
            m_AI.OnUnitDamageTargetDone(this, fDamage, target);
        }
    }

    // 攻击数据消除时被通知，通常由投射物携带攻击数据，二者生存期一致
    public void OnProjectileEffect(Projectile projectile, Unit target) {
        if (!isServer) {
            return;
        }

        TriggerOnProjectileEffect(projectile, target);

        if (m_AI != null) {
            m_AI.OnUnitProjectileEffect(this, projectile, target);
        }
    }

    public bool OnProjectileArrive(Projectile projectile) {
        if (!isServer) {
            return true;
        }

        if (TriggerOnProjectileArrive(projectile) == false) {
            return false;
        }

        if (m_AI != null) {
            m_AI.OnUnitProjectileArrive(this, projectile);
        }

        return true;
    }

    public void OnAddActiveSkill(ActiveSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitAddActiveSkill(this, skill);
        }
    }

    public void OnDelActiveSkill(ActiveSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitDelActiveSkill(this, skill);
        }
    }

    public void OnAddPassiveSkill(PassiveSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitAddPassiveSkill(this, skill);
        }
    }

    public void OnDelPassiveSkill(PassiveSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitDelPassiveSkill(this, skill);
        }
    }

    public void OnAddBuffSkill(BuffSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitAddBuffSkill(this, skill);
        }
    }

    public void OnDelBuffSkill(BuffSkill skill) {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitDelBuffSkill(this, skill);
        }
    }

    // 技能CD开始时被通知
    public void OnSkillCD(Skill skill)  // 以后将区分出onItemCD
    {
        if (!isServer) {
            return;
        }

        if (m_AI != null) {
            m_AI.OnUnitSkillCD(this, skill);
        }
    }

    // 技能CD结束时被通知
    public void OnSkillReady(Skill skill)  // 以后将区分出onItemReady
    {
        if (!isServer) {
            return;
        }

        skill.OnUnitSkillReady();

        if (m_AI != null) {
            m_AI.OnUnitSkillReady(this, skill);
        }
    }

    // Trigger
    public const uint kTriggerOnReviveTrigger = 1 << 0;
    public const uint kTriggerOnDyingTrigger = 1 << 1;
    public const uint kTriggerOnDeadTrigger = 1 << 2;
    public const uint kTriggerOnHpChangedTrigger = 1 << 3;
    public const uint kTriggerOnTickTrigger = 1 << 4;
    public const uint kTriggerOnAttackTargetTrigger = 1 << 5;
    public const uint kTriggerOnAttackedTrigger = 1 << 6;
    public const uint kTriggerOnDamagedSurfaceTrigger = 1 << 7;
    public const uint kTriggerOnDamagedInnerTrigger = 1 << 8;
    public const uint kTriggerOnDamagedDoneTrigger = 1 << 9;
    public const uint kTriggerOnDamageTargetDoneTrigger = 1 << 10;
    public const uint kTriggerOnProjectileEffectTrigger = 1 << 11;
    public const uint kTriggerOnProjectileArriveTrigger = 1 << 12;
    public const uint kTriggerOnCalcDamageTargetTrigger = 1 << 13;

    public const uint kTriggerMaskNoMasked = 0;
    public const uint kTriggerMaskAll = 0xFFFFFFFF;
    public const uint kTriggerMaskActiveTrigger = kTriggerOnAttackTargetTrigger | kTriggerOnDamageTargetDoneTrigger;

    protected HashSet<Skill> m_onAttackTargetTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onAttackedTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDamagedSurfaceTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDamagedInnerTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDamagedDoneTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDamageTargetDoneTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onHpChangedTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onReviveTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDyingTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onDeadTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onTickTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onProjectileEffectTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onProjectileArriveTriggerSkills = new HashSet<Skill>();
    protected HashSet<Skill> m_onCalcDamageTargetTriggerSkills = new HashSet<Skill>();

    protected HashSet<Skill> m_triggerSkillsToAdd = new HashSet<Skill>();
    protected HashSet<Skill> m_triggerSkillsToDel = new HashSet<Skill>();

    // 添加触发器
    public void AddSkillToTriggers(Skill skill) {
        Debug.Assert(skill != null);
        uint triggerFlags = skill.triggerFlags;
        if (triggerFlags == 0) {
            return;
        }

        if (TriggerFree == false) {
            m_triggerSkillsToAdd.Add(skill);
            return;
        }

        if ((triggerFlags & kTriggerOnReviveTrigger) != 0) {
            m_onReviveTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDyingTrigger) != 0) {
            m_onDyingTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDeadTrigger) != 0) {
            m_onDeadTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnHpChangedTrigger) != 0) {
            m_onHpChangedTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnTickTrigger) != 0) {
            m_onTickTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnAttackTargetTrigger) != 0) {
            m_onAttackTargetTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnAttackedTrigger) != 0) {
            m_onAttackedTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedSurfaceTrigger) != 0) {
            m_onDamagedSurfaceTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedInnerTrigger) != 0) {
            m_onDamagedInnerTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedDoneTrigger) != 0) {
            m_onDamagedDoneTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnDamageTargetDoneTrigger) != 0) {
            m_onDamageTargetDoneTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnProjectileEffectTrigger) != 0) {
            m_onProjectileEffectTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnProjectileArriveTrigger) != 0) {
            m_onProjectileArriveTriggerSkills.Add(skill);
        }

        if ((triggerFlags & kTriggerOnCalcDamageTargetTrigger) != 0) {
            m_onCalcDamageTargetTriggerSkills.Add(skill);
        }
    }

    // 删除触发器
    public void RemoveSkillFromTriggers(Skill skill) {
        Debug.Assert(skill != null);
        uint triggerFlags = skill.triggerFlags;
        if (triggerFlags == 0) {
            return;
        }

        if (TriggerFree == false) {
            m_triggerSkillsToDel.Add(skill);
            return;
        }

        if ((triggerFlags & kTriggerOnReviveTrigger) != 0) {
            m_onReviveTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDyingTrigger) != 0) {
            m_onDyingTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDeadTrigger) != 0) {
            m_onDeadTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnHpChangedTrigger) != 0) {
            m_onHpChangedTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnTickTrigger) != 0) {
            m_onTickTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnAttackTargetTrigger) != 0) {
            m_onAttackTargetTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnAttackedTrigger) != 0) {
            m_onAttackedTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedSurfaceTrigger) != 0) {
            m_onDamagedSurfaceTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedInnerTrigger) != 0) {
            m_onDamagedInnerTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDamagedDoneTrigger) != 0) {
            m_onDamagedDoneTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnDamageTargetDoneTrigger) != 0) {
            m_onDamageTargetDoneTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnProjectileEffectTrigger) != 0) {
            m_onProjectileEffectTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnProjectileArriveTrigger) != 0) {
            m_onProjectileArriveTriggerSkills.Remove(skill);
        }

        if ((triggerFlags & kTriggerOnCalcDamageTargetTrigger) != 0) {
            m_onCalcDamageTargetTriggerSkills.Remove(skill);
        }
    }

    // 只能在triggerFree的时候调用
    protected void UpdateTriggerSkillsWhenTriggerFree() {
        Debug.Assert(TriggerFree);

        foreach (var skill in m_triggerSkillsToAdd) {
            AddSkillToTriggers(skill);
        }

        foreach (var skill in m_triggerSkillsToDel) {
            RemoveSkillFromTriggers(skill);
        }

        m_triggerSkillsToAdd.Clear();
        m_triggerSkillsToDel.Clear();
    }

    // trigger之间是有可能存在嵌套关系的
    // 为了安全增删trigger，需要维护一个引用计数
    protected int m_triggerRefCount;

    protected void BeginTrigger() {
        ++m_triggerRefCount;
    }

    protected void EndTrigger() {
        Debug.Assert(m_triggerRefCount > 0);
        --m_triggerRefCount;
        if (m_triggerRefCount == 0) {
            UpdateTriggerSkillsWhenTriggerFree();
        }
    }

    protected bool TriggerFree {
        get {
            return m_triggerRefCount == 0;
        }
    }

    // 触发器链的触发，内部调用
    protected void TriggerOnRevive() {
        BeginTrigger();
        foreach (Skill skill in m_onReviveTriggerSkills) {
            skill.OnUnitRevive();
        }
        EndTrigger();
    }

    protected void TriggerOnDying() {
        BeginTrigger();
        foreach (Skill skill in m_onDyingTriggerSkills) {
            skill.OnUnitDying();
        }
        EndTrigger();
    }

    protected void TriggerOnDead() {
        BeginTrigger();
        foreach (Skill skill in m_onDeadTriggerSkills) {
            skill.OnUnitDead();
        }
        EndTrigger();
    }

    protected void TriggerOnHpChanged(float changed) {
        BeginTrigger();
        foreach (Skill skill in m_onHpChangedTriggerSkills) {
            skill.OnUnitHpChanged(changed);
        }
        EndTrigger();
    }

    protected void TriggerOnTick(float dt) {
        BeginTrigger();
        foreach (Skill skill in m_onTickTriggerSkills) {
            if (skill.interval > 0.0f) {
                skill.intervalElapsed += dt;

                while (skill.intervalElapsed >= skill.interval) {
                    skill.OnUnitInterval();
                    if (skill.interval > 0.0f) {
                        skill.intervalElapsed -= skill.interval;
                    } else {
                        skill.intervalElapsed = 0;
                        break;
                    }
                }
            }
        }
        EndTrigger();
    }

    protected void TriggerOnAttackTarget(AttackData attack, Unit target) {
        BeginTrigger();
        foreach (Skill skill in m_onAttackTargetTriggerSkills) {
            skill.OnUnitAttackTarget(attack, target);
        }
        EndTrigger();
    }

    protected bool TriggerOnAttacked(AttackData attack, Unit source) {
        BeginTrigger();
        bool res = true;
        foreach (Skill skill in m_onAttackedTriggerSkills) {
            if (skill.OnUnitAttacked(attack, source) == false) {
                res = false;
                break;
            }
        }
        EndTrigger();
        return res;
    }

    protected void TriggerOnDamagedSurface(AttackData attack, Unit source) {
        BeginTrigger();
        foreach (Skill skill in m_onDamagedSurfaceTriggerSkills) {
            skill.OnUnitDamaged(attack, source);
        }
        EndTrigger();
    }

    protected void TriggerOnDamagedInner(AttackData attack, Unit source) {
        BeginTrigger();
        foreach (Skill skill in m_onDamagedInnerTriggerSkills) {
            skill.OnUnitDamaged(attack, source);
        }
        EndTrigger();
    }

    protected void TriggerOnDamagedDone(float fDamage, Unit source) {
        BeginTrigger();
        foreach (Skill skill in m_onDamagedDoneTriggerSkills) {
            skill.OnUnitDamagedDone(fDamage, source);
        }
        EndTrigger();
    }

    protected void TriggerOnDamageTargetDone(float fDamage, Unit target) {
        BeginTrigger();
        foreach (Skill skill in m_onDamageTargetDoneTriggerSkills) {
            skill.OnUnitDamageTargetDone(fDamage, target);
        }
        EndTrigger();
    }

    protected void TriggerOnProjectileEffect(Projectile projectile, Unit target) {
        BeginTrigger();
        foreach (Skill skill in m_onProjectileEffectTriggerSkills) {
            skill.OnUnitProjectileEffect(projectile, target);
        }
        EndTrigger();
    }

    protected bool TriggerOnProjectileArrive(Projectile projectile) {
        BeginTrigger();
        bool res = true;
        foreach (Skill skill in m_onProjectileArriveTriggerSkills) {
            if (skill.OnUnitProjectileArrive(projectile) == false) {
                res = false;
                break;
            }
        }
        EndTrigger();
        return res;
    }

    protected void TriggerOnCalcDamageTarget(float fDamage, Unit target) {
        BeginTrigger();
        foreach (Skill skill in m_onCalcDamageTargetTriggerSkills) {
            skill.OnUnitCalcDamageTarget(fDamage, target);
        }
        EndTrigger();
    }

    // Skill

    public HashSet<ActiveSkill> ActiveSkills {
        get {
            return m_activeSkills;
        }
    }

    public HashSet<PassiveSkill> PassiveSkills {
        get {
            return m_passiveSkills;
        }
    }

    public HashSet<BuffSkill> BuffSkills {
        get {
            return m_buffSkills;
        }
    }

    public HashSet<PassiveSkill> SystemSkills {
        get {
            return m_systemSkills;
        }
    }

    public void SkillCD(Skill skill) {
        World w = m_world;
        if (w.IsSkillCD(skill)) {
            return;
        } else if (!skill.coolingDown) {
            skill.coolingDownElapsed = float.MaxValue;
            return;
        }

        w.AddSkillCD(skill);
        OnSkillCD(skill);
    }

    public void UpdateSkillCD(Skill skill) {
        Debug.Assert(m_world != null);
        m_world.UpdateSkillCD(skill);
    }

    public void AddActiveSkill(ActiveSkill skill, bool notify = true) {
        ActiveSkill copySkill = skill.Clone() as ActiveSkill;
        m_activeSkills.Add(copySkill);
        copySkill.OnAddToUnit(this);  // 消息传递
        AddSkillToTriggers(copySkill);

        if (notify) {
            OnAddActiveSkill(copySkill);
        }
    }

    public void RemoveActiveSkill(ActiveSkill skill, bool notify = true) {
        if (!m_activeSkills.Contains(skill)) {
            return;
        }

        if (notify) {
            OnDelActiveSkill(skill);
        }

        if (skill.coolingDown) {
            m_world.RemoveSkillCD(skill);
        }

        skill.OnDelFromUnit();
        RemoveSkillFromTriggers(skill);
        m_activeSkills.Remove(skill);
    }

    public void AddPassiveSkill(PassiveSkill skill, bool notify = true) {
        PassiveSkill copySkill = skill.Clone() as PassiveSkill;
        m_passiveSkills.Add(copySkill);
        copySkill.OnAddToUnit(this);  // 消息传递
        AddSkillToTriggers(copySkill);

        if (notify) {
            OnAddPassiveSkill(copySkill);
        }
    }

    public void RemovePassiveSkill(PassiveSkill skill, bool notify = true) {
        if (!m_passiveSkills.Contains(skill)) {
            return;
        }

        if (notify) {
            OnDelPassiveSkill(skill);
        }

        if (skill.coolingDown) {
            m_world.RemoveSkillCD(skill);
        }

        skill.OnDelFromUnit();
        RemoveSkillFromTriggers(skill);
        m_passiveSkills.Remove(skill);
    }

    public void AddBuffSkill(BuffSkill skill, bool notify = true) {
        if (skill.Stackable == false) {
            foreach (BuffSkill buff in m_buffSkills) {
                if (buff.baseId == skill.baseId) {
                    buff.SourceUnit = skill.SourceUnit;
                    buff.name = skill.name;
                    buff.Duration = skill.Duration;
                    buff.Elapsed = 0;
                    buff.OnUnitDisplaceSkill(skill);

                    if (skill.AppendBuff != null) {
                        AddBuffSkill(skill.AppendBuff, skill.SourceUnit, true);
                    }

                    return;
                }
            }
        }

        m_buffSkills.Add(skill);
        skill.OnAddToUnit(this);  // 消息传递
        AddSkillToTriggers(skill);

        if (notify) {
            OnAddBuffSkill(skill);
        }

        if (skill.AppendBuff != null) {
            AddBuffSkill(skill.AppendBuff, skill.SourceUnit, true);
        }
    }

    public void AddBuffSkill(BuffSkill skill, Unit source, bool notify = true) {
        BuffSkill copySkill = skill.Clone() as BuffSkill;

        copySkill.SourceUnit = source;
        AddBuffSkill(copySkill, notify);
    }

    public void RemoveBuffSkill(BuffSkill skill, bool notify = true) {
        if (!m_buffSkills.Contains(skill)) {
            return;
        }

        if (notify) {
            OnDelBuffSkill(skill);
        }

        if (skill.coolingDown) {
            m_world.RemoveSkillCD(skill);
        }

        skill.OnDelFromUnit();
        RemoveSkillFromTriggers(skill);
        m_buffSkills.Remove(skill);
    }

    public void AddSystemSkill(PassiveSkill skill) {
        m_systemSkills.Add(skill);
        skill.OnAddToUnit(this);  // 消息传递
        AddSkillToTriggers(skill);
    }

    public void RemoveSystemSkill(PassiveSkill skill) {
        if (!m_systemSkills.Contains(skill)) {
            return;
        }

        if (skill.coolingDown) {
            m_world.RemoveSkillCD(skill);
        }

        skill.OnDelFromUnit();
        RemoveSkillFromTriggers(skill);
        m_systemSkills.Remove(skill);
    }

    protected void UpdateBuffSkillElapsed(float dt) {
        List<BuffSkill> delayToDel = new List<BuffSkill>();
        foreach (BuffSkill buff in m_buffSkills) {
            buff.Elapsed += dt;
            if (buff.Done) {
                if (buff.coolingDown) {
                    m_world.RemoveSkillCD(buff);
                }

                delayToDel.Add(buff);
            }
        }

        foreach (BuffSkill buff in delayToDel) {
            buff.OnDelFromUnit();
            RemoveSkillFromTriggers(buff);
            m_buffSkills.Remove(buff);
        }
        delayToDel.Clear();
    }

    protected HashSet<ActiveSkill> m_activeSkills = new HashSet<ActiveSkill>();
    protected HashSet<PassiveSkill> m_passiveSkills = new HashSet<PassiveSkill>();
    protected HashSet<BuffSkill> m_buffSkills = new HashSet<BuffSkill>();
    protected HashSet<PassiveSkill> m_systemSkills = new HashSet<PassiveSkill>();

    public ActiveSkill AttackSkill {
        get {
            return m_attackSkill;
        }

        set {
            m_attackSkill = value;
        }
    }

    protected ActiveSkill m_attackSkill;

    // Hp
    public float Hp {
        get {
            return m_hp;
        }

        set {
            if (Dead) {
                return;
            }

            float maxHp = m_maxHp.v;
            float oldHp = m_hp;
            m_hp = Mathf.Min(value, maxHp);
            if (m_hp != oldHp) {
                OnHpChanged(m_hp - oldHp);
            }

            if (m_hp <= 0) {
                //OnDying();
            }
        }
    }

    public float MaxHp {
        get {
            return m_maxHp.v;
        }
    }

    public float MaxHpBase {
        get {
            return m_maxHp.x;
        }

        set {
            float fOldMaxHp = m_maxHp.v;
            m_maxHp.x = Mathf.Max(value, 1.0f);
            float newHp = m_hp * m_maxHp.v / fOldMaxHp;
            if (newHp < 1) {
                newHp = 1;
            }
            Hp = newHp;
        }
    }

    public Coeff MaxHpCoeff {
        get {
            return m_maxHp.coeff;
        }

        set {
            float oldMaxHp = m_maxHp.v;
            m_maxHp.coeff = value;
            float newHp = m_hp * m_maxHp.v / oldMaxHp;
            if (newHp < 1) {
                newHp = 1;
            }
            Hp = newHp;
        }
    }

    public bool Dead {
        get {
            return m_hp <= 0;
        }
    }

    public bool Revive(float hp) {
        if (Dead) {
            m_hp = Mathf.Min(Mathf.Max(hp, 1), MaxHp);
            OnHpChanged(m_hp);
            OnRevive();
            Killer = null;
            return true;
        }

        return false;
    }

    public void Die() {
        Stop();
        StartDoing(kDoingDying);
        m_renderer.StopAllActions();
        m_renderer.DoAnimate(ObjectRenderer.kActionDie, null, 1, OnDyingDone, 1.0f);

        if (isServer) {
            if (client != null) {
                //client.RpcDie(0, m_doingFlags, m_renderer.Node.position, m_renderer.Node.flippedX, ObjectRenderer.kActionDie, 1.0f);
            } else {
                //localClient.RpcDie(m_id, m_doingFlags, m_renderer.Node.position, m_renderer.Node.flippedX, ObjectRenderer.kActionDie, 1.0f);
            }
        }
    }

    void OnDyingDone() {
        if (Dead == false) {
            return;
        }

        EndDoing(0xFFFFFFFF);

        OnDead();
        //     if (w->getUnitToRevive(id) != nullptr)
        //     {
        //         // !!! 只有能重生的单位才会触发此事件，不合理
        //         u->onDead();
        //     }

        if (Dead) {
            m_world.RemoveUnit(this, Revivable);
        }
    }

    public bool Revivable {
        get {
            return m_revivable;
        }

        set {
            m_revivable = value;
        }
    }

    protected float m_hp = 1;
    protected Value m_maxHp = new Value(1);
    protected bool m_revivable = false;

    public void CommandStop() {
        if (Dead || Suspended || Fixed) {
            return;
        }

        Stop();
    }

    protected void Stop(bool defaultFrame = true) {
        m_renderer.StopAction(m_moveToActionId);
        m_moveToActionId = 0;

        m_renderer.StopAction(m_moveActionId);
        m_moveActionId = 0;

        EndDoing(kDoingMoving);

        StopCast(false);

        if (defaultFrame) {
            m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
        }
    }

    public Unit Killer {
        get {
            return m_killer;
        }

        set {
            m_killer = value;
        }
    }

    protected Unit m_killer;

    //////////////////// attack & damaged ////////////////////////

    // 高层攻击函数，用于最初生成攻击数据，一个攻击动作生成的攻击数据，一般调用该函数
    // 攻击动作，可对目标造成动作，如普通攻击、技能等
    // 攻击数据，描述这次攻击的数据体，详见 CAttackData 定义
    // 内部会自行调用中层、底层攻击函数，对攻击数据进行传递并处理，通常返回处理后的攻击数据，也可以返回 null
    // 内部会根据人物属性对攻击数据进行一次变换，如力量加成等
    // 触发 OnAttackTarget，
    public bool Attack(AttackData attack, Unit target, uint triggerMask = kTriggerMaskNoMasked) {
        OnAttackTarget(attack, target, triggerMask);

        if (target == null || target.OnAttacked(attack, this, triggerMask) == false) {
            return false;
        }

        AttackLow(attack, target, triggerMask);

        return true;
    }

    // 底层攻击函数，目前无逻辑，只是将传递过来的攻击数据返回给上层
    public void AttackLow(AttackData attack, Unit target, uint triggerMask = kTriggerMaskNoMasked) {
    }

    // 高层伤害函数，攻击者生成的攻击到达目标后，目标将调用该函数，计算自身伤害
    // 内部会对攻击数据进行向下传递
    // 触发 OnAttacked，如果OnAttacked返回 null，伤害将不会继续向下层函数传递，函数返回false。比如说，闪避成功，伤害无需继续计算
    // 触发 OnDamaged
    // 遍历攻击数据携带的BUFF链，根据附着概率对单位自身进行BUFF附加
    // 根据单位属性，进行攻击数据变换，如抗性对攻击数据的影响
    // 根据单位护甲，进行攻击数据中的攻击数值变换
    public void Damaged(AttackData attack, Unit source, uint triggerMask = kTriggerMaskNoMasked) {
        if (Dead) {
            return;
        }

        while (source != null && source.Ghost) {
            Unit go = source.GhostOwner;
            if (go == source) {
                break;
            }
            source = go;
        }

        OnDamaged(attack, source, triggerMask);

        if (source != null) {
            foreach (var ab in attack.AttackBuffs) {
                // copy BUFF from TemplateAbilities
                AddBuffSkill(ab.buffTemplate, source, true);
            }
        }

        //transformDamageByAttribute(attack);
        float fDamage = CalcDamage(attack.attackValue.type,
                            attack.attackValue.v,
                            ArmorType,
                            ArmorValue);

        DamagedLow(fDamage, source, triggerMask);

        if (source != null) {
            int key = source.GetHashCode();
            if (m_hpChanged.ContainsKey(key)) {
                m_hpChanged[key] += fDamage;
            } else {
                m_hpChanged.Add(key, fDamage);
            }
        }
    }

    // 底层伤害函数，直接扣除指定量的HP值
    // 触发伤害源的 OnDamaeTarget
    // 调用 Hp = x，从而会触发 OnHpChanged，可能会触发OnDying
    public void DamagedLow(float fDamage, Unit source, uint triggerMask = kTriggerMaskNoMasked) {
        if (Dead) {
            return;
        }

        while (source != null && source.Ghost) {
            Unit go = source.GhostOwner;
            if (go == source) {
                break;
            }
            source = go;
        }

        if (fDamage >= m_hp) {
            m_killer = source;
            m_renderer.SetHp(0);
        } else {
            m_renderer.SetHp(m_hp - fDamage);
        }

        OnDamagedDone(fDamage, source, triggerMask);

        if (source != null) {
            source.OnDamageTargetDone(fDamage, this, triggerMask);
        }
    }

    protected float CalcDamage(AttackValue.Type eAttackType, float fAttackValue, ArmorValue.Type eArmorType, float fArmorValue) {
        float aa = ArmorAttackTable.Table[(int)eArmorType, (int)eAttackType];
        float ret;
        if (fArmorValue > 0) {
            ret = fArmorValue * aa * 0.06f;
            ret = 1 - ret / (ret + 1);
        } else {
            ret = 2 - Mathf.Pow(0.94f, -fArmorValue * (aa < 1 ? (2 - aa) : (1 / aa)));
        }
        ret *= fAttackValue;

        return ret;
    }

    public void healLow(float heal, Unit source, uint triggerMask = kTriggerMaskNoMasked) {
        float newHp = Mathf.Min(MaxHp, Hp + heal);
        float value = newHp - Hp;
        if (m_hpChanged.ContainsKey(0)) {
            m_hpChanged[0] += value;
        } else {
            m_hpChanged.Add(0, value);
        }


        Hp = (newHp);
    }

    // AI
    public UnitAI AI {
        get {
            return m_AI;
        }

        set {
            m_AI = value;
        }
    }

    protected UnitAI m_AI;

    // Fixed
    public bool Fixed {
        get {
            return m_fixed;
        }

        set {
            m_fixed = value;
        }
    }

    protected bool m_fixed;

    public float HostilityRange {
        get {
            return m_hostilityRange;
        }
    }

    protected float m_hostilityRange = 3.0f;

    // Suspend
    public bool Suspended {
        get {
            return m_suspendCount > 0;
        }
    }

    void Suspend() {
        EndDoing(kDoingObstinate);
        ++m_suspendCount;
        // TODO: stop actions
        if (m_suspendCount == 1) {
            //LOG("%s不能动了", getName());
        }
    }

    public void Resume() {
        if (m_suspendCount == 0) {
            return;
        }

        --m_suspendCount;
        if (m_suspendCount == 0) {
            // TODO: resume
            //LOG("%s能动了", getName());
        }
    }

    protected int m_suspendCount;

    // Ghost
    public void SetUnitAsGhost(Unit iGhostOwner) {
        bool bGhost = m_ghostOwner != null;
        m_ghostOwner = iGhostOwner;

        if ((m_ghostOwner != null) == bGhost) {
            return;
        }

        if (m_world == null) {
            return;
        }

        if (m_ghostOwner != null) {
            m_world.OnDelNormalAttributes(this);
        } else {
            m_world.OnAddNormalAttributes(this);
        }
    }

    public bool Ghost {
        get {
            return m_ghostOwner != null;
        }
    }

    public Unit GhostOwner {
        get {
            return m_ghostOwner;
        }
    }

    public Unit RootGhostOwner {
        get {
            Unit res = this;
            while (res != null && res.Ghost) {
                Unit go = res.GhostOwner;
                if (go == res) {
                    break;
                }
                res = go;
            }
            return res;
        }
    }

    protected Unit m_ghostOwner;

    // Doing
    public const uint kDoingDying = 1 << 16;
    public const uint kDoingMoving = 1 << 17;
    public const uint kDoingObstinate = 1 << 18;
    public const uint kDoingAlongPath = 1 << 19;
    public const uint kDoingCasting = 1 << 20;
    public const uint kDoingSpinning = 1 << 21;

    public void StartDoing(uint doingFlags) {
        m_doingFlags |= doingFlags;
    }

    public void EndDoing(uint doingFlags) {
        m_doingFlags &= (~doingFlags);
    }

    public bool IsDoingOr(uint doingFlags) {
        return (m_doingFlags & doingFlags) != 0;
    }

    public bool IsDoingAnd(uint doingFlags) {
        return (m_doingFlags & doingFlags) == doingFlags;
    }

    public bool IsDoingNothing() {
        return m_doingFlags == 0;
    }

    protected internal uint m_doingFlags;

    // Cast
    public CommandTarget CastTarget {
        get {
            return m_castTarget;
        }

        set {
            m_castTarget = value;
        }
    }

    public ActiveSkill CastActiveSkill {
        get {
            return m_castActiveSkill;
        }

        set {
            m_castActiveSkill = value;
        }
    }

    public int CastActionId {
        get {
            return m_castActionId;
        }

        set {
            m_castActionId = value;
        }
    }

    CommandTarget m_castTarget;
    protected internal int m_castActionId;
    ActiveSkill m_castActiveSkill;
    // 正在进行的施法，包括追逐状态中的技能
    ActiveSkill m_waitForCastTargetActiveSkill;
    // 等待目标的技能

    public int CommandCastSpell(CommandTarget target, ActiveSkill activeSkill, bool obstinate = true)  // 可能是施法失败，施法中，施法追逐中，所以返回类型为int
    {
        if (Suspended || Dead) {
            return -1;
        }

        ActiveSkill skill = activeSkill;
        if (skill == null) {
            return -1;
        }

        Unit t = null;
        UnitRenderer td = null;
        bool flippedX = m_renderer.Node.flippedX;

        switch (skill.CastTargetType) {
        case CommandTarget.Type.kNoTarget:
            break;

        case CommandTarget.Type.kUnitTarget:
            if (target.TargetType != CommandTarget.Type.kUnitTarget) {
                return -1;
            }

            t = target.TargetUnit;
            if (t == null) {
                return -1;
            }

            td = t.Renderer;
            Debug.Assert(td != null);

            flippedX = (this == target.TargetUnit) ? m_renderer.Node.flippedX : (td.Node.position.x < m_renderer.Node.position.x);

            break;

        case CommandTarget.Type.kPointTarget:
            if (target.TargetType != CommandTarget.Type.kPointTarget) {
                return -1;
            }

            flippedX = (target.TargetPoint.x < m_renderer.Node.position.x);

            break;
        }

        if (skill.CheckConditions(target) == false) {
            return -1;
        }

        bool disOk = CheckCastTargetDistance(skill, m_renderer.Node.position, target, t);
        if (disOk == false && Fixed) {
            return -1;
        }

        bool isSameTarget = false;
        if (target.TargetType != CommandTarget.Type.kNoTarget) {
            CastTarget = target;
            isSameTarget = (target == CastTarget);
        } else {
            isSameTarget = true;
        }

        if (obstinate) {
            StartDoing(kDoingObstinate);
        } else {
            EndDoing(kDoingObstinate);
        }

        bool isSameSkill = (m_castActiveSkill == activeSkill);
        m_castActiveSkill = activeSkill;
        StartDoing(kDoingCasting);

        if (disOk == false) {
            MoveToCastPosition(skill, t);
            return 1;
        }

        if (m_renderer.IsDoingAction(m_castActionId) &&
            isSameSkill &&
            isSameTarget &&
            flippedX == m_renderer.Node.flippedX) {
            return 0;
        }

        if (flippedX != m_renderer.Node.flippedX && !Fixed) {
            m_renderer.SetFlippedX(flippedX);
        }

        return CastSpell(skill);
    }

    int CastSpell(ActiveSkill skill) {
        if (m_renderer.IsDoingAction(m_castActionId)) {
            m_renderer.StopAction(m_castActionId);
        }

        if (m_renderer.IsDoingAction(m_moveActionId)) {
            StopMove();
        }

        if (skill.coolingDown) {
            return -1;
        }

        float spd = 1;
        if (m_castActiveSkill == m_attackSkill) {
            AttackAct atk = m_castActiveSkill as AttackAct;
            spd = atk.coolDownSpeedCoeff;
        }

        int aniId = skill.castRandomAnimation;
        if (aniId != -1) {
            m_renderer.DoAnimate(aniId, OnCastEffect, 1, OnCastFinished, spd);
            m_castActionId = aniId;
        } else {
            OnCastEffect();
            OnCastFinished();
        }

        return 0;
    }

    public bool CheckCastTargetDistance(ActiveSkill skill, Vector2 pos, CommandTarget target, Unit td /* for fast calc */) {
        Vector2 pos2 = new Vector2();
        switch (skill.CastTargetType) {
        case CommandTarget.Type.kNoTarget:
            return true;

        case CommandTarget.Type.kUnitTarget:
            if (this == td) {
                return true;
            }
            pos2 = td.Renderer.Node.position;
            break;

        case CommandTarget.Type.kPointTarget:
            pos2 = target.TargetPoint;
            break;
        }

        if (skill.CastHorizontal && Mathf.Abs(pos.y - pos2.y) > ActiveSkill.CONST_MAX_HOR_CAST_Y_RANGE) {
            return false;
        }

        float fDis = Vector2.Distance(pos, pos2) - m_renderer.HalfOfWidth - (td != null ? td.Renderer.HalfOfWidth : 0.0f);
        if (fDis < skill.CastMinRange || fDis > skill.CastRange) {
            return false;
        }

        return true;
    }

    void MoveToCastPosition(ActiveSkill skill, Unit td) {
        float dis = (td != null ? td.Renderer.HalfOfWidth : 0) + m_renderer.HalfOfWidth + (skill.CastMinRange + skill.CastRange) * 0.5f;
        Vector2 pos1 = m_renderer.Node.position;
        Vector2 pos2 = (td != null ? td.Renderer.Node.position : CastTarget.TargetPoint);

        if (skill.CastHorizontal) {
            Move(new Vector2(pos2.x + ((pos1.x > pos2.x) ? dis : -dis), pos2.y));
        } else {
            Move(Utils.GetForwardPoint(pos2, pos1, dis));
        }
    }

    public void StopCast(bool defaultFrame = true) {
        if (m_castActionId != 0) {
            m_renderer.StopAction(m_castActionId);
        }
        m_castActionId = 0;
        m_castActiveSkill = null;

        EndDoing(kDoingCasting);

        //if (defaultFrame) {
        if (m_moveActionId == 0) {
            m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
        }
    }

    /// <summary>
    /// 释放技能后，技能动画播放到起效点时。如法杖举到最高点；若没有施法动作，则立即开始奏效
    /// </summary>
    protected internal void OnCastEffect() {
        ActiveSkill skill = m_castActiveSkill;
        if (skill == null) {
            return;
        }

        if (skill != AttackSkill) {
            m_renderer.AddBattleTip(skill.name, "", 32, Color.black);
        }

        skill.Effect();
    }

    /// <summary>
    /// 技能动画播放完毕后。
    /// </summary>
    protected internal void OnCastFinished() {
        ActiveSkill skill = m_castActiveSkill;
        if (skill == null) {
            return;
        }

        if (m_attackSkill != null && skill == m_attackSkill) {
            // 拥有攻击技能，正在释放的技能就是攻击技能
            m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
            m_castActionId = 0;
            return;
        }

        StopCast();

        if (m_castActiveSkill != m_attackSkill ||
            CommandCastSpell(m_castTarget, m_castActiveSkill, IsDoingOr(kDoingObstinate)) < 0) {
            // 如果刚释放完毕的技能不是攻击技能 或 攻击技能无法继续释放在同一个目标身上，则
            // 施法(非攻击)结束，去除固执状态
            EndDoing(kDoingObstinate);
        }
    }

    // Move
    public virtual void CommandMove(Vector2 pos, bool obstinate = true, cca.Function onFinished = null) {
        if (Dead || Suspended || Fixed) {
            return;
        }

        if (IsDoingOr(kDoingCasting)) {
            StopCast(false);
        }

        if (IsDoingOr(kDoingAlongPath)) {
            EndDoing(kDoingAlongPath);
        }

        if (obstinate) {
            StartDoing(kDoingObstinate);
        } else {
            EndDoing(kDoingObstinate);
        }

        Move(pos, onFinished);
    }

    public void Move(Vector2 pos, cca.Function onFinished = null) {
        if (Suspended) {
            return;
        }

        m_lastMoveTo = pos;

        var here = m_renderer.Node.position;

        if (pos.x != here.x) {
            m_renderer.SetFlippedX(pos.x < here.x);
        }

        float moveSpeed = m_moveSpeed.x;
        float duration = Vector2.Distance(here, pos) / Mathf.Max(moveSpeed, float.Epsilon);
        float speed = MoveSpeed / moveSpeed;

        // 突发移动指令，打断旧移动，打断攻击，打断施法
        if (m_renderer.IsDoingAction(m_moveToActionId)) {
            m_renderer.StopAction(m_moveToActionId);
            m_moveToActionId = 0;
        }

        if (m_renderer.IsDoingAction(m_castActionId)) {
            m_renderer.StopAction(m_castActionId);
            m_castActionId = 0;
        }

        StartDoing(kDoingMoving);

        if (m_moveActionId == 0 && IsDoingOr(kDoingSpinning) == false) {
            m_renderer.DoAnimate(ObjectRenderer.kActionMove, null, ObjectRenderer.CONST_LOOP_FOREVER, null, speed);
            m_moveActionId = ObjectRenderer.kActionMove;
        }

        if (m_moveToActionId == 0) {
            m_renderer.DoMoveTo(pos, duration, OnMoveToFinished, speed);
            m_moveToActionId = ObjectRenderer.kActionMoveTo;
        }
    }        

    protected internal void OnMoveToFinished() {
        EndDoing(kDoingObstinate);  // 移动自行停止后，需要去除固执状态
        StopMove();
    }

    protected UnitPath m_movePath;
    protected int m_pathCurPos;
    protected float m_pathBufArrive = 0.1f;
    protected bool m_pathObstinate;

    // 移动速度
    protected Value m_moveSpeed = new Value(1.0f);

    public float MoveSpeed {
        get {
            float moveSpeed = m_moveSpeed.x;
            float realMoveSpeed = m_moveSpeed.v;
            // 取最小移动速度和最小减速后速度的最大值作为最小移动速度
            float minMoveSpeed = moveSpeed * CONST_MIN_MOVE_SPEED_MULRIPLE;
            minMoveSpeed = Mathf.Max(CONST_MIN_MOVE_SPEED, minMoveSpeed);
            // 计算实际移动速度，不得超过上述计算所得的最小值
            realMoveSpeed = Mathf.Max(realMoveSpeed, minMoveSpeed);
            // 计算实际移动速度，不得超过最大移动速度
            return Mathf.Min(realMoveSpeed, CONST_MAX_MOVE_SPEED);
        }
    }

    public float MoveSpeedBase {
        get {
            return m_moveSpeed.x;
        }

        set {
            value = Mathf.Max(value, CONST_MIN_MOVE_SPEED);
            value = Mathf.Min(value, CONST_MAX_MOVE_SPEED);
            m_moveSpeed.x = value;
            UpdateMoveActionSpeed();
        }
    }

    public Coeff MoveSpeadCoeff {
        get {
            return m_moveSpeed.coeff;
        }

        set {
            m_moveSpeed.coeff = value;
            UpdateMoveActionSpeed();
        }
    }

    protected void UpdateMoveActionSpeed() {
        float moveSpeed = m_moveSpeed.x;
        if (moveSpeed < float.Epsilon) {
            StopMove();
            return;
        }
        float spd = MoveSpeed / moveSpeed;
        m_renderer.SetActionSpeed(ObjectRenderer.kActionMove, spd);
        m_renderer.SetActionSpeed(ObjectRenderer.kActionMoveTo, spd);
    }

    public void StopMove() {
        m_renderer.StopAction(ObjectRenderer.kActionMove);
        m_moveActionId = 0;

        m_renderer.StopAction(ObjectRenderer.kActionMoveTo);
        m_moveToActionId = 0;

        EndDoing(kDoingMoving | kDoingAlongPath);

        m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
    }

    protected const float CONST_MIN_MOVE_SPEED = 0.1f;
    protected static readonly float CONST_MAX_MOVE_SPEED = 3.0f;
    protected const float CONST_MIN_MOVE_SPEED_MULRIPLE = 0.2f;
    // 最小变为基础速度的20%

    protected Vector2 m_lastMoveTo;
    protected internal int m_moveActionId;
    protected internal int m_moveToActionId;

    // 护甲
    protected ArmorValue m_armorValue = new ArmorValue(global::ArmorValue.Type.kHeavy, 0);

    public float ArmorValue {
        get {
            return m_armorValue.v;
        }
    }

    public ArmorValue.Type ArmorType {
        get {
            return m_armorValue.type;
        }

        set {
            m_armorValue.type = value;
        }
    }

    public float ArmorValueBase {
        get {
            return m_armorValue.x;
        }

        set {
            m_armorValue.x = value;
        }
    }

    public Coeff ArmorValueCoeff {
        get {
            return m_armorValue.coeff;
        }

        set {
            m_armorValue.coeff = value;
        }
    }

    // 暴击率
    protected Value m_critRate = new Value(0.2f);

    public float CriticalRate {
        get {
            return m_critRate.v;
        }
    }

    public float CriticalRateBase {
        get {
            return m_critRate.x;
        }

        set {
            m_critRate.x = value;
        }
    }

    public Coeff CriticalRateCoeff {
        get {
            return m_critRate.coeff;
        }

        set {
            m_critRate.coeff = value;
        }
    }

    // 暴击伤害
    protected Value m_critDmg = new Value(0.5f);

    public float CriticalDamage {
        get {
            return m_critDmg.v;
        }
    }

    public float CriticalDamageBase {
        get {
            return m_critDmg.x;
        }

        set {
            m_critDmg.x = value;
        }
    }

    public Coeff CriticalDamageCoeff {
        get {
            return m_critDmg.coeff;
        }

        set {
            m_critDmg.coeff = value;
        }
    }
}

public class UnitPath {
    public UnitPath() {
        m_points = new List<Vector2>();
    }

    public UnitPath(List<Vector2> points) {
        m_points = new List<Vector2>(points);
    }

    public void addPoint(Vector2 roPos) {
    }

    public Vector2 getCurTargetPoint(int curPos) {
        Debug.Assert(curPos < m_points.Count);
        return m_points[curPos];
    }

    public bool arriveCurTargetPoint(ref int rCurPos)  // return true when end
    {
        if (rCurPos < m_points.Count) {
            ++rCurPos;
        }

        if (rCurPos < m_points.Count) {
            return false;
        }

        return true;
    }

    protected List<Vector2> m_points;
}

public class CommandTarget {
    public enum Type {
        kNoTarget,
        kUnitTarget,
        kPointTarget
    }

    public CommandTarget() {
        m_targetType = Type.kNoTarget;
        m_targetUnit = null;
    }

    public CommandTarget(Unit target) {
        m_targetType = Type.kUnitTarget;
        m_targetUnit = target;
    }

    public CommandTarget(Vector2 target) {
        m_targetType = Type.kPointTarget;
        m_targetPoint = target;
        m_targetUnit = null;
    }

    protected bool Equels(CommandTarget target) {
        switch (target.m_targetType) {
        case Type.kNoTarget:
            return true;

        case Type.kPointTarget:
            return m_targetPoint == target.m_targetPoint;

        case Type.kUnitTarget:
            return m_targetUnit == target.m_targetUnit;
        }

        return false;
    }

    public Type TargetType {
        get {
            return m_targetType;
        }
    }

    public Vector2 TargetPoint {
        get {
            return m_targetPoint;
        }
    }

    public Unit TargetUnit {
        get {
            return m_targetUnit;
        }
    }

    public void UpdateTargetPoint() {
        Debug.Assert(m_targetType == Type.kUnitTarget);
        m_targetPoint = m_targetUnit.Renderer.Node.position;
    }

    protected Type m_targetType;
    protected Vector2 m_targetPoint;
    protected Unit m_targetUnit;

    public void setTarget() {
        m_targetType = Type.kNoTarget;
        m_targetUnit = null;
        m_targetPoint.x = m_targetPoint.y = 0;
    }

    public void setTarget(Unit target) {
        m_targetType = Type.kUnitTarget;
        m_targetUnit = target;
        m_targetPoint.x = m_targetPoint.y = 0;
    }

    public void setTarget(Vector2 target) {
        m_targetType = Type.kPointTarget;
        m_targetUnit = null;
        m_targetPoint = target;
    }
}
