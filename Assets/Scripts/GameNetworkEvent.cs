using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class GameNetworkEvent : MonoBehaviour
{
    //public UnityAction<string, string> m_OnReceivedBroadcast;
    public GameObject m_uiObject;

    [Serializable]
    class EventOnReceivedBroadcast : UnityEvent<string, string> { }
    EventOnReceivedBroadcast _event = new EventOnReceivedBroadcast();

    void Start () {
        StartUI ui = m_uiObject.GetComponent<StartUI>();
        _event.AddListener(ui.OnReceivedBroadcast);
    }
	
	public void OnReceivedBroadcast(string fromAddress, string data)
    {
        _event.Invoke(fromAddress, data);
    }
}
