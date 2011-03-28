using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using doru;



public class Game : bs
{
    public hostDebug hostDebug;
    
    [FindAsset("Player")]
    public GameObject PlayerPrefab;
    [FindAsset("Zombie")]
    public Zombie ZombiePrefab;

    public new Player _PlayerOwn;
    public new Player _PlayerOther;
    public bool singlePlayer;
    public List<bs> networkItems = new List<bs>();
    public List<Zombie> Zombies = new List<Zombie>();
    public List<ZombieSpawn> ZombieSpawns = new List<ZombieSpawn>();
    
    public TimerA timer = new TimerA();
    public override void Awake()
    {
        
        base.Awake();
        AddToNetwork();        
        //if (hostDebug == hostDebug.singlePlayer)
        //    Action(MenuAction.single);
    }
    void Start()
    {

        Screen.lockCursor = true;
        var g = (GameObject)Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)NetworkGroup.Player);
        _MenuGui.enabled = false;
        networkView.RPC("Test", RPCMode.Others);
    }
    [RPC]
    void Test()
    {
        Debug.Log("test");
        networkView.RPC("Test2", RPCMode.Others);
    }
    [RPC]
    void Test2()
    {
        Debug.Log("Test2");
    }
    void Update()
    {
        if (timer.TimeElapsed(2000) && Network.isServer && Zombies.Count < 10)
        {
            var zsp = ZombieSpawns.Random();
            var zmb = Network.Instantiate(ZombiePrefab, zsp.pos, zsp.rot, (int)NetworkGroup.Zombie);            
            
        }
        if (DebugKey(KeyCode.Q))
        {
            Debug.Log("Disconnect");
            Network.Disconnect();
        }
        timer.Update();
    }
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    public override void OnEditorGui()
    {
        hostDebug = (hostDebug)UnityEditor.EditorGUILayout.EnumPopup(hostDebug);
        base.OnEditorGui();
    }
#endif
    void Action(MenuAction a)
    {
        if (a == MenuAction.wait)
        {
            Network.InitializeServer(2, 5300, !Network.HavePublicAddress());
            _Loader.WriteDebug("Waiting For Players");
            _MenuGui.enabled = false;
        }
        if (a == MenuAction.join)
        {
            _Loader.WriteDebug("Connecting");
            var ips = new List<string>();

            for (int i = 0; i < 255; i++)
                ips.Add("192.168.30." + i);
            Network.Connect(ips.ToArray(), 5300);
        }
        if (a == MenuAction.single)
        {
            singlePlayer = true;
            Network.InitializeServer(1, 5300, true);
        }
    }
    private void OnConnect()
    {
        _Loader.WriteDebug("Connected");
        var nws = networkItems.Where(a => a is Game).Union(networkItems);
        foreach (var n in nws)
            n.enabled = true;
    }
    void onDisconnect()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    void OnConnectedToServer() { OnConnect(); }
    void OnServerInitialized() { if (singlePlayer) OnConnect(); }
    void OnPlayerConnected() { OnConnect(); }
    void OnDisconnectedFromServer() { onDisconnect(); }
    void OnPlayerDisconnected() { onDisconnect(); }
    
    
    
}