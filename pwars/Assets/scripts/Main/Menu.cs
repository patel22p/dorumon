using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
public enum ScoreBoardTables { Zombie_Kill, Played_Time, Player_Kill, Player_Deaths, Custom_Zombie_Survive }
public class Menu : Base
{
    public static UserView user { get { return _Loader.UserView; } }
    string host = "http://localhost/";
    public string gameVersionName = "Swiborg3";
    public string ip { get { return _ServersWindow.Ipaddress; } set { _ServersWindow.Ipaddress = value; } }
    public bool enableIrc;
    public override void Awake()
    {
        base.Awake();
        enabled = true;
    }
    protected override void Start()
    {
        lockCursor = false;
        if (_Loader.dedicated)
        {
            Dedicated();
            return;
        }
        var nk = PlayerPrefs.GetString("nick");

        if (_LoginWindow.AutoLogin && nk != "")
        {
            user.nick = nk;
            user.guest = PlayerPrefs.GetInt("guest").toBool();
            _Loader.password = PlayerPrefs.GetString("pass");
            OnLogin();
        }

        foreach (WindowBase o in Component.FindObjectsOfType(typeof(WindowBase))) 
            o.SendMessage("HideWindow", SendMessageOptions.DontRequireReceiver);
        if (_Loader.loggedin)
            _MenuWindow.Show(this);
        else
            _LoginWindow.Show(this);
        onRefresh();
        //_UserWindow.imgBallRender = GameObject.Find("RenderCam").camera.targetTexture;
        _HostWindow.lGameMode = Enum.GetNames(typeof(GameMode));
        _ScoreBoardWindow.lScoreboard_ordeby = Enum.GetNames(typeof(ScoreBoardTables));
        _ScoreBoardWindow.iScore_table = 0;
        foreach (var s in Enum.GetNames(typeof(ScoreBoardTables)))
            GetScoreBoard(s);        
    }
    private void Dedicated()
    {
        user.nick = "Server";
        mapSettings = TakeRandom(_Loader.mapsets);
        mapSettings.gameMode = TakeRandom(mapSettings.supportedModes);
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(mapSettings.maxPlayers, _Loader.port, useNat);
        MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(gameVersionName, "DedicatedServer", mapSettings.mapName + "," + _HostWindow.GameMode);
        _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
    }
    public HostData[] hosts { get { return MasterServer.PollHostList(); } }
    public Material customMaterial;
    void Update()
    {
        if (_TimerA.TimeElapsed(100))
        {
            _UserWindow.imgAvatar = user.Avatar;
            _UserWindow.MaterialName = _Loader.playerTextures[user.MaterialId].name;
            Sphere.renderer.material = _Loader.playerTextures[user.MaterialId];
            if (user.BallTexture != null)
            {
                customMaterial.SetTexture("_MainTex", user.BallTexture);
            }
        }
        if (_TimerA.TimeElapsed(100))
            _HostWindow.imgGameImage = (Texture)Resources.Load(_HostWindow.Map);
        if (_TimerA.TimeElapsed(1000))
            _ServersWindow.lServersTable = ParseHosts(hosts).ToArray();
    }
    [FindTransform(scene = true)]
    public GameObject Sphere;
    public string[][] ScoreBoard = new string[10][];

    public void GetScoreBoard(string gamename)
    {
        WWWSend(host + "stats.php?game=" + gamename + "&find=" + _ScoreBoardWindow.FindUserName, TableParse);
    }
    
    public void SaveScoreBoard(string gamename,string user,string passw,bool guest, int frags,int deaths)
    {
        WWWSend(host + "stats.php?user=" + user + "&guest=" + (guest ? "1" : "") + "&passw=" + passw + "&frags=" + frags + "&deaths=" + deaths + "&game=" + gamename, TableParse);
    }

