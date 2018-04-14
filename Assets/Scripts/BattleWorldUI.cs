using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattleWorldUI : MonoBehaviour {
    public BottomStatusBarUI bottomStatusBar;
    public PortraitGroupUI portraitGroup;

    static BattleWorldUI _current;
    
    void Awake() {
        Debug.Assert(_current == null);
        _current = this;
    }

    void OnDestroy() {
        _current = null;
    }

	// Use this for initialization
	void Start() {
		
	}
	
	// Update is called once per frame
	void Update() {
		
	}
    public static BattleWorldUI Current {
        get { return _current; }
    }

    public void OnTestBtn() {
        World.Current.StopWorld();
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
