using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomUI : MonoBehaviour, INetworkable<GamePlayerController> {

    public List<GameObject> m_playerSlots;
    [HideInInspector]
    public List<RoomPlayerUI> m_playerUIs;

    public Button m_start;

    // Use this for initialization
    void Start() {
        if (isServer) {
            m_start.enabled = true;
            m_start.GetComponentInChildren<Text>().text = "Start";
        } else {
            m_start.enabled = false;
            m_start.GetComponentInChildren<Text>().text = "Waiting for server to start...";
        }

        m_playerUIs = new List<RoomPlayerUI>(m_playerSlots.Count);
        foreach (var slot in m_playerSlots) {
            m_playerUIs.Add(slot.GetComponent<RoomPlayerUI>());
        }
    }

    public void OnStartClick() {
        GameNetworkDiscovery.singleton.StopBroadcast();
        localClient.RpcStart();
        m_start.enabled = false;
    }

    public void ShowAllProgressText() {
        foreach (var slot in m_playerUIs) {
            slot.ShowProgressText();
        }
    }

    public bool IsAllProgressActionDone {
        get {
            bool value = true;
            foreach (var slot in m_playerUIs) {
                if (slot.IsRunningProgressAction) {
                    value = false;
                    break;
                }
            }
            return value;
        }
    }

    // INetworkable
    public GamePlayerController localClient {
        get {
            return GamePlayerController.localClient;
        }
    }

    public bool isServer {
        get {
            return GameController.isServer;
        }
    }
}
