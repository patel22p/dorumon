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
public class Menu : bs
{
    public static UserView user { get { return _Loader.UserView; } }
    public const string host = "http://localhost/";
    public string gameVersionName = "Swiborg3";
    public string ip { get { return _ServersWindow.Ipaddress; } set { _ServersWindow.Ipaddress = value; } }
    public bool enableIrc;
    public override void Awake()
    {
        base.Awake();
        enabled = true;
    }
    
    public override void Init()
    {                
        base.Init();
        rt = new RenderTexture(256, 256, 0);
        
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
        foreach (WindowBase o in Component.FindObjectsOfType(typeof(WindowBase))) 
            o.SendMessage("HideWindow", SendMessageOptions.DontRequireReceiver);
        if (_Loader.loggedin)
            _MenuWindow.Show(this);
        else
        {
            _LoginWindow.Show(this);
            if (_LoginWindow.AutoLogin && nk != "")
            {
                user.nick = nk;
                user.guest = PlayerPrefs.GetInt("guest").toBool();
                _Loader.password = PlayerPrefs.GetString("pass");
                OnLogin();
            }            
        }
        onRefresh();        
        _HostWindow.lGameMode = Enum.GetNames(typeof(GameMode));
        _ScoreBoardWindow.lScoreboard_orderby = Enum.GetNames(typeof(ScoreBoardTables));
        _ScoreBoardWindow.iScoreboard_orderby = 0;        

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
    public RenderTexture rt;
    public HostData[] hosts { get { return MasterServer.PollHostList(); } }
    public Material customMaterial;
    void Update()
    {
        
        if (_TimerA.TimeElapsed(100))
        {
            if (_ScoreBoardWindow.iScoreboard_orderby != -1)
            {
                string[] table = ScoreBoard[(int)_ScoreBoardWindow.Scoreboard_orderby.Parse<ScoreBoardTables>()];
                if (table != null)
                    _ScoreBoardWindow.lScore_table = table;
                else
                    _ScoreBoardWindow.lScore_table = new string[] { };
            }            

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

    public void WWWSend(string s, Action<string> a)
    {
        WWWSend(s, null, a);
    }
    public void WWWSend(string s, WWWForm form, Action<string> a)
    {
        s = s + "&r=" + Random.value;
        Debug.Log("WWW: " + s);
        var w = form == null ? new WWW(s) : new WWW(s, form); ;
        _TimerA.AddMethod(() => w.isDone, delegate
        {
            if (w.error == "" || w.error == null)
                a(w.text);
            else
                Debug.Log(w.error);
        });
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
        Match m = Regex.Match(sa[0], @"table:(\w+)");
        if (!m.Success)
            return "error : " + p;
        ScoreBoardTables scType = m.Groups[1].Value.Parse<ScoreBoardTables>();
        List<string> table = new List<string>();
        var tableTitle = GenerateTable(_ScoreBoardWindow.TableHeader);
        foreach (var row in sa.Skip(1))
        {
            var cell = row.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            table.Add(string.Format(tableTitle, "", cell[0], cell[1], cell[2], cell[3], "", ""));
        }
        ScoreBoard[(int)scType] = table.ToArray();
        foreach (var a in sa) //check if user in tabble
        {
            //                         1     2       3     4
            var r = Regex.Match(a, @"(\d+)\t(.+?)\t(\d+)\t(\d+)");
            if (r.Success && r.Groups[2].Value == user.nick)
            {
                user.scoreboard[(int)scType].frags = int.Parse(r.Groups[3].Value);
                user.scoreboard[(int)scType].place = int.Parse(r.Groups[1].Value);
                user.scoreboard[(int)scType].deaths = int.Parse(r.Groups[4].Value);
                UpdateUserScores();
                SaveUser();
                break;
            }
        }        
        return "tabble success";
    }
    
    public HostData hdf;
    public void StartServer()
    {        
        if (_HostWindow.Name.Length < 4) ShowPopup("Game name is to short");
        else
        {
            _TimerA.Clear();
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == _HostWindow.Map);                        
            mapSettings.gameMode = _HostWindow.GameMode.Parse<GameMode>();
            mapSettings.fragLimit = _HostWindow.MaxFrags;
            mapSettings.maxPlayers = _HostWindow.MaxPlayers;
            mapSettings.kickIfAfk = _HostWindow.Kick_if_AFK;
            mapSettings.kickIfErrors = _HostWindow.KickIfErrors;            
            mapSettings.timeLimit = _HostWindow.MaxTime;
            mapSettings.pointsPerZombie = _HostWindow.Money_per_frag;
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
        WWWSend(host + "user.php?user=" + nick, delegate(string text)
        {
            _UserWindow.vRefreshUserInfo = true;
            try
            {
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
            }
            catch (Exception e) { Debug.Log("Cannot Load UserView " + e); }
        });
    }

    private static void UpdateUserScores()
    {
        var h = GenerateTable(_UserWindow.Tableheader);
        var us = new List<string>();
        var nms = Enum.GetNames(typeof(ScoreBoardTables));
        for (int i = 0; i < nms.Length; i++)
        {
            var a = user.scoreboard[i];
            us.Add(string.Format(h, "", a.place == 0 ? "" : "" + a.place, nms[i], a.frags, a.deaths, "", ""));
            _UserWindow.lUserScores = us.ToArray();
        }
        
    }
    public void SaveUser() //usersave
    {
        _UserWindow.vSaveUser = false;
        using (StringWriter sw = new StringWriter())
        {
            UserView.xml.Serialize(sw, user);
            var form = new WWWForm();
            form.AddField("xml", sw.ToString(), Encoding.UTF8);
            WWWSend(host + "user.php?user=" + user.nick + "&passw=" + _Loader.passwordHash,form, delegate(string text)
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
        UpdateUserScores();
        foreach (var s in Enum.GetNames(typeof(ScoreBoardTables)))
            GetScoreBoard(s);
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
        Debug.Log(user.MaterialId);
        if (n == "About")
            _AboutWindow.Show(this);
        if (n == "LogOut")
        {
            PlayerPrefs.SetString("nick", "");
            _LoginWindow.Show(this);
        }
        if (n == "Scoreboard_orderby")
        {
        }
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
            GetScoreBoard(_ScoreBoardWindow.Scoreboard_orderby);
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
            StartServer();
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

