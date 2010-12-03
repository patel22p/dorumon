using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

public enum GameMode { ZombieSurive, TeamZombieSurvive, DeathMatch, TeamDeathMatch }


public class Game : MapObject
{
    new public Player[] players = new Player[10];
    public List<IPlayer> iplayers = new List<IPlayer>();
    public List<Zombie> zombies = new List<Zombie>();
    public IEnumerable<Zombie> AliveZombies { get { return zombies.Where(a => a.Alive == true); } }
    public IEnumerable<Zombie> deadZombies  { get { return zombies.Where(a => a.Alive == false); } }
    public GameObject bounds;
    public List<Base> dynamic = new List<Base>();
    public GameMode gameMode { get { return mapSettings.gameMode; } set { mapSettings.gameMode = value; } }
    public new IPlayer _localiplayer;
    public new Player _localPlayer;
    [PathFind("GameEffects",true)]
    public GameObject effects;
    public int stage;
    public float timeleft = 20;
    public bool wait;
    public bool win;
    public int RedFrags = 0, BlueFrags = 0;
    public int maxzombies = 0;
    public List<Particles> particles = new List<Particles>();
    public int zombiespawnindex = 0;
    public GameObject MapCamera;
    public bool cameraActive { get { return _Cam.camera.gameObject.active; } }
    [LoadPath("player")]
    public GameObject playerPrefab;
    protected override void Awake()
    {
        Debug.Log("+game Awake");
        base.Awake();
        clearObjects("SpawnNone");
        clearObjects("SpawnZombie");

        if (nick == " ") nick = "Guest " + UnityEngine.Random.Range(0, 999);        
        _Level = Level.z4game;
        Debug.Log("cmdserver:" + _Loader.cmd.Contains("server"));
        print("mapSettings.host " + mapSettings.host);
        if (Network.peerType == NetworkPeerType.Disconnected)
            if ((mapSettings.host && Application.isEditor) || _Loader.cmd.Contains("server"))
                Network.InitializeServer(mapSettings.maxPlayers, mapSettings.port, false);
            else
                Network.Connect(mapSettings.ipaddress, _ServersWindow.Port);
        else
            foreach (Base o in Component.FindObjectsOfType(typeof(Base))) o.SendMessage("Enable", SendMessageOptions.DontRequireReceiver);
    }
    private void clearObjects(string name)
    {

        foreach (var spwn in GameObject.FindGameObjectsWithTag(name))
            foreach (var a in spwn.GetComponents<Component>())
                if (!(a is Transform))
                    DestroyImmediate(a);

    }
    public override void Init()
    {
        bounds = GameObject.Find("bounds");
        MapCamera = GameObject.Find("MapCamera");
        if (bounds == null) Debug.Log("warning no bounds founded");
        base.Init();
    }
    
