using UnityEngine;
using System.Collections.Generic;

public class UnitGroup
{
    protected List<Unit> m_units;

    public delegate bool MatchFunction(Unit unit, UnitForce force);

    public const int CONST_COUNT_UNLIMITED = -1;

    public UnitGroup()
    { }

    public UnitGroup(World pWorld, Vector2 roPos, float fRadius, int iMaxCount = CONST_COUNT_UNLIMITED, MatchFunction match = null, UnitForce force = null)
    {
        if (fRadius < float.Epsilon)
        {
            return;
        }

        var units = pWorld.Units;
        foreach (var kv in units)
        {
            Unit u = kv.Key;
            if (u.Ghost)
            {
                continue;
            }

            UnitNode d = u.Renderer;
            if (m_units.Count >= iMaxCount)
            {
                return;
            }
            if (Vector2.Distance(d.Node.position, roPos) - d.HalfOfWidth < fRadius && (match == null || (match(u, force))))
            {
                m_units.Add(u);
            }
        }
    }

    public UnitGroup(World pWorld, int iMaxCount = CONST_COUNT_UNLIMITED, MatchFunction match = null, UnitForce force = null)
    {
        var units = pWorld.Units;
        foreach(var kv in units)
        {
            Unit u = kv.Key;
            if (u.Ghost)
            {
                continue;
            }

            if (m_units.Count >= iMaxCount)
            {
                return;
            }

            if (match == null || (match(u, force)))
            {
                m_units.Add(u);
            }
        }
    }

    public Unit getUnitByIndex(int iIndex)
    {
        if (iIndex < 0 || iIndex >= m_units.Count)
        {

            return null;
        }

        return m_units[iIndex];
    }

    public Unit getRandomUnit()
    {
        return m_units.Count == 0 ? null : m_units[Utils.Random.Next(m_units.Count)];
    }

    public Unit getNearestUnitInRange(Vector2 roPos, float fRadius, MatchFunction match = null, UnitForce force = null)
    {
        Unit target = null;
        float fMinDis = float.MaxValue;
        float fDis;

        foreach (var u in m_units)
        {
            if (u.Ghost)
            {
                continue;
            }

            UnitNode d = u.Renderer;
            if ((fDis = Vector2.Distance(d.Node.position, roPos) - d.HalfOfWidth) < fRadius && fMinDis > fDis && (match == null || (match(u, force))))
            {
                target = u;
                fMinDis = fDis;
            }
        }

        return target;
    }

    public void addUnit(Unit unit)
    {
        m_units.Add(unit);
    }

    public static Unit getNearestUnitInRange(World pWorld, Vector2 roPos, float fRadius, MatchFunction match = null, UnitForce force = null)
    {
        Unit target = null;
        float fMinDis = float.MaxValue;
        float fDis;

        var units = pWorld.Units;
        foreach (var kv in units)
        {
            Unit u = kv.Key;
            if (u.Ghost)
            {
                continue;
            }

            UnitNode d = u.Renderer;
            if ((fDis = Vector2.Distance(d.Node.position, roPos) - d.HalfOfWidth) < fRadius && fMinDis > fDis && (match == null || (match(u, force))))
            {
                target = u;
                fMinDis = fDis;
            }
        }

        return target;
    }

    public void cleanUnits()
    {
        m_units.Clear();
    }

    public int getUnitsCount()
    {
        return m_units.Count;
    }

    public void damaged(AttackData attack, Unit source, uint triggerMask = Unit.kTriggerMaskNoMasked)
    {
        AttackData ad = null;
        foreach(var u in m_units)
        {
            if (u.Ghost)
            {
                continue;
            }

            ad = ad != null ? ad.Clone() : attack;
            u.Damaged(ad, source, triggerMask);
        }
    }

    public static bool MatchFunctionLivingAlly(Unit unit, UnitForce force)
    {
        return !unit.Dead && unit.IsMyAlly(force);
    }

    public static bool MatchFunctionLivingEnemy(Unit unit, UnitForce force)
    {
        return !unit.Dead && force.IsMyEnemy(unit);
    }
}
