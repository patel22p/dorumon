using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;

public enum GameMode { DeathMatch, TeamDeathMatch, TeamZombieSurvive, ZombieSurive }
public partial class  ConsoleWindow : Base
{
    
    void Awake()
    {
        _cw = this;
    }

    void Start()
    {
        ConsoleRect = CenterRect(.8f, .6f);
    }
    Rect ConsoleRect;
    void OnLevelWasLoaded(int level)
    {

        old = selectedTeam = 0;
        lockCursor = false;
        _TimerA.Clear();        
    }   

    void Update()
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
    [RPC]
    public new void rpcwrite(string s)
    {
        CallRPC(true, s);
        write(s);
    }

    public new static void write(string s)
    {
        UnityEngine.Debug.Log(s);
        lastStr = s;
        output = s + "\r\n" + output;
    }

    internal int old;
    internal int fraglimit = 20;
    internal GameMode gameMode = GameMode.TeamZombieSurvive;
    void OnGUI()
    {    
        if (lockCursor) return;
        GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
        ConsoleRect = GUILayout.Window(0, ConsoleRect, Window, "Console");
    }

    [RPC]
    public void SetGameMode(int p, int frag)
    {
        CallRPC(true, p, frag);
        gameMode = (GameMode)p;
        fraglimit = frag;
    }

    private void Window(int id)
    {
        if (Application.loadedLevelName == _Loader.disconnectedLevel)
        {
            if (Network.isServer)
            {
                foreach (string level in _Loader.supportedNetworkLevels)
                    if (GUILayout.Button("Start Game: " + level))
                    {
                        SetGameMode((int)gameMode, fraglimit);
                        _Loader.LoadLevelRPC(level);
                    }
                gameMode = (GameMode)GUILayout.Toolbar((int)gameMode, new string[] { "Death Match", "Team DeathMatch", "Team Zombie Survive", "Zombie Survive" });

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
                    }
            }
            else
            {
                GUILayout.Label("Red Team Score:" + _Spawn.RedFrags);
                foreach (Player pl in TP(Team.ata))
                    PrintPlayer(pl);
                GUILayout.Label("Blue Team Score:" + _Spawn.BlueFrags);
                foreach (Player pl in TP(Team.def))
                    PrintPlayer(pl);
            }
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Options", GUILayout.ExpandWidth(false))) _options.enabled = true;
        if (Network.peerType != NetworkPeerType.Disconnected && GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
        {
            if (Network.isServer && _Spawn != null)
                _Loader.LoadLevelRPC(disconnectedLevel);
            else
                Network.Disconnect();
        }
      
        GUILayout.EndHorizontal();
        if (_Spawn != null)
        {
            string[] arr = dm || zombisurive ? new string[] { "Spectator", "Join Game" } : new string[] { "Spectator", "Red Team", "Blue Team" };
            if ((selectedTeam = GUILayout.Toolbar(selectedTeam, arr)) != old)
            {
                old = selectedTeam;
                if (selectedTeam == 0) _Spawn.Spectator();
                if (selectedTeam == 1) _Spawn.OnTeamSelect(Team.ata);
                if (selectedTeam == 2) _Spawn.OnTeamSelect(Team.def);
            }
        }
        
        input = GUILayout.TextField(input);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextField(output);
        GUILayout.EndScrollView(); 
        
        GUI.DragWindow();
    }
    Vector2 scrollPosition;
    int selectedTeam;
    public static string input = "";
    private void PrintPlayer(Player pl)
    {

        const string table = "{0,10}{1,20}{2,10}{3,10}{4,10}";
        GUILayout.Label(String.Format(table, "", "Kills", "Ping", "Fps", "Dead"), _Loader._GUIStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label(String.Format(table, pl.Nick, pl.frags, pl.ping, pl.fps, pl.dead), _Loader._GUIStyle);
        //GUILayout.Label(pl.Nick + "         Kills:{0,10}" + pl.frags + "               Ping:" + pl.ping + "               Fps:" + pl.fps + "         Dead:" + pl.dead);
        if (Network.isServer && pl.networkView.owner != Network.player && GUILayout.Button("Kick"))
        {
            rpcwrite(pl.Nick + " kicked");
            Network.CloseConnection(pl.networkView.owner, true);
        }
        GUILayout.EndHorizontal();
    }
    protected override void OnConsole(string s)
    {
        rpcwrite(GuiConnection.Nick + ": " + s);
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
