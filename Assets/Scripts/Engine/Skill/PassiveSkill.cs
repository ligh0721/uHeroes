using UnityEngine;
using System.Collections;
using System;

public class PassiveSkill : Skill
{
	new protected class SkillData : Skill.SkillData
	{
	}

    public PassiveSkill(string pName, float fCoolDown)
        : base(pName, fCoolDown)
    {
    }

    public override Skill Clone()
    {
        throw new NotImplementedException();
    }

    new protected void CopyDataFrom(Skill from)
    {
        base.CopyDataFrom(from);
    }

	public override Skill Clone(string data)
	{
		throw new NotImplementedException();
	}

	protected void CopyDataFrom(SkillData data)
	{
		base.CopyDataFrom (data);
	}
}

public class SplashPas :  PassiveSkill
{
	new protected class SkillData : Skill.SkillData
	{
	}

    public SplashPas(string pName, float fNearRange, Coeff roExNearDamage, float fFarRange, Coeff roExFarDamage, uint dwTriggerMask = Unit.kTriggerOnAttackTargetTrigger, uint dwEffectiveTypeFlags = UnitForce.kEnemy)
        : base(pName, 0)
    {
        m_fNearRange = fNearRange;
        m_oExNearDamage = roExNearDamage;
        m_fFarRange = fFarRange;
        m_oExFarDamage = roExFarDamage;
        m_dwTriggerMask = dwTriggerMask;
        m_dwEffectiveTypeFlags = dwEffectiveTypeFlags;
        SetTriggerFlags(Unit.kTriggerOnAttackTargetTrigger);
    }

    public override Skill Clone()
    {
        SplashPas ret = new SplashPas(m_name, m_fNearRange, m_oExNearDamage, m_fFarRange, m_oExFarDamage, m_dwTriggerMask, m_dwEffectiveTypeFlags);
        ret.CopyDataFrom(this);
        return ret;
    }

    new protected void CopyDataFrom(Skill from)
    {
        base.CopyDataFrom(from);
    }

	public override Skill Clone(string data)
	{
		SplashPas ret = new SplashPas(m_name, m_fNearRange, m_oExNearDamage, m_fFarRange, m_oExFarDamage, m_dwTriggerMask, m_dwEffectiveTypeFlags);
		SkillData sd = JsonUtility.FromJson<SkillData> (data);
		ret.CopyDataFrom (sd);
		return ret;
	}

	protected void CopyDataFrom(SkillData data)
	{
		base.CopyDataFrom (data);
	}

    public override void OnUnitAttackTarget(AttackData pAttack, Unit pTarget)
    {
        Unit o = m_owner;
        if (!pTarget || !o)
        {
            return;
        }

        UnitRenderer td = pTarget.Renderer;
        float fDis;
        World w = o.World;
        var units = w.Units;
        foreach (Unit pUnit in units.Keys)
        {
            if (pUnit == pTarget)
            {
                continue;
            }

            UnitRenderer pDraw = pUnit.Renderer;
            if (!pUnit || pUnit.Ghost)
            {
                continue;
            }

            fDis = Mathf.Max(0.0f, Vector2.Distance(pDraw.Node.position, td.Node.position) - pDraw.HalfOfWidth);
            if (fDis <= m_fFarRange && o.CanEffect(pUnit, m_dwEffectiveTypeFlags))
            {
                AttackData ad = pAttack.Clone();
                AttackValue av = ad.attackValue;

                if (fDis <= m_fNearRange)
                {
                    av.x = m_oExNearDamage.GetValue(av.v);
                }
                else
                {
                    av.x = m_oExFarDamage.GetValue(av.v);
                }

                pUnit.Damaged(ad, o, m_dwTriggerMask);
            }
        }
    }

    float m_fNearRange;
    Coeff m_oExNearDamage;
    float m_fFarRange;
    Coeff m_oExFarDamage;
    uint m_dwTriggerMask;
    uint m_dwEffectiveTypeFlags;
}
