using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class SyncGameAction {
    public SyncGameAction() {
        unitId = -1;
    }

    public SyncGameAction(Unit unit) {
        unitId = unit.Id;
    }

    public SyncGameAction(UnitNode node) {
        unitId = node.Id;
    }

    public Unit Unit {
        get {
            Unit u = World.Current.GetUnit(unitId);
            return u;
        }
    }

    public UnitNode Node {
        get {
            Unit u = Unit;
            return u != null ? u.Node : null;
        }
    }

    public World world {
        get { return World.Current; }
    }

    public bool valid {
        get { return unitId != 0; }
    }

    public virtual void Play() {
    }

    int unitId;
}

[Serializable]
public class SyncStopAction : SyncGameAction {
    public SyncStopAction(UnitNode node, int tag)
        : base(node) {
        this.tag = tag;
    }

    public override void Play() {
        Node.StopAction(tag);
    }

    int tag;
}

[Serializable]
public class SyncStopAllActions : SyncGameAction {
    public SyncStopAllActions(UnitNode node)
        : base(node) {
    }

    public override void Play() {
        Node.StopAllActions();
    }
}

[Serializable]
public class SyncSetActionSpeed : SyncGameAction {
    public SyncSetActionSpeed(UnitNode node, int tag, float speed)
        : base(node) {
        this.tag = tag;
        this.speed = speed;
    }

    public override void Play() {
        Node.SetActionSpeed(tag, speed);
    }

    int tag;
    float speed;
}

[Serializable]
public class SyncSetFrame : SyncGameAction {
    public SyncSetFrame(UnitNode node, int id)
        : base(node) {
        this.id = id;
    }

    public override void Play() {
        Node.SetFrame(id);
    }

    int id;
}

[Serializable]
public class SyncSetFlippedX : SyncGameAction {
    public SyncSetFlippedX(UnitNode node, bool flippedX)
        : base(node) {
        this.flippedX = flippedX;
    }

    public override void Play() {
        Node.SetFlippedX(flippedX);
    }

    bool flippedX;
}

[Serializable]
public class SyncDoMoveTo : SyncGameAction {
    public SyncDoMoveTo(UnitNode node, Vector2 pos, float duration, float speed)
        : base(node) {
        posX = pos.x;
        posY = pos.y;
        this.duration = duration;
        this.speed = speed;
    }

    public override void Play() {
        Node.DoMoveTo(new Vector2(posX, posY), duration, null, speed);
    }

    float posX;
    float posY;
    float duration;
    float speed;
}

[Serializable]
public class SyncDoAnimate : SyncGameAction {
    public SyncDoAnimate(UnitNode node, int id, int loop, float speed, bool stopAllFirst)
        : base(node) {
        this.id = id;
        this.loop = loop;
        this.speed = speed;
        this.stopAllFirst = stopAllFirst;
    }

    public override void Play() {
        Node.DoAnimate(id, null, loop, null, speed, stopAllFirst);
    }

    int id;
    int loop;
    float speed;
    bool stopAllFirst;
}

[Serializable]
public class SyncSetHp : SyncGameAction {
    public SyncSetHp(Unit unit, float hp)
        : base(unit) {
        this.hp = hp;
    }

    public override void Play() {
        Unit.SetHp(hp);
    }

    float hp;
}

[Serializable]
public class SyncCreateUnit : SyncGameAction {
    public SyncCreateUnit(SyncUnitInfo syncInfo, int playerId) {
        this.syncInfo = syncInfo;
        this.playerId = playerId;
    }

    public override void Play() {
        Debug.Log("SyncCreateUnit");
        World.Current.CreateUnit(syncInfo, playerId);
    }

    SyncUnitInfo syncInfo;
    int playerId;
}

[Serializable]
public class SyncCreateProjectile : SyncGameAction {
    public SyncCreateProjectile(ProjectileSyncInfo syncInfo) {
        this.syncInfo = syncInfo;
    }

    public override void Play() {
        World.Current.CreateProjectile(syncInfo);
    }

    ProjectileSyncInfo syncInfo;
}

[Serializable]
public class SyncStartWorld : SyncGameAction {
    public SyncStartWorld() {
    }

    public override void Play() {
        world.StartWorld();
    }
}

[Serializable]
public class SyncRemoveUnit : SyncGameAction {
    public SyncRemoveUnit(Unit unit, bool revivalbe)
        : base(unit) {
        this.revivalbe = revivalbe;
    }

    public override void Play() {
        Unit u = Unit;
        u.World.RemoveUnit(u, revivalbe);
    }

    bool revivalbe;
}

[Serializable]
public class SyncRemoveUnitHUD : SyncGameAction {
    public SyncRemoveUnitHUD(Unit unit)
        : base(unit) {
    }

    public override void Play() {
        Unit u = Unit;
        u.World.RemoveUnitHUD(u);
    }
}









// ============ Tanks =============
[Serializable]
public class SyncCreateTank : SyncGameAction {
    public SyncCreateTank(TankSyncInfo syncInfo, int playerId) {
        this.syncInfo = syncInfo;
        this.playerId = playerId;
    }

    public override void Play() {
        Debug.Log("SyncCreateTank");
        World.Current.CreateTank(syncInfo, playerId);
    }

    TankSyncInfo syncInfo;
    int playerId;
}
