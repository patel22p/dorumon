using System;
using UnityEngine;
using doru;
using System.Collections.Generic;
using System.Linq;

public class Game : bs
{
    public TimerA timer = new TimerA();
    public Animation deadAnim;
    [FindTransform]
    public Base cursor;
    public bool AutoConnect = true;
    public List<Score> scores = new List<Score>();
    public GameObject PlayerPrefab;
    internal new Player _Player;
    public List<Player> players2 = new List<Player>();
    public IEnumerable<Player> players { get { return players2.Where(a => a != null); } }
    
    public bool pause;
    internal float prestartTm = 3;
    internal List<bs> networkItems = new List<bs>();
    

    public override void Awake()
    {
        Debug.Log("Game awake");
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            Debug.Log("Game Awake Autoconnect:" + AutoConnect);
            if (AutoConnect)
            {
                var ips = new List<string>();
                for (int i = 0; i < 255; i++)
                    ips.Add("192.168.30." + i);
                Network.Connect(ips.ToArray(), 5300);
            }
            if (!AutoConnect)
                InitServer();
        }
        AddToNetwork();
        base.Awake();
    }
    internal Transform water;
    public void Start()
    {
        _MyGui.Hide();
        Debug.Log("Game start");
        water = GameObject.Find("water").transform;
         var g = (GameObject)Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, 1);
         _Player = g.GetComponent<Player>();
    }
    void Update()
    {        
        timer.Update();
        prestartTm -= Time.deltaTime;
        UpdateTimeText();
        UpdateOther();
    }
    
    void UpdateTimeText()
    {
        if (prestartTm > 0 && !debug)
        {
            _GameGui.CenterTime.text = (Mathf.Ceil(prestartTm)) + "";
            return;
        }
        else
            _GameGui.CenterTime.enabled = false;
    }
    void UpdateOther()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _MyGui.Show(Wind.ExitToMenuWindow);
    }
    [RPC]
    private void WinGame()
    {
        pause = true;
        deadAnim.Play();
        if(Network.isServer)
            timer.AddMethod(2000, delegate { _Loader.NextLevel(); });
    }
    public void OnConnect()
    {
        Debug.Log("Connected");
        foreach (var a in networkItems)
            a.enabled = true;
    }
    
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player Conencted: " + player);
    }
    public override void Init()
    {
        IgnoreAll("Ignore Raycast");
        IgnoreAll("IgnoreColl");
        IgnoreAll("Water");
        base.Init();
    }
    private static void AddColl(string a, string b)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(a), LayerMask.NameToLayer(b), false);
    }
    
    public override void OnEditorGui()
    {
        AutoConnect = GUILayout.Toggle(AutoConnect, "AutoConnect", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("InitWalls"))
        {
            foreach (Wall a in GameObject.FindObjectsOfType(typeof(Wall)))
            {
                a.Init();
            }
        }
        base.OnEditorGui();
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {        
        Debug.Log("Player disc " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnConnectedToServer() { OnConnect(); }
    void OnServerInitialized() { OnConnect(); }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        //if (info == NetworkDisconnection.LostConnection)
        //    timer.AddMethod(delegate { _MyGui.Show(MyGui.Wind.Disconnected); });
        _Loader.totalScores = 0;
        Application.LoadLevel(0);
    }
    private void InitServer()
    {
        Network.InitializeServer(8, 5300, !Network.HavePublicAddress());
    }
    void OnFailedToConnect(NetworkConnectionError err)
    {
        Debug.Log("Could not connect to server: " + err);
        InitServer();
    }
}
