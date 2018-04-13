using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent(typeof(TankNode))]
public class Tank : Unit {
    Dictionary<int, TankGunNode> m_guns = new Dictionary<int, TankGunNode>();

    public void AddGun(int index) {
        GameObject gunGameObject = new GameObject("gun");
        gunGameObject.AddComponent<SpriteRenderer>();
        TankGunNode gunNode = gunGameObject.AddComponent<TankGunNode>();
        gunNode.parent = Node;
        gunNode.positionZ = -1;
        gunNode.SetFrame(ModelNode.kFrameDefault);
        m_guns.Add(index, gunNode);
    }

    public void SetGunRotation(int index, float rotation) {
        TankGunNode gunNode;
        if (!m_guns.TryGetValue(index, out gunNode)) {
            return;
        }
        gunNode.rotation = rotation;
    }

    public void SetGunPosition(int index, Vector3 position) {
        TankGunNode gunNode;
        if (!m_guns.TryGetValue(index, out gunNode)) {
            return;
        }
        gunNode.position = position;
        gunNode.positionZ = -1;
    }


    // 转向速度
    protected Value m_rotateSpeed = new Value(1.0f);
}

[Serializable]
public class SyncTankGunInfo {
    public Vector3Serializable position;
    public float rotation;
    public float rotateSpeed;
}

[Serializable]
public class SyncTankInfo {
    public SyncTankInfo() {
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
    public List<SyncTankGunInfo> guns = new List<SyncTankGunInfo>();
}