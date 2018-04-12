using UnityEngine;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(ProjectileNode))]
public class Projectile : MonoBehaviour, INetworkable<GamePlayerController> {

#if UNITY_EDITOR
    void Reset() {
        Awake();
    }
#endif

    void Awake() {
        m_node = GetComponent<ProjectileNode>();
        Debug.Assert(m_node != null);
    }

    ProjectileNode m_node;

    public ProjectileNode Node {
        get { return m_node; }
    }

    public int Id {
        get { return m_id; }
    }

    public string Model {
        get { return m_model; }
    }

    public World World {
        get { return m_world; }
    }

    // Effect Flags
    public const uint kEffectOnDying = 1 << 0;
    public const uint kEffectOnContact = 1 << 1;

    // 抛射物作用时机
    public uint EffectFlags {
        get { return m_effectFlags; }

        set { m_effectFlags = value; }
    }

    bool HasEffectFlag(uint effectFlag) {
        return (m_effectFlags & effectFlag) != 0;
    }

    protected uint m_effectFlags;

    protected void OnEffect() {
        PlayEffectSound();
        Unit t = (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) ? m_toUnit.Unit : null;
        Effect(t);
    }

    protected void OnDyingDone() {
        m_node.stopAllActions();
        m_world.RemoveProjectile(this);
    }

    protected void PlayEffectSound() {
        // TODO:
    }

    protected void PlayFireSound() {
        // TODO:
    }

    public void AddEffectSound(params string[] sounds) {
        m_effectSounds.AddRange(sounds);
    }

    public void AddFireSound(params string[] sounds) {
        m_fireSounds.AddRange(sounds);
    }

    protected List<string> m_effectSounds = new List<string>();
    protected List<string> m_fireSounds = new List<string>();

    public void Step(float dt) {
        OnTick(dt);
    }

    protected void OnTick(float dt) {
        if (HasEffectFlag(kEffectOnContact)) {
            Unit s = m_srcUnit;
            if (s == null) {
                return;
            }

            foreach (Unit u in m_world.Units.Keys) {
                if (u.Ghost || !s.force.CanEffect(u.force, m_effectiveTypeFlags)) {
                    continue;
                }

                UnitNode d = u.Node;
                if (Vector2.Distance(d.position, m_node.position) - d.HalfOfWidth - Radius <= 0 && !m_contactedUnits.Contains(u)) {
                    if (u.OnProjectileArrive(this) == false) {
                        continue;
                    }

                    m_contactedUnits.Add(u);
                    Effect(u);

                    if (m_contactLeft == 0) {
                        m_effectFlags &= (~kEffectOnContact);
                        Die();
                        return;
                    }
                }
            }
        }
    }

    protected void Die() {
        m_node.stopAllActions();
        cca.Function onEffect = null;
        if (HasEffectFlag(kEffectOnDying)) {
            onEffect = OnEffect;
        }
        m_node.DoAnimate(ModelNode.kActionDie, onEffect, 1, OnDyingDone);
    }

    protected void Effect(Unit target) {
        Unit s = m_srcUnit;
        if (s == null) {
            return;
        }

        DecContactLeft();

#if true  // FIXME: FOR TEST
        if (m_attackData != null && s != null) {
        if (s.Attack(m_attackData, target, m_triggerMask)) {
                target.Damaged(m_attackData, s, m_triggerMask);
            }
        }
#endif

        if (m_srcSkill != null && m_srcSkill.valid) {
            m_srcSkill.PlayEffectSound();
            m_srcSkill.OnUnitSkillEffect(this, target);
        }

        s.OnProjectileEffect(this, target);
    }

    // Contact
    public void DecContactLeft(int dec = 1) {
        if (m_contactLeft > 0) {
            m_contactLeft -= dec;
        }
    }

    protected int m_contactLeft = -1;
    protected HashSet<Unit> m_contactedUnits = new HashSet<Unit>();

    // 决定能影响的势力群组
    public uint EffectiveTypeFlags {
        get { return m_effectiveTypeFlags; }

        set { m_effectiveTypeFlags = value; }
    }

    protected uint m_effectiveTypeFlags = UnitForce.kEnemy;

