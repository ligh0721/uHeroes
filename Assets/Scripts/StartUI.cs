using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartUI : MonoBehaviour {

    public Button m_searchGame;
    public InputField m_remoteHost;
    public Button m_joinGame;
    public InputField m_playerName;

    public List<string> m_preNames;

    NetworkManager _nm;
    NetworkManager nm
    {
        get
        {
            return _nm ?? (_nm = NetworkManager.singleton);
        }
    }
    NetworkDiscovery _nd;
    NetworkDiscovery nd
    {
        get
        {
            return _nd ?? (_nd = GameNetworkDiscovery.singleton);
        }
    }

    void Start () {
        string name = PlayerPrefs.GetString("name");
        m_playerName.text = name == "" ? m_preNames[Utils.Random.Next(m_preNames.Count)] : name;
    }

    public void OnCreateGameClick()
    {
        nm.StartHost();
        if (nd.running)
        {
            nd.StopBroadcast();
        }

        nd.Initialize();
        nd.StartAsServer();

        PlayerPrefs.SetString("name", m_playerName.text);
        GameManager.Init(true);
    }

    public void OnJoinGameClick()
    {
        //get
        string remoteHost = m_remoteHost.text;
        nm.networkAddress = remoteHost == "" ? "localhost" : remoteHost;
        nm.StartClient();

        PlayerPrefs.SetString("name", m_playerName.text);
        GameManager.Init(false);
    }

    public void OnSearchGameClick()
    {
        if (!nd.running)
        {
            nd.Initialize();
            nd.StartAsClient();
            m_searchGame.GetComponentInChildren<Text>().text = "Searching..";
        }
        else
        {
            nd.StopBroadcast();
            m_searchGame.GetComponentInChildren<Text>().text = "Search Game";
        }
    }

    public void OnReceivedBroadcast(string fromAddress, string data)
    {
        if (nd.running)
        {
            string[] splitData = fromAddress.Split(':');
            m_remoteHost.text = splitData[3];
            nd.StopBroadcast();
            m_searchGame.GetComponentInChildren<Text>().text = "Search Game";
        }
    }
}
