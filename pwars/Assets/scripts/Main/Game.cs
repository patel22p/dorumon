using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

public enum GameMode { ZombieSurive, TeamZombieSurvive, DeathMatch, TeamDeathMatch }


public class Game : Base  
{    
    new public Player[] players = new Player[10];
    public List<IPlayer> iplayers = new List<IPlayer>();
    public List<Zombie> zombies = new List<Zombie>();
    public List<Base> dynamic = new List<Base>();
    public GameMode gameMode { get { return mapSettings.gameMode; } set { mapSettings.gameMode = value; } }
    public new IPlayer _localiplayer;
    public new Player _localPlayer;
    public Team team = Team.None;
    public Transform effects;    
    public int stage ;
    public float timeleft = 20;
    public bool wait;
    public bool win;
    public int RedFrags = 0, BlueFrags = 0;
    public int maxzombies = 0;
    public int zombiespawnindex;
    public ParticleEmitter[] metalSparkEmiters;
    public Transform metalSpark;
    public ParticleEmitter[] impactSparkEmiters;
    public Transform impactSpark; 
    public Transform Blood;
    public ParticleEmitter[] BloodEmitors;
    public Transform decal;
    public bool enablePathFinding;
    public GameObject MapCamera;
    public bool cameraActive { get { return _Cam.camera.gameObject.active; } }
    protected override void Awake()
    {
        decal = Load("decal").transform;
        base.Awake(); 
        enabled = false;
        metalSpark = ((GameObject)Instantiate(Resources.Load("Prefabs/particle_metal"))).transform;
        metalSparkEmiters = metalSpark.GetComponentsInChildren<ParticleEmitter>();
        impactSpark = ((GameObject)Instantiate(Resources.Load("Prefabs/Impact"))).transform;
        impactSparkEmiters = impactSpark.GetComponentsInChildren<ParticleEmitter>();
        Blood = ((GameObject)Instantiate(Resources.Load("Prefabs/BloodSplatters"))).transform;
        BloodEmitors = Blood.GetComponentsInChildren<ParticleEmitter>();
        if (nick == " ") nick = "Guest " + UnityEngine.Random.Range(0, 999);        
        effects = GameObject.Find("GameEffects").transform;                
        _Level = Level.z4game;
        print(mapSettings.host);
        if (Network.peerType == NetworkPeerType.Disconnected)
            if (mapSettings.host)
                Network.InitializeServer(mapSettings.maxPlayers , mapSettings.port, false);
            else
                Network.Connect(mapSettings.ipaddress, _ServersWindow.Port);
        else
            foreach (Base o in Component.FindObjectsOfType(typeof(Base))) o.SendMessage("Enable", SendMessageOptions.DontRequireReceiver);
    }
    void OnServerInitialized() { Enable(); }
    void OnConnectedToServer() { Enable(); }
    void Enable() { enabled = true; }
    void Start()
    {
        print("ZGameStart");
        _Music.Start("music.txt");        
        //_vk.enabled = false;        
        print(mapSettings.timeLimit);
        if (Network.isServer)
            RPCGameSettings(version.ToString(), (int)gameMode, mapSettings.fragLimit,mapSettings.timeLimit);
        RPCWriteMessage("Игрок законектился " + nick);        
        LocalUserV.nwid = Network.player;
        LocalUserV.team = Team.Spectator;
        RPCSetUserView(LocalUserV.nwid, LocalUserV.nick, LocalUserV.uid, LocalUserV.photo, LocalUserV.totalKills, LocalUserV.totalDeaths, LocalUserV.totalZombieKills, LocalUserV.totalZombieDeaths);
        userViews[LocalUserV.nwid.GetHashCode()] = LocalUserV;        
    }

    
    void Update()
    {
        
        timeleft  -= Time.deltaTime / 60;
        var ts = TimeSpan.FromMinutes(timeleft);
        _GameWindow.TimeLeft = +ts.Minutes + ":" + ts.Seconds;
        if (mapSettings.Team)
            _GameWindow.TeamScore = BlueFrags + "/" + RedFrags;
        if (DebugKey(KeyCode.P)) RPCPause();
        if (_localiplayer != null)
        {
            _GameWindow.Life = _localiplayer.Life;
            GunBase g = _localiplayer.guns[_localiplayer.selectedgun];
            _GameWindow.Patrony = g._Name + ":" + g.bullets;
            _GameWindow.Energy = (int)_localiplayer.nitro;
            _GameWindow.Stage = stage;
            if (mapSettings.zombi) _GameWindow.Zombies = AliveZombies.Count;
            _GameWindow.Frags = LocalUserV.frags;
        }
        if (Input.GetKeyDown(KeyCode.M))
            onShowMap();
            
        if (Input.GetKeyDown(KeyCode.Tab))
            _GameStatsWindow.Show(this);
        if (Input.GetKeyUp(KeyCode.Tab))
            _GameStatsWindow.Hide();

        if (_GameStatsWindow.enabled)
        {
            _GameStatsWindow.PlayerStats = "";
            string table = GenerateTable(_GameStatsWindow.PlayerStatsTitle);            
            foreach (UserView user in userViews)
                if (user != null)
                    _GameStatsWindow.PlayerStats += string.Format(table, "", user.nick, user.team, user.score, user.frags, user.deaths, user.fps, user.ping) + "\r\n";
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) lockCursor = !lockCursor;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            lockCursor = !lockCursor;
            if (!lockCursor)
            {
                _GameWindow.isReadOnlyMsg = false;
                _GameWindow.focusMsg = true;
            }
            else 
            {
                if (_GameWindow.Msg != "")
                    WriteChatMessage(nick + ": " + _GameWindow.Msg);
                _GameWindow.Msg = "";
                _GameWindow.focusEnergy = true;
                _GameWindow.isReadOnlyMsg = true;
            }
        }