    // fire
    // you need to set m_fireType, m_fromToType, m_toUnit or m_toPos, m_fromUnit or m_fromPos before call me
    public void Fire() {
        PlayFireSound();

        switch (m_fireType) {
        case FireType.kFollow:
            {
                Debug.Assert(m_fromToType == FromToType.kUnitToUnit || m_fromToType == FromToType.kPointToUnit);
                if (m_effectFlags == 0) {
                    m_effectFlags = kEffectOnDying;
                }

                if (m_fromToType == FromToType.kUnitToUnit) {
                    Unit u = m_fromUnit;
                    UnitNode d = u.Node;

                    m_node.height = m_usingFirePoint ?
                            d.height + d.FireOffset.y :
                            d.height + d.HalfOfHeight;
                    m_fromPos = m_usingFirePoint ?
                            d.position + new Vector2(d.flippedX ? -d.FireOffset.x : d.FireOffset.x, 0) :
                            d.position;
                }

                Unit t = m_toUnit;
                UnitNode td = t.Node;

                float fDis = Vector2.Distance(m_fromPos, td.position + new Vector2(0, td.HalfOfHeight));
                FireFollow(fDis / Mathf.Max(float.Epsilon, m_moveSpeed));
            }

            break;

        case FireType.kLink:
            {
                switch (m_fromToType) {
                case FromToType.kUnitToUnit:
                    FireLink();
                    break;

                default:
                    Debug.Assert(false);
                    break;
                }
            }

            break;

        case FireType.kStraight:
            {
                Debug.Assert(m_fromToType == FromToType.kUnitToPoint || m_fromToType == FromToType.kPointToPoint);

                if (m_effectFlags == 0) {
                    m_effectFlags = kEffectOnContact;
                }

                if (m_fromToType == FromToType.kUnitToPoint || m_fromToType == FromToType.kUnitToUnit) {
                    Unit u = m_fromUnit;
                    UnitNode d = u.Node;

                    m_node.height = m_usingFirePoint ?
                            d.height + d.FireOffset.y :
                            d.height + d.HalfOfHeight;
                    m_fromPos = m_usingFirePoint ?
                            d.position + new Vector2(d.flippedX ? -d.FireOffset.x : d.FireOffset.x, 0) :
                            d.position;
                }

                if (m_fromToType == FromToType.kUnitToUnit) {
                    m_toPos = m_toUnit.Node.position;
                }

                float fDis = Vector2.Distance(m_fromPos, m_toPos);
                FireStraight(fDis / Mathf.Max(float.Epsilon, m_moveSpeed));
            }

            break;
        }
    }

    /// <summary>
    /// need fromPos,toUnit
    /// </summary>
    /// <param name="duration"></param>
    void FireFollow(float duration) {
        //m_fromPos = fromPos;
        //m_toUnit = toUnit;
        Unit toUnit = m_toUnit.Unit;
        if (toUnit == null) {
            return;
        }

        m_node.position = m_fromPos;

        m_node.stopAllActions();

        m_node.DoAnimate(ModelNode.kActionMove, null, ModelNode.CONST_LOOP_FOREVER, null);
        cca.Function onMoveToFinished = delegate {
            if (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) {
                if (m_toUnit.Unit != null && m_toUnit.Unit.OnProjectileArrive(this) == false) {
                    // 当目标单位存活且目标单位拒绝(反射)抛射物成功，抛射物不死亡(可能被反弹)
                    return;
                }
            }

            Die();
        };
        m_node.DoMoveToUnit(toUnit.Node, true, m_maxHeightDelta, duration, onMoveToFinished);
    }

    /// <summary>
    /// need fromUnit,toUnit
    /// </summary>
    void FireLink() {
        Unit u = m_fromUnit;
        UnitNode d = u.Node;

        Unit t = m_toUnit;
        UnitNode td = t.Node;

        Debug.Assert(u != null && t != null && d != null && td != null);

        m_fromPos = d.position;
        m_toPos = td.position;
        m_node.stopAllActions();

        cca.Function onEffect = null;
        if (HasEffectFlag(kEffectOnDying)) {
            onEffect = OnEffect;
        }
        m_node.DoLinkUnitToUnit(d, td, ModelNode.kActionDie, onEffect, 1, OnDyingDone);
    }

    /// <summary>
    /// need fromPos,toPos
    /// </summary>
    /// <param name="duration"></param>
    void FireStraight(float duration) {
        m_contactedUnits.Clear();

        m_node.position = m_fromPos;

        m_node.stopAllActions();

        m_node.DoAnimate(ModelNode.kActionMove, null, ModelNode.CONST_LOOP_FOREVER, null);
        cca.Function onMoveToFinished = delegate {
            if (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) {
                if (m_toUnit.Unit != null && m_toUnit.Unit.OnProjectileArrive(this) == false) {
                    return;
                }
            }

            Die();
        };
        m_node.DoMoveTo(m_toPos, duration, onMoveToFinished);
    }

