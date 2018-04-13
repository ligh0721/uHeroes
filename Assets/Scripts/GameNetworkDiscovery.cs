using UnityEngine.Networking;

public class GameNetworkDiscovery : NetworkDiscovery
{
    public static NetworkDiscovery singleton;

    GameNetworkEvent _ne;

    void Start () {
        singleton = this;
        _ne = GetComponent<GameNetworkEvent>();
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        _ne.OnReceivedBroadcast(fromAddress, data);
    }
}
