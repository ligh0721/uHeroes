using UnityEngine;
using System.Collections.Generic;
using cca;
using System;

public class Projectile : INetworkable<GamePlayerController> {
    public Projectile() {
    }

    public Projectile(ProjectileRenderer renderer) {
        Init(renderer);
    }

    public void Init(ProjectileRenderer renderer) {
        m_renderer = renderer;
        renderer.m_projectile = this;
        m_renderer.SetFrame(ObjectRenderer.kFrameDefault);
    }

    public int Id {
        get {
            return m_id;
        }
    }

    public ProjectileRenderer Renderer {
        get {
            return m_renderer;
        }
    }

    public string Root {
        get {
            return m_root;
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

    public virtual Projectile Clone() {
        Projectile ret = ProjectileController.Create(m_root).projectile;
        CopyDataTo(ret);
        return ret;
    }

    protected virtual void CopyDataTo(Projectile to) {
        to.m_moveSpeed = m_moveSpeed;
        to.m_maxHeightDelta = m_maxHeightDelta;
        to.m_srcUnit = m_srcUnit;
        to.m_effectiveTypeFlags = m_effectiveTypeFlags;
        to.m_effectFlags = m_effectFlags;
        to.m_fromToType = m_fromToType;
        to.m_fireType = m_fireType;
        to.m_fireSounds.AddRange(m_fireSounds);
        to.m_effectSounds.AddRange(m_effectSounds);
    }

    // Effect Flags
    public const uint kEffectOnDying = 1 << 0;
    public const uint kEffectOnContact = 1 << 1;

    // 抛射物作用时机
    public uint EffectFlags {
        get {
            return m_effectFlags;
        }

        set {
            m_effectFlags = value;
        }
    }

    bool HasEffectFlag(uint effectFlag) {
        return (m_effectFlags & effectFlag) != 0;
    }

    protected uint m_effectFlags;

    protected void OnEffect() {
        PlayEffectSound();
        Unit t = (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) ? m_toUnit : null;
        Effect(t);
    }

    protected void OnDyingDone() {
        m_renderer.Node.stopAllActions();
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
            if (s == null || !s.Valid) {
                return;
            }

            foreach (var kv in s.World.Units) {
                var u = kv.Key;
                if (u.Ghost || !s.CanEffect(u, m_effectiveTypeFlags)) {
                    continue;
                }

                UnitRenderer d = u.Renderer;
                if (Vector2.Distance(d.Node.position, m_renderer.Node.position) - d.HalfOfWidth - Radius <= 0 && !m_contactedUnits.Contains(u)) {
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
        m_renderer.Node.stopAllActions();
        cca.Function onEffect = null;
        if (HasEffectFlag(kEffectOnDying)) {
            onEffect = OnEffect;
        }
        m_renderer.DoAnimate(ObjectRenderer.kActionDie, onEffect, 1, OnDyingDone);
    }

    protected void Effect(Unit target) {
        Unit s = m_srcUnit;
        if (m_srcUnit == null || !s.Valid) {
            return;
        }

        DecContactLeft();

#if true  // FOR TEST
        if (m_attackData != null && m_srcUnit != null && m_srcUnit.Valid) {
            if (m_srcUnit.Attack(m_attackData, target, m_triggerMask)) {
                target.Damaged(m_attackData, m_srcUnit, m_triggerMask);
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
        get {
            return m_effectiveTypeFlags;
        }

        set {
            m_effectiveTypeFlags = value;
        }
    }

    protected uint m_effectiveTypeFlags = UnitForce.kEnemy;

    // fire
    // you need to set m_fireType, m_fromToType, m_toUnit or m_toPos, m_fromUnit or m_fromPos before call me
    public void Fire() {
        GameController.instance.FireProjectile(this);

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
                    UnitRenderer d = u.Renderer;

                    m_renderer.Node.height = m_usingFirePoint ?
                            d.Node.height + d.FireOffset.y :
                            d.Node.height + d.HalfOfHeight;
                    m_fromPos = m_usingFirePoint ?
                            d.Node.position + new Vector2(d.Node.flippedX ? -d.FireOffset.x : d.FireOffset.x, 0) :
                            d.Node.position;
                }

                Unit t = m_toUnit;
                UnitRenderer td = t.Renderer;

                float fDis = Vector2.Distance(m_fromPos, td.Node.position + new Vector2(0, td.HalfOfHeight));
                FireFollow(fDis / Mathf.Max(float.Epsilon, m_moveSpeed));
            }

            break;

        case FireType.kLink:
            {
                switch (m_fromToType) {
                case FromToType.kUnitToUnit:
                    FireLink(m_fromUnit, m_toUnit);
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

                if (m_fromToType == FromToType.kUnitToPoint) {
                    Unit u = m_fromUnit;
                    UnitRenderer d = u.Renderer;

                    m_renderer.Node.height = m_usingFirePoint ?
                            d.Node.height + d.FireOffset.y :
                            d.Node.height + d.HalfOfHeight;
                    m_fromPos = m_usingFirePoint ?
                            d.Node.position + new Vector2(d.Node.flippedX ? -d.FireOffset.x : d.FireOffset.x, 0) :
                            d.Node.position;
                }

                float fDis = Vector2.Distance(m_fromPos, m_toPos);
                FireStraight(m_fromPos, m_toPos, fDis / Mathf.Max(float.Epsilon, m_moveSpeed), m_maxHeightDelta);
            }

            break;
        }
    }

    void FireFollow(float duration) {
        //m_fromPos = fromPos;
        //m_toUnit = toUnit;
        Debug.Assert(m_toUnit.Valid);

        m_renderer.Node.position = m_fromPos;

        m_renderer.Node.stopAllActions();

        m_renderer.DoAnimate(ObjectRenderer.kActionMove, null, ObjectRenderer.CONST_LOOP_FOREVER, null);
        cca.Function onMoveToFinished = delegate {
            if (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) {
                if (m_toUnit.Valid && m_toUnit.OnProjectileArrive(this) == false) {
                    return;
                }
            }

            Die();
        };
        m_renderer.DoMoveToUnit(m_toUnit.Renderer, true, m_maxHeightDelta, duration, onMoveToFinished);
    }

    void FireLink(Unit fromUint, Unit toUnit) {
        m_fromUnit = fromUint;
        m_toUnit = toUnit;

        Unit u = fromUint;
        UnitRenderer d = u.Renderer;

        Unit t = toUnit;
        UnitRenderer td = t.Renderer;

        Debug.Assert(u != null && t != null && d != null && td != null);

        m_fromPos = d.Node.position;
        m_toPos = td.Node.position;
        m_renderer.Node.stopAllActions();

        cca.Function onEffect = null;
        if (HasEffectFlag(kEffectOnDying)) {
            onEffect = OnEffect;
        }
        m_renderer.DoLinkUnitToUnit(d, td, ObjectRenderer.kActionDie, onEffect, 1, OnDyingDone);
    }

    void FireStraight(Vector2 fromPos, Vector2 toPos, float duration, float maxHeightDelta) {
        m_contactedUnits.Clear();

        m_fromPos = fromPos;
        m_toPos = toPos;

        m_renderer.Node.position = fromPos;

        m_renderer.Node.stopAllActions();

        m_renderer.DoAnimate(ObjectRenderer.kActionMove, null, ObjectRenderer.CONST_LOOP_FOREVER, null);
        cca.Function onMoveToFinished = delegate {
            if (m_fromToType == FromToType.kPointToUnit || m_fromToType == FromToType.kUnitToUnit) {
                if (m_toUnit.Valid && m_toUnit.OnProjectileArrive(this) == false) {
                    return;
                }
            }

            Die();
        };
        m_renderer.DoMoveTo(toPos, duration, onMoveToFinished);
    }

    public Vector2 FromPosition {
        get {
            return m_fromPos;
        }

        set {
            m_fromPos = value;
        }
    }

    public Vector2 ToPosition {
        get {
            return m_toPos;
        }

        set {
            m_toPos = value;
        }
    }

    public Unit FromUnit {
        get {
            return m_fromUnit;
        }

        set {
            m_fromUnit = value;
        }
    }

    public Unit ToUnit {
        get {
            return m_toUnit;
        }

        set {
            m_toUnit = value;
        }
    }

    public AttackData AttackData {
        get {
            return m_attackData;
        }

        set {
            m_attackData = value;
        }
    }

    public uint TriggerMask {
        get {
            return m_triggerMask;
        }

        set {
            m_triggerMask = value;
        }
    }

    public Unit SourceUnit {
        get {
            return m_srcUnit;
        }

        set {
            m_srcUnit = value;
        }
    }

    public Skill SourceSkill {
        get {
            return m_srcSkill;
        }

        set {
            m_srcSkill = value;
        }
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
        get {
            return m_fromToType;
        }

        set {
            m_fromToType = value;
        }
    }

    public FireType TypeOfFire {
        get {
            return m_fireType;
        }

        set {
            m_fireType = value;
        }
    }

    public bool UseFireOffset {
        get {
            return m_usingFirePoint;
        }

        set {
            m_usingFirePoint = value;
        }
    }

    protected Vector2 m_fromPos;
    protected Vector2 m_toPos;
    protected Unit m_fromUnit;
    protected Unit m_toUnit;
    protected Unit m_srcUnit;
    protected bool m_usingFirePoint = true;
    protected AttackData m_attackData;
    protected uint m_triggerMask = Unit.kTriggerMaskNoMasked;
    protected Skill m_srcSkill;
    protected FromToType m_fromToType = FromToType.kPointToPoint;
    protected FireType m_fireType = FireType.kFollow;

    public float MoveSpeed {
        get {
            return m_moveSpeed;
        }

        set {
            m_moveSpeed = value;
        }
    }

    public float MaxHeightDelta {
        get {
            return m_maxHeightDelta;
        }

        set {
            m_maxHeightDelta = value;
        }
    }

    public float Radius {
        get {
            Vector2 size = m_renderer.Node.size;
            return (size.x + size.y) / 2;
        }
    }

    protected float m_moveSpeed = 1;
    protected float m_maxHeightDelta;

    protected ProjectileRenderer m_renderer;
    protected World m_world;
    protected internal int m_id;
    protected internal string m_root;

    // Networkable
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
}
