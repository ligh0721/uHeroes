using UnityEngine;
using System.Collections;

// 攻击，默认以单位作为目标
public class AttackAct : ActiveSkill {
    new protected class SkillData : ActiveSkill.SkillData {
    }

    public const float CONST_MIN_ATTACK_SPEED_INTERVAL = 0.02f;
    public const float CONST_MIN_ATTACK_SPEED_MULRIPLE = 0.2f;
    public const float CONST_MAX_ATTACK_SPEED_MULRIPLE = 100.0f;

    public AttackAct(string name, float coolDown, AttackValue attackValue, float attackValueRandomRange = 0.15f)
        : base(name, coolDown, CommandTarget.Type.kUnitTarget, UnitForce.kEnemy) {
        m_attackValue = attackValue;
        m_attackValueRandomRange = new Value(attackValueRandomRange);
    }

    public override Skill Clone() {
        AttackAct ret = new AttackAct(m_name, m_coolDown.x, m_attackValue, m_attackValueRandomRange.x);
        ret.CopyDataFrom(this);
        return ret;
    }

    new protected void CopyDataFrom(Skill from) {
        base.CopyDataFrom(from);
    }

    public override Skill Clone(string data) {
        AttackAct ret = new AttackAct(m_name, m_coolDown.x, m_attackValue, m_attackValueRandomRange.x);
        SkillData sd = JsonUtility.FromJson<SkillData>(data);
        ret.CopyDataFrom(sd);
        return ret;
    }

    protected void CopyDataFrom(SkillData data) {
        base.CopyDataFrom(data);
    }

    public override void OnUnitAddSkill() {
        Unit o = m_owner;
        Debug.Assert(o != null);
        m_origin = o.AttackSkill;
        o.AttackSkill = this;
    }

    public override void OnUnitDelSkill() {
        Unit o = m_owner;
        Debug.Assert(o != null);
        if (!m_origin.valid) {
            m_origin = null;
        }

        if (o.AttackSkill == this) {
            o.AttackSkill = m_origin;
        }
    }

    public override bool CheckConditions(CommandTarget rTarget) {
        Unit t = rTarget.TargetUnit;
        if (t == null || t.Dead) {
            return false;
        }

        return true;
    }

    public override void OnUnitSkillEffect(Projectile pProjectile, Unit target) {
        Unit o = m_owner;
        if (o == null) {
            return;
        }

        if (pProjectile != null) {
            pProjectile.DecContactLeft();
        }

        AttackData ad = new AttackData();
        ad.SetAttackValueBase(m_attackValue.type, RandomAttackValue);
        if (o.Attack(ad, target) == false) {
            return;
        }

        if (Utils.Random.NextDouble() < o.CriticalRate) {
            ad.SetAttackValueCoef(1.0f + o.CriticalDamage, ad.attackValue.b);
        }

        if (target != null) {
            target.Damaged(ad, o);
        }
    }

    public float AttackValue {
        get { return m_attackValue.v; }
    }

    public float RandomAttackValue {
        get {
            float fAttackValueRandomRange = m_attackValueRandomRange.v;
            if (fAttackValueRandomRange > 0.001) {
                return (float)Utils.RandomValue(m_attackValue.v, fAttackValueRandomRange);
            }
            return m_attackValue.v;
        }
    }

    public AttackValue.Type AttackType {
        get { return m_attackValue.type; }

        set { m_attackValue.type = value; }
    }

    public float AttackValueBase {
        get { return m_attackValue.x; }

        set { m_attackValue.x = value; }
    }

    public Coeff AttackValueCoeff {
        get { return m_attackValue.coeff; }

        set { m_attackValue.coeff = value; }
    }

    public float AttackValueRandomRange {
        get { return m_attackValueRandomRange.v; }
    }

    public float AttackValueRandomRangeBase {
        get { return m_attackValueRandomRange.x; }
    }

    public Coeff AttackValueRandomRangeCoeff {
        get { return m_attackValueRandomRange.coeff; }
    }

    // 攻击间隔
    public override float coolDown {
        get {
            // 取攻击速度系数，不得小于最小值
            float speed = Mathf.Min(CONST_MAX_ATTACK_SPEED_MULRIPLE, Mathf.Max(CONST_MIN_ATTACK_SPEED_MULRIPLE, 1 / m_coolDown.a));
            float fRealAttackInterval = Mathf.Max(CONST_MIN_ATTACK_SPEED_INTERVAL, m_coolDown.x / speed);
            return fRealAttackInterval;
        }
    }

    // 攻速系数
    public override float coolDownSpeedCoeff {
        get {
            float speed = Mathf.Min(CONST_MAX_ATTACK_SPEED_MULRIPLE, Mathf.Max(CONST_MIN_ATTACK_SPEED_MULRIPLE, 1 / m_coolDown.a));
            return speed;
        }

        set {
            base.coolDownSpeedCoeff = value;
            UpdateAttackActionSpeed();
        }
    }

    protected void UpdateAttackActionSpeed() {
        Unit o = m_owner;
        Debug.Assert(o != null);
        o.UpdateSkillCD(this);
        UnitNode d = o.Node;
        if (o.CastActiveSkill == o.AttackSkill) {
            d.SetActionSpeed(o.CastActionId, coolDownSpeedCoeff);
        }
    }

    protected AttackValue m_attackValue;
    protected Value m_attackValueRandomRange;
    protected AttackAct m_origin;
}