        CheckWin();

        if (mapSettings.TeamZombiSurvive || mapSettings.ZombiSurvive)
            ZUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
            _GameMenuWindow.Show(this);

        if (_TimerA.TimeElapsed(1000))
            RPCPingFps(Network.player.GetHashCode(),
                (Network.connections.Length > 0 && Network.isClient ? Network.GetLastPing(Network.connections[0]) : 0),
                _Console.fps);

    }

    public void Emit(ParticleEmitter[] emitors, Transform obj, Vector3 pos, Quaternion rot) { Emit(emitors, obj, pos, rot, Vector3.zero); }
    public void Emit(ParticleEmitter[] emitors, Transform obj, Vector3 pos, Quaternion rot, Vector3 vel)
    {

        obj.position = pos;
        obj.rotation = rot;
        if (obj.audio != null) obj.audio.Play();
        foreach (ParticleEmitter emitor in emitors)
        {
            if (emitor.particleCount < 50)
            {
                emitor.worldVelocity = vel;
                emitor.Emit();
            }
        }
    }
    
    void onMenu() { _GameMenuWindow.Show(this); }
    void onIrcChatButton() { _IrcChatWindow.Show(this); }
    void onTeams() { _TeamSelectWindow.tabImages = _TeamSelectWindow.iTeams; }
    void onObserver() { Spectator(); }
    void onOptions() { _SettingsWindow.Show(_Loader); }
    void onDisconnect() { Network.Disconnect(); }
    void onTeamSelectButton() { _TeamSelectWindow.Show(this); }
    void onScoreBoard() { _GameStatsWindow.Show(this); }
    void onShowMap() { MapCamera.active = !MapCamera.active; _Cam.camera.gameObject.active = !MapCamera.active; lockCursor = true; }
    [RPC]
    private void RPCGameSettings(string version, int gameMode, int frags,float timelimit)
    {        
        CallRPC(version, gameMode, frags,timelimit);
        if (version != version.ToString())
            print(string.Format("У вас неправильная версия", version, version.ToString()));

        mapSettings.gameMode = (GameMode)gameMode;
        timeleft = mapSettings.timeLimit = timelimit;
        _TeamSelectWindow.tabGameType = (int)mapSettings.gameMode;
        _TeamSelectWindow.Fraglimit = mapSettings.fragLimit = frags;
        _GameWindow.enabledZombiScore = mapSettings.zombi;
        if (!mapSettings.Team) _TeamSelectWindow.enabledTeamsView = false;
        _TeamSelectWindow.Show(this);
        _GameWindow.Show(this);       
    }
    public void OnPlayerConnected(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        print(pr);
        networkView.RPC("SetTimeLeft", np,timeleft);
        if (mapSettings.zombi && stage != 0) networkView.RPC("RPCNextStage", np, stage);        
        networkView.RPC("RPCGameSettings", np, version.ToString(), (int)gameMode, mapSettings.fragLimit, mapSettings.timeLimit);
        networkView.RPC("RPCSetUserView", np, LocalUserV.nwid, LocalUserV.nick, LocalUserV.uid, LocalUserV.photo, LocalUserV.totalKills, LocalUserV.totalDeaths, LocalUserV.totalZombieKills, LocalUserV.totalZombieDeaths);
        foreach (Box b in GameObject.FindObjectsOfType(typeof(Box)))
            b.OnPlayerConnected1(np);
    }
    [RPC]
    void SetTimeLeft(float time)
    {
        timeleft = time;
    }
    [RPC]
    void RPCPause()
    {
        CallRPC();
        Debug.Break();
    }

    public List<Zombie> AliveZombies = new List<Zombie>();
    
    private void ZUpdate()
    {
        
        if (HasAny(players) && Network.isServer)
        {
            if (_TimerA.TimeElapsed(500) && zombiespawnindex < maxzombies)
            {
                Transform zsp = transform.Find("zsp");
                Transform a = zsp.GetChild(UnityEngine.Random.Range(0, zsp.GetChildCount() - 1));
                if (zombiespawnindex < zombies.Count)
                    CreateZombie(zombies[zombiespawnindex]);
                else
                {
                    GameObject t = (GameObject)Network.Instantiate(Resources.Load("Prefabs/Zombie"), a.position, Quaternion.identity, (int)GroupNetwork.Zombie);
                    Zombie z = (t).GetComponent<Zombie>();
                    CreateZombie(z);
                }
                zombiespawnindex++;
            }
            if (AliveZombies.Count == 0 && zombiespawnindex == maxzombies && !wait)
            {
                wait = true;
                _TimerA.AddMethod(5000, delegate { RPCNextStage(stage + 1); });
            }
        }
    }
    
    [RPC]
    void WriteChatMessage(string msg)
    {
        CallRPC(msg);
        _GameWindow.Messages += msg + "\r\n";
    }
    public void WriteMessage(string s)
    {
        _GameWindow.Killmessages += s + "\r\n";
    }
    [RPC]
    public void RPCWriteMessage(string s)
    {
        CallRPC(s);
        _GameWindow.Killmessages += s + "\r\n";
    }
    
    [RPC]
    private void RPCSetUserView(NetworkPlayer nwid, string nick, int uid, string photo, int tk, int td, int tzk, int tzd)
    {        
        CallRPC(LocalUserV.nwid, LocalUserV.nick, LocalUserV.uid, LocalUserV.photo, tk, td, tzk, tzd);
        if (nwid == Network.player) return;        
        UserView user = this.gameObject.AddComponent<UserView>();
        user.nick = nick;
        user.uid = uid;
        user.photo = photo;
        user.nwid = nwid;
        user.totalKills = tk;
        user.totalDeaths = td;
        user.totalZombieKills = tzk;
        user.totalZombieDeaths = tzd;
        if (photo != "")
            new WWW2(photo).done += delegate(WWW2 www)
            {
                print("loaded texture");
                user.texture = www.www.texture;
                DontDestroyOnLoad(user.texture);
            };        
        userViews[nwid.GetHashCode()] =  user;
        print(pr + "Received RPCSetUserView" + nwid.GetHashCode() + "," + userViews[nwid.GetHashCode()]);        
    }
    
    [RPC]    
    void RPCPingFps(int id, int ping, int fps)
    {
        CallRPC(id, ping, fps);
        userViews[id].fps = fps;
        userViews[id].ping = ping;        
    }
    
    private void CreateZombie(Zombie zombie)
    {
        zombie.RPCSetup(5 + UnityEngine.Random.Range(.3f * stage, .3f * (stage + 3)),
            UnityEngine.Random.Range(5 * stage, 5 * (stage + 20)));
        wait = false;
    }
    [RPC]
    private void RPCNextStage(int stage)
    {
        
        CallRPC(stage);
        PlaySound("stage");
        this.stage = stage;        
        maxzombies = mapSettings.fragLimit + stage;
        zombiespawnindex = 0;
        _Cam.LevelText.text = "Stage " + stage;
        _Cam.LevelText.animation.Play();
        foreach (Player p in players)
            if (p != null)
                if (p.dead)
                    p.RPCSpawn();
    }
    
    public void onTeamSelect()
    {
        _TeamSelectWindow.Hide();

        team = (mapSettings.DM || mapSettings.ZombiSurvive) ? Team.None : (Team)_TeamSelectWindow.iTeams;         
        lockCursor = true;

        if (_localPlayer == null)
            Network.Instantiate(Resources.Load("Prefabs/Player"), Vector3.zero, Quaternion.identity, (int)GroupNetwork.Player);
    }
    public void Spectator()
    {
        lockCursor = true;
        _TeamSelectWindow.Hide();
        if (_localPlayer != null && _localPlayer.car == null)
        {            
            _localPlayer.RPCShow(false);
            _localPlayer.RPCSetTeam((int)Team.Spectator);            
        }
    }
    void OnDisconnectedFromServer(NetworkDisconnection nd)
    {
        _Loader.LoadLevel("Menu", _Loader.lastLevelPrefix + 1);
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        RPCWriteMessage(userViews[player.GetHashCode()].nick + " Вышел из игры");
        int playerid = player.GetHashCode();        
        userViews[playerid] = null;
        //DestroyPlayer(player);
        foreach (Box box in GameObject.FindObjectsOfType(typeof(Box)))
            if (!(box is Player))
            {
                if (box.selected == playerid)
                    box.RPCResetOwner();
                
                foreach (NetworkView nw in box.GetComponents<NetworkView>())
                    if (nw.owner.GetHashCode() == playerid) Destroy(nw.viewID);
            }
        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);
    }
    [RPC]
    private void DestroyPlayer(NetworkPlayer p)
    {
        CallRPC(p);
        players[p.GetHashCode()].Destroy();
    }
    [RPC]
    private void Destroy(NetworkViewID v)
    {
        CallRPC(v);
        NetworkView nw = NetworkView.Find(v);
        nw.enabled = false;
        Component.Destroy(nw);
    }
    public bool HasAny<T>(T[] ts)
    {
        foreach (T t in ts)
            if (t != null) return true;
        return false;
    }
    void CheckWin()
    {
        if (_TimerA.TimeElapsed(1000))
        {
            BlueFrags = RedFrags = 0;
            foreach (UserView pl in TP(Team.Red))
                RedFrags += pl.frags;

            foreach (UserView pl in TP(Team.Blue))
                BlueFrags += pl.frags;

            if (Network.isServer && !win)
            {

                if (mapSettings.DM)
                    DMCheck();
                else if (mapSettings.TeamZombiSurvive)
                    ZombiTDMCheck();
                else if (mapSettings.TDM)
                    TDMCheck();
                else if (mapSettings.ZombiSurvive)
                    ZombieSuriveCheck();
            }
        }
    }
    private void ZombieSuriveCheck()
    {
        int live = 0;
        foreach (Player p in players)
            if (p != null && !p.dead) live++;
        if (live == 0 && HasAny(players) || TimeCaput)
        {
            RPCWriteMessage(String.Format("Вы умерли дожив до {0} раунда", stage));
            ShowEndStats();
        }
    }
    bool TimeCaput { get { return timeleft < 0; } }
    private void DMCheck()
    {
        foreach (Player pl in players)
            if (pl != null && pl.OwnerID != -1)
            {
                if (!win && pl.frags >= mapSettings.fragLimit || TimeCaput)
                {
                    RPCWriteMessage(pl.nick + " Win");
                    ShowEndStats();
                }
            }
    }
    private void TDMCheck()
    {
      

        if ((BlueFrags >= mapSettings.fragLimit || RedFrags >= mapSettings.fragLimit || TimeCaput))
        {                        
            RPCWriteMessage((BlueFrags > RedFrags ? "Синяя" : "Красная") + " команда Выграла");
            ShowEndStats();
        }
    }
    void ShowEndStats()
    {
        win = true;
        _GameStatsWindow.Show(this);
        _TimerA.AddMethod(5000, WinGame);
    }
    private void ZombiTDMCheck()
    {
        bool BlueteamLive = false, RedteamLive = false;
        int rcount = 0, bcount = 0;
        foreach (Player p in TP2(Team.Blue))
        {
            rcount++;
            if (!p.dead) BlueteamLive = true;
        }
        foreach (Player p in TP2(Team.Red))
        {
            bcount++;
            if (!p.dead) RedteamLive = true;
        }

        if (Network.isServer && rcount > 0 && bcount > 0 && (!RedteamLive || !BlueteamLive) || TimeCaput)
        {
            RPCWriteMessage((!RedteamLive ? "Blue" : "Red") + " Team Win");
            ShowEndStats();
        }
    }
    private void WinGame()
    {
        print(pr);
        Debug.Break();
        Network.Disconnect();
    }

}
public enum GroupNetwork { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player, Zombie }