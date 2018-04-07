using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class SyncGameAction {
    public SyncGameAction(int unit) {
        unitId = unit;
    }

    public Unit unit {
        get {
            Unit u = WorldController.instance.world.GetUnit(unitId);
            return u;
        }
    }

    public UnitNode renderer {
        get {
            Unit u = unit;
            return u != null ? u.Renderer : null;
        }
    }

    public World world {
        get {
            return WorldController.instance.world;
        }
    }

    public bool valid {
        get {
            return unitId != 0;
        }
    }

    public virtual void Play() {
    }

    int unitId;
}

[Serializable]
public class SyncStopAction : SyncGameAction {
    public SyncStopAction(int unit, int tag)
        : base(unit) {
        this.tag = tag;
    }

    public override void Play() {
        renderer.StopAction(tag);
    }

    int tag;
}

[Serializable]
public class SyncStopAllActions : SyncGameAction {
    public SyncStopAllActions(int unit)
        : base(unit) {
    }

    public override void Play() {
        renderer.StopAllActions();
    }
}

[Serializable]
public class SyncSetActionSpeed : SyncGameAction {
    public SyncSetActionSpeed(int unit, int tag, float speed)
        : base(unit) {
        this.tag = tag;
        this.speed = speed;
    }

    public override void Play() {
        renderer.SetActionSpeed(tag, speed);
    }

    int tag;
    float speed;
}

[Serializable]
public class SyncSetFrame : SyncGameAction {
    public SyncSetFrame(int unit, int id)
        : base(unit) {
        this.id = id;
    }

    public override void Play() {
        renderer.SetFrame(id);
    }

    int id;
}

[Serializable]
public class SyncSetFlippedX : SyncGameAction {
    public SyncSetFlippedX(int unit, bool flippedX)
        : base(unit) {
        this.flippedX = flippedX;
    }

    public override void Play() {
        renderer.SetFlippedX(flippedX);
    }

    bool flippedX;
}

[Serializable]
public class SyncDoMoveTo : SyncGameAction {
    public SyncDoMoveTo(int unit, Vector2 pos, float duration, float speed)
        : base(unit) {
        posX = pos.x;
        posY = pos.y;
        this.duration = duration;
        this.speed = speed;
    }

    public override void Play() {
        renderer.DoMoveTo(new Vector2(posX, posY), duration, null, speed);
    }

    float posX;
    float posY;
    float duration;
    float speed;
}

[Serializable]
public class SyncDoAnimate : SyncGameAction {
    public SyncDoAnimate(int unit, int id, int loop, float speed)
        : base(unit) {
        this.id = id;
        this.loop = loop;
        this.speed = speed;
    }

    public override void Play() {
        renderer.DoAnimate(id, null, loop, null, speed);
    }

    int id;
    int loop;
    float speed;
}

[Serializable]
public class SyncSetHp : SyncGameAction {
    public SyncSetHp(int unit, float hp)
        : base(unit) {
        this.hp = hp;
    }

    public override void Play() {
        unit.Hp = hp;
    }

    float hp;
}

[Serializable]
public class SyncCreateUnit : SyncGameAction {
    public SyncCreateUnit(SyncUnitInfo syncInfo, int playerId)
        : base(-1) {
        this.syncInfo = syncInfo;
        this.playerId = playerId;
    }

    public override void Play() {
        Debug.Log("SyncCreateUnit");
        WorldController.instance.world.CreateUnit(syncInfo, playerId);
    }

    SyncUnitInfo syncInfo;
    int playerId;
}

[Serializable]
public class SyncStartWorld : SyncGameAction {
    public SyncStartWorld()
        : base(-1) {
    }

    public override void Play() {
        world.Start();
    }
}

[Serializable]
public class SyncRemoveUnit : SyncGameAction {
    public SyncRemoveUnit(int unit, bool revivalbe)
        : base(unit) {
        this.revivalbe = revivalbe;
    }

    public override void Play() {
        world.RemoveUnit(unit, revivalbe);
    }

    bool revivalbe;
}

[Serializable]
public class SyncFireProjectile : SyncGameAction {
    public SyncFireProjectile(SyncProjectileInfo syncInfo)
        : base(-1) {
        this.syncInfo = syncInfo;
    }

    public override void Play() {
        ProjectileController projCtrl = ProjectileController.Create(syncInfo);
        Projectile projectile = projCtrl.projectile;
        projectile.Fire();
    }

    SyncProjectileInfo syncInfo;
}









// ============ Tanks =============
[Serializable]
public class SyncCreateTank : SyncGameAction {
    public SyncCreateTank(SyncTankInfo syncInfo, int playerId)
        : base(-1) {
        this.syncInfo = syncInfo;
        this.playerId = playerId;
    }

    public override void Play() {
        Debug.Log("SyncCreateTank");
        GamePlayerController.localClient.CreateTank(syncInfo, playerId);
    }

    SyncTankInfo syncInfo;
    int playerId;
}
