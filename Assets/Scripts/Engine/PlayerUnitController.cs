using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnitController : MonoBehaviour, INetworkable<GamePlayerController> {
    static PlayerUnitController _current;
    bool m_recoverTimer = false;
    Vector3 m_cameraOrg;
    MouseStatus m_mouse = new MouseStatus();

    UnitSafe m_controlling;

    void Awake() {
        Debug.Assert(_current == null);
        _current = this;
    }

    // Use this for initialization
    void Start() {
	}
	
    void OnDestroy() {
        _current = null;
    }

    void LateUpdate() {
        m_mouse.update();

        switch (m_mouse.status) {
        case MouseStatus.Status.kDown:
            break;
        case MouseStatus.Status.kStartMove:
            if (World.Current.cameraCtrl.enabled) {
                World.Current.SetCameraFollowedEnabled(false);
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
                Unit u = m_controlling;
                if (u != null) {
                    bool touchUI = (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) || EventSystem.current.IsPointerOverGameObject();
                    if (!touchUI) {
                        localClient.CmdMove(u.Id, m_mouse.nowWorld, true);
                    }
                }
            }
            break;
        default:
            break;
        }
    }

    void RecoveryCameraFollow() {
        World.Current.SetCameraFollowedEnabled(true);
        m_recoverTimer = false;
    }

    public static PlayerUnitController Current {
        get { return _current; }
    }

    public Unit Controlling {
        get { return m_controlling; }
        set {
            m_controlling.Set(value);
            World.Current.SetCameraFollowed(value.gameObject);
            if (value == null) {
                foreach (PortraitUI portrait in BattleWorldUI.Current.portraitGroup.Portraits) {
                    portrait.Selected = false;
                }
            } else {
                foreach (PortraitUI portrait in BattleWorldUI.Current.portraitGroup.Portraits) {
                    if (portrait.Unit == value) {
                        portrait.Selected = true;
                        break;
                    }
                }
            }
            BattleWorldUI.Current.bottomStatusBar.SetUnit(value);
            BattleWorldUI.Current.bottomStatusBar.Show();
        }
    }

    public GamePlayerController localClient {
        get {
            return GamePlayerController.localClient;
        }
    }

    public bool isServer {
        get {
            return GamePlayerController.localClient.isServer;
        }
    }

}
