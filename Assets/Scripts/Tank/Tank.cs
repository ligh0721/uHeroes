using UnityEngine;
using System.Collections.Generic;


public class Tank : Unit {
    public Tank(TankRenderer renderer)
        : base(renderer){
    }

    Dictionary<int, TankGunRenderer> m_guns = new Dictionary<int, TankGunRenderer>();

    public void AddGun(int index) {
        GameObject gunGameObject = new GameObject("gun");
        gunGameObject.AddComponent<SpriteRenderer>();
        TankGunRenderer gunRenderer = new TankGunRenderer(null, gunGameObject);
        gunRenderer.Node.parent = Renderer.Node;
        gunRenderer.Node.positionZ = -1;
        gunRenderer.SetFrame(ObjectRenderer.kFrameDefault);
        m_guns.Add(index, gunRenderer);
    }

    public void SetGunRotation(int index, float rotation) {
        TankGunRenderer gunRenderer;
        if (!m_guns.TryGetValue(index, out gunRenderer)) {
            return;
        }
        gunRenderer.Node.rotation = rotation;
    }

    public void SetGunPosition(int index, Vector3 position) {
        TankGunRenderer gunRenderer;
        if (!m_guns.TryGetValue(index, out gunRenderer)) {
            return;
        }
        gunRenderer.Node.position = position;
        gunRenderer.Node.positionZ = -1;
    }


    // 转向速度
    protected Value m_rotateSpeed = new Value(1.0f);
}
