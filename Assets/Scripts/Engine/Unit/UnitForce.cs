using UnityEngine;
using System.Collections;

public class UnitForce
{
    public const uint kSelf = 1 << 0;
    public const uint kOwn = 1 << 1;
    public const uint kAlly = 1 << 2;
    public const uint kEnemy = 1 << 3;

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
    public bool CanEffect(UnitForce force, uint effectiveTypeFlags)
    {
        return ((this == force) && (effectiveTypeFlags & kSelf) != 0) ||
           ((this != force) && (this.IsMyAlly(force) && (effectiveTypeFlags & kAlly) != 0)) ||
           (this.IsMyEnemy(force) && (effectiveTypeFlags & kEnemy) != 0);
    }

    protected uint m_forceFlag;
    protected uint m_allyMaskFlag;
}
