using UnityEngine;
using System;


public class ActiveSkill : Skill {
    new protected class SkillData : Skill.SkillData {
    }

    public ActiveSkill(string name, float coolDown, CommandTarget.Type castType = CommandTarget.Type.kNoTarget, uint effectiveTypeFlags = UnitForce.kSelf | UnitForce.kOwn | UnitForce.kAlly | UnitForce.kEnemy)
        : base(name, coolDown) {
        m_castTargetType = castType;
        m_effectiveTypeFlags = effectiveTypeFlags;
    }

    public override Skill Clone() {
        throw new NotImplementedException();
    }

    new protected void CopyDataFrom(Skill from) {
        base.CopyDataFrom(from);
        ActiveSkill a = from as ActiveSkill;
        m_castTargetType = a.m_castTargetType;
        m_effectiveTypeFlags = a.m_effectiveTypeFlags;
        m_castMinRange = a.m_castMinRange;
        m_castRange = a.m_castRange;
        m_castTargetRadius = a.m_castTargetRadius;
        m_projectileTemplate = a.m_projectileTemplate;
        m_castHorizontal = a.m_castHorizontal;
    }

    public override Skill Clone(string data) {
        throw new NotImplementedException();
    }

    protected void CopyDataFrom(SkillData data) {
        base.CopyDataFrom(data);
    }

    public static readonly float CONST_MAX_CAST_BUFFER_RANGE = 0.5f;
    public static readonly float CONST_MAX_HOR_CAST_Y_RANGE = 0.2f;

    public virtual bool CheckConditions(CommandTarget rTarget) {
        return true;
    }

    public virtual void OnUnitCastSkill() {
    }

    /// <summary>
    /// 当技能起效时，单位会调用该函数
    /// </summary>
    public void Effect() {
        Unit o = m_owner;
        UnitNode d = o.Node;

        StartCoolingDown();
        OnUnitCastSkill();

        switch (m_castTargetType) {
        case CommandTarget.Type.kNoTarget:
            PlayEffectSound();
            OnUnitSkillEffect(null, null);
            break;

        case CommandTarget.Type.kUnitTarget:
            {
                Unit t = o.CastTarget.TargetUnit;

                if (t == null || t.Dead) {
                    return;
                }

                if (m_projectileTemplate != null && t != o) {
                    SyncProjectileInfo syncInfo = new SyncProjectileInfo();
                    syncInfo.baseInfo = m_projectileTemplate;
                    syncInfo.fromTo = Projectile.FromToType.kUnitToUnit;
                    syncInfo.useFireOffset = true;
                    syncInfo.srcUnit = o.Id;
                    syncInfo.fromUnit = o.Id;
                    syncInfo.toUnit = t.Id;

                    World.Main.CreateProjectile(syncInfo, this);

                    //UnitNode td = t.Node;
                    //Debug.Assert(td != null);
                } else {
                    PlayEffectSound();
                    OnUnitSkillEffect(null, t);
                }
            }
            break;

        case CommandTarget.Type.kPointTarget:
            if (m_projectileTemplate != null) {
                SyncProjectileInfo syncInfo = new SyncProjectileInfo();
                syncInfo.baseInfo = m_projectileTemplate;
                syncInfo.fromTo = Projectile.FromToType.kUnitToPoint;
                syncInfo.useFireOffset = true;
                syncInfo.srcUnit = o.Id;
                syncInfo.fromUnit = o.Id;
                syncInfo.toPos = Utils.GetForwardPoint(d.position, o.CastTarget.TargetPoint, m_castRange);
                World.Main.CreateProjectile(syncInfo, this);
            } else {
                PlayEffectSound();
                OnUnitSkillEffect(null, null);
            }
            break;
        }
    }

    public CommandTarget.Type CastTargetType {
        get { return m_castTargetType; }

        set { m_castTargetType = value; }
    }

    public float CastMinRange {
        get { return m_castMinRange; }

        set { m_castMinRange = value; }
    }

    public float CastRange {
        get { return m_castRange; }

        set { m_castRange = value; }
    }

    public float CastTargetRadius {
        get { return m_castTargetRadius; }

        set { m_castTargetRadius = value; }
    }

    public ProjectileInfo ProjectileTemplate {
        get { return m_projectileTemplate; }

        set { m_projectileTemplate = value; }
    }

    public bool CastHorizontal {
        get { return m_castHorizontal; }

        set { m_castHorizontal = value; }
    }

    protected CommandTarget.Type m_castTargetType;
    protected float m_castMinRange;
    protected float m_castRange;
    protected float m_castTargetRadius;
    protected ProjectileInfo m_projectileTemplate;
    protected bool m_castHorizontal;

    public Vector2 GetAbilityEffectPoint(Projectile projectile, Unit target) {
        Debug.Assert(m_owner != null);
        Unit o = m_owner;
        if (projectile != null) {
            return projectile.Node.position;
        }

        if (target != null) {
            UnitNode td = target.Node;
            return td.position;
        }

        UnitNode od = m_owner.Node;

        switch (m_castTargetType) {
        case CommandTarget.Type.kNoTarget:
            return od.position;

        case CommandTarget.Type.kPointTarget:
            return o.CastTarget.TargetPoint;
        }

        Unit u = o.CastTarget.TargetUnit;
        if (u == null) {
            Debug.LogError("GetAbilityEffectPoint() err.");
            return od.position;
        }

        UnitNode ud = u.Node;
        return ud.position;
    }
}
