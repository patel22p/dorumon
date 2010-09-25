using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;


public partial class z0ConsoleWindow : WindowBase
{
    void Start()
    {      
        _cw = this;
        AudioListener.volume = .1f;       
        size = new Vector2(700, 400);
        music = GetComponent<Music>();
        music.Start("loops.txt");
        audio.loop = true;
    }
    public Music music;
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
                if (_Level == Level.z2menu && _vk._Status == z0Vk.Status.connected)
                    _vk.SendChatMsg(input);
                else if (_Level == Level.z3labby || _Level == Level.z4game)
                    rpcwrite(z2Menu.Nick + ": " + input);
                else
                    printC(lc.chatdisabled);
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
    
    
    protected override void OnGUI()
    {
        if (lockCursor) return;
        GUI.Label(new Rect(Screen.width - 200, 0, Screen.width, 20), lastStr);
        title = lc.physxwarsver + " " + version + " /  " + Application.streamedBytes / 1024 / 1024 + " " + lc.loaded;        
        
        base.OnGUI();
    }



    protected override void Window(int id)
    {
        if (!build && GUILayout.Button("skip"))
            skip = true;
        InLabby();        
        InGame();

        if (!skip)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(lc.options.ToString(), GUILayout.ExpandWidth(false))) _options.enabled = true;
            if (Network.peerType != NetworkPeerType.Disconnected && GUILayout.Button(lc.disc.ToString(), GUILayout.ExpandWidth(false)))
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
        rpcwrite(lc.playerdisc + z2Menu.Nick);
        write(lc.dfg.ToString());
        MasterServer.UnregisterHost();
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
            //if (skip)
            //{
            //    GUILayout.Label(lc.copyright .ToString());
            //    if (GUILayout.Button(lc.beginthegame.ToString()))
            //    {
            //        _Spawn.OnTeamSelect(Team.ata);
            //    }
            //}
            //else
            {
                GUILayout.Label(lc.fraglimit .ToString() + _hw.fraglimit);
                if (dm || zombisurive)
                {
                    foreach (z0Vk.user pl in userviews.Values)
                        PrintPlayer(pl);
                }
                else
                {
                    GUILayout.Label(lc.redteamscore .ToString() + _Spawn.RedFrags);
                    foreach (z0Vk.user pl in TP(Team.ata))
                        PrintPlayer(pl);
                    GUILayout.Label(lc.blueteamscore .ToString() + _Spawn.BlueFrags);
                    foreach (z0Vk.user pl in TP(Team.def))
                        PrintPlayer(pl);
                }

                string[] arr = dm || zombisurive ? new string[] { lc.spectator .ToString(), lc.joingame .ToString() } : new string[] { lc.spectator .ToString(), lc.redteam .ToString(), lc.blueteam .ToString() };
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
        RPCServerData(version.ToString(),(int)_hw.gameMode,(int)_hw.selectedlevel,_hw.fraglimit);
        OnConnected();
    }
    void OnConnectedToServer()
    {
        OnConnected();
    }
    private void OnConnected()
    {
        
        print("on connected");
        if(!skip) rpcwrite(lc.plj + z2Menu.Nick);
        localuser.nwid = Network.player;
        RPCSetUserView(localuser.nwid,localuser.nick, localuser.uid, localuser.photo,localuser.totalkills,localuser.totaldeaths,localuser.totalzombiekills,localuser.totalzombiedeaths);
        userviews.Add(localuser.nwid.GetHashCode(), localuser);
        Application.LoadLevel(Level.z3labby.ToString());
    }
    [RPC]
    private void RPCServerData(string v, int gamemode, int level, int frags)
    {
        CallRPC(true, v, gamemode, level, frags);
        if (v != version.ToString())
        {
            printC(string.Format(lc.WrongVersion.ToString(), v, version.ToString()));
            Network.Disconnect();
        }
        _hw.gameMode = (GameMode)gamemode;
        _hw.selectedlevel = (GameLevels)level;
        _hw.fraglimit = frags;
        
    }
    [RPC]
    private void RPCSetUserView(NetworkPlayer nwid, string nick, int uid, string photo, int tk, int td, int tzk, int tzd)
    {

        CallRPC(true, localuser.nwid, localuser.nick, localuser.uid, localuser.photo, tk, td, tzk, tzd);
        if (nwid == Network.player) return;

        z0Vk.user user = new z0Vk.user();
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
            if (GUILayout.Button(lc.startgame.ToString()) || skip)
            {
                bool loaded = true;
                foreach (z0Vk.user u in userviews.Values)
                    if (u.loaded != 1)
                    {
                        if(!skip) printC(lc.notall .ToString());
                        loaded = false;
                    }
                if (loaded)
                    _Loader.RPCLoadLevel(_hw.selectedlevel.ToString());
            }
            

            
            GUILayout.Space(20);
        }
        if (_Level == Level.z3labby)
        {

            if (_TimerA.TimeElapsed(1000))
                RPCPingFps(Network.player.GetHashCode(),
                    (Network.connections.Length > 0 ? Network.GetLastPing(Network.connections[0]) : 0),
                    _Loader.fps,
                    (isWebPlayer ? Application.GetStreamProgressForLevel(_hw.selectedlevel.ToString()) : 1));
                        
            const string table = "{0,15}{1,10}{2,10}{3,10}{4,10}{5,10}{6,10}{7,10}";
            GUILayout.Label(String.Format(table, "", lc.zkills, lc.zdeaths, lc.kills, lc.deaths, lc.ping, lc.fps, lc.maploaded));

            foreach (z0Vk.user user in userviews.Values)
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
    private void PrintPlayer(z0Vk.user user)
    {
        const string table = "{0,15}{1,10}{2,10}{3,10}{4,10}";
        GUILayout.Label(String.Format(table, "", lc.kills, lc.ping, lc.fps, lc.deaths));
        GUILayout.BeginHorizontal();        
        GUILayout.Label(String.Format(table, user.nick, user.frags, user.ping, user.fps, user.deaths));
        AddKickButton(user);
        GUILayout.EndHorizontal();
    }

    private void AddKickButton(z0Vk.user user)
    {
        GUILayout.Label(user.texture, GUILayout.Width(50), GUILayout.Height(40));
        if (Network.isServer && user.nwid != Network.player && GUILayout.Button(lc.kick.ToString()))
        {
            rpcwrite(user.nick + lc.kicked);
            Network.CloseConnection(user.nwid, true);
            RPCUserDisconnected(user.nwid.GetHashCode());
        }        
    }

    public static string output = @"";


}
