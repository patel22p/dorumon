using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;

public enum GameMode { DeathMatch, TeamDeathMatch, TeamZombieSurvive, ZombieSurive }
public partial class  ConsoleWindow : WindowBase
{    
    
    void Start()
    {         
        _cw = this;                
    }
    
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
                rpcwrite(Menu.Nick + ": " + input);
                input = "";
            }
        }

    }
    static string lastStr = "";
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
    internal GameMode gameMode = GameMode.ZombieSurive;
    protected override void OnGUI()
    {
        
        if (lockCursor) return;
        try
        {
            GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
            rect = GUILayout.Window(id, rect, Window, lc.physxwarsver + version, GUILayout.Width(700), GUILayout.Height(400));
            
        }
        catch (Exception e) { print(e); }
        base.OnGUI();
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

        InLabby();        
        InGame();

        if (!skip)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Options", GUILayout.ExpandWidth(false))) _options.enabled = true;
            if (Network.peerType != NetworkPeerType.Disconnected && GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
            {
                if (Network.isServer && _Spawn != null)
                    _Loader.RPCLoadLevel(Level.z2menu.ToString());
                else
                    Network.Disconnect();
            }
            GUILayout.EndHorizontal();

            input = GUILayout.TextField(input);
            
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextField(output);
        GUILayout.EndScrollView(); 
        
        GUI.DragWindow();
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        userviews.Clear();        
        RPCUserDisconnected(Network.player.GetHashCode());
        if(!skip) rpcwrite("Player disconnected " + Menu.Nick);
        if (!skip) write("Disconnected from game:" + info);
        Application.LoadLevel(Level.z2menu.ToString());
    }
    [RPC]
    void RPCUserDisconnected(int id)
    {
        CallRPC(true, id);
        userviews.Remove(id);
    }    
    

    [RPC]
    void RPCPingFps(int id,int ping, int fps,float loaded)
    {
        CallRPC(true, id, ping, fps,loaded);
        userviews[id].fps = fps;
        userviews[id].ping = ping;
        userviews[id].loaded = loaded;
    }


    private void InGame()
    {
        
        if (_Level == Level.z4game)
        {
            if (skip)
            {
                GUILayout.Label("Copyright © 2010 Levochkin Igor");
                if (GUILayout.Button("Начать игру"))
                {
                    _Spawn.OnTeamSelect(Team.ata);
                }
            }
            else
            {
                GUILayout.Label("Frags Limit:" + fraglimit);
                if (dm || zombisurive)
                {
                    foreach (Vk.user pl in userviews.Values)
                        PrintPlayer(pl);
                }
                else
                {
                    GUILayout.Label("Red Team Score:" + _Spawn.RedFrags);
                    foreach (Vk.user pl in TP(Team.ata))
                        PrintPlayer(pl);
                    GUILayout.Label("Blue Team Score:" + _Spawn.BlueFrags);
                    foreach (Vk.user pl in TP(Team.def))
                        PrintPlayer(pl);
                }

                string[] arr = dm || zombisurive ? new string[] { "Spectator", "Join Game" } : new string[] { "Spectator", "Red Team", "Blue Team" };
                if ((selectedTeam = GUILayout.Toolbar(selectedTeam, arr)) != old)
                {
                    old = selectedTeam;
                    if (selectedTeam == 0) _Spawn.Spectator();
                    if (selectedTeam == 1) _Spawn.OnTeamSelect(Team.ata);
                    if (selectedTeam == 2) _Spawn.OnTeamSelect(Team.def);
                }
            }
        }
    }


    void OnServerInitialized()
    {

        OnConnected();
    }
    void OnConnectedToServer()
    {
        OnConnected();
    }
    private void OnConnected()
    {        
        if(!skip) rpcwrite("Player joined " + Menu.Nick);
        localuser.nwid = Network.player;
        RPCSetUserView(localuser.nwid,localuser.nick, localuser.uid, localuser.photo,localuser.totalkills,localuser.totaldeaths,localuser.totalzombiekills,localuser.totalzombiedeaths);
        userviews.Add(localuser.nwid.GetHashCode(), localuser);
        Application.LoadLevel(Level.z3labby.ToString());
    }

    [RPC]
    private void RPCSetUserView(NetworkPlayer nwid, string nick, int uid, string photo, int tk, int td, int tzk, int tzd)
    {

        CallRPC(true, localuser.nwid, localuser.nick, localuser.uid, localuser.photo, tk, td, tzk, tzd);
        if (nwid == Network.player) return;

        Vk.user user = new Vk.user();
        user.nick = nick;
        user.uid = uid;
        user.photo = photo;
        user.nwid = nwid;
        user.totalkills = tk;
        user.totaldeaths = td;
        user.totalzombiekills = tzk;
        user.totalzombiedeaths = tzd;

        if (photo != "")
            new WWW2(photo).done += delegate(WWW2 www)
            {
                print("loaded texture");
                user.texture = www.texture;
                DontDestroyOnLoad(user.texture);
            };
        userviews.Add(nwid.GetHashCode(), user);

    }

    private void InLabby()
    {
        
        if (_Level == Level.z3labby && Network.isServer)
        {
            if (GUILayout.Button("Start Game") || skip)
            {
                bool loaded = true;
                foreach (Vk.user u in userviews.Values)
                    if (u.loaded != 1)
                    {
                        if(!skip) printC("not all users loaded map");
                        loaded = false;
                    }
                if (loaded)
                {
                    SetGameMode((int)gameMode, fraglimit);
                    _Loader.RPCLoadLevel(Level.z4game.ToString());
                }
            }
            gameMode = (GameMode)GUILayout.Toolbar((int)gameMode, new string[] { "Death Match", "Team DeathMatch", "Team Zombie Survive", "Zombie Survive" });

            GUILayout.BeginHorizontal();
            GUILayout.Label("Zombie/Frag limit:", GUILayout.ExpandWidth(false));
            int.TryParse(GUILayout.TextField(fraglimit.ToString(), 2, GUILayout.Width(60)), out fraglimit);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }
        if (_Level == Level.z3labby)
        {

            if (_TimerA.TimeElapsed(1000))
                RPCPingFps(Network.player.GetHashCode(),
                    (Network.connections.Length > 0 ? Network.GetLastPing(Network.connections[0]) : 0),
                    _Loader.fps,
                    (isWebPlayer ? Application.GetStreamProgressForLevel(Level.z4game.ToString() ) : 1));
                        
            const string table = "{0,15}{1,10}{2,10}{3,10}{4,10}{5,10}{6,10}{7,10}";
            GUILayout.Label(String.Format(table, "", "ZKills", "ZDeaths", "kills", "deaths", "Ping", "Fps", "Loaded"));

            foreach (Vk.user user in userviews.Values)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(String.Format(table, user.nick, user.totalzombiekills, user.totalzombiedeaths, user.totalkills, user.totaldeaths, user.ping, user.fps, (int)(user.loaded * 100)));
                AddKickButton(user);
                GUILayout.EndHorizontal();
            }
        }

    }
    Vector2 scrollPosition;
    int selectedTeam;
    public static string input = "";
    private void PrintPlayer(Vk.user user)
    {
        const string table = "{0,10}{1,20}{2,10}{3,10}{4,10}";
        GUILayout.Label(String.Format(table, "", "Kills", "Ping", "Fps", "deaths"));
        GUILayout.BeginHorizontal();        
        GUILayout.Label(String.Format(table, user.nick, user.frags, user.ping, user.fps, user.deaths));
        AddKickButton(user);
        GUILayout.EndHorizontal();
    }

    private void AddKickButton(Vk.user user)
    {
        if (Network.isServer && user.nwid != Network.player && GUILayout.Button("Kick"))
        {
            rpcwrite(user.nick + " kicked");
            Network.CloseConnection(user.nwid, true);
            RPCUserDisconnected(user.nwid.GetHashCode());
        }
        GUILayout.Label(user.texture, GUILayout.Width(50), GUILayout.Height(40));
    }

    public static string output = @"
Задача игры - уничтожать зомби

WASD - движение,
ПРОБЕЛ - тормоз, 
SHIFT - ускорение, 
F - использование техники, 
цифры 1-3 - выбор оружия, 
мышь - обзор, 
ЛКМ - огонь, 
ПКМ - меню.
";
//alt enter - fullscreen
//tab - close/open console
//f - go in/out car
//shift - nitro
//a,s,d,w move keys
//1 - machinegun 
//2 - rocketlauncher
//3 - physxgun
//4 - healthgun
//";

}
