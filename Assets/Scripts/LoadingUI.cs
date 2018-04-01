using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour, INetworkable<GamePlayerController> {

    public Text m_progress;
    // Use this for initialization
    void Start () {
        StartCoroutine(ResourceManager.instance.LoadResourcesFromQueueAndSwitchScene(delegate (int current, int total) {
            m_progress.text = string.Format("{0}/{1}", current, total);
        }));
    }

    // Update is called once per frame
    void Update () {
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
