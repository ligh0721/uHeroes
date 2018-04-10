using UnityEngine;
using System.Collections.Generic;
using System;


public class SkillInfoOnlyBaseId {
    public string baseId;
}

public class Skill {
    protected class SkillData : SkillInfoOnlyBaseId {
    }

    public Skill(string name, float coolDown = 0) {
        m_name = name;
        m_coolDown = new Value(coolDown);
    }

    public Unit owner {
        get { return m_owner; }

        set { m_owner = value; }
    }

    public bool valid {
        get { return m_owner != null && m_owner.enabled; }
    }

    public string name {
        get { return m_name; }

        set { m_name = value; }
    }

    public virtual Skill Clone() {
        throw new NotImplementedException();
    }

    protected void CopyDataFrom(Skill from) {
        m_base = from.m_base;
        m_triggerFlags = from.m_triggerFlags;
        m_interval = from.m_interval;
        m_castAnimations.AddRange(from.m_castAnimations);
        m_effectSounds.AddRange(from.m_effectSounds);
    }

    // data: json
    public virtual Skill Clone(string data) {
        throw new NotImplementedException();
    }

    protected void CopyDataFrom(SkillData data) {
    }

    public string baseId {
        get { return m_base; }
    }

    // 技能持有者事件响应，只覆被注册的触发器相应的事件函数即可
    // @override
    public virtual void OnLevelChanged(int changed) {
    }

    public virtual void OnUnitAddSkill() {
    }

    public virtual void OnUnitDelSkill() {
    }

    public virtual void OnUnitSkillReady() {
    }

    public virtual void OnUnitRevive() {
    }

    public virtual void OnUnitDying() {
    }

    public virtual void OnUnitDead() {
    }

    public virtual void OnUnitHpChanged(float changed) {
    }

    public virtual void OnUnitTick(float dt) {
    }

    public virtual void OnUnitInterval() {
    }

    public virtual void OnUnitAttackTarget(AttackData attack, Unit target) {
    }

    public virtual bool OnUnitAttacked(AttackData attack, Unit source) {
        return true;
    }

    public virtual void OnUnitDamaged(AttackData attack, Unit source) {
    }

    public virtual void OnUnitDamagedDone(float fDamage, Unit source) {
    }

    public virtual void OnUnitDamageTargetDone(float fDamage, Unit target) {
    }

    public virtual void OnUnitProjectileEffect(Projectile projectile, Unit target) {
    }

    public virtual bool OnUnitProjectileArrive(Projectile projectile) {
        return true;
    }

    public virtual void OnUnitSkillEffect(Projectile projectile, Unit target) {  // no need to register this trigger
    }

    public virtual void OnUnitCalcDamageTarget(float fDamage, Unit target) {
    }

    public void OnAddToUnit(Unit pOwner) {
        m_owner = pOwner;
        OnUnitAddSkill();
    }

    public void OnDelFromUnit() {
        OnUnitDelSkill();
        m_owner = null;
    }

    public void PlayEffectSound() {
    }

    public void AddEffectSound(params string[] sounds) {
        m_effectSounds.AddRange(sounds);
    }

    public virtual float coolDown {
        get { return m_coolDown.v; }
    }

    public float coolDownBase {
        get { return m_coolDown.x; }

        set { m_coolDown.x = value; }
    }

    public virtual float coolDownCoeff {
        get { return m_coolDown.a; }

        set { m_coolDown.a = value; }
    }

    public virtual float coolDownSpeed {
        get { return 1 / coolDown; }
    }

    public virtual float coolDownSpeedCoeff {
        get { return 1 / coolDownCoeff; }

        set { coolDownCoeff = 1 / value; }
    }

    public float coolingDownElapsed {
        get { return m_coolingDownElapsed; }

        set { m_coolingDownElapsed = value; }
    }

    public bool coolingDown {
        get { return m_coolingDownElapsed < coolDown; }
    }

    public void ResetCD() {
        m_coolingDownElapsed = float.MaxValue;
        m_owner.UpdateSkillCD(this);
    }

    public void StartCoolingDown(float fromPercent = 0) {
        m_coolingDownElapsed = fromPercent * coolDown;
        m_owner.SkillCD(this);
    }

    public float interval {
        get { return m_interval; }

        set {
            if (value <= float.Epsilon) {
                m_interval = 0;
                return;
            }

            m_triggerFlags = Unit.kTriggerOnTickTrigger;
            m_interval = value;
        }
    }

    public float intervalElapsed {
        get { return m_intervalElapsed; }

        set { m_intervalElapsed = value; }
    }

    public uint triggerFlags {
        get { return m_triggerFlags; }

        set { m_triggerFlags = value; }
    }

    public void SetTriggerFlags(uint triggerFlags) {
        m_triggerFlags |= triggerFlags;
    }

    public void UnsetTriggerFlags(uint triggerFlags) {
        m_triggerFlags &= ~triggerFlags;
    }

    public void AddCastAnimation(params int[] ids) {
        m_castAnimations.AddRange(ids);
    }

    public List<int> castAnimations {
        get { return m_castAnimations; }
    }

    public int castRandomAnimation {
        get {
            if (m_castAnimations.Count == 0) {
                return -1;
            }

            return m_castAnimations[Utils.Random.Next(m_castAnimations.Count)];
        }
    }

    public uint effectiveTypeFlags {
        get { return m_effectiveTypeFlags; }

        set { m_effectiveTypeFlags = value; }
    }

    protected string m_name;
    protected Unit m_owner;
    protected Value m_coolDown;
    protected float m_coolingDownElapsed = float.MaxValue;
    protected float m_interval = 0;
    protected float m_intervalElapsed = 0;
    protected uint m_triggerFlags = 0;
    protected List<int> m_castAnimations = new List<int>();
    protected uint m_effectiveTypeFlags;
    List<string> m_effectSounds = new List<string>();
    protected string m_base;
}
