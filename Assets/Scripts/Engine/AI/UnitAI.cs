using UnityEngine;
using System.Collections;
using System;

public class UnitAI : IUnitEvent
{
    public virtual void OnUnitTick(Unit unit, float dt)
    {
        if (unit.Suspended || unit.IsDoingOr(Unit.kDoingObstinate))
        {
            // 如果正在固执做事或正在施法
            return;
        }

        if (unit.IsDoingOr(Unit.kDoingCasting) && !unit.IsDoingOr(Unit.kDoingMoving))
        {
            // 原地施法
            return;
        }

        UnitRenderer d = unit.Renderer;

        ActiveSkill atk = unit.AttackSkill;
        if (atk == null)
        {
            return;
        }

        // 追击目标仍在仇恨区内就继续追击
        if (unit.CastActiveSkill == atk && unit.CastTarget.TargetType == CommandTarget.Type.kUnitTarget)
        {
            Unit tt = unit.CastTarget.TargetUnit;
            if (tt != null && tt.Valid)
            {
                UnitRenderer ttd = tt.Renderer;
                if (ttd != null && unit.IsDoingAnd(Unit.kDoingCasting | Unit.kDoingMoving) && Vector2.Distance(d.Node.position, ttd.Node.position) < unit.HostilityRange)
                {
                    // 正在追击施法，距离在仇恨范围内
                    return;
                }
            }
        }

        Unit t = UnitGroup.getNearestUnitInRange(unit.World, d.Node.position, unit.HostilityRange, UnitGroup.MatchFunctionLivingEnemy, unit);
        if (t == null || !t.Valid || t.Dead)
        {
            // 搜不到仇恨区内的目标，有没有必要设置为doNothing？
            return;
        }

        if (unit.CastActiveSkill != atk || unit.CastTarget.TargetUnit != t)
        {
            //Debug.LogFormat("{0} want to attack {1}.", unit.Name, t.Name);
            unit.CommandCastSpell(new CommandTarget(t), atk, false);
        }
    }

    public virtual void OnUnitDamagedDone(Unit unit, float fDamage, Unit source)
    {
        if (!unit.Valid || unit.Suspended || unit.IsDoingOr(Unit.kDoingObstinate))
        {
            return;
        }
        //Debug.LogErrorFormat("{0}.", u.isDoingAnd(Unit.kDoingObstinate) ? "Obs" : "NOT");
        if (source == null || !source.Valid || source.Dead || source.IsMyAlly(unit))
        {
            return;
        }

        UnitRenderer d = unit.Renderer;
        if (d == null)
        {
            return;
        }

        ActiveSkill atk = unit.AttackSkill;
        if (atk == null)
        {
            return;
        }

        ActiveSkill atking = ((atk == unit.CastActiveSkill) ? atk : null);
        Unit t = null;
        if (atking != null && unit.CastTarget.TargetType == CommandTarget.Type.kUnitTarget)
        {
            // 正在进行攻击，而且攻击目标类型为单位目标
            if (unit.CastTarget.TargetUnit != source)
            {
                // 伤害源不是当前的攻击目标
                t = unit.CastTarget.TargetUnit;
                if (t == null)
                {
                    // 当前攻击目标已经不存在
                    //Debug.LogFormat("setCastTarget(CommandTarget()).");
                    unit.CastTarget = new CommandTarget();
                    unit.EndDoing(Unit.kDoingCasting);
                    unit.CastActiveSkill = null;
                }
            }
            else
            {
                return;
            }
        }

        // 当前目标存在！   如果能打到之前的目标 或 之前的目标在仇视范围内    (目标非建筑  或(是建筑，且源也是建筑))
        // 果伤害源
        if (t != null && t.Valid && (Vector2.Distance(d.Node.position, t.Renderer.Node.position) < unit.HostilityRange || unit.CheckCastTargetDistance(atking, d.Node.position, unit.CastTarget, t)))
        {
            // 如果能打到之前的目标，不改变攻击目标
            return;
        }

        if (unit.IsDoingOr(Unit.kDoingSpinning))
        {
            return;
        }

        if (atk == null)
        {
            return;
        }

        unit.CommandCastSpell(new CommandTarget(source), atk, false);

        return;
    }

    public virtual void OnUnitLevelChanged(Unit unit, int changed)
    {
    }

    public virtual void OnUnitRevive(Unit unit)
    {
    }

    public virtual void OnUnitDying(Unit unit)
    {
    }

    public virtual void OnUnitDead(Unit unit)
    {
    }

    public virtual void OnUnitHpChanged(Unit unit, float changed)
    {
    }

    public virtual void OnUnitAttackTarget(Unit unit, AttackData attack, Unit target)
    {
    }

    public bool OnUnitAttacked(Unit unit, AttackData attack, Unit source)
    {
        return true;
    }

    public virtual void OnUnitDamaged(Unit unit, AttackData attack, Unit source)
    {
    }

    public virtual void OnUnitDamageTargetDone(Unit unit, float damage, Unit target)
    {
    }

    public virtual void OnUnitProjectileEffect(Unit unit, Projectile projectile, Unit target)
    {
    }

    public bool OnUnitProjectileArrive(Unit unit, Projectile projectile)
    {
        return true;
    }

    public virtual void OnUnitAddActiveSkill(Unit unit, ActiveSkill skill)
    {
    }

    public virtual void OnUnitDelActiveSkill(Unit unit, ActiveSkill skill)
    {
    }

    public virtual void OnUnitAddPassiveSkill(Unit unit, PassiveSkill skill)
    {
    }

    public virtual void OnUnitDelPassiveSkill(Unit unit, PassiveSkill skill)
    {
    }

    public virtual void OnUnitAddBuffSkill(Unit unit, BuffSkill skill)
    {
    }

    public virtual void OnUnitDelBuffSkill(Unit unit, BuffSkill skill)
    {
    }

    public virtual void OnUnitSkillCD(Unit unit, Skill skill)
    {
    }

    public virtual void OnUnitSkillReady(Unit unit, Skill skill)
    {
    }

    public virtual void OnUnitAddItem(Unit unit, int index)
    {
    }

    public virtual void OnUnitDelItem(Unit unit, int index)
    {
    }

    public static UnitAI instance
    {
        get
        {
            return s_instance ?? (s_instance = new UnitAI());
        }
    }

    protected static UnitAI s_instance;
}
