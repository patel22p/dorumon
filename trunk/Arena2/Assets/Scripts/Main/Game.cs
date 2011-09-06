using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using doru;
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
using UnityEditor;
using GUI = UnityEditor.EditorGUILayout;
using gui = UnityEngine.GUILayout;
#endif

public class Game : bs
{
    public hostDebug hostDebug;
    public AnimationCurve SpeedCurv;
    public GameObject PlayerPrefab;
    public Zombie ZombiePrefab;
    [FindTransform]
    public GUIText GameText;
    public new Player _PlayerOwn;
    public new Player _PlayerOther;
    public bool enableZombies;
    public bool enableAutoStart;
    public int stage = 0;
    public bool CantDie;
    internal Player[] players = new Player[3];
    internal bool singlePlayer;
    internal List<bs> networkItems = new List<bs>();
    internal List<Zombie> Zombies = new List<Zombie>();
    internal IEnumerable<Zombie> AliveZombies { get { return Zombies.Where(a => a != null && a.Alive); } }
    internal List<ZombieSpawn> ZombieSpawns = new List<ZombieSpawn>();
    internal TimerA timer = new TimerA();
    internal List<bs> alwaysUpdate = new List<bs>();

    public override void Init()
    {
        IgnoreAll("Dead");
        base.Init();
    }
    public override void Awake()
    {
        base.Awake();
        AddToNetwork();
        if (hostDebug == hostDebug.singl)
            OnGuiEvent(MenuAction.single);
        if (hostDebug == hostDebug.wait)
            OnGuiEvent(MenuAction.wait);
        if (hostDebug == hostDebug.join)
            OnGuiEvent(MenuAction.join);

    }
    private void InstZombie()
    {
        var zsp = ZombieSpawns.Random();
        Network.Instantiate(ZombiePrefab, zsp.pos, zsp.rot, (int)NetworkGroup.Zombie);
    }
    void Start()
    {
        Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)NetworkGroup.Player);
        _MenuGui.enabled = false;
    }
    void Update()
    {
        UpdateZombieSpawn();
    }
    private void UpdateZombieSpawn()
    {
        foreach (bs bs in alwaysUpdate)
            if (bs != null)
                bs.AlwaysUpdate();
        if (enableZombies)
        {
            if (timer.TimeElapsed(2000) && Network.isServer)
            {
                if (Zombies.Count < 1 + stage)
                {
                    InstZombie();
                }
                if (Zombies.Where(a => a == null || a.Alive == false).Count() == Zombies.Count)
                    NextLevel();
            }
        }
        if (DebugKey(KeyCode.Q))
        {
            Debug.Log("Disconnect");
            Network.Disconnect();
        }
        _Loader.WriteVar("Stage:" + stage);
        timer.Update();
    }
    private void NextLevel()
    {
        Zombies.Clear();
        stage++;
    }
    void OnGuiEvent(MenuAction a)
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
            var ip = Network.player.ipAddress;
            ip = ip.Substring(0, ip.LastIndexOf('.')) + ".";
            Debug.Log(ip);
            for (int i = 0; i < 255; i++)
                ips.Add(ip + i);
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
            if (n != null)
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
    
#if UNITY_EDITOR
    public override void OnEditorGui()
    {
        if (gui.Button("Die"))
            _PlayerOwn.networkView.RPC("Die", RPCMode.All);
        if (gui.Button("WakeUp"))
            _PlayerOwn.networkView.RPC("WakeUp", RPCMode.All);
        CantDie = GUI.Toggle("CantDie", CantDie);
        hostDebug = (hostDebug)GUI.EnumPopup(hostDebug);
        enableZombies = GUI.Toggle("zombies", enableZombies);
        if (gui.Button("CreateZombie"))
            InstZombie();
        //enableAutoStart = gui.Toggle("autostart", enableAutoStart);
        base.OnEditorGui();
    }
#endif   
    
}