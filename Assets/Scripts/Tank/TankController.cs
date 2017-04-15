using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TankController : UnitController {
    public static new TankController Create(SyncUnitInfo syncInfo, GamePlayerController client) {
        Debug.Log("CreateTank");
        GameObject gameObject = GameObjectPool.instance.Instantiate(WorldController.instance.unitPrefab);
        TankController unitCtrl = gameObject.GetComponent<TankController>();
        unitCtrl.m_client = client;

        //ResourceManager.instance.Load<UnitResInfo>(syncInfo.baseInfo.root);
        TankRenderer r = new TankRenderer(WorldController.instance.unitPrefab, gameObject);
        //ResourceManager.instance.PrepareUnitResource(syncInfo.baseInfo.root, r);

        //Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}", path, frame));
        //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);

        Tank unit = new Tank(r);
        unit.m_id = syncInfo.id;
        unit.m_client = client;
        unit.m_root = syncInfo.baseInfo.root;
        if (unitCtrl.isServer) {
            //unit.AI = UnitAI.instance;
        }

        unit.Name = syncInfo.baseInfo.name;
        unit.MaxHpBase = (float)syncInfo.baseInfo.maxHp;
        if (syncInfo.baseInfo.attackSkill.valid) {
            AttackAct atk = new AttackAct(syncInfo.baseInfo.attackSkill.name, (float)syncInfo.baseInfo.attackSkill.cd, new AttackValue(AttackValue.NameToType(syncInfo.baseInfo.attackSkill.type), (float)syncInfo.baseInfo.attackSkill.value), (float)syncInfo.baseInfo.attackSkill.vrange);
            atk.CastRange = (float)syncInfo.baseInfo.attackSkill.range;
            atk.CastHorizontal = syncInfo.baseInfo.attackSkill.horizontal;
            foreach (var ani in syncInfo.baseInfo.attackSkill.animations) {
                atk.AddCastAnimation(ObjectRenderer.NameToId(ani));
            }
            atk.ProjectileTemplate = ProjectileController.CreateProjectileTemplate(syncInfo.baseInfo.attackSkill.projectile);
            unit.AddActiveSkill(atk);
        }
        unit.Renderer.Node.position = new Vector2(syncInfo.positionX, syncInfo.positionY);
        //unit.Renderer.SetFlippedX(syncInfo.flippedX);
        unit.Hp = syncInfo.hp;
        unit.Force = syncInfo.force;
        unit.MoveSpeedBase = (float)syncInfo.baseInfo.move;
        unit.Revivable = syncInfo.baseInfo.revivable;
        unit.Fixed = syncInfo.baseInfo.isfixed;

        unitCtrl.m_unit = unit;
        WorldController.instance.world.AddUnit(unit);

        return unitCtrl;
    }
}
