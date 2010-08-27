using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;

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
    Rect ConsoleRect;
    public Transform root;
    public string[] supportedNetworkLevels;

    float cmx { get { return Screen.height / 2; } }
    void onLog(string condition, string stackTrace, LogType type)
    {
        StreamWriter a;
        using (a = new StreamWriter(File.Open("log.txt", FileMode.Append, FileAccess.Write)))
            a.WriteLine("fps:" + fps + "Type:" + type + "\r\n" + condition + "\r\n" + stackTrace + "\r\n");
    }
    void OnLevelWasLoaded(int level)
    {
        win = false;
        old = selectedTeam = 0;
        lockCursor = false;
        _TimerA.Clear();
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }

    void Awake()
    {
        if (GameObject.FindObjectsOfType(typeof(Loader)).Length == 2)
        {
            Destroy(this.gameObject);
            return;
        }
        File.Delete("log.txt");
        Application.RegisterLogCallback(onLog);
        print("start");
        _Loader = this;
        DontDestroyOnLoad(this);
        networkView.group = 1;

        ConsoleRect = CenterRect(.8f, .6f);
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
        if (lockCursor) return;
        ConsoleRect = GUILayout.Window(0, ConsoleRect, ConsoleWindow, "Console");
        if (options) optionsrect = GUILayout.Window(5, optionsrect, OptionsWindow, "Options");

    }
    Rect optionsrect = new Rect(0, 0, 300, 300);

    public int selectedTeam = 0;

    internal int quality = 3;
    int oldquality = 3;
    private void OptionsWindow(int id)
    {

        string[] qs = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
        if (oldquality != (quality = GUILayout.Toolbar(quality, qs)))
        {
            oldquality = quality;            
            QualitySettings.currentLevel = (QualityLevel)quality;
            print("Set Quality" + QualitySettings.currentLevel);
        }

        if (GUILayout.Button("close")) options = false;
        GUI.DragWindow();
    }
    GuiConnection _GuiConnection { get { return GuiConnection._This; } }
    public enum GameMode { DeathMatch, TeamDeathMatch, TeamZombieSurvive }
    internal GameMode gameMode = GameMode.TeamZombieSurvive;
    bool win;
    internal int fraglimit = 20;
    private void ConsoleWindow(int id)
    {
        if (Application.loadedLevelName == disconnectedLevel)
        {
            if (Network.isServer)
            {
                foreach (string level in supportedNetworkLevels)
                    if (GUILayout.Button(level))
                    {
                        SetGameMode((int)gameMode,fraglimit);
                        LoadLevelRPC(level);
                    }
                gameMode = (GameMode)GUILayout.Toolbar((int)gameMode, new string[] { "Death Match", "Team DeathMatch", "Team Zombie Survive" });
                GUILayout.BeginHorizontal();
                GUILayout.Label("Zombie/Frag limit:", GUILayout.ExpandWidth(false));
                int.TryParse(GUILayout.TextField(fraglimit.ToString(), 2, GUILayout.Width(60)), out fraglimit);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(20);
        if (_Spawn != null)
        {
            GUILayout.Label("Frags Limit:" + fraglimit);
            if (dm)
            {
                foreach (Player pl in players.Values)
                    if (pl.OwnerID != -1)
                    {
                        PrintPlayer(pl);
                        if (Network.isServer && !win &&  pl.frags >= fraglimit)
                        {
                            rpcwrite(pl.Nick + " Win");
                            win = true;
                            _TimerA.AddMethod(5000,WinGame);
                        }
                    }
            }
            else
            {
                int rs = 0, bs = 0;

                foreach (Player pl in players.Values)
                    if (pl.team == Team.ata && pl.OwnerID != -1)
                    {
                        PrintPlayer(pl);
                        rs += pl.frags;
                    }
                GUILayout.Label("Red Team Score:" + rs);
                foreach (Player pl in players.Values)
                    if (pl.team == Team.def && pl.OwnerID != -1)
                    {
                        PrintPlayer(pl);
                        bs += pl.frags;
                    }
                GUILayout.Label("Blue Team Score:" + bs);
                if (zombi)
                {
                    if (Network.isServer && !win && _TimerA.TimeElapsed(1000))
                    {
                        bool BlueteamLive = false, RedteamLive = false;                        
                        int rcount = 0, bcount = 0;                        
                        foreach (Player p in TP(Team.def))
                        {
                            rcount++;
                            if (!p.dead) BlueteamLive = true;
                        }
                        foreach (Player p in TP(Team.ata))
                        {
                            bcount++;
                            if (!p.dead) RedteamLive = true;
                        }

                        if (rcount > 0 && bcount > 0 && (!RedteamLive || !BlueteamLive))
                        {
                            rpcwrite((!RedteamLive ? "Blue" : "Red") + " Team Win");
                            win = true;
                            _TimerA.AddMethod(5000, WinGame);
                        }
                    }

                }
                else if (Network.isServer && !win && (bs == fraglimit || rs == fraglimit))
                {
                    rpcwrite((bs > rs ? "Blue" : "Red") + " Team Win");
                    win = true;
                    _TimerA.AddMethod(5000, WinGame);
                }
                
            }
        }
        
        GUILayout.BeginHorizontal();
        if (Network.peerType != NetworkPeerType.Disconnected && GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
        {
            if (Network.isServer && _Spawn != null)
                LoadLevelRPC(disconnectedLevel);
            else
                Network.Disconnect();
        }
        if (GUILayout.Button("Options"))
        {            
            options = true;
        }
        GUILayout.EndHorizontal();
        if (_Spawn != null)
        {
            string[] arr = dm ? new string[] { "Spectator", "Join Game" } : new string[] { "Spectator", "Red Team", "Blue Team" };
            if ((selectedTeam = GUILayout.Toolbar(selectedTeam, arr)) != old)
            {
                old = selectedTeam;
                if (selectedTeam == 0) _Spawn.Spectator();
                if (selectedTeam == 1) _Spawn.OnTeamSelect(Team.ata);
                if (selectedTeam == 2) _Spawn.OnTeamSelect(Team.def);                
            }
        }

        input = GUILayout.TextField(input);
        GUILayout.TextField(output);
        GUI.DragWindow();
    }
    IEnumerable<Player> TP(Team t)
    {
        foreach (Player p in players.Values)
            if (p.team == t) yield return p;
    }
    private void WinGame()
    {
        
        LoadLevelRPC(disconnectedLevel);
    }
    [RPC]
    private void SetGameMode(int p,int frag)
    {
        CallRPC(true,p,frag);
        gameMode = (GameMode)p;
        fraglimit = frag;
    }

    private void PrintPlayer(Player pl)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(pl.Nick + "                                      Kills:" + pl.frags + "               Ping:" + pl.ping + "               Fps:" + pl.fps);
        if (Network.isServer && pl.networkView.owner != Network.player && GUILayout.Button("Kick"))
        {
            rpcwrite(pl.Nick + " kicked");
            Network.CloseConnection(pl.networkView.owner, true);
        }
        GUILayout.EndHorizontal();
    }
    bool options;
    int old = 0;
    protected override void OnConsole(string s)
    {
        rpcwrite(GuiConnection.Nick + ": " + s);
    }
    public void LoadLevelRPC(string level)
    {
        for (int i = 0; i < 20; i++)
            Network.RemoveRPCsInGroup(i);
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
        if (autostart) _Loader.LoadLevelRPC(supportedNetworkLevels[0]);
    }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        rpcwrite("Player disconnected " + GuiConnection.Nick);
        write("Disconnected from game:"+info);
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
    
    public static void write(string s)
    {
        UnityEngine.Debug.Log(s);
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
tab - close/open console
f - go in/out car
shift - nitro
a,s,d,w move keys
1 - machinegun 
2 - rocketlauncher
3 - physxgun
4 - healthgun
";
}