    private void TableParse(string s)
    {
        try
        {
            Debug.Log(ParseTable(s));
        }
        catch (Exception e) { Debug.Log(e.Message); }
    }
    private string ParseTable(string p)
    {
        var sa = p.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (sa.Length == 0)
            return "table length is zero";
        Match m = Regex.Match(sa[0], @"tabble:(\w+)");
        if (!m.Success)
            return "error " + sa[0];
        ScoreBoardTables sc = m.Groups[1].Value.Parse<ScoreBoardTables>();
        ScoreBoard[(int)sc] = sa.Skip(1).ToArray();
        foreach (var a in sa)
        {
            //                         1     2       3     4
            var r = Regex.Match(a,@"(\d+)\t(.+?)\t(\d+)\t(\d+)");
            if (r.Success && r.Groups[2].Value == user.nick)
            {
                user.scoreboard[(int)sc].frags = int.Parse(r.Groups[3].Value);
                user.scoreboard[(int)sc].place = int.Parse(r.Groups[1].Value);
                user.scoreboard[(int)sc].deaths = int.Parse(r.Groups[4].Value);
                SaveUser();
            }
        }
        UpdateScoreBoard();
        return "tabble success";
    }
    private void UpdateScoreBoard()
    {
        _ScoreBoardWindow.lScore_table = ScoreBoard[(int)_ScoreBoardWindow.Scoreboard_ordeby.Parse<ScoreBoardTables>()];
    }
    public HostData hdf;
    private void onStartServer()
    {
        if (_HostWindow.Name.Length < 4) ShowPopup("Game name is to short");
        else
        {
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == _HostWindow.Map);
            mapSettings.gameMode = _HostWindow.GameMode.Parse<GameMode>();
            mapSettings.fragLimit = _HostWindow.MaxFrags;
            mapSettings.maxPlayers = _HostWindow.MaxPlayers;
            mapSettings.kickIfAfk = _HostWindow.Kick_if_AFK;
            mapSettings.kickIfErrors = _HostWindow.KickIfErrors;            
            mapSettings.timeLimit = _HostWindow.MaxTime;
            mapSettings.PointsPerZombie = _HostWindow.Money_per_frag;
            _Loader.port = _HostWindow.Port;
            _Loader.host = true;
            if (mapSettings.gameMode == GameMode.CustomZombieSurvive)
            {
                mapSettings.zombiesAtStart = (int)_HostWindow.ZombiesAtStart;
                mapSettings.StartMoney = (int)_HostWindow.Startup_Money;
                mapSettings.stage = (int)_HostWindow.Startup_Level;
                mapSettings.ZombieDamage = _HostWindow.Zombie_Damage;
                mapSettings.zombieLifeFactor = _HostWindow.Zombie_Life;
                mapSettings.zombieSpeedFactor = _HostWindow.Zombie_Speed;
            }
            bool useNat = !Network.HavePublicAddress();
            Network.InitializeServer(mapSettings.maxPlayers, _Loader.port, useNat);
            MasterServer.RegisterHost(gameVersionName, _HostWindow.Name, mapSettings.mapName + "," + _HostWindow.GameMode);
            _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
        }
    }
    private void onConnect()
    {
        if (_ServersWindow.Ipaddress == "" || hdf == null)
            ShowPopup("Select Server from Server List or write Ipaddress first");
        else
        {
            _Loader.ipaddress = _ServersWindow.Ipaddress.Split(',');
            _Loader.port = _ServersWindow.Port;
            _Loader.host = false;
            ShowPopup("Connecting to "+_Loader.ipaddress);
            Network.Connect(_Loader.ipaddress, _ServersWindow.Port);
            _ServersWindow.Hide();
        }
    }
    private void onRefresh()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(gameVersionName);
    }
    
    IEnumerable<string> GetMapsByMode(GameMode gamemode)
    {
        foreach (MapSetting m in _Loader.mapsets)
            if (m.supportedModes.Contains(gamemode))
                yield return m.mapName;
    }
    IEnumerable<string> ParseHosts(HostData[] hosts)
    {
        foreach (HostData host in hosts)
        {
            string[] data = host.comment.Split(',');
            yield return string.Format(GenerateTable(_ServersWindow.ServersTitle), "", host.gameName, data[0], data[1], host.connectedPlayers + "/" + host.playerLimit, _Loader.GetPing(host.ip[0]));
        }
    }
    
    void OnFailedToConnect(NetworkConnectionError error)
    {
        ShowPopup("Could not connect to server: " + error);
    }
    
    //WWW Send(string s, params string[] f)
    //{
    //    Debug.Log("WWW: " + s);
    //    WWWForm form = new WWWForm();
    //    for (int i = 0; i < f.Length; i+=2)
    //        form.AddField(f[i], f[i + 1]);
    //    var w = new WWW(s, form);
    //    return w;
    //}
    private void GetUserInfo(string nick)
    {

        _UserWindow.vRefreshUserInfo = false;
        _UserWindow.UserNick = nick;
        WWWSend(host + "user.php?user=" + nick,delegate(string text)
        {
            _UserWindow.vRefreshUserInfo = true;
            //try
            //{
                if (text == "") throw new Exception("UserView not Set");
                using (StringReader sr = new StringReader(text))
                {
                    var user = (UserView)UserView.xml.Deserialize(sr);
                    //CopyS(_UserWindow, user);
                    _UserWindow.AvatarUrl = user.AvatarUrl;
                    _UserWindow.Desctiption = user.Desctiption;
                    _UserWindow.FirstName = user.FirstName;
                    _UserWindow.BallImage = user.BallTextureUrl;                    
                    bool own = _Loader.UserView.nick == user.nick;
                    _Loader.UserView = user;
                    _UserWindow.vAvatarUrl = own;
                    _UserWindow.vSaveUser = own;
                    _UserWindow.rDesctiption = !own;
                    _UserWindow.rBallImage = !own;
                    if (own) _Loader.UserView = user;
                }
            //}
            //catch { Debug.Log("Cannot Load UserView " + text); }
        });
    }
    public void SaveUser() //usersave
    {
        _UserWindow.vSaveUser = false;
        using (StringWriter sw = new StringWriter())
        {
            UserView.xml.Serialize(sw, user);
            var form = new WWWForm();
            form.AddField("xml", sw.ToString());
            Debug.Log(user.nick);
            WWWSend(host + "user.php?user=" + user.nick + "&passw=" + Ext.CalculateMD5Hash(_Loader.password),form, delegate(string text)
            {
                _UserWindow.vSaveUser = true;
                if (text != "Success")
                    Debug.Log("Could not Save User data" + text);
                else
                    Debug.Log("Save success");
            });
        }
    }
    private void OnLogin()
    {
        PlayerPrefs.SetString("nick", user.nick);
        PlayerPrefs.SetString("pass", _Loader.password);
        PlayerPrefs.SetInt("guest", user.guest.toInt());
        _Loader.loggedin = true;
        _MenuWindow.Show(this);

        GetUserInfo(user.nick);
    }
    public void Action(string n)
    {
        Debug.Log("Action: "+n);
        var pt = _Loader.playerTextures;
        if (n == "ShowKeyboard")
            _KeyboardWindow.Show(this);
        if (n == "Next")
            user.MaterialId++;
        if (n == "Prev")
            user.MaterialId--;
        user.MaterialId = Math.Max(0, Math.Min(pt.Length-1, user.MaterialId));
        
        if (n == "About")
            _AboutWindow.Show(this);
        if (n == "LogOut")
        {
            PlayerPrefs.SetString("nick", "");
            _LoginWindow.Show(this);
        }
        if (n == "Scoreboard_ordeby")
            UpdateScoreBoard();
        if (n == "LoginAsGuest")
            if (_LoginWindow.Nick != "")
            {                
                user.nick = _LoginWindow.Nick;
                user.guest = true;
                _MenuWindow.vAccountInfo = false;                
                OnLogin();
            }
        if (n == "Login")
        {
            _LoginWindow.vLogin = false;
            WWWSend(host + "login.php?user=" + _LoginWindow.LoginNick + "&passw=" + Ext.CalculateMD5Hash(_LoginWindow.LoginPassw),
            delegate(string text)
            {
                if (text == "Success")
                {
                    Debug.Log("Login Success");
                    user.guest = false;
                    _Loader.password = _LoginWindow.LoginPassw;
                    user.nick = _LoginWindow.LoginNick;
                    Debug.Log(user.nick);
                    _MenuWindow.vAccountInfo = true;
                    OnLogin();
                }
                else
                    ShowPopup("Could Not Login: " + text);
                _LoginWindow.vLogin = true;
            });
        }
        if (n == "Registr")
        {
            WWWSend(host + "registr.php?user=" + _LoginWindow.RegNick + "&email=" + _LoginWindow.Email + "&passw=" + Ext.CalculateMD5Hash(_LoginWindow.RegPassw),
            delegate(string text)
            {
                if (text == "Success")
                    ShowPopup("Registration Success. Now you can login");
                else
                    ShowPopup("Could Not Register: " + text);
            });
        }

        if (n == "SaveUser")
        {
            //CopyS(_UserWindow, user);
            user.AvatarUrl = _UserWindow.AvatarUrl;
            user.Desctiption = _UserWindow.Desctiption;
            user.FirstName = _UserWindow.FirstName;
            user.BallTextureUrl = _UserWindow.BallImage;            
            SaveUser();
        }
        if (n == "AccountInfo")
            _UserWindow.Show(this);
        if (n == "RefreshUserInfo")
        {
            Debug.Log(user.nick);
            GetUserInfo(user.nick);
        }
        if (n == "Score_Board")
            _ScoreBoardWindow.Show(this);
        if (n == "RefreshScoreBoard")
            GetScoreBoard(_ScoreBoardWindow.Score_table);
        if (n == "Close")
            _MenuWindow.Show();
        if (n == "Create")
        {
            _HostWindow.Show(this);
            Action("GameMode");
        }
        if (n == "GameMode")
        {
            var g = _HostWindow.GameMode.Parse<GameMode>();
            _HostWindow.lMap = GetMapsByMode(g).ToArray();
            _HostWindow.iMap = 0;
            //if (g == GameMode.ZombieSurive || g == GameMode.CustomZombieSurvive)
            if (g == GameMode.CustomZombieSurvive)
                _HostWindow.vums = true;
            else
                _HostWindow.vums = false;
        }
        if (n == "StartServer")
            onStartServer();
        if (n == "Connect")
            onConnect();
        if (n == "Settings")
            _SettingsWindow.Show(_Loader);
        if (n == "Servers")
            _ServersWindow.Show(this);
        if (n == "Refresh")
            onRefresh();
        if (n == "ServersTable")
        {
            var sv = hosts[_ServersWindow.iServersTable];
            _ServersWindow.Ipaddress = sv.ip[0];
            _ServersWindow.Port = sv.port;
            ShowPopup("Connecting" + _Loader.ipaddress);
            Network.Connect(sv);
            _ServersWindow.Hide();
            //_ServersWindow.Ipaddress = _ServersWindow.ServersTable[_ServersWindow.iServersTable];
        }
    }

    

    

    

    
}

