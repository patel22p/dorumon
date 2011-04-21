using System;
using UnityEngine;
using doru;
using System.Collections.Generic;
using System.Linq;

public class Game : bs
{
    public List<bs> alwaysUpdate = new List<bs>();  
    public TimerA timer = new TimerA();
    public Animation deadAnim;
    [FindTransform]
    public Base cursor;
    public bool AutoConnect = true;
    public List<Score> blues = new List<Score>();
    public GameObject PlayerPrefab;
    public new Player _Player;
    float TimeSpeed = 1;
    internal float Fall = -7;
    public bool Pause;
    internal float prestartTm = 3;
    public float TimeElapsed;
    internal List<bs> networkItems = new List<bs>();

    public override void Awake()
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
        AddToNetwork();
        base.Awake();
    }

    void OnFailedToConnect(NetworkConnectionError err)
    {
        Debug.Log("Could not connect to server: " + err);
        InitServer();
    }
    private void InitServer()
    {
        Network.InitializeServer(8, 5300, !Network.HavePublicAddress());
    }
    public void Start()
    {                
         var g = (GameObject)Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, 1);
         _Player = g.GetComponent<Player>();
    }
    
    void Update()
    {        
        timer.Update();
        prestartTm -= Time.deltaTime;
        UpdateTimeWarp();
        UpdateTimeText();        
        //UpdatePlayerScores();
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
        TimeElapsed += Time.deltaTime;
        _GameGui.time.text = TimeToSTr(TimeElapsed);
    }
    void UpdateOther()
    {
        foreach (var a in alwaysUpdate)
            a.AlwaysUpdate();
    }
    
    private void UpdateTimeWarp()
    {
        if (Input.GetKey(KeyCode.Space))
            TimeSpeed = ((TimeSpeed * 5) + .1f) / 6f;
        else
            TimeSpeed = ((TimeSpeed * 5) + 1) / 6f;
        _Music.audio.pitch = TimeSpeed;
        Time.timeScale = TimeSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    private void UpdatePlayerScores()
    {
        _GameGui.scores.text = _Player.scores + "/" + blues.Count;
        if (_Player.scores == blues.Count && !Pause && !debug) 
        {
            Pause = true;
            deadAnim.Play();
            var f = PlayerPrefs.GetFloat(Application.loadedLevelName);
            if (TimeElapsed < f || f == 0)
            {
                _GameGui.time.text = "New Record:" + TimeToSTr(TimeElapsed);
                PlayerPrefs.SetFloat(Application.loadedLevelName, TimeElapsed);                
            }
            timer.AddMethod(2000, delegate { _Loader.NextLevel(); });
            _Player.gameObject.active = false;
        }
    }
    public void OnConnect()
    {
        Debug.Log("Connected");
        foreach (var a in networkItems)
            a.enabled = true;
    }
    public void onDisconnect()
    {
        Debug.Log("Disc");
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
        if (GUILayout.Button("AddNW"))
        {
            foreach (Wall a in GameObject.FindObjectsOfType(typeof(Wall)))
            {
                if (a.networkView != null)
                    DestroyImmediate(a.networkView);
                //if (a.animation != null && a.animation.clip != null)
                //{
                //    if(a.networkView==null)
                //        a.gameObject.AddComponent<NetworkView>();
                //    a.networkView.observed = a.animation;
                //}
            }
        }
        base.OnEditorGui();
    }
    void OnConnectedToServer() { OnConnect(); }
    void OnServerInitialized() { OnConnect(); }
    
    void OnDisconnectedFromServer() { onDisconnect(); }    
}
