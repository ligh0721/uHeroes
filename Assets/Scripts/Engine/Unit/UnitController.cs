using System;
using UnityEngine;
using UnityEngine.EventSystems;


[Serializable]
public class SyncUnitInfo {
    public SyncUnitInfo() {
    }

    public SyncUnitInfo(GameObject from) {
        Unit unit = from.GetComponent<Unit>();
        UnitNode node = from.GetComponent<UnitNode>();

        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo.model = unit.Model;
        syncInfo.baseInfo.name = unit.Name;
        syncInfo.baseInfo.maxHp = unit.MaxHpBase;
        AttackAct attack = unit.AttackSkill as AttackAct;
        if (attack != null) {
            syncInfo.baseInfo.attackSkill = new AttackInfo();
            syncInfo.baseInfo.attackSkill.cd = attack.coolDownBase;
            syncInfo.baseInfo.attackSkill.type = AttackValue.TypeToName(attack.AttackType);
            syncInfo.baseInfo.attackSkill.value = attack.AttackValueBase;
            syncInfo.baseInfo.attackSkill.range = attack.CastRange;
            syncInfo.baseInfo.attackSkill.horizontal = attack.CastHorizontal;
            var castAnimations = attack.castAnimations;
            syncInfo.baseInfo.attackSkill.animations = new string[castAnimations.Count];
            for (int i = 0; i < castAnimations.Count; ++i) {
                syncInfo.baseInfo.attackSkill.animations[i] = ModelNode.IdToName(castAnimations[i]);
            }
            syncInfo.baseInfo.attackSkill.projectile = attack.ProjectileTemplate.Model;
        }

        syncInfo.position = node.position;
        syncInfo.flippedX = node.flippedX;
        syncInfo.hp = unit.Hp;
        syncInfo.force = unit.force.Force;
        syncInfo.baseInfo.move = unit.MoveSpeedBase;
        syncInfo.baseInfo.revivable = unit.Revivable;
        syncInfo.baseInfo.isfixed = unit.Fixed;
    }

    public int id;
    public UnitInfo baseInfo = new UnitInfo();
    public Vector2Serializable position;
    public bool flippedX;
    public float hp;
    public int force;
}


public class UnitController : MonoBehaviour, INetworkable<GamePlayerController> {
    void Start() {
        
    }

    protected MouseStatus m_mouse = new MouseStatus();
    CameraFollowPlayer m_cameraFollow;

    protected CameraFollowPlayer Follow {
        get { return m_cameraFollow ?? (m_cameraFollow = Camera.main.GetComponent<CameraFollowPlayer>()); }
    }

    protected bool m_recoverTimer = false;
    protected Vector3 m_cameraOrg;

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
                }
            }
            break;
        default:
            break;
        }
    }

    protected void RecoveryCameraFollow() {
        Follow.follow = true;
        m_recoverTimer = false;
    }

    // Networkable
    internal GamePlayerController m_client;

    public GamePlayerController client {
        get { return m_client; }
    }

    public GamePlayerController localClient {
        get { return GamePlayerController.localClient; }
    }

    public bool isServer {
        get { return localClient.isServer; }
    }
}
