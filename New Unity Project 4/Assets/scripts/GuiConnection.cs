using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System;
using System.Reflection;
[assembly: AssemblyVersion("1.0.*")]

public class GuiConnection : Base
{
    public static string gamename = "Swiborg";
    public string Nick;
    const int port = 5300;
    public string ip { get { return PlayerPrefs.GetString("ip"); } set { PlayerPrefs.SetString("ip", value); } }
    private void InitServer()
    {        
        Network.useNat = false; 
        Network.InitializeServer(32, port);
        MasterServer.RegisterHost(gamename, Application.loadedLevelName + " Version " + version, Nick + "s Game");
        LoadLevel(0);
    }
    protected override void Start()
    {
        Nick = "G" + UnityEngine.Random.Range(99,999);        
    }
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Screen.lockCursor = false;
    }
    Version version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
    protected override void OnGUI()
    {        
        
        if (GUILayout.Button("Active"))  
            Screen.lockCursor = true;
        if (Network.peerType == NetworkPeerType.Disconnected)
        {

            GUILayout.Label("ipaddress:");
            ip = GUILayout.TextField(ip);

            if (GUILayout.Button("Connect") && Nick.Length > 0)
            {                
                Network.Connect(ip, port);
            }
            GUILayout.Label("NickName:");
            Nick = GUILayout.TextField(Nick);

            if (GUILayout.Button("host") && Nick.Length > 0)
                InitServer();
            GUILayout.Window(0, CenterRect(.6f, .5f), MakeWindow, "Servers");
        }
        
    }
    void MakeWindow(int q)
    {
        GUILayout.Label("your version is: " + version);
        if (GUILayout.Button("refresh server list"))
            MasterServer.RequestHostList(gamename);
        
        HostData[] data = MasterServer.PollHostList();        
        foreach (HostData element in data)
        {
            GUILayout.BeginHorizontal();
            string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
            GUILayout.Label(name);
            GUILayout.Space(5);

            string hostInfo = "[";
            foreach (string host in element.ip)
                hostInfo = hostInfo + host + ":" + element.port + " ";
            hostInfo = hostInfo + "]";
            GUILayout.Label(hostInfo);
            GUILayout.Space(5);
            GUILayout.Label(element.comment);
            GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect"))
            {
                // Set NAT functionality based on the host information
                Network.useNat = element.useNat;
                if (Network.useNat)
                    print("Using Nat punchthrough to connect to host");
                else
                    print("Connecting directly to host");
                ip = element.ip[0];
                Network.Connect(element.ip, port);
            }
            GUILayout.EndHorizontal();

        }
    }
    private Rect CenterRect(float w, float h)
    {
        
        Vector2 c = new Vector2(Screen.width, Screen.height);
        Vector2 v = new Vector2(w * c.x, h * c.y);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }

    private int lastLevelPrefix = 0;
    
    protected override void OnApplicationQuit()
    {
        enabled = false;
        
    }


    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        myNetworkView.RPC("LoadLevel", RPCMode.All, lastLevelPrefix + 1);
    }
    [RPC]
    private void LoadLevel(int levelPrefix)
    {
        //lastLevelPrefix = levelPrefix;
        //Network.SetSendingEnabled(0, false);	
        Network.isMessageQueueRunning = false;
        //Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(Application.loadedLevel);                
    }
    protected override void OnLevelWasLoaded(int level)
    {
        Network.isMessageQueueRunning = true;
        //Network.SetSendingEnabled(0, true);
    }
}
public class Trace : UnityEngine.Debug { }
//protected override void OnDisconnectedFromServer()
//{
//    if (enabled)
//        Application.LoadLevel(Application.loadedLevel);
//}