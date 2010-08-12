using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using doru;

public class Loader : Base
{
    public static bool Online;
    void Start()
    {
        center = CenterRect(.8f, .6f);
        
    }
    public String disconnectedLevel;
    public static int lastLevelPrefix = 0;

    void Awake()
    {
        DontDestroyOnLoad(this);
        networkView.group = 1;
        if (Application.loadedLevel == 0)
        {
            Application.LoadLevel(disconnectedLevel);
            Online = true;
        }
    }
    public void LoadLevelRPC(string level)
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }
    protected override void OnServerInitialized()
    {
        rpcwrite("Player joined " + GuiConnection.Nick);
    }
    
    [RPC]
    public void rpcwrite(string s)
    {
        CallRPC(true, s);
        write(s);
    }
    public bool autostart;
    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        if (autostart) Find<Loader>().LoadLevelRPC(supportedNetworkLevels[0]);
    }
    protected override void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        rpcwrite("Player disconnected " + GuiConnection.Nick);
        Application.LoadLevel(disconnectedLevel);
    }
    protected override void OnConnectedToServer()
    {
        rpcwrite("Player joined " + GuiConnection.Nick);
    }
    [RPC]
    void LoadLevel(String level, int levelPrefix)
    {


        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);

    }
    protected override void OnLevelWasLoaded(int level)
    {
        // Allow receiving data again
        Network.isMessageQueueRunning = true;
        // Now the level has been loaded and we can start sending out data to clients
        Network.SetSendingEnabled(0, true);

        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
    }
    protected override void OnConsole(string s)
    {
        rpcwrite(GuiConnection.Nick + ": " + s);
    }
    protected void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (input != "")
            {
                foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
                    go.SendMessage("OnConsole", input, SendMessageOptions.DontRequireReceiver);
                input = "";
            }
        }


    }
    static string lastStr;
    public static void write(string s)
    {
        lastStr = s;
        output = s + "\r\n" + output;
    }

    public GUISkin guiskin;
    public static string input = "";

    Rect center = CenterRect(.5f, .6f);

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
        if (Screen.lockCursor) return;
        center = GUILayout.Window(0, center, ConsoleWindow, "Console");
    }


    public string[] supportedNetworkLevels = new string[] { "zredline", "zmyl", "zisland" };
    public int gametime = 5;    
    private void ConsoleWindow(int id)
    {
        if (Application.loadedLevelName == disconnectedLevel && Network.isServer)
            foreach (string level in supportedNetworkLevels)
            {
                if (GUILayout.Button(level))
                    Find<Loader>().LoadLevelRPC(level);
            }

        if (spawn != null)
        {
            TimeSpan ts = TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
            GUILayout.Label("Time Left:" + ts.ToString().Split('.')[0]);
            GUILayout.Label("Base Life:" + Find<Tower>().Life);

            if (ts.Milliseconds < 0)
            {
                write("team " + Team.def + " win");                
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("Team " + Team.def);
        foreach (Player a in FindObjectsOfType(typeof(Player)))
            if (a.team == Team.def)
                GUILayout.Label(a.Nick + "                                      Kills:" + a.score + "               Ping:" + Network.GetLastPing(a.OwnerID.Value));
        GUILayout.Label("Team " + Team.ata);
        foreach (Player a in FindObjectsOfType(typeof(Player)))
            if (a.team == Team.ata)
                GUILayout.Label(a.Nick + "                                      Kills:" + a.score + "               Ping:" + Network.GetLastPing(a.OwnerID.Value));


        input = GUILayout.TextField(input);
        output = GUILayout.TextField(output);
        GUI.DragWindow();
    }

    float cmx { get { return Screen.height / 2; } }
    public Rect dockup()
    {
        Vector2 c = new Vector2(Screen.width, Screen.height);
        return new Rect(0, 0, c.x, c.y - cmx);
    }
    public Rect dockdown()
    {
        Vector2 c = new Vector2(Screen.width, Screen.height);
        return new Rect(0, c.y - cmx, c.x, c.y);
    }
    public static string output = @"
alt enter - fullscreen
Escape - close/open console
tab - scoreboard
f - go in/out car
a,s,d,w move keys
1 - machinegun 
2 - rocketlauncher

";
}

//GUILayout.Label("isMine:" + a.networkView.isMine + "  Owner:" + a.OwnerID.ToString() + "     Nick:" + a.Nick + " Score:" + a.score +
//    "     Life:" + a.Life + "    Ping:" + Network.GetLastPing(a.OwnerID.Value) +
//    "   IPAddress:" + a.OwnerID.Value.ipAddress + "port:" + a.OwnerID.Value.port);