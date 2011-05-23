using System;
using UnityEngine;
using doru;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public enum Scene { Menu, RaceLevel, RaceLevel2, Level3Unfinished, Level4Unfinished }
public enum Mask { Player = 8, NotPlayer, Level, IgnoreColl, Particles }
public class Game : bs
{
    public TimerA timer = new TimerA();
    public GUIText wingamegui;
    public float networkTime;
    //public Animation deadAnim;
    public ParticleEmitter[] particles;
    public Vector3 spawn;
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
    public LayerMask RopeColl;
    public override void Awake()
    {
        InitLoader();        
        AddToNetwork();
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
     
    }
    internal Transform water;
    
    public void Start()
    {

        Debug.Log("Game start"+(int)Mask.NotPlayer);
        SetupSpawnPos();
        SetupOther();
        SetupPlayer();
        
    }
    public override void Init()
    {        
        IgnoreAll("Ignore Raycast");
        IgnoreAll("IgnoreColl");
        IgnoreAll("Particles", "Level", "Default");        
        IgnoreAll("Water");
        base.Init();
    }
    
    private void SetupOther()
    {
        water = GameObject.Find("water").transform;
        water.SetActive(QualitySettings.currentLevel != QualityLevel.Fastest);
        if (debug) prestartTm = 0;
        _MenuGui.Hide();
        
    }

    private void SetupPlayer()
    {        
        var g = (GameObject)Network.Instantiate(PlayerPrefab, spawn, Quaternion.identity, 1);
        _Player = g.GetComponent<Player>();
    }

    private void SetupSpawnPos()
    {
        var t = GameObject.Find("spawn");        
        if (t != null && debug)
            spawn = t.transform.position;
    }
    void Update()
    {

        networkTime += Time.deltaTime;
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
        if (Input.GetKeyDown(KeyCode.X))
            Debug.Break();
        if (Input.GetKeyDown(KeyCode.Return))
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
                _Loader.NextLevel();
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
    

    private static void Call(string s)
    {
        foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
            go.SendMessage(s, SendMessageOptions.DontRequireReceiver);
    }
    
    private static void AddColl(string a, string b)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(a), LayerMask.NameToLayer(b), false);
    }
    
    public override void OnEditorGui()
    {
        AutoConnect = GUILayout.Toggle(AutoConnect, "AutoConnect", GUILayout.ExpandWidth(false));

        base.OnEditorGui();
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {                
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnConnectedToServer() { OnConnect(); }
    void OnServerInitialized() { OnConnect(); }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        _Loader.totalScores = 0;

        Application.LoadLevel((int)Scene.Menu);
        _Popup.ShowPopup(info + "");
    }
    private void InitServer()
    {

        Network.InitializeServer(8, 5300, true);
    }
    void OnFailedToConnect(NetworkConnectionError err)
    {
        Debug.Log("Could not connect to server: " + err);
        InitServer();
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        stream.Serialize(ref networkTime);
    }
    public void Emit(Vector3 pos, params int[] id)
    {
        foreach (var i in id)
        {
            var p = particles[i];
            p.transform.position = pos;            
            p.Emit();
        }
    }
}
