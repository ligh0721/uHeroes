using System;
using UnityEngine;
using UnityEngine.EventSystems;


[Serializable]
public class SyncUnitInfo {
    public static SyncUnitInfo Create(Unit unit) {
        SyncUnitInfo syncInfo = new SyncUnitInfo();
        syncInfo.baseInfo.root = unit.Root;
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
                syncInfo.baseInfo.attackSkill.animations[i] = ObjectRenderer.IdToName(castAnimations[i]);
            }
            syncInfo.baseInfo.attackSkill.projectile = attack.ProjectileTemplate.Root;
        }

        Vector2 position = unit.Renderer.Node.position;
        syncInfo.positionX = position.x;
        syncInfo.positionY = position.y;
        syncInfo.flippedX = unit.Renderer.Node.flippedX;
        syncInfo.hp = unit.Hp;
        syncInfo.force = unit.Force;
        syncInfo.baseInfo.move = unit.MoveSpeedBase;
        syncInfo.baseInfo.revivable = unit.Revivable;
        syncInfo.baseInfo.isfixed = unit.Fixed;

        return syncInfo;
    }

    public int id;
    public UnitInfo baseInfo = new UnitInfo();
    public float positionX;
    public float positionY;
    public bool flippedX;
    public float hp;
    public int force;
}


public class UnitController : MonoBehaviour, INetworkable<GamePlayerController> {
    public Texture2D m_texHpBorder;
    public Texture2D m_texHpFill;

    protected Unit m_unit;

    public Unit unit {
        get {
            return m_unit;
        }
    }

    public static UnitController Create(SyncUnitInfo syncInfo, GamePlayerController client) {
        Debug.Log("CreateUnit");
        GameObject gameObject = GameObjectPool.instance.Instantiate(WorldController.instance.unitPrefab);
        UnitController unitCtrl = gameObject.GetComponent<UnitController>();
        unitCtrl.m_client = client;

        ResourceManager.instance.Load<UnitResInfo>(syncInfo.baseInfo.root);
        UnitRenderer r = new UnitRenderer(WorldController.instance.unitPrefab, gameObject);
        ResourceManager.instance.PrepareUnitResource(syncInfo.baseInfo.root, r);

        Unit unit = new Unit(r);
        unit.m_id = syncInfo.id;
        unit.m_client = client;
        unit.m_root = syncInfo.baseInfo.root;
        if (unitCtrl.isServer) {
            unit.AI = UnitAI.instance;
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
        unit.Renderer.SetFlippedX(syncInfo.flippedX);
        unit.Hp = syncInfo.hp;
        unit.Force = syncInfo.force;
        unit.MoveSpeedBase = (float)syncInfo.baseInfo.move;
        unit.Revivable = syncInfo.baseInfo.revivable;
        unit.Fixed = syncInfo.baseInfo.isfixed;

        unitCtrl.m_unit = unit;
        WorldController.instance.world.AddUnit(unit);

        return unitCtrl;
    }

    void OnGUI() {
        if (m_unit != null && !m_unit.Dead) {
            Vector3 barSize = new Vector3(m_unit.Renderer.HalfOfWidth * 2 + 0.1f, 0.15f, 0);
            Vector2 uPos = m_unit.Renderer.Node.worldPosition;
            Vector3 pos = new Vector3(uPos.x - barSize.x * 0.5f, uPos.y + m_unit.Renderer.HalfOfHeight * 2.5f + 0.2f, 0);
            Vector3 pos2 = new Vector3(pos.x + barSize.x, pos.y - barSize.y, 0);
            Vector2 posUI = Camera.main.WorldToScreenPoint(pos);
            Vector2 pos2UI = Camera.main.WorldToScreenPoint(pos2);
            Vector2 barSizeUI = new Vector2(pos2UI.x - posUI.x, posUI.y - pos2UI.y);
            posUI.y = Screen.height - posUI.y;
            GUI.DrawTexture(new Rect(posUI, barSizeUI), m_texHpBorder, ScaleMode.ScaleAndCrop);
            float percent = m_unit.Hp / m_unit.MaxHp;
            GUI.color = new Color(Mathf.Min(1.0f, (1.00f - percent) * 2.0f), Mathf.Min(1.0f, 2.0f * percent), 0);
            barSizeUI.x *= percent;
            GUI.DrawTexture(new Rect(posUI, barSizeUI), m_texHpFill, ScaleMode.ScaleAndCrop);
        }
    }

    protected MouseStatus m_mouse = new MouseStatus();
    CameraFollowPlayer m_cameraFollow;
    protected CameraFollowPlayer Follow {
        get {
            return m_cameraFollow ?? (m_cameraFollow = Camera.main.GetComponent<CameraFollowPlayer>());
        }
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
    protected GamePlayerController m_client;
    public GamePlayerController client {
        get {
            return m_client;
        }
    }

    public GamePlayerController localClient {
        get {
            return GamePlayerController.localClient;
        }
    }

    public bool isServer {
        get {
            return localClient.isServer;
        }
    }
}
