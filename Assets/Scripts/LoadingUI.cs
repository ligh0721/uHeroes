using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour, INetworkable<GamePlayerController> {

    public Text m_progressType;
    public Text m_progress;
    // Use this for initialization
    void Start () {
        StartCoroutine(ResourceManager.instance.LoadResourcesFromQueueAndReplaceScene(delegate (ResourceManager.LoadingProgressInfo prog) {
            switch (prog.type) {
            case ResourceManager.LoadingProgressType.Resource:
                if (m_progress.text != "Loading Models") {
                    m_progressType.text = "Loading Models";
                }
                break;
            case ResourceManager.LoadingProgressType.Scene:
                if (m_progress.text != "Loading Scene") {
                    m_progressType.text = "Loading Scene";
                }
                break;
            case ResourceManager.LoadingProgressType.Done:
                GamePlayerController.localClient.CmdClientLoadSceneFinished();
                break;
            }
            m_progress.text = string.Format("{0:N0}%", prog.value * 100.0f / prog.max);
        }, null));
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
