using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattleWorldUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnTestBtn() {
        World.Main.StopWorld();
        GameManager.Reset();

        if (GameManager.isServer) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();
        }

        if (GameNetworkDiscovery.singleton.running) {
            GameNetworkDiscovery.singleton.StopBroadcast();
        }
    }
}
