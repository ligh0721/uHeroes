using UnityEngine;
using System;


public class ActiveSkill : Skill
{
	new protected class SkillData : Skill.SkillData
	{
	}

	public ActiveSkill (string name, float coolDown, CommandTarget.Type castType = CommandTarget.Type.kNoTarget, uint effectiveTypeFlags = UnitForce.kSelf | UnitForce.kOwn | UnitForce.kAlly | UnitForce.kEnemy)
		: base (name, coolDown)
	{
		m_castTargetType = castType;
		m_effectiveTypeFlags = effectiveTypeFlags;
	}

	public override Skill Clone ()
	{
		throw new NotImplementedException();
	}

	new protected void CopyDataFrom (Skill from)
	{
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

	public override Skill Clone(string data)
	{
		throw new NotImplementedException();
	}

	protected void CopyDataFrom(SkillData data)
	{
		base.CopyDataFrom (data);
	}

	public static readonly float CONST_MAX_CAST_BUFFER_RANGE = 0.5f;
	public static readonly float CONST_MAX_HOR_CAST_Y_RANGE = 0.2f;

	public virtual bool CheckConditions (CommandTarget rTarget)
	{
        return true;
	}

	public virtual void OnUnitCastSkill ()
	{
	}

	public void Effect ()
	{
		Unit o = m_owner;
		UnitRenderer d = o.Renderer;

		StartCoolingDown();
		OnUnitCastSkill();

		switch (m_castTargetType)
		{
		case CommandTarget.Type.kNoTarget:
			PlayEffectSound();
			OnUnitSkillEffect(null, null);
			break;

		case CommandTarget.Type.kUnitTarget:
			{
				Unit t = o.CastTarget.TargetUnit;

				if (t == null || t.Dead)
				{
					return;
				}

				if (m_projectileTemplate != null && t != o)
				{
					Projectile p = m_projectileTemplate.Clone();
					p.SourceUnit = o;
					p.SourceSkill = this;
					p.EffectiveTypeFlags = m_effectiveTypeFlags;
                    
					UnitRenderer td = t.Renderer;
					Debug.Assert(td != null);

					p.TypeOfFromTo = Projectile.FromToType.kUnitToUnit;
					p.FromUnit = o;
					p.ToUnit = t;
                    
					p.Fire();
				}
				else
				{
					PlayEffectSound();
					OnUnitSkillEffect(null, t);
				}
			}
			break;

		case CommandTarget.Type.kPointTarget:
			if (m_projectileTemplate != null)
			{
				Projectile p = m_projectileTemplate.Clone();
				p.SourceUnit = o;
				p.SourceSkill = this;
				p.EffectiveTypeFlags = m_effectiveTypeFlags;

				p.TypeOfFromTo = Projectile.FromToType.kUnitToPoint;
				p.FromUnit = o;
				p.ToPosition = Utils.GetForwardPoint(d.Node.position, o.CastTarget.TargetPoint, m_castRange);

				p.Fire();
			}
			else
			{
				PlayEffectSound();
				OnUnitSkillEffect(null, null);
			}

			break;
		}
	}

	public CommandTarget.Type CastTargetType {
		get {
			return m_castTargetType;
		}

		set {
			m_castTargetType = value;
		}
	}

	public float CastMinRange {
		get {
			return m_castMinRange;
		}

		set {
			m_castMinRange = value;
		}
	}

	public float CastRange {
		get {
			return m_castRange;
		}

		set {
			m_castRange = value;
		}
	}

	public float CastTargetRadius {
		get {
			return m_castTargetRadius;
		}

		set {
			m_castTargetRadius = value;
		}
	}

	public Projectile ProjectileTemplate {
		get {
			return m_projectileTemplate;
		}

		set {
			m_projectileTemplate = value;
		}
	}

	public bool CastHorizontal {
		get {
			return m_castHorizontal;
		}

		set {
			m_castHorizontal = value;
		}
	}

	protected CommandTarget.Type m_castTargetType;
	protected float m_castMinRange;
	protected float m_castRange;
	protected float m_castTargetRadius;
	protected Projectile m_projectileTemplate;
	protected bool m_castHorizontal;

	public Vector2 GetAbilityEffectPoint (Projectile pProjectile, Unit target)
	{
		if (pProjectile != null)
		{
			return pProjectile.Renderer.Node.position;
		}

		if (target != null)
		{
			UnitRenderer td = target.Renderer;
			return td.Node.position;
		}

		UnitRenderer od = m_owner.Renderer;

		switch (m_castTargetType)
		{
		case CommandTarget.Type.kNoTarget:
			return od.Node.position;

		case CommandTarget.Type.kPointTarget:
			return m_owner.CastTarget.TargetPoint;
		}

		Unit u = m_owner.CastTarget.TargetUnit;
		if (u == null)
		{
			Debug.LogError("GetAbilityEffectPoint() err.");
			return od.Node.position;
		}

		UnitRenderer ud = u.Renderer;
		return ud.Node.position;
	}
}
