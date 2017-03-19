using UnityEngine.Networking;

public interface INetworkable<TYPE>
    where TYPE : NetworkBehaviour
{
    TYPE localClient { get; }
    bool isServer { get; }
}
