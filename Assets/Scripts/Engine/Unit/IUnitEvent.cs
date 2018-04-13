public interface IUnitEvent
{
    void OnUnitLevelChanged(Unit unit, int changed);
    void OnUnitRevive(Unit unit);
    void OnUnitDying(Unit unit);
    void OnUnitDead(Unit unit);
    void OnUnitHpChanged(Unit unit, float changed);
    void OnUnitTick(Unit unit, float dt);
    void OnUnitAttackTarget(Unit unit, AttackData attack, Unit target);
    bool OnUnitAttacked(Unit unit, AttackData attack, Unit source);
    void OnUnitDamaged(Unit unit, AttackData attack, Unit source);
    void OnUnitDamagedDone(Unit unit, float damage, Unit source);
    void OnUnitDamageTargetDone(Unit unit, float damage, Unit target);
    void OnUnitProjectileEffect(Unit unit, Projectile projectile, Unit target);
    bool OnUnitProjectileArrive(Unit unit, Projectile projectile);
    void OnUnitAddActiveSkill(Unit unit, ActiveSkill skill);
    void OnUnitDelActiveSkill(Unit unit, ActiveSkill skill);
    void OnUnitAddPassiveSkill(Unit unit, PassiveSkill skill);
    void OnUnitDelPassiveSkill(Unit unit, PassiveSkill skill);
    void OnUnitAddBuffSkill(Unit unit, BuffSkill skill);
    void OnUnitDelBuffSkill(Unit unit, BuffSkill skill);
    void OnUnitSkillCD(Unit unit, Skill skill);
    void OnUnitSkillReady(Unit unit, Skill skill);
    void OnUnitAddItem(Unit unit, int index);
    void OnUnitDelItem(Unit unit, int index);
    //void OnUnitChangeItemStackCount(Unit unit, Item item, int change) {}
}
