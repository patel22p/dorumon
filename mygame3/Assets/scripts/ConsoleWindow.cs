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
    internal GameMode gameMode = GameMode.TeamZombieSurvive;
    void OnGUI()
    {    
        if (lockCursor) return;
        try
        {
            GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
            ConsoleRect = GUILayout.Window(0, ConsoleRect, Window, "Console");
        }
        catch (Exception e) { print(e); }
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
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextField(output);
        GUILayout.EndScrollView(); 
        
        GUI.DragWindow();
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        userviews.Clear();        
        RPCUserDisconnected(Network.player.GetHashCode());
        rpcwrite("Player disconnected " + Menu.Nick);
        write("Disconnected from game:" + info);
        Application.LoadLevel(Level.z2menu.ToString());
    }
    [RPC]
    void RPCUserDisconnected(int id)
    {
        CallRPC(true, id);
        userviews.Remove(id);
    }    
    

    [RPC]
    void RPCPingFps(int id,int ping, int fps)
    {
        CallRPC(true, id, ping, fps);
        userviews[id].fps = fps;
        userviews[id].ping = ping;        
    }


    private void InGame()
    {
        
        if (_Level == Level.z4game)
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
        rpcwrite("Player joined " + Menu.Nick);
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
            if (GUILayout.Button("Start Game"))
            {
                SetGameMode((int)gameMode, fraglimit);
                _Loader.RPCLoadLevel(Level.z4game.ToString());
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

            if (_TimerA.TimeElapsed(1000) && Network.connections.Length > 0)
                RPCPingFps(Network.player.GetHashCode(), Network.GetLastPing(Network.connections[0]), _Loader.fps);

            const string table = "{0,30}{1,20}{2,10}{3,10}{4,10}{5,10}{6,10}";
            GUILayout.Label(String.Format(table, "", "ZombieKills", "ZDeaths", "kills", "deaths", "Ping", "Fps"), _Loader._GUIStyle);

            foreach (Vk.user user in userviews.Values)
            {
                GUILayout.BeginHorizontal();

                //GUILayout.Label(String.Format(table,       "",     "ZombieKills",         "ZDeaths",    "kills",    "deaths",    "Ping",    "Fps"), _Loader._GUIStyle);
                GUILayout.Label(String.Format(table, user.nick, user.totalzombiekills, user.totalzombiedeaths, user.totalkills, user.totaldeaths, user.ping, user.fps), _Loader._GUIStyle);
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
        GUILayout.Label(String.Format(table, "", "Kills", "Ping", "Fps", "deaths"), _Loader._GUIStyle);
        GUILayout.BeginHorizontal();        
        GUILayout.Label(String.Format(table, user.nick, user.frags, user.ping, user.fps, user.deaths), _Loader._GUIStyle);
        AddKickButton(user);
        GUILayout.EndHorizontal();
    }

    private void AddKickButton(Vk.user user)
    {
        if (Network.isServer && user.nwid != Network.player && GUILayout.Button("Kick"))
        {
            rpcwrite(user.nick + " kicked");
            Network.CloseConnection(user.nwid, true);
        }
        GUILayout.Label(user.texture, GUILayout.Width(50), GUILayout.Height(40));
    }
    protected override void OnConsole(string s)
    {
        rpcwrite(Menu.Nick + ": " + s);
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
