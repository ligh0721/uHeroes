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
