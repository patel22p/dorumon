using System;
using UnityEngine;
using System.Collections.Generic;
enum NetworkGroup { Player }
public class Game : bs
{
    [FindTransform("debug")]
    public GUIText debugGui;
    [FindAsset("Player")]
    public GameObject PlayerPrefab;
    public new Player _Player;
    public new Player _Player2;
    public bool singlePlayer;
    public List<bs> networkItems = new List<bs>();
    public override void Awake()
    {
        base.Awake();
        AddToNetwork();        
    }
    string debugtext;
    void WriteDebug(string s)
    {
        debugtext += s + "\r\n";
    }
    void Update()
    {
        debugGui.text = debugtext;
    }
    void Action(MenuAction a)
    {
        if (a == MenuAction.wait)
        {
            Network.InitializeServer(2, 5300, !Network.HavePublicAddress());
            
        }
        if ( a == MenuAction.join)
            Network.Connect("192.168.30.255", 5300);
        if (a == MenuAction.single)
        {
            singlePlayer = true;
            Network.InitializeServer(1, 5300, true);
        }
    }
    void OnConnectedToServer() { OnConnect(); }
    void OnServerInitialized() { if (singlePlayer) OnConnect(); }
    void OnPlayerConnected() { OnConnect(); }
    void OnDisconnectedFromServer() { onDisconnect(); }
    void OnPlayerDisconnected() { onDisconnect(); }
    void onDisconnect() { Application.LoadLevel(Application.loadedLevel); }
    private void OnConnect()
    {
        foreach (var n in networkItems)
            n.enabled = true;
    }
    
    
    void Start()
    {
        var g = (GameObject)Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)NetworkGroup.Player);        
    }
}