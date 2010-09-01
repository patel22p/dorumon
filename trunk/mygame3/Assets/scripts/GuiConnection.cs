using UnityEngine;
using System.Collections;


using System;
using System.Reflection;
using System.Collections.Generic;
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
    public string masterip { get { return PlayerPrefs.GetString("ip2"); } set { PlayerPrefs.SetString("ip2", value); } }    
    Version version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
    
    private void InitServer()
    { 
        
        Network.useNat = false; 
        Network.InitializeServer(32, port);
        if (masterip != "") MasterServer.ipAddress = masterip;
        MasterServer.RegisterHost(gamename, Application.loadedLevelName + " Version " + version, Nick + "s Game");
    }
    public static GuiConnection _This;
    void Awake()
    {
        _This = this;
    }
    void Start()
    {
        r2 = CenterRect(.6f, .5f);
        Network.incomingPassword = "";// version.ToString();
        print(Network.incomingPassword);
    }
    void OnGUI()
    {
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
    }
    public void ConnWindow(int id)
    {
        

            GUILayout.Label("Ipaddress:   port:5300");
            GUILayout.BeginHorizontal();    
            ip = GUILayout.TextField(ip,20);
            
            int.TryParse(GUILayout.TextField(port.ToString(),10), out port);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Connect"))
            {
                if (Nick.Length > 0)
                {
                    Network.Connect(ip, port,Network.incomingPassword);                    
                }
                else
                    print("Enter username first");
            }
            GUILayout.Label("MasterServer:");
            masterip = GUILayout.TextField(masterip, 20);
            GUILayout.Label("NickName:");
            Nick = GUILayout.TextField(Nick,20);

            if (GUILayout.Button("host"))
                if (Nick.Length > 0)
                    InitServer();
                else print("Enter Nick Name");
            GUI.DragWindow();                     
    }
    public static Dictionary<string, Ping> hdps = new Dictionary<string, Ping>();
    int GetPing(string ip)
    {
        Ping p;
        if (!hdps.ContainsKey(ip))
            hdps.Add(ip, p = new Ping(ip));
        else
            p = hdps[ip];
        
        return p.time;
    }
    HostData[] data;
    SortedList<int, HostData> sorteddata = new SortedList<int, HostData>();
    void ServerListWindow(int q)
    {
         
        GUILayout.Label("your version is: " + version);
        if (GUILayout.Button("refresh server list"))
        {
            if (masterip != "") MasterServer.ipAddress = masterip;
            MasterServer.RequestHostList(gamename);
        }

        if (MasterServer.PollHostList() != data)
        {
            data = MasterServer.PollHostList();
            sorteddata.Clear();
            foreach (HostData d in data)
                sorteddata.Add(GetPing(d.ip[0]), d);
        }
        foreach (HostData element in sorteddata.Values) 
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
            GUILayout.Label("ping:" + GetPing(element.ip[0]));
            
            GUILayout.FlexibleSpace();
            GUILayout.Space(5);


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
    void OnLevelWasLoaded(int level)
    {
        foreach (Ping p in hdps.Values)
            p.DestroyPing();
        hdps.Clear();
    }
    void OnFailedToConnect(NetworkConnectionError error)
    {
        print("Could not connect to server: " + error.ToString().Replace("InvalidPassword", "Game Already Started"));
    }
    void OnFailedToConnectToMasterServer(NetworkConnectionError error)
    {
        print("Could not connect to master server: " + error);
    }
}
public class Trace : UnityEngine.Debug { }
