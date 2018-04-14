using UnityEngine;
using UnityEngine.Networking;


public class GameNetworkManager : NetworkManager
{
    //GameNetworkEvent _ne;

    void Start () {
        //_ne = GetComponent<GameNetworkEvent>();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!GameNetworkDiscovery.singleton.running)
        {
            conn.Disconnect();
        }
        else
        {
            base.OnServerConnect(conn);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect.");
        base.OnServerDisconnect(conn);
        if (World.Main != null)
        {
            World.Main.StopWorld();
        }
        GameManager.Reset();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect.");
        base.OnClientDisconnect(conn);
        if (World.Main != null)
        {
            World.Main.StopWorld();
        }

        GameManager.Reset();
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        World.Main.RemovePlayerUnits(player.gameObject.GetComponent<GamePlayerController>());
        base.OnServerRemovePlayer(conn, player);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
    }
}
