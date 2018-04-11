using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class TankController : UnitController {
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
