using System;
using UnityEngine;


[Serializable]
public class SyncUnitInfo {
    public SyncUnitInfo() {
    }

    public SyncUnitInfo(int id, UnitInfo baseInfo) {
        Debug.Assert(GamePlayerController.localClient.isServer);
        this.id = id;
        this.baseInfo = baseInfo;
    }

#if false
    public SyncUnitInfo(Unit unit) {
        UnitNode node = unit.Node;

        baseInfo.model = unit.Model;
        baseInfo.name = unit.Name;
        baseInfo.maxHp = unit.MaxHpBase;
        AttackAct attack = unit.AttackSkill as AttackAct;
        if (attack != null) {
            baseInfo.attackSkill = new AttackInfo();
            baseInfo.attackSkill.cd = attack.coolDownBase;
            baseInfo.attackSkill.type = AttackValue.TypeToName(attack.AttackType);
            baseInfo.attackSkill.value = attack.AttackValueBase;
            baseInfo.attackSkill.range = attack.CastRange;
            baseInfo.attackSkill.horizontal = attack.CastHorizontal;
            List<int> castAnimations = attack.castAnimations;
            baseInfo.attackSkill.animations = new string[castAnimations.Count];
            for (int i = 0; i < castAnimations.Count; ++i) {
                baseInfo.attackSkill.animations[i] = ModelNode.IdToName(castAnimations[i]);
            }
            baseInfo.attackSkill.projectile = attack.ProjectileTemplate.Model;
        }

        position = node.position;
        flippedX = node.flippedX;
        hp = unit.Hp;
        force = unit.force.Force;
        baseInfo.move = unit.MoveSpeedBase;
        baseInfo.revivable = unit.Revivable;
        baseInfo.isfixed = unit.Fixed;
    }
#endif

    public int id;
    public UnitInfo baseInfo = new UnitInfo();
    public Vector2Serializable position;
    public bool flippedX;
    public float hp;
    public int force;
}
