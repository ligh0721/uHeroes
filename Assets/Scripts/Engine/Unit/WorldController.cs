using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LitJson;
using cca;


public class WorldController : MonoBehaviour {
    public GameObject unitPrefab;
    public GameObject projectilePrefab;
    public GameObject unitUIPrefab;

    GameObject m_worldCanvas;

    World m_world;
    static WorldController s_instance;

    public World world {
        get {
            return m_world;
        }
    }

    public static WorldController instance {
        get {
            return s_instance;
        }
    }

    public GameObject worldCanvas {
        get {
            if (m_worldCanvas == null) {
                m_worldCanvas = GameObject.Find("HUDCanvas");
            }
            return m_worldCanvas;
        }
    }

    // Use this for initialization
    void Start() {
        Debug.Assert(s_instance == null);
        s_instance = this;

        m_world = new World();
        m_world.m_ctrl = this;

        GameObjectPool.ResetFunction reset = delegate (GameObject gameObject) {
            gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        };

        GameObjectPool.instance.Alloc(unitPrefab, 50, reset);
        GameObjectPool.instance.Alloc(projectilePrefab, 50, reset);

        //ObjectPool<RendererNode>.instance.Alloc(100);
        ObjectPool<ProjectileNode>.instance.Alloc(50);
        ObjectPool<Projectile>.instance.Alloc(50);
        //MutiObjectPool.instance.Alloc<UnitRenderer>(50);
    }

    // Update is called once per frame
    void FixedUpdate() {
        m_world.Step(Time.fixedDeltaTime);
    }

    void Update() {
        ActionManager.instance.update(Time.deltaTime);
        if (GamePlayerController.localClient && GamePlayerController.localClient.isServer) {
            GamePlayerController.localClient.ServerSyncActions();
        }
    }

    public void StartWorld() {
        m_world.Start();
    }

    public void StopWorld() {
        m_world.Shutdown();
        Node.destroyAll();
    }

    public void RemovePlayer(GameObject gameObject) {
        var units = m_world.Units;
        Unit toDel = null;
        foreach (var kv in units) {
            if (kv.Key.Renderer.Node.gameObject == gameObject) {
                toDel = kv.Key;
                break;
            }
        }

        if (toDel != null) {
            m_world.RemoveUnit(toDel);
        }
    }

    public void OnTestBtn() {
        StopWorld();
        GameController.Reset();

        if (GameController.isServer) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();
        }

        if (GameNetworkDiscovery.singleton.running) {
            GameNetworkDiscovery.singleton.StopBroadcast();
        }
    }





    // world logic
}
