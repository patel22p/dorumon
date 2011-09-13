using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class Game : Bs {
    public enum StartUp { OfflineMode, AutoHost, ShowMenu }
    public enum group { Player }
    public StartUp startUp;
    public GameObject PlayerPrefab;
    public Transform Fx;
    internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }
    public int PlayerScore;
    public int PlayerDeaths;
    public int PlayerPing;

    internal float GameTime = TimeSpan.FromMinutes(18).Milliseconds;

    public override void Awake()
    {
        Application.targetFrameRate = 300;
        Debug.Log("Game Awake");                    
    }
	void Start () {
        if (startUp != StartUp.ShowMenu)
            _GameGui.enabled = false;
    
        if (startUp == StartUp.AutoHost)
        {            
            if (Network.InitializeServer(6, port, false) != NetworkConnectionError.NoError)
                Network.Connect("127.0.0.1", port);
            return;
        }
	}
    

    void OnConnected()
    {
        Debug.Log("Connected");
        _GameGui.enabled = false;
        DestroyImmediate(_Player.gameObject);
        if (PlayerPrefab.networkView == null)
        {
            var nw = PlayerPrefab.AddComponent<NetworkView>();
            nw.stateSynchronization = NetworkStateSynchronization.Unreliable;
            nw.observed = PlayerPrefab.GetComponent<Player>();
        }
        Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)group.Player);
    }
	void Update () {
        GameTime -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && Network.peerType != NetworkPeerType.Disconnected)
            Screen.lockCursor = true;
        if (Input.GetMouseButtonDown(1))
            Screen.lockCursor = false;
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.lockCursor = false;
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            
	}
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected");        
        _GameGui.enabled = true;
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////7
    void OnServerInitialized() { OnConnected(); }
    void OnConnectedToServer() { OnConnected(); }    

}
