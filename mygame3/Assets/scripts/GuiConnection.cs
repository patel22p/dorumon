using UnityEngine;
using System.Collections;
using System.Diagnostics;

using System;
using System.Reflection;
[assembly: AssemblyVersion("1.0.*")]

public class GuiConnection : Base2
{
    
    public static string gamename = "Swiborg";        
    bool guiloaded;
    public Rect r = new Rect(0, 0, 200, 300);
    public Rect r2;
    internal int port = 5300;
    public static string Nick = "Guest " + UnityEngine.Random.Range(0, 99);
    public string ip { get { return PlayerPrefs.GetString("ip"); } set { PlayerPrefs.SetString("ip", value); } }
    public string ip2 { get { return PlayerPrefs.GetString("ip2"); } set { PlayerPrefs.SetString("ip2", value); } }    
    Version version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
    
    private void InitServer()
    {        
        
        Network.useNat = false; 
        Network.InitializeServer(32, port);
        if (ip2 != "") MasterServer.ipAddress = ip2;
        MasterServer.RegisterHost(gamename, Application.loadedLevelName + " Version " + version, Nick + "s Game");
        
    }
    void Start()
    {
        
        if (_Loader.autostart)
        {
            Network.Connect(ip, port);            
        }
        r2 = CenterRect(.6f, .5f);
        Network.incomingPassword = "";        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Screen.lockCursor = false;
    }            
    void OnGUI()
    {
        
        //GUI.skin = Find<Loader>().guiskin;
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            r = GUILayout.Window(2, r, ConnWindow, "connection");
            r2 = GUILayout.Window(1, r2 , ServerListWindow, "Servers");
            if (!guiloaded)
            {
                GUI.BringWindowToFront(2);
                GUI.BringWindowToFront(1);
                guiloaded = true;
            }                    
        }        

        //GUILayout.BeginArea(new Rect(0, Screen.height - 30, Screen.width, 30));
        //GUILayout.BeginHorizontal();


        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();
        //GUILayout.EndArea();

    }
    public void ConnWindow(int id)
    {
        

            GUILayout.Label("ipaddress:   port:5300");
            GUILayout.BeginHorizontal();    
            ip = GUILayout.TextField(ip);
            
            int.TryParse(GUILayout.TextField(port.ToString()), out port);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Connect"))
            {
                if (Nick.Length > 0)
                {
                    Network.Connect(ip, port);                    
                }
                else
                    print("Enter username first");
            }
            GUILayout.Label("MasterServer:");
            ip2 = GUILayout.TextField(ip2);
            GUILayout.Label("NickName:");
            Nick = GUILayout.TextField(Nick);

            if (GUILayout.Button("host"))
                if (Nick.Length > 0)
                    InitServer();
                else print("Enter Nick Name");
            GUI.DragWindow();                     

    }
    void ServerListWindow(int q)
    {
        GUILayout.Label("your version is: " + version);
        if (GUILayout.Button("refresh server list"))
        {
            if (ip2 != "") MasterServer.ipAddress = ip2;
            MasterServer.RequestHostList(gamename);
        }

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
            if (GUILayout.Button("Copy IP"))
            {
                Network.useNat = element.useNat;
                //if (Network.useNat)
                //    print("Using Nat punchthrough to connect to host");
                //else
                //     print("Connecting directly to host");
                ip = element.ip[0];
                port = element.port;

            }
            GUILayout.EndHorizontal();

        }
        GUI.DragWindow();
    }
    void OnFailedToConnect(NetworkConnectionError error)
    {
        print("Could not connect to server: " + error);
    }
    void OnFailedToConnectToMasterServer(NetworkConnectionError error)
    {
        print("Could not connect to master server: " + error);
    }
    
    
}
public class Trace : UnityEngine.Debug { }
