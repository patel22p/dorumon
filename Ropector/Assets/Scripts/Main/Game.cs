using System;
using UnityEngine;
using doru;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class Game : Gamebs
{
    public TimerA timer = new TimerA();
    public GUIText wingamegui;
    //public Animation deadAnim;
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
        _Popup.enabled = false;
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            Debug.Log("Game Awake Autoconnect:" + AutoConnect);
            if (AutoConnect)
                LocalConnect();
            if (!AutoConnect)
                InitServer();
        }
        AddToNetwork();
        
        base.Awake();
    }
    internal Transform water;
    
    public void Start()
    {
        
        SetupSpawnPos();
        SetupOther();
        SetupPlayer();        
    }

    private void SetupOther()
    {
        if (debug) prestartTm = 0;
        _MenuGui.Hide();
        Debug.Log("Game start");
        water = GameObject.Find("water").transform;
    }

    private void SetupPlayer()
    {        
        var g = (GameObject)Network.Instantiate(PlayerPrefab, spawn, Quaternion.identity, 1);
        _Player = g.GetComponent<Player>();
    }

    private void SetupSpawnPos()
    {
        var t = GameObject.Find("spawn");        
        if (t != null)
            spawn = t.transform.position;
    }
    void Update()
    {        
        prestartTm -= Time.deltaTime;
        UpdateTimeText();
        UpdateOther();
        timer.Update();
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
        if (Input.GetKeyDown(KeyCode.Space))
            if (_MenuGui.enabled)
                _MenuGui.Hide();
            else
                _MenuGui.Show(Wind.ExitToMenuWindow);
    }
    [RPC]
    private void WinGame(string name)
    {
        _GameGui.deadAnim.animation.Play();
        pause = true;        
        wingamegui.text = name + " Win";
        wingamegui.animation.Play();
        if (Network.isServer)
        {

            timer.AddMethod(7000, delegate
            {
                _Loader.LoadMap(delegate
                {
                    _Loader.StartGame(_Loader.Map);
                });
            });
        }
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
        _Loader.totalScores = 0;        
        if (_Loader.EditorTest)
            Application.LoadLevel((int)Scene.mapEditor);            
        else
        {
            Application.LoadLevel((int)Scene.Menu);
            _Popup.ShowPopup(info + ""); 
            //_Loader.timer.AddMethod(() => Application.loadedLevel == 0, delegate { _MyGui.ShowPopup(info + ""); });
        }
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