    public Vector2 FromPosition {
        get { return m_fromPos; }

        set { m_fromPos = value; }
    }

    public Vector2 ToPosition {
        get { return m_toPos; }

        set { m_toPos = value; }
    }

    public Unit FromUnit {
        get { return m_fromUnit; }

        set { m_fromUnit.Set(value); }
    }

    public Unit ToUnit {
        get { return m_toUnit; }

        set { m_toUnit.Set(value); }
    }

    public AttackData AttackData {
        get { return m_attackData; }

        set { m_attackData = value; }
    }

    public uint TriggerMask {
        get { return m_triggerMask; }

        set { m_triggerMask = value; }
    }

    public Unit SourceUnit {
        get { return m_srcUnit; }

        set { m_srcUnit.Set(value); }
    }

    public Skill SourceSkill {
        get { return m_srcSkill; }

        set { m_srcSkill = value; }
    }

    public enum FromToType {
        kPointToUnit,
        kPointToPoint,
        kUnitToUnit,
        kUnitToPoint
    }

    public enum FireType {
        kFollow,
        kLink,
        kStraight
    }

    public static FireType FireNameToType(string name) {
        switch (name) {
        default:
            return FireType.kFollow;

        case "Link":
            return FireType.kLink;

        case "Straight":
            return FireType.kStraight;
        }
    }

    public static string FireTypeToName(FireType type) {
        switch (type) {
        default:
            return "Follow";

        case FireType.kLink:
            return "Link";

        case FireType.kStraight:
            return "Straight";
        }
    }

    public FromToType TypeOfFromTo {
        get { return m_fromToType; }

        set { m_fromToType = value; }
    }

    public FireType TypeOfFire {
        get { return m_fireType; }

        set { m_fireType = value; }
    }

    public bool UseFireOffset {
        get { return m_usingFirePoint; }

        set { m_usingFirePoint = value; }
    }

    protected Vector2 m_fromPos;
    protected Vector2 m_toPos;
    protected UnitSafe m_fromUnit;
    protected UnitSafe m_toUnit;
    protected UnitSafe m_srcUnit;
    protected bool m_usingFirePoint = true;
    protected AttackData m_attackData;
    protected uint m_triggerMask = Unit.kTriggerMaskNoMasked;
    protected Skill m_srcSkill;
    protected FromToType m_fromToType = FromToType.kPointToPoint;
    protected FireType m_fireType = FireType.kFollow;

    public float MoveSpeed {
        get { return m_moveSpeed; }

        set { m_moveSpeed = value; }
    }

    public float MaxHeightDelta {
        get { return m_maxHeightDelta; }

        set { m_maxHeightDelta = value; }
    }

    public float Radius {
        get {
            Vector2 size = m_node.size;
            return (size.x + size.y) / 2;
        }
    }

    protected float m_moveSpeed = 1;
    protected float m_maxHeightDelta;

    protected internal World m_world;
    protected internal int m_id;
    protected internal string m_model;

    // Networkable
    public GamePlayerController localClient {
        get { return GamePlayerController.localClient; }
    }

    public bool isServer {
        get { return localClient.isServer; }
    }
}

[Serializable]
public class SyncProjectileInfo {
    public SyncProjectileInfo() {
    }

#if false
    public SyncProjectileInfo(Projectile projectile) {
        ProjectileNode node = projectile.Node;

        baseInfo.model = projectile.Model;
        baseInfo.move = projectile.MoveSpeed;
        baseInfo.height = projectile.MaxHeightDelta;
        baseInfo.fire = Projectile.FireTypeToName(projectile.TypeOfFire);
        baseInfo.effect = (int)projectile.EffectFlags;

        //position = node.position;
        //visible = node.visible;
        fromTo = projectile.TypeOfFromTo;
        useFireOffset = projectile.UseFireOffset;
        srcUnit = projectile.SourceUnit != null ? projectile.SourceUnit.Id : 0;
        fromUnit = projectile.FromUnit != null ? projectile.FromUnit.Id : 0;
        toUnit = projectile.ToUnit != null ? projectile.ToUnit.Id : 0;
        fromPos = projectile.FromPosition;
        toPos = projectile.ToPosition;
    }
#endif

    public int id;
    public ProjectileInfo baseInfo = new ProjectileInfo();
    //public Vector2Serializable position;
    //public bool visible;
    public Projectile.FromToType fromTo;
    public bool useFireOffset;
    public int srcUnit;
    public int fromUnit;
    public int toUnit;
    public Vector2Serializable fromPos;
    public Vector2Serializable toPos;
}