    void Start()
    {
        Debug.Log("+game Start");
        //if (_Loader.disablePathFinding) GameObject.Find("PathFinding").active = false;
        Fragment();
        print("ZGameStart");
        //_vk.enabled = false;        
        print("timelimit"+mapSettings.timeLimit);
        if (Network.isServer)
            RPCGameSettings(version, (int)gameMode, mapSettings.fragLimit, mapSettings.timeLimit);
        RPCWriteMessage("Игрок законектился " + nick);
        Network.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, (int)GroupNetwork.Player);
    }
    public void onTeamSelect()
    {
        _TeamSelectWindow.Hide();
        _localPlayer.team = (mapSettings.DM || mapSettings.ZombiSurvive) ? Team.None : (Team)_TeamSelectWindow.iTeams;
        if (!_localPlayer.spawned) _localPlayer.RPCAlive(true);
        lockCursor = true;
    }
    void Update()
    {
        timeleft -= Time.deltaTime / 60;
        var ts = TimeSpan.FromMinutes(timeleft);
        _GameWindow.time.text = ts.Minutes + ":" + ts.Seconds;
        if (mapSettings.Team)
        {
            _GameWindow.blueTeam.text = BlueFrags.ToString();
            _GameWindow.redTeam.text = RedFrags.ToString();
        }
        if (DebugKey(KeyCode.P)) RPCPause();
        if (_localiplayer != null)
        {
            _GameWindow.life = _localiplayer.Life;
            _GameWindow.gunPatrons.text = _localPlayer.gun._Name + ":" + _localPlayer.gun.patronsLeft;

            _GameWindow.energy = (int)_localiplayer.nitro;
            if (mapSettings.zombi)
            {
                _GameWindow.zombiesLeft.text = "Зомби" + AliveZombies.Count().ToString();
                _GameWindow.level.text = "Уровень" + stage.ToString();
            }
            _GameWindow.frags.text = "Фраги " + _localPlayer.frags.ToString();

            _GameWindow.Score.text = "Очки " + _localPlayer.score.ToString();
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
            foreach (Player pl in players)
                if (pl != null)
                    _GameStatsWindow.PlayerStats += string.Format(table, "", pl.nick, pl.team, pl.score, pl.frags, pl.deaths, pl.fps, pl.ping) + "\r\n";
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) lockCursor = !lockCursor;

        if (!_Loader.dontcheckwin) CheckWin();

        if (mapSettings.TeamZombiSurvive || mapSettings.ZombiSurvive)
            ZUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
            _GameMenuWindow.Toggle(this);

        if (_TimerA.TimeElapsed(1000))
            RPCPingFps(Network.player.GetHashCode(),
                (Network.connections.Length > 0 && Network.isClient ? Network.GetLastPing(Network.connections[0]) : 0),
                _GameWindow.fps);

    }
    void onMenu() { _GameMenuWindow.Show(this); }
    void onIrcChatButton() { _IrcChatWindow.Show(this); }
    void onTeams() { _TeamSelectWindow.tabImages = _TeamSelectWindow.iTeams; }
    //void onObserver() { Spectator(); }
    void onOptions() { _SettingsWindow.Show(_Loader); }
    void onDisconnect() { Network.Disconnect(); }
    void onTeamSelectButton() { _TeamSelectWindow.Show(this); }
    void onScoreBoard() { _GameStatsWindow.Show(this); }
    void onShowMap()
    {        
        MapCamera.active = !MapCamera.active;
        _Cam.camera.gameObject.active = !MapCamera.active; 
        lockCursor = false;                
        
    }
    [RPC]
    private void RPCGameSettings(string version, int gameMode, int frags, float timelimit)
    {
        CallRPC(version, gameMode, frags, timelimit);
        mapSettings.gameMode = (GameMode)gameMode;
        timeleft = mapSettings.timeLimit = timelimit;
        _TeamSelectWindow.tabGameType = (int)mapSettings.gameMode;
        _TeamSelectWindow.Fraglimit = mapSettings.fragLimit = frags;
        if (!mapSettings.Team) _TeamSelectWindow.enabledTeamsView = false;
        _TeamSelectWindow.Show(this);
    }
    public void OnPlayerConnected(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        print(pr);
        networkView.RPC("SetTimeLeft", np, timeleft);
        if (mapSettings.zombi && stage != 0) networkView.RPC("RPCNextStage", np, stage);
        networkView.RPC("RPCGameSettings", np, version, (int)gameMode, mapSettings.fragLimit, mapSettings.timeLimit);
        foreach (Box b in GameObject.FindObjectsOfType(typeof(Box)))
            b.OnPlayerConnected1(np);
    }
    [RPC]
    void SetTimeLeft(float time)
    {
        timeleft = time;
    }
    [RPC]
    public void RPCPause()
    {
        CallRPC();
        Debug.Break();
    }

    
    bool HasAny() { return players.Count(a => a != null && a.spawned) > 0; }
    [LoadPath("Zombie")]
    public GameObject ZombiePrefab;
    private void ZUpdate()
    {
        
        if ( HasAny() && Network.isServer)
        {
            if (_TimerA.TimeElapsed(500) && zombiespawnindex < maxzombies)
            {
                if (zombiespawnindex < zombies.Count)
                    CreateZombie(zombies[zombiespawnindex]);
                else
                {
                    GameObject t = (GameObject)Network.Instantiate(ZombiePrefab, Vector3.zero, Quaternion.identity, (int)GroupNetwork.Zombie);
                    Zombie z = (t).GetComponent<Zombie>();
                    CreateZombie(z);
                }
                zombiespawnindex++;
            }
            if (AliveZombies.Count() == 0 && zombiespawnindex == maxzombies && !wait)
            {
                wait = true;
                _TimerA.AddMethod(100, delegate { RPCNextStage(stage + 1); });
            }
        }
    }
    
    public void WriteMessage(string s)
    {
        _GameWindow.AppendSystemMessage(s);        
    }
    [RPC]
    public void RPCWriteMessage(string s)
    {
        CallRPC(s);
        WriteMessage(s);
    }
    
    
    [RPC]    
    void RPCPingFps(int id, int ping, int fps)
    {
        CallRPC(id, ping, fps);
        players[id].fps = fps;
        players[id].ping = ping;        
    }
    
    private void CreateZombie(Zombie zombie)
    {   
     
        zombie.RPCSetup(3 + UnityEngine.Random.Range(.3f * stage, .3f * (stage + 3)),
            UnityEngine.Random.Range(5 * stage, 5 * (stage + 20)));
        wait = false;
    }
    [LoadPath("stage")]
    public AudioClip stageSound;
    [RPC]
    private void RPCNextStage(int stage)
    {
        CallRPC(stage);
        Debug.Log("Next Stage"+stage);
        PlaySound(stageSound);
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
    
    
    //public void Spectator()
    //{
    //    lockCursor = true;
    //    _TeamSelectWindow.Hide();
    //    if (_localPlayer != null && _localPlayer.car == null)
    //    {                        
    //        _localPlayer.RPCSetTeam((int)Team.None);
    //        _localPlayer.RPCAlive(false);
    //    }
    //}
    void OnDisconnectedFromServer(NetworkDisconnection nd)
    {
        _Loader.LoadLevel("Menu", _Loader.lastLevelPrefix + 1);
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {        
        int playerid = player.GetHashCode();
        RPCWriteMessage(players[playerid].nick + " Вышел из игры" + player);
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
    private void Destroy(NetworkViewID v)
    {
        CallRPC(v);
        NetworkView nw = NetworkView.Find(v);
        nw.enabled = false;
        Component.Destroy(nw);
    }
    
    void CheckWin()
    {
        if (_TimerA.TimeElapsed(1000))
        {
            BlueFrags = RedFrags = 0;
            foreach (Player pl in TP(Team.Red))
                RedFrags += pl.frags;

            foreach (Player pl in TP(Team.Blue))
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
        if (live == 0 && HasAny() || TimeCaput)
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
        foreach (Player p in TP(Team.Blue))
        {
            rcount++;
            if (!p.dead) BlueteamLive = true;
        }
        foreach (Player p in TP(Team.Red))
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
    private void Fragment()
    {
        GameObject[] gs = GameObject.FindGameObjectsWithTag("MapFragment");
        foreach (var g in gs)
        {
            Transform cur = g.transform.Find("Box02");
            AddFragment(cur, g.transform, true);
        }

    }
    private void AddFragment(Transform cur, Transform root, bool first)
    {
        Fragment f = cur.gameObject.AddComponent<Fragment>();
        f.first = first;
        ((MeshCollider)cur.collider).convex = true;
        if (!first)
        {
            cur.gameObject.active = false;
            cur.gameObject.layer = LayerMask.NameToLayer("HitLevelOnly");
        }
        int i = 1;
        for (; ; i++)
        {
            string nwpath = cur.name + "_frag_0" + i;
            Transform nw = root.Find(nwpath);
            if (nw == null) break;
            nw.parent = cur;
            AddFragment(nw, root, false);
        }


    }
}
public enum GroupNetwork { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player, Zombie,Gun }