using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TankGunSyncInfo {
    public Vector3Serializable position;
    public float rotation;
    public float rotateSpeed;
}

[Serializable]
public class TankSyncInfo {
    public TankSyncInfo() {
    }

    public TankSyncInfo(int id, TankInfo baseInfo) {
        Debug.Assert(GamePlayerController.localClient.isServer);
        this.id = id;
        this.baseInfo = baseInfo;
    }

#if false
    public SyncTankInfo(Tank unit) {
        SyncTankInfo syncInfo = new SyncTankInfo();
        syncInfo.baseInfo.model = unit.Model;
        syncInfo.baseInfo.name = unit.Name;
        syncInfo.baseInfo.maxHp = unit.MaxHpBase;

        syncInfo.position = unit.Node.position;
        syncInfo.rotation = unit.Node.rotation;

        syncInfo.hp = unit.Hp;
        syncInfo.force = unit.force.Force;
        syncInfo.baseInfo.move = unit.MoveSpeedBase;
        syncInfo.baseInfo.revivable = unit.Revivable;
        syncInfo.baseInfo.isfixed = unit.Fixed;

        return syncInfo;
    }
#endif

    public int id;
    public TankInfo baseInfo = new TankInfo();
    public Vector2Serializable position;
    public float rotation;
    public float hp;
    public int force;
    public List<TankGunSyncInfo> guns = new List<TankGunSyncInfo>();
}