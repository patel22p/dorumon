using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Runtime.Serialization.Formatters.Binary;
public enum GameMode { ZombieSurvival, DotA, DeathMatch, TeamDeathMatch, CustomZombieSurvival }

public class Game : bs
{
    List<string> chat = new List<string>();
    new internal Player[] players = new Player[maxConId];
    bool win;
    float afk;
    int maxzombies = 0;
    int zombiespawnindex = 0;
    public float stageTime; 
    private float fixedDeltaTime;

    internal List<Zombie> zombies = new List<Zombie>();
    internal int ping;
    internal List<Patron> patrons = new List<Patron>();
    internal List<Tower> towers = new List<Tower>();
    internal List<Shared> boxes = new List<Shared>();
    internal List<MapItem> mapitems = new List<MapItem>();
    internal int fps;
    internal int RedFrags = 0, BlueFrags = 0;
    internal Vector3 gravity;

    [FindTransform("bounds", true)]
    public GameObject bounds;
    internal new Player _localPlayer;
    [FindTransform("GameEffects", true)]
    public GameObject effects;
    [FindTransform("GameEffects/decals", true)]
    public GameObject decals;
    [GenerateEnums("ParticleTypes")]
    public Particles[] particles;
    [GenerateEnums("DecalTypes")]
    public Decal[] decalPresets;
    [FindAsset("chat")]
    public AudioClip[] chatSounds;
    [FindAsset("Player")]
    public GameObject playerPrefab;
    [FindAsset]
    public AudioClip antigrav;
    [FindAsset]
    public AudioClip timewarp;
    [GenerateEnums("GameTypeIconsEnum")]
    [FindAsset("gamemode")]
    public Texture2D[] GameModeIcons;
    public bool wait;
    public AnimationCurve scorefactor;
    public float nwt;
    public override void Awake()
    {
        nwt = (float)Network.time;
        if (!_Loader.loaded)
        {
            Debug.Log("game Awake " + Application.loadedLevelName);
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == Application.loadedLevelName);
            var u = _Loader.UserView;
            u.nick = "a";
            _Loader.passpref = "a";
            u.guest = false;
            _Loader.loggedin = true;
        }
        gravity = Physics.gravity;
        fixedDeltaTime = Time.fixedDeltaTime;
        base.Awake();
        _Level = Level.z4game;
        if (Network.peerType == NetworkPeerType.Disconnected)
            if ((_Loader.host && Application.isEditor) || _Loader.cmd.Contains("server"))
                Network.InitializeServer(_Game.mapSettings.maxPlayers, _Loader.port, false);
            else
                Network.Connect(_Loader.ipaddress, _Loader.port);
        else
            foreach (bs o in Component.FindObjectsOfType(typeof(bs))) o.SendMessage("Enable", SendMessageOptions.DontRequireReceiver);
    }
    public void Start()
    {
        _Music.Play("game");
    }
    protected override void Enable()
    {
        if (Network.isServer)
            RPCGameSettings(_Loader.version, Serialize(_Loader.mapSettings, MapSetting.xml));
        ((GameObject)Network.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, (int)GroupNetwork.Player)).GetComponent<Player>();
        base.Enable();
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

        Physics.gravity = new Vector3(0, -20, 0);
        //particles = FindObjectsOfType(typeof(Particles)).Cast<Particles>().ToArray();                    
    }
    public override void InitValues()
    {        
        base.InitValues();
    }
    bool chatEnabled;
    public void addChatAlfa(float value, bool set)
    {
        var m = _GameWindow.chatOutput.material;
        var c = m.color;
        c.a = set ? value : c.a + value;
        m.color = c;
    }
    void Update()
    {
        stageTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            chatEnabled = !chatEnabled;
            Screen.lockCursor = !chatEnabled;
        }
        addChatAlfa(-Time.deltaTime, false);

        if (chatEnabled)
        {
            _GameWindow.chatInput.text += Input.inputString;
        }
        else if (_GameWindow.chatInput.text != "")
        {
            RPCSendChantMessage(_GameWindow.chatInput.text, _localPlayer.OwnerID);
            _GameWindow.chatInput.text = "";
        }

        if (sendto != null) Debug.Log("warning,sendto is not null");
        timeleft -= Time.deltaTime / 60;

        if (Input.GetKeyDown(KeyCode.P)) RPCPause();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _GameStatsWindow.Show(this);
            lockCursor = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
            _GameStatsWindow.Hide();

        if (_GameStatsWindow.enabled)
        {
            _GameStatsWindow.PlayerStats = "";

            string table = GenerateTable(_GameStatsWindow.PlayerStatsTitle);
            foreach (Player pl in players.Where(a => a != null).OrderByDescending(a => a.Score))
                _GameStatsWindow.PlayerStats += string.Format(table, "", pl.nick, pl.team, (int)pl.Score, pl.frags, pl.deaths, pl.fps, pl.ping) + "\r\n";
        }

        if (Input.GetMouseButtonDown(1)) lockCursor = !lockCursor;

        if (!_Loader.dontcheckwin) CheckWin();

        if (_Game.mapSettings.TeamZombiSurvival || _Game.mapSettings.ZombiSurvival)
            ZUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _GameMenuWindow.Toggle(this);
        }

        if (_TimerA.TimeElapsed(10000))
            RPCPingFps(Network.player.GetHashCode(), ping, fps);

        ping = Network.connections.Length > 0 && Network.isClient ? Network.GetLastPing(Network.connections[0]) : 0;
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse X") != 0)
            afk = 0;
        afk += Time.deltaTime;
        if (Network.peerType == NetworkPeerType.Client)
        {
            if (afk > 90 && _Game.mapSettings.kickIfAfk)
            {
                ShowPopup("You have been kicked, Reason:AFK");
                Network.Disconnect();
            }
            if ((_Console.errorcount > 0 || _Console.exceptionCount > 0) && _Game.mapSettings.kickIfErrors)
            {
                ShowPopup("You have been kicked, Reason:Errors");
                Network.Disconnect();
            }
            if (_TimerA.TimeElapsed(5000) && ping > _Game.mapSettings.MaxPing && _Game.mapSettings.MaxPing != 0)
            {
                ShowPopup("You have been kicked, Reason:High ping " + ping);
                Network.Disconnect();
            }
        }
    }
    void onMenu() { _GameMenuWindow.Show(this); }
    public void Action(string s)
    {
        if (s == "Menu")
            _GameMenuWindow.Show(this);

        if (s == "Options")
            _SettingsWindow.Show(_Loader);
        if (s == "Disconnect")
            Network.Disconnect();
        if (s == "TeamSelectButton")
            _TeamSelectWindow.Show(this);
        if (s == "ScoreBoard")
            _GameStatsWindow.Show(this);
        if (s == "TeamSelect")
        {
            if (_TeamSelectWindow.iTeams == -1 && _Game.mapSettings.Team) ShowPopup("Select team first");
            else
            {
                _TeamSelectWindow.Hide();
                _localPlayer.team = (_Game.mapSettings.DM || _Game.mapSettings.ZombiSurvival) ? Team.None : _TeamSelectWindow.Teams.Parse<Team>();
                if (!_localPlayer.spawned)
                {
                    if (!_Game.mapSettings.zombi) _localPlayer.ResetSpawn();
                    _localPlayer.RPCSetAlive(true);
                }
                lockCursor = true;
            }
        }
    }
    public void RPCGameSettings(string version, byte[] s) { CallRPC("GameSettings", version, s); }
    [RPC]
    public void GameSettings(string version, byte[] s) //rpcmapsettings
    {
        
        _Game.mapSettings = (MapSetting)Deserialize(s, MapSetting.xml);
        Debug.Log("GameSettings ");
        Debug.Log(mapSettings.timeLimit);
        Debug.Log(mapSettings.stage);        
        var tw = _TeamSelectWindow;        
        if (!_Game.mapSettings.Team) tw.vTeamsView = false;
        tw.lTeams = new string[] { Team.Blue + "", Team.Red + "" };
        var gmi = GameModeIcons;
        switch (_Game.mapSettings.gameMode)
        {
            case GameMode.ZombieSurvival: tw.vZombi = true; tw.imgRed = gmi[(int)GameTypeIconsEnum.IconZombie]; break;
            case GameMode.DeathMatch: tw.vDeathmatch = true; tw.imgRed = gmi[(int)GameTypeIconsEnum.DeathmatchIcon]; break;
            case GameMode.TeamDeathMatch: tw.vTeamDeathMatch = true; tw.imgRed = gmi[(int)GameTypeIconsEnum.TeamDeathmatchIcon]; break;
            case GameMode.CustomZombieSurvival: tw.vZombi = true; tw.imgRed = gmi[(int)GameTypeIconsEnum.IconZombie]; break;
        }

        if (_Game.mapSettings.gameMode == GameMode.ZombieSurvival)
            _GameMenuWindow.vfraglimit = false;
        _GameMenuWindow.Fraglimit = _Game.mapSettings.fragLimit;
        tw.Show(this);
    }
    public void OnPlayerConnected(NetworkPlayer np)
    {
        Debug.Log("On Player Connected ");
        sendto = np;        
        RPCGameSettings(_Loader.version, Serialize(mapSettings, MapSetting.xml));        
        RPCSetTimeLeft(timeleft);
        if (_Game.mapSettings.zombi) RPCNextStage(stage, stageTime);
        var sorted = GameObject.FindObjectsOfType(typeof(Player))
        .Union(GameObject.FindObjectsOfType(typeof(Gun)))
        .Union(GameObject.FindObjectsOfType(typeof(Destroible)))
        .Union(GameObject.FindObjectsOfType(typeof(Box)))
        .Union(GameObject.FindObjectsOfType(typeof(MapItem)))
        .Union(GameObject.FindObjectsOfType(typeof(bs)));
        foreach (bs b in sorted)
            b.OnPlayerConnectedBase(np);
        sendto = null;
    }
    public void RPCSetTimeLeft(float time) { CallRPC("SetTimeLeft", time); }
    [RPC]
    public void SetTimeLeft(float time)
    {
        Debug.Log("set time " + time);
        timeleft = time;
    }
    public void RPCSetGravityBomb(bool enable) { CallRPC("SetGravityBomb", enable); }
    [RPC]
    public void SetGravityBomb(bool enable)
    {
        Physics.gravity = enable ? new Vector3(0, 0.1f, 0) : gravity;
        if (enable)
        {
            foreach (Rigidbody a in FindObjectsOfType(typeof(Rigidbody)))
                if (!a.isKinematic)
                {
                    a.AddForce(Vector3.up * 100 * Random.value * fdt);
                    a.angularVelocity = Random.insideUnitSphere * 100;
                }
            _Cam.Vingetting.enabled = true;
            root.audio.clip = antigrav;
            root.audio.Play();
        }
        else
        {
            root.audio.Stop();
            _Cam.Vingetting.enabled = false;
        }
    }
    public void RPCSetTimeBomb(float timescale) { CallRPC("SetTimeBomb", timescale); }
    [RPC]
    public void SetTimeBomb(float timescale)
    {

        Time.timeScale = timescale;
        Time.fixedDeltaTime = fixedDeltaTime * timescale;
        foreach (AudioSource a in FindObjectsOfTypeIncludingAssets(typeof(AudioSource)))
            a.pitch = Time.timeScale;
        if (Time.timeScale == 1)
        {
            _Cam.Vingetting.enabled = false;
            root.audio.Stop();
        }
        else
        {
            root.audio.clip = timewarp;
            root.audio.Play();
            _Cam.Vingetting.enabled = true;
        }

    }
    public void RPCPause() { CallRPC("Pause"); }
    [RPC]
    public void Pause()
    {
        Debug.Break();
    }
    bool HasAny() { return players.Count(a => a != null && a.spawned) > 0; }
    [FindAsset("Zombie")]
    public GameObject ZombiePrefab;
    private void ZUpdate()
    {
        if (_TimerA.TimeElapsed(500) && HasAny() && Network.isServer)
        {
            if (zombiespawnindex < maxzombies)
            {
                if (zombiespawnindex < zombies.Count)
                {
                    zombies[zombiespawnindex].CreateZombie(stage);
                }
                else
                {
                    GameObject t = (GameObject)Network.Instantiate(ZombiePrefab, Vector3.zero, Quaternion.identity, (int)GroupNetwork.Zombie);
                    Zombie z = (t).GetComponent<Zombie>();
                    z.CreateZombie(stage);
                }
                wait = false;
                zombiespawnindex++;
            }
            if (AliveZombies.Count() == 0 && zombiespawnindex == maxzombies && !wait)
            {
                wait = true;
                _TimerA.AddMethod(build ? 10000 : 0, delegate { if (HasAny()) RPCNextStage(stage + 1, 0); });
            }
        }
    }
    public void RPCWriteMessage(string s) { CallRPC("WriteMessage", s); }
    [RPC]
    public void WriteMessage(string s)
    {
        _GameWindow.AppendSystemMessage(s);
    }
    public void RPCPingFps(int id, int ping, int fps) { CallRPC("PingFps", id, ping, fps); }
    [RPC]
    public void PingFps(int id, int ping, int fps)
    {
        players[id].fps = fps;
        players[id].ping = ping;
    }
    [FindAsset("nextLevel")]
    public AudioClip[] stageSound;
    public void RPCNextStage(int stage,float time) { CallRPC("NextStage", stage, time); }
    [RPC]
    public void NextStage(int stage, float time)
    {
        stageTime = 0;
        PlayRandSound(stageSound);
        this.stage = stage;
        maxzombies = _Game.mapSettings.zombiesAtStart + stage;
        zombiespawnindex = 0;
        _Cam.LevelText.text = "Stage " + stage;
        _Cam.LevelText.animation.Play();
        foreach (Player p in players)
            if (p != null)
                if (!p.Alive && p.spawned)
                    p.RPCSetAlive(true);
    }
    
    
    
    public void RPCSendChantMessage(string msg, int userid) { CallRPC("ChantMessage", msg, userid); }
    [RPC]
    public void ChantMessage(string s, int id) //writechat
    {
        addChatAlfa(6, true);
        string d = players[id] + ": " + s;
        chat.Add(d);
        PlayRandSound(chatSounds);
        _GameWindow.chatOutput.text = string.Join("\r\n", chat.TakeLast(7).ToArray());
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        int playerid = player.GetHashCode();
        RPCWriteMessage(players[playerid].nick + " Player Leaved" + player);
        foreach (Shared box in GameObject.FindObjectsOfType(typeof(Shared)))
            if (!(box is Player))
            {
                if (box.selected == playerid)
                    box.RPCResetOwner();

                foreach (NetworkView nw in box.GetComponents<NetworkView>())
                    if (nw.owner.GetHashCode() == playerid) RPCDestroy(nw.viewID);
            }

        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);
    }
    public void RPCDestroy(NetworkViewID v) { CallRPC("Destroy", v); }
    [RPC]
    public void Destroy(NetworkViewID v)
    {

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
                if (_Game.mapSettings.DM)
                    DMCheck();
                else if (_Game.mapSettings.TDM)
                    TDMCheck();
                else if (_Game.mapSettings.ZombiSurvival)
                    ZombieSuriveCheck();
            }
        }
    }
    private void ZombieSuriveCheck()
    {
        int live = 0;
        foreach (Player p in players)
            if (p != null && (p.Alive || !p.spawned)) live++;
        if (live == 0 && HasAny() || TimeEnd)
        {
            RPCWriteMessage(String.Format("You Survivald until {0} level.", stage));
            RPCShowEndStats();
        }
    }
    bool TimeEnd { get { return timeleft < 0; } }
    private void DMCheck()
    {
        foreach (Player pl in players)
            if (pl != null && pl.OwnerID != -1)
            {
                if (!win && pl.frags >= _Game.mapSettings.fragLimit || TimeEnd)
                {
                    RPCWriteMessage(pl.nick + " Win");
                    RPCShowEndStats();
                }
            }
    }
    private void TDMCheck()
    {
        if ((BlueFrags >= _Game.mapSettings.fragLimit || RedFrags >= _Game.mapSettings.fragLimit || TimeEnd))
        {
            RPCWriteMessage((BlueFrags > RedFrags ? "Red" : "Blue") + " Team Win");
            RPCShowEndStats();
        }
    }
    [FindAsset]
    public AudioClip endmusic;
    public void RPCShowEndStats() { CallRPC("ShowEndStats"); }
    [RPC]
    public void ShowEndStats()
    {
        win = true;
        _GameStatsWindow.Show(this);
        _Music.Play("end");
        _TimerA.AddMethod(build ? 15000 : 1000, WinGameEndScore);
    }
    private void WinGameEndScore()
    {
        Debug.Log("disconnect");
        Network.Disconnect();
    }
    void OnDisconnectedFromServer(NetworkDisconnection nd)
    {        
        Debug.Log("OnDisconnectedFromServer");
        _TimerA.Clear();
        if (!_localPlayer.user.guest)
        {
            _TimerA.AddMethod(500, delegate
            {
                _ScoreBoardWindow.Show(_Menu);
                SaveScores(ScoreBoardTables.Time, (int)Time.timeSinceLevelLoad, 0);
                if (_Loader.mapSettings.ZombiSurvival)
                    SaveScores(ScoreBoardTables.ZombieSurvival, _localPlayer.frags, _localPlayer.deaths);
                if (_Loader.mapSettings.DM)
                    SaveScores(ScoreBoardTables.DeathMatch, _localPlayer.frags, _localPlayer.deaths);
                if (_Loader.mapSettings.TDM)
                    SaveScores(ScoreBoardTables.TeamDeathMatch, _localPlayer.frags, _localPlayer.deaths);

                
            });
        }
        _Loader.LoadLevel("Menu", _Loader.lastLevelPrefix + 1);
    }
    private void SaveScores(ScoreBoardTables t,int frags,int deaths)
    {
        var u = _localPlayer.user;//Time.timeSinceLevelLoad                
        var sc = u.scoreboard[(int)t];
        sc.frags += frags;
        sc.deaths += deaths;
        _Menu.SaveScoreBoard(t + "", u.nick, _Loader.passwordHash, u.guest, sc.frags, sc.deaths);
        _ScoreBoardWindow.Scoreboard_orderby = t + "";        
    }
    public void AddDecal(DecalTypes t, Vector3 pos, Vector3 normal, Transform addto)
    {
        if (_SettingsWindow.Decals)
        {

            Decal d = decalPresets[(int)t];
            d.mesh.renderer.material = d.mat;
            d.mesh.transform.localScale = Vector3.one * d.scale;
            var g = ((GameObject)Instantiate(d.mesh, pos, Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), normal) * Quaternion.LookRotation(normal)));
            Destroy(g, 10);
            g.transform.parent = addto;
        }
    }
    internal IEnumerable<MapTag> spawns { get { return GameObject.FindObjectsOfType(typeof(MapTag)).Cast<MapTag>(); } }
    internal IEnumerable<Zombie> AliveZombies { get { return zombies.Where(a => a.Alive == true); } }
    internal IEnumerable<Zombie> deadZombies { get { return zombies.Where(a => a.Alive == false); } }
    internal int stage { get { return _Game.mapSettings.stage; } set { _Game.mapSettings.stage = value; } }
    internal float timeleft { get { return _Game.mapSettings.timeLimit; } set { _Game.mapSettings.timeLimit = value; } }
    internal GameMode gameMode { get { return _Game.mapSettings.gameMode; } set { _Game.mapSettings.gameMode = value; } }
    internal MapSetting mapSettings { get { return _Loader.mapSettings; } set { _Loader.mapSettings = value; } }
    internal bool cameraActive { get { return _Cam.camera.gameObject.active; } }
    //public float stageTimeElapsed { get { return (float)Network.time - _Game.stageTime; } }
}
public enum GroupNetwork { PlView, RPCSetID, Default, Shared, staticfield, Spawn, Nick, Gun, SetMovement, Player, Zombie, Tower }

