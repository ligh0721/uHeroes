using UnityEngine;
using System.Collections;
using System;

public class UnitForce
{
    public int Force
    {
        get
        {
            return m_forceFlag != 0 ? (int)Mathf.Log(m_forceFlag, 2) : -1;
        }

        set
        {
            m_forceFlag = (uint)1 << value;
        }
    }

    // it ONLY means that force is my ally
    public bool IsMyAlly(UnitForce force)
    {
        return (m_forceFlag == force.m_forceFlag) || ((m_allyMaskFlag & force.m_forceFlag) != 0);
    }

    // it ONLY means that force is my enemy
    public bool IsMyEnemy(UnitForce force)
    {
        return !IsMyAlly(force);
    }

    // the result of 'this.canEffect(force)' is not always same as 'force.canEffect(this)'
    // it ONLY means that 'this' can effect 'force'
    public bool CanEffect(UnitForce force, ForceEffective effectiveTypeFlags)
    {
        return ((this == force) && (effectiveTypeFlags & ForceEffective.kSelf) != 0) ||
           ((this != force) && (this.IsMyAlly(force) && (effectiveTypeFlags & ForceEffective.kAlly) != 0)) ||
           (this.IsMyEnemy(force) && (effectiveTypeFlags & ForceEffective.kEnemy) != 0);
    }

    protected uint m_forceFlag;
    protected uint m_allyMaskFlag;
}

[Flags]
public enum ForceEffective : uint {
    kSelf = 1 << 0,
    kOwn = 1 << 1,
    kAlly = 1 << 2,
    kEnemy = 1 << 3
}