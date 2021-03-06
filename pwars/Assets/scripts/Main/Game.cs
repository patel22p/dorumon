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
using System.Threading;
public enum GameMode { ZombieSurvival, DeathMatch, TeamDeathMatch, CustomZombieSurvival }


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
    internal int ping;
    internal List<Zombie> zombies = new List<Zombie>();
    internal List<Patron> patrons = new List<Patron>();
    internal List<Tower> towers = new List<Tower>();
    internal List<Box> boxes = new List<Box>();
    internal List<MapItem> mapitems = new List<MapItem>();
    
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
    //public AnimationCurve scorefactor;
    public float nwt;
    public override void Awake()
    {
        if (!_Loader.loaded)
        {
            Debug.Log("game Awake is host" + _Loader.host + " ip" + _Loader.ipaddress + " hostport " + _Loader.hostport);
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == Application.loadedLevelName).Clone();
            var u = _Loader.UserView;
            u.nick = "a" + Random.Range(0, 99);
            u.guest = true;
            _Loader.passpref = "a";
            _Loader.loggedin = true;
            //_TimerA.AddMethod(1000, delegate { _localPlayer.Score = 1000; });
        }
        _TeamSelectWindow.ResetValues();
        _GameMenuWindow.ResetValues();
        foreach (GUIText text in _GameWindow.GetComponentsInChildren<GUIText>())
            if (!text.text.StartsWith(" ")) text.text = "";

        nwt = (float)Network.time;

        gravity = Physics.gravity = new Vector3(0, -20, 0);
        fixedDeltaTime = Time.fixedDeltaTime;
        base.Awake();
        _Level = Level.z4game;
        if (Network.peerType == NetworkPeerType.Disconnected)
            if ((_Loader.host && Application.isEditor) || _Loader.cmd.Contains("server"))
                Network.InitializeServer(_Game.mapSettings.maxPlayers, _Loader.hostport, false);
            else
            {
                Network.Connect(_Loader.ipaddress, _Loader.hostport + (_Loader.proxy ? 1 : 0));
            }
        else
            foreach (bs o in Component.FindObjectsOfType(typeof(bs))) o.SendMessage("Enable", SendMessageOptions.DontRequireReceiver);
        
    }
    public void Start()
    {
        _PopUpWindow.enabled = false;
        
    }
    protected override void Enable()
    {
        if (Network.isServer)
            RPCGameSettings(_Loader.Version+"", Serialize(_Loader.mapSettings, MapSetting.xml));
        ((GameObject)Network.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, (int)GroupNetwork.Player)).GetComponent<Player>();
        base.Enable();
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
        c.a = Math.Max(c.a, .1f);
        m.color = c;
    }
    const int pingSmooth = 15;
    List<int> pingAverage = new List<int>(new int[pingSmooth]);
    void Update()
    {
        if (DebugKey(KeyCode.O))
            RPCNextStage(stage + 1, 0);

        stageTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            chatEnabled = !chatEnabled;
            Screen.lockCursor = !chatEnabled;
        }
        addChatAlfa(-Time.deltaTime / 3, false);
        var c = _GameWindow.chatInput;
        if (chatEnabled)
        {
            c.text += Input.inputString;
            if (Input.GetKeyDown(KeyCode.Backspace) && c.text != "")
                c.text = "";
        }
        else if (c.text != "")
        {
            RPCSendChatMessage(c.text, _localPlayer.OwnerID);
            c.text = "";
        }

        if (sendto != null) Debug.Log("warning,sendto is not null");
        timeleft -= Time.deltaTime / 60;

        if (Input.GetKeyDown(KeyCode.P) && Application.isEditor) RPCPause();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _GameStatsWindow.Show(this);
            lockCursor = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
            _GameStatsWindow.Hide();

        var gw = _GameStatsWindow;
        if (gw.enabled)
        {
            gw.PlayerStats = "";

            string table = GenerateTable(gw.PlayerStatsTitle);
            foreach (Player pl in players.Where(a => a != null).OrderByDescending(a => a.frags))
                gw.PlayerStats += string.Format(table, "", pl.nick, pl.Alive ? "" : "Dead", pl.team, (int)pl.Score, pl.frags, pl.deaths, pl.fps, pl.ping, pl.errors) + "\r\n";
        }

        if (Input.GetMouseButtonDown(1)) lockCursor = !lockCursor;

        if (!_Loader.dontcheckwin) CheckWin();

        if (_Game.mapSettings.zombi)
            ZUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _GameMenuWindow.Toggle(this);
        }

        ping = Network.connections.Length > 0 && Network.isClient ? Network.GetLastPing(Network.connections[0]) : 0;
        if (_TimerA.TimeElapsed(4000))
            RPCPingFps(Network.player.GetHashCode(), ping, _Loader.fps, _Console.exceptionCount);
        if (_TimerA.TimeElapsed(1000))
        {
            if (pingAverage.Count > pingSmooth)
                pingAverage.Remove(0);
            pingAverage.Add(ping);
        }
        
        
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse X") != 0)
            afk = 0;
        afk += Time.deltaTime;
        if (Network.peerType == NetworkPeerType.Client)
        {
            if (afk > 90 && _Game.mapSettings.kickIfAfk)
            {
                RPCWriteMessage(_localPlayer.nick + " afk");
                ShowPopup("You have been kicked, Reason:AFK");
                Network.Disconnect();
            }
            if ((_Console.errorcount > 0 || _Console.exceptionCount > 0) && _Game.mapSettings.kickIfErrors)
            {
                RPCWriteMessage(_localPlayer.nick + " errors");
                ShowPopup("You have been kicked, Reason:Errors");
                Network.Disconnect();
            }
            if (_TimerA.TimeElapsed(5000))
                _Loader.pingAverage = (float)pingAverage.Average();
            if (_TimerA.TimeElapsed(5000) && _Loader.pingAverage > _Game.mapSettings.maxPing && _Game.mapSettings.maxPing != 0)
            {
                RPCWriteMessage(_localPlayer.nick + " kicked too hight ping");
                _TimerA.AddMethod(800, delegate { ShowPopup("You have been kicked, Reason:High ping " + ping); });
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
        var tw = _TeamSelectWindow;     
        tw.Show(this);
        _Game.mapSettings = (MapSetting)Deserialize(s, MapSetting.xml);
        Debug.Log("GameSettings ");
           
        tw.vTeamsView = _Game.mapSettings.Team;
        Debug.Log(_Game.mapSettings.Team);
        tw.lTeams = new string[] { Team.Blue + "", Team.Red + "" };
        var gmi = GameModeIcons;
        switch (_Game.mapSettings.gameMode)
        {
            case GameMode.ZombieSurvival: tw.imgRed = gmi[(int)GameTypeIconsEnum.IconZombie]; break;        
            case GameMode.DeathMatch: tw.imgRed = gmi[(int)GameTypeIconsEnum.DeathmatchIcon]; break;
            case GameMode.TeamDeathMatch: tw.imgRed = gmi[(int)GameTypeIconsEnum.TeamDeathmatchIcon]; break;
            case GameMode.CustomZombieSurvival: tw.imgRed = gmi[(int)GameTypeIconsEnum.IconZombie]; break;
        }
        tw.Description = GetDescr(_Game.mapSettings.gameMode);
        _GameMenuWindow.vfraglimit = _Game.mapSettings.gameMode != GameMode.ZombieSurvival;
        _GameMenuWindow.Fraglimit = _Game.mapSettings.fragLimit;
    }
    public void OnPlayerConnected(NetworkPlayer np)
    {
        Debug.Log("On Player Connected ");
        sendto = np;        
        RPCGameSettings(_Loader.Version+"", Serialize(mapSettings, MapSetting.xml));        
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
        Debug.Log(s);
        _GameWindow.AppendSystemMessage(s);
    }
    public void RPCPingFps(int id, int ping, int fps,int errors) { CallRPC("PingFps", id, ping, fps,errors); }
    [RPC]
    public void PingFps(int id, int ping, int fps,int errors)
    {
        players[id].fps = fps;
        players[id].ping = ping;
        players[id].errors = errors;
    }
    [FindAsset("nextLevel")]
    public AudioClip[] stageSound;
    public void RPCNextStage(int stage,float time) { CallRPC("NextStage", stage, time); }
    [RPC]
    public void NextStage(int stage, float time, NetworkMessageInfo info)
    {
        WriteMessage("Stage " + stage);
        stageTime = time;// +(float)(Network.time - info.timestamp);
        PlayRandSound(stageSound);
        this.stage = stage;
        maxzombies = _Game.mapSettings.zombiesAtStart + ((stage - 1) * mapSettings.zombiesPerStage);
        zombiespawnindex = 0;
        _Cam.LevelText.text = "Stage " + stage;
        _Cam.LevelText.animation.Play();
        foreach (Player p in players)
            if (p != null)
                if (!p.Alive && p.spawned)
                    p.RPCSetAlive(true);
    }
    public List<string> split(string s, int m)
    {
        List<string> strs = new List<string>();       
        for (int i = 0; i < s.Length; i += m)        
            strs.Add(s.Substring(i, Math.Min(m, s.Length - (i))));
        return strs;
    }

    public void RPCSendChatMessage(string msg, int userid) { CallRPC("ChatMessage", msg, userid); }
    [RPC]
    public void ChatMessage(string s, int id) //writechat
    {        
        addChatAlfa(2, true);
        string d = players[id].nick + ": " + s;
        chat.Add(d);
        //chat.AddRange(split(d, 50)); 
        PlayRandSound(chatSounds);
        _GameWindow.chatOutput.text = string.Join("\r\n", chat.TakeLast(7).ToArray());
    }
    void OnPlayerDisconnected(NetworkPlayer player) //destroy
    {
        int playerid = player.GetHashCode();
        RPCWriteMessage(players[playerid].nick + " Disconnected");
        foreach (Box box in GameObject.FindObjectsOfType(typeof(Box)))
            //if (!(box is Player))
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
                if (_Game.mapSettings.DeathMatch)
                    DMCheck();
                else if (_Game.mapSettings.TeamDeathMatch)
                    TDMCheck();
                else if (_Game.mapSettings.zombi)
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
            RPCShowEndStats(String.Format("You Survived until {0} level.", stage));
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
                    RPCShowEndStats(pl.nick + " Win");
                }
            }
    }
    private void TDMCheck()
    {
        if ((BlueFrags >= _Game.mapSettings.fragLimit || RedFrags >= _Game.mapSettings.fragLimit || TimeEnd))
            RPCShowEndStats((BlueFrags > RedFrags ? "Red" : "Blue") + " Team Win");
    }
    [FindAsset]
    public AudioClip endmusic;
    public void RPCShowEndStats(string s) { CallRPC("ShowEndStats",s); }
    [RPC]
    public void ShowEndStats(string s)
    {
        WriteMessage(s);
        _Cam.Levelcomplete.text = s;
        win = true;
        _Music.audio.Stop();
        _Music.audio.audio.PlayOneShot(endmusic, 3);
        _Cam.Levelcomplete.animation.Play();        
        _TimerA.AddMethod(5000, delegate { _GameStatsWindow.Show(this); });
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
            var mode = _Game.mapSettings.gameMode;
            var stage = this.stage;
            _TimerA.AddMethod(500, delegate
            {
                _ScoreBoardWindow.Show(_Menu);
                int tm = (int)(Time.timeSinceLevelLoad * 60);
                Debug.Log("Time Played + " +tm);
                SaveScores(ScoreBoardTables.Time, tm, 0, false);
                if (mode == GameMode.ZombieSurvival)
                {
                    SaveScores(ScoreBoardTables.ZombieSurvival, _localPlayer.frags, _localPlayer.deaths,false);
                    SaveScores(ScoreBoardTables.LevelsComplete, stage, _localPlayer.deaths, true);
                }
                if (mode == GameMode.DeathMatch)
                    SaveScores(ScoreBoardTables.DeathMatch, _localPlayer.frags, _localPlayer.deaths, false);
                if (mode == GameMode.TeamDeathMatch)
                    SaveScores(ScoreBoardTables.TeamDeathMatch, _localPlayer.frags, _localPlayer.deaths, false);
                if (mode == GameMode.CustomZombieSurvival)
                    SaveScores(ScoreBoardTables.CustomZombie, _localPlayer.frags, _localPlayer.deaths, false);
            });
        }
        _Loader.LoadLevel("Menu", _Loader.lastLevelPrefix + 1);
    }
    private void SaveScores(ScoreBoardTables t,int frags,int deaths, bool max)
    {
        var u = _localPlayer.user;//Time.timeSinceLevelLoad                
        var sc = u.scoreboard[(int)t];
        sc.frags = max ? frags : sc.frags + frags;
        sc.deaths = sc.deaths + deaths;
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

