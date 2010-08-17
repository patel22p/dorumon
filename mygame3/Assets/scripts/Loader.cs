using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;

public class Loader : Base
{
    public static bool Online;
    public String disconnectedLevel;
    public static int lastLevelPrefix = 0;
    public bool autostart;
    public int fps;
    public GUISkin guiskin;
    static string lastStr;
    public static string input = "";
    Rect center;
    public Transform root;
    public string[] supportedNetworkLevels;
    public int gametime = 5;
    float cmx { get { return Screen.height / 2; } }
    void Awake()
    {


        if (GameObject.FindObjectsOfType(typeof(Loader)).Length == 2)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        networkView.group = 1;

        center = CenterRect(.8f, .6f);
        if (Application.loadedLevel == 0)
        {
            Application.LoadLevel(disconnectedLevel);
            Online = true;
        }
    }

    void Update()
    {

        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        _TimerA.Update();
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
    void OnGUI()
    {
        GUILayout.Label("fps: " + fps);

        GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
        if (Screen.lockCursor) return;
        center = GUILayout.Window(0, center, ConsoleWindow, "Console");
    }

    public int GameTime = 10;
    private void ConsoleWindow(int id)
    {
        if (Application.loadedLevelName == disconnectedLevel && Network.isServer)
            foreach (string level in supportedNetworkLevels)
            {


                GUILayout.BeginHorizontal();
                GUILayout.Label("Game Time Minutes:");
                int.TryParse(GUILayout.TextField(GameTime.ToString()), out GameTime);
                GUILayout.EndHorizontal();

                if (GUILayout.Button(level))
                    LoadLevelRPC(level);

            }

        if (_Spawn != null)
        {
            TimeSpan ts = TimeSpan.FromMinutes(gametime) - TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
            GUILayout.Label("Time Left:" + ts.ToString().Split('.')[0]);

            if (ts.Milliseconds < 0)
            {
                write("team " + "Blue Team" + " win");
                _Loader.LoadLevelRPC(_Loader.disconnectedLevel);
                Screen.lockCursor = false;
            }
        }


        GUILayout.Space(20);
        GUILayout.Label("Team " + "Blue Team");
        foreach (Player pl in FindObjectsOfType(typeof(Player)))
            if (pl.team == Team.def && pl.OwnerID != null)
                GUILayout.Label(pl.Nick + "                                      Kills:" + pl.frags + "               Ping:" + pl.ping + "               Fps:" + pl.fps);
        GUILayout.Label("Team " + "Red Team");
        foreach (Player pl in FindObjectsOfType(typeof(Player)))
            if (pl.team == Team.ata && pl.OwnerID != null)
                GUILayout.Label(pl.Nick + "                                      Kills:" + pl.frags + "               Ping:" + pl.ping + "               Fps:" + pl.fps);
        
        GUILayout.BeginHorizontal();
        if (Network.peerType != NetworkPeerType.Disconnected && GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
        {
            Network.Disconnect();
        }
        bool ata ,def;
        if (Find<Spawn>() != null)
            if ((ata = GUILayout.Button("Red Team")) || (def = GUILayout.Button("Blue Team")))
                Find<Spawn>().OnTeamSelect(ata ? Team.ata : Team.def);

        GUILayout.EndHorizontal();
        input = GUILayout.TextField(input);
        output = GUILayout.TextField(output);


    }
    protected override void OnConsole(string s)
    {
        rpcwrite(GuiConnection.Nick + ": " + s);
    }
    public void LoadLevelRPC(string level)
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }
    void OnServerInitialized()
    {
        rpcwrite("Player joined " + GuiConnection.Nick);
    }
    [RPC]
    public void rpcwrite(string s)
    {
        CallRPC(true, s);
        write(s);
    }
    void OnPlayerConnected(NetworkPlayer player)
    {
        if (autostart) Find<Loader>().LoadLevelRPC(supportedNetworkLevels[0]);
    }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        rpcwrite("Player disconnected " + GuiConnection.Nick);
        write("Disconnected from game");
        Application.LoadLevel(disconnectedLevel);
    }
    void OnConnectedToServer()
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
    void OnLevelWasLoaded(int level)
    {
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }
    public static void write(string s)
    {
        lastStr = s;
        output = s + "\r\n" + output;
    }
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

