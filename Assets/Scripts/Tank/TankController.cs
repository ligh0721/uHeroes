using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class SyncTankGunInfo {
    public Vector3Serializable position;
    public float rotation;
}

[Serializable]
public class SyncTankInfo {
    public static SyncTankInfo Create(Unit unit) {
        SyncTankInfo syncInfo = new SyncTankInfo();
        syncInfo.baseInfo.root = unit.Root;
        syncInfo.baseInfo.name = unit.Name;
        syncInfo.baseInfo.maxHp = unit.MaxHpBase;
        
        syncInfo.position = unit.Renderer.Node.position;
        syncInfo.rotation = unit.Renderer.Node.rotation;

        syncInfo.hp = unit.Hp;
        syncInfo.force = unit.Force;
        syncInfo.baseInfo.move = unit.MoveSpeedBase;
        syncInfo.baseInfo.revivable = unit.Revivable;
        syncInfo.baseInfo.isfixed = unit.Fixed;

        return syncInfo;
    }

    public int id;
    public TankInfo baseInfo = new TankInfo();
    public Vector2Serializable position;
    public float rotation;
    public float hp;
    public int force;
    public List<SyncTankGunInfo> guns = new List<SyncTankGunInfo>();
}

public class TankController : UnitController {
    public static TankController Create(SyncTankInfo syncInfo, GamePlayerController client) {
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
        unit.Renderer.Node.position = syncInfo.position;
        unit.Renderer.Node.rotation = syncInfo.rotation;
        unit.Hp = syncInfo.hp;
        unit.Force = syncInfo.force;
        unit.MoveSpeedBase = (float)syncInfo.baseInfo.move;
        unit.Revivable = syncInfo.baseInfo.revivable;
        unit.Fixed = syncInfo.baseInfo.isfixed;

        for (int i = 0; i < syncInfo.guns.Count; ++i){
            unit.AddGun(i);
            unit.SetGunPosition(i, syncInfo.guns[i].position);
            unit.SetGunRotation(i, syncInfo.guns[i].rotation);
        }

        unitCtrl.m_unit = unit;
        WorldController.instance.world.AddUnit(unit);

        return unitCtrl;
    }

    void LateUpdate() {
        if (client == null || !client.isLocalPlayer) {
            // exit from update if this is not the local player
            return;
        }

        m_mouse.update();

        switch (m_mouse.status) {
        case MouseStatus.Status.kDown:
            break;
        case MouseStatus.Status.kStartMove:
            if (Follow.follow) {
                Follow.follow = false;
            }
            m_cameraOrg = Camera.main.transform.position;
            if (m_recoverTimer) {
                CancelInvoke("RecoveryCameraFollow");
                m_recoverTimer = false;
            }
            break;
        case MouseStatus.Status.kMove:
            Camera.main.transform.position = Camera.main.ScreenToWorldPoint(m_mouse.startMove) - m_mouse.nowWorld + m_cameraOrg;
            break;
        case MouseStatus.Status.kUp:
            if (m_mouse.moved) {
                if (m_recoverTimer) {
                    CancelInvoke("RecoveryCameraFollow");
                }
                Invoke("RecoveryCameraFollow", 2.0f);
                m_recoverTimer = true;
            } else {
                //Follow.enabled = true;
                bool touchUI = (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) || EventSystem.current.IsPointerOverGameObject();

                if (!touchUI) {
                    localClient.CmdMove(m_mouse.nowWorld, true);
                    //localClient.CmdMoveTank(m_mouse.nowWorld, true);
                }
            }
            break;
        default:
            break;
        }
    }
}
