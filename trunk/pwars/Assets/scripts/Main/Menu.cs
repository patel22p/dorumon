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
public enum ScoreBoardTables { ZombieSurvival, Time, DeathMatch, TeamDeathMatch, CustomZombie }
public class Menu : bs
{
    internal string[][] ScoreBoard = new string[10][];
    internal HostData[] hosts { get { return MasterServer.PollHostList(); } }
    [FindTransform(scene = true)]
    public GameObject Sphere;
    public override void Awake()
    {
        //MasterServer.ipAddress = "";
        //MasterServer.port = 23456;
        base.Awake();
        enabled = true;
    }
    public override void Init()
    {
        base.Init();
    }
    public void Start()
    {
        lockCursor = false;
        if (_Loader.dedicated)
        {
            Dedicated();
            return;
        }

        foreach (WindowBase o in Component.FindObjectsOfType(typeof(WindowBase)))
            o.SendMessage("HideWindow", SendMessageOptions.DontRequireReceiver);
        if (_Loader.loggedin)
        {
            _MenuWindow.Show(this);                            
        }
        else
        {
            _LoginWindow.Show(this);
            if (_LoginWindow.AutoLogin && _Loader.nickpref != "")
            {
                LocalUser.nick = _Loader.nickpref;
                LocalUser.guest = _Loader.guestpref;
                OnLogin();
            }
        }
        
        _HostWindow.lGameMode = Enum.GetNames(typeof(GameMode));
        _ScoreBoardWindow.lScoreboard_orderby = Enum.GetNames(typeof(ScoreBoardTables));
        RefreshMasterServer();        
    }
    private void Dedicated()
    {
        LocalUser.nick = "Server";
        mapSettings = TakeRandom(_Loader.mapsets);
        mapSettings.gameMode = TakeRandom(mapSettings.supportedModes);
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(mapSettings.maxPlayers, _Loader.port, useNat);
        MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(_Loader.version, "DedicatedServer", mapSettings.mapName + "," + _HostWindow.GameMode);
        _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
    }
    public Material customMaterial;
    void Update()
    {
        _MenuWindow.vAccountInfo = !_Loader.UserView.guest;
        if (_TimerA.TimeElapsed(100))
        {
            if (_HostWindow.GameMode != "")
            {
                var g = _HostWindow.GameMode.Parse<GameMode>();
                _HostWindow.lMap = GetMapsByMode(g).ToArray();

                if (g == GameMode.CustomZombieSurvival)
                    _HostWindow.vums = true;
                else
                    _HostWindow.vums = false;
                _HostWindow.Description = GetDescr(g);
                if (_HostWindow.Map != "")
                {
                    mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == _HostWindow.Map);
                    mapSettings.gameMode = g;
                }

            }
            _HostWindow.vfragCanvas = !mapSettings.zombi;
            _UserWindow.vSaveUser = _UserWindow.UserNick == LocalUser.nick;
        }

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

            _UserWindow.MaterialName = _Loader.playerTextures[LocalUser.MaterialId].name;
            Sphere.renderer.material = _Loader.playerTextures[LocalUser.MaterialId];
            
        }
        if (_TimerA.TimeElapsed(100) && _HostWindow.Map != "")
            _HostWindow.imgGameImage = (Texture)Resources.Load(_HostWindow.Map);
        if (_TimerA.TimeElapsed(1000) && hosts != null)
            _ServersWindow.lServersTable = ParseHosts(hosts).ToArray();
    }
    public void GetScoreBoard(string gamename, string find, string user)
    {
        _ScoreBoardWindow.vRefreshScoreBoard = false;
        WWWSend("stats.php?game=" + gamename + "&find=" + find + "&user=" + user, TableParse);
    }
    public void SaveScoreBoard(string gamename, string user, string passw, bool guest, int frags, int deaths)
    {
        Debug.Log("SaveScoreBoard");
        WWWSend("stats.php?user=" + user + "&guest=" + (guest ? "1" : "") + "&passw=" + passw + "&frags=" + frags + "&deaths=" + deaths + "&game=" + gamename, TableParse);
    }
    private void TableParse(string s)
    {
        _ScoreBoardWindow.vRefreshScoreBoard = true;
        try
        {
            Debug.Log(ParseTable(s));
        }
        catch (Exception e) { Debug.Log(e.Message); }
    }
    private string ParseTable(string text) //scoreboard
    {
        var sa = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (sa.Length == 0)
            return "table length is zero";
        Match m = Regex.Match(sa[0], @"table:(\w+)");
        if (!m.Success)
            return "error : " + text;
        ScoreBoardTables scType = m.Groups[1].Value.Parse<ScoreBoardTables>();
        _ScoreBoardWindow.Scoreboard_orderby = scType.ToString();
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
            if (r.Success && r.Groups[2].Value == LocalUser.nick)
            {
                LocalUser.scoreboard[(int)scType].frags = int.Parse(r.Groups[3].Value);
                LocalUser.scoreboard[(int)scType].place = int.Parse(r.Groups[1].Value);
                LocalUser.scoreboard[(int)scType].deaths = int.Parse(r.Groups[4].Value);
                UpdateUserScores(_Loader.UserView);                
                _TimerA.AddMethod(100, SaveUser);
                break;
            }
        }        
        return "tabble success";
    }
    public void StartServer() //createserver
    {
        if (_HostWindow.Name.Length < 1 || mapSettings == null || _HostWindow.GameMode == "" || _HostWindow.Map == "")
            ShowPopup("Game name is to short/map not selected");
        else
        {
            _TimerA.Clear();
            Debug.Log("start Server");
            mapSettings.fragLimit = _HostWindow.MaxFrags;            
            mapSettings.maxPlayers = Math.Min(4, _HostWindow.MaxPlayers);
            mapSettings.kickIfAfk = _HostWindow.Kick_if_AFK;
            mapSettings.kickIfErrors = _HostWindow.KickIfErrors;
            mapSettings.timeLimit = _HostWindow.MaxTime;
            _Loader.port = _HostWindow.Port;
            _Loader.host = true;
            if (mapSettings.DM)
                CopyValuesCommon(true);                
            if (mapSettings.gameMode == GameMode.CustomZombieSurvival)
                CopyValuesZombie(true);                
            bool useNat = !Network.HavePublicAddress();
            var conn = Network.InitializeServer(mapSettings.maxPlayers - 1, _Loader.port, useNat);
            if (conn == NetworkConnectionError.NoError)
            {
                MasterServer.RegisterHost(_Loader.version, _HostWindow.Name, mapSettings.mapName + "," + _HostWindow.GameMode);
                _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
            }
            else
                ShowPopup("Connection failed: " + conn);

        }
    }
    private void CopyValuesCommon(bool save)
    {
        if (save)
        {
            mapSettings.StartMoney = (int)_HostWindow.StartMoney;
            mapSettings.damageFactor = _HostWindow.DamageFactor;
            mapSettings.pointsPerZombie = _HostWindow.Money_per_playerKill;
        }
        else
        {
            _HostWindow.StartMoney = mapSettings.StartMoney;
            _HostWindow.DamageFactor = mapSettings.damageFactor;
            _HostWindow.Money_per_playerKill = mapSettings.pointsPerZombie;
        }
    }
    private void CopyValuesZombie(bool save)
    {
        if (save)
        {
            mapSettings.pointsPerZombie = _HostWindow.Money_per_frag;
            mapSettings.zombiesAtStart = (int)_HostWindow.ZombiesAtStart;
            mapSettings.StartMoney = (int)_HostWindow.Startup_Money;
            mapSettings.stage = (int)_HostWindow.Startup_Level;
            mapSettings.zombieDamage = _HostWindow.Zombie_Damage;
            mapSettings.zombieLifeFactor = _HostWindow.Zombie_Life;
            mapSettings.zombieSpeedFactor = _HostWindow.Zombie_Speed;
        }
        else
        {
            _HostWindow.Money_per_frag = mapSettings.pointsPerZombie;
            _HostWindow.ZombiesAtStart = mapSettings.zombiesAtStart;
            _HostWindow.Startup_Money = mapSettings.StartMoney;
            _HostWindow.Startup_Level = mapSettings.stage;
            _HostWindow.Zombie_Damage = mapSettings.zombieDamage;
            _HostWindow.Zombie_Life = mapSettings.zombieLifeFactor;
            _HostWindow.Zombie_Speed = mapSettings.zombieSpeedFactor;            
        }
    }
    private void onConnect()
    {
        if (_ServersWindow.Ipaddress == "")
            ShowPopup("Write Ip address first or select server from list");
        else
        {
            _Loader.ipaddress = _ServersWindow.Ipaddress.Split(',');
            _Loader.port = _ServersWindow.Port;
            _Loader.host = false;
            ShowPopup("Connecting to " + string.Join(",", _Loader.ipaddress));
            var conn = Network.Connect(_Loader.ipaddress, _ServersWindow.Port);
            if (conn != NetworkConnectionError.NoError)
                ShowPopup("Connection failed: " + conn);
            _ServersWindow.Hide();
        }
    }
    void OnFailedToConnect(NetworkConnectionError error)
    {
        ShowPopup("Could not connect to server: " + error);
    }
    private void GetUserInfo(string nick)
    {

        WWWSend("user.php?user=" + nick, ParseUser);
    }
    private void ParseUser(string text)
    {        
        if (text == "") throw new Exception("UserView not Set");
        using (StringReader sr = new StringReader(text))
        {
            try
            {
                UserView user = (UserView)UserView.xml.Deserialize(sr);
                //CopyS(_UserWindow, user);
                _UserWindow.AvatarUrl = user.AvatarUrl;
                _UserWindow.Desctiption = user.Desctiption;
                _UserWindow.FirstName = user.FirstName;
                _UserWindow.BallImage = user.BallTextureUrl;
                _UserWindow.UserNick = user.nick;
                bool own = _Loader.UserView.nick == user.nick;
                _UserWindow.vAvatarUrl = own;
                _UserWindow.vBallImage = own;
                UpdateImage(user);
                if (own)
                {
                    _Loader.UserView = user;
                }
                UpdateUserScores(user);
            }
            catch (Exception e) { Debug.Log("Cannot Parse UserView " + text + "\r\n" + e); }
        }

    }
    private void UpdateImage(UserView user)
    {
        var w = new WWW(Menu.webserver + "image.php?image=" + user.BallTextureUrl);
        _TimerA.AddMethod(() => w.isDone, delegate { customMaterial.SetTexture("_MainTex", w.texture); });
        var wn = new WWW(Menu.webserver + "image.php?image=" + user.AvatarUrl);
        _TimerA.AddMethod(() => wn.isDone, delegate { _UserWindow.imgAvatar = wn.texture; });
    }
    private static void UpdateUserScores(UserView user)
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
        WWWSend("user.php?user=" + LocalUser.nick + "&passw=" + _Loader.passwordHash, SerializeToStr(LocalUser, UserView.xml), delegate { UpdateImage(LocalUser); });
    }
    private void OnLogin()
    {
        Debug.Log("isguest: " + LocalUser.guest);
        _Loader.nickpref = LocalUser.nick;
        _Loader.guestpref = LocalUser.guest;
        _Loader.loggedin = true;
        _MenuWindow.Show(this);
        if (!LocalUser.guest)
            GetUserInfo(LocalUser.nick);
        
        //foreach (var s in Enum.GetNames(typeof(ScoreBoardTables)))
        //    GetScoreBoard(s);
    }
    public void Action(string n)
    {
        Debug.Log("Action: " + n);
        var pt = _Loader.playerTextures;
        if (n == "ShowKeyboard")
            _KeyboardWindow.Show(this);        
        if (n == "Next")
            LocalUser.MaterialId++;
        if (n == "Prev")
            LocalUser.MaterialId--;
        LocalUser.MaterialId = Math.Max(0, Math.Min(pt.Length - 1, LocalUser.MaterialId));
        if (n == "About")
            _AboutWindow.Show(this);
        if (n == "LogOut")
        {
            _Loader.nickpref = "";
            _LoginWindow.Show(this);
        }
        if (n == "Scoreboard_orderby")
        {
        }
        if (n == "LoginAsGuest")
            if (_LoginWindow.Nick != "")
            {
                LocalUser.nick = _LoginWindow.Nick;
                LocalUser.guest = true;
                OnLogin();
            }
        if (n == "Login")
        {
            _LoginWindow.vLogin = false;

            WWWSend("user.php?login=1&user=" + _LoginWindow.LoginNick + "&passw=" + Ext.CalculateMD5Hash(_LoginWindow.LoginPassw),
            delegate(string text)
            {
                if (text == "Success")
                {
                    Debug.Log("Login Success");
                    LocalUser.guest = false;
                    _Loader.passpref = _LoginWindow.LoginPassw;
                    LocalUser.nick = _LoginWindow.LoginNick;
                    Debug.Log(LocalUser.nick);
                    OnLogin();
                }
                else
                    ShowPopup("Could Not Login: " + text);
                _LoginWindow.vLogin = true;
            });
        }
        if (n == LoginWindowEnum.Registr + "")
        {
            _LoginWindow.vRegistr = false;
            WWWSend("user.php?reg=1&user=" + _LoginWindow.RegNick + "&email=" + _LoginWindow.Email + "&passw=" + Ext.CalculateMD5Hash(_LoginWindow.RegPassw), SerializeToStr(new UserView() { nick = _LoginWindow.RegNick }, UserView.xml),
            delegate(string text)
            {
                _LoginWindow.vRegistr = true;
                if (text == "Success")
                    ShowPopup("Registration Success. Now you can login");
                else
                    ShowPopup("Could Not Register: " + text);
            });
        }

        if (n == UserWindowEnum.SaveUser + "")
        {
            LocalUser.AvatarUrl = _UserWindow.AvatarUrl;
            LocalUser.Desctiption = _UserWindow.Desctiption;
            LocalUser.FirstName = _UserWindow.FirstName;
            LocalUser.BallTextureUrl = _UserWindow.BallImage;
            SaveUser();
        }
        if (n == "AccountInfo")
            _UserWindow.Show(this);
        if (n == "RefreshUserInfo")
        {
            GetUserInfo(LocalUser.nick);
        }
        if (n == "Score_Board")
        {
            _ScoreBoardWindow.Show(this);                        
            
        }
        if (n == "RefreshScoreBoard")
            if (_ScoreBoardWindow.iScoreboard_orderby != -1)
                GetScoreBoard(_ScoreBoardWindow.Scoreboard_orderby, _ScoreBoardWindow.FindUserName, "");
            else
                ShowPopup("select order first");
        if (n == ScoreBoardWindowEnum.Score_table + "")
        {
            GetUserInfo(_ScoreBoardWindow.Score_table.Split(" ")[1]);
            _UserWindow.Show(this);
        }
        if (n == UserWindowEnum.UserScores + "")
        {
            _ScoreBoardWindow.Show(this);
            GetScoreBoard(Regex.Match(_UserWindow.UserScores, @"[^\d ]+").Value, "", _UserWindow.UserNick);           
        }
        if (n == "Close")
            _MenuWindow.Show();
        if (n == "Create")//createServer
        {
            _HostWindow.Show(this); 
            _HostWindow.lMap = new string[0];
            Action("GameMode");
        }
        if (n == "Map")
        {
            CopyValuesCommon(false);
            CopyValuesZombie(false);
        }
        if (n == "GameMode" && _HostWindow.GameMode!="")
        {
            _HostWindow.iMap = 0; 
        }
        if (n == "StartServer")
            StartServer();
        if (n == "Connect")
            onConnect();
        if (n == "Settings")
            _SettingsWindow.Show(_Loader);
        if (n == "Servers")
        {
            _ServersWindow.ResetValues();
            _ServersWindow.Show(this);
        }
        if (n == "Refresh")
            RefreshMasterServer();

        if (n == "ServersTable")
        {
            var sv = hosts[_ServersWindow.iServersTable];
            _ServersWindow.Ipaddress = sv.ip[0];
            _ServersWindow.Port = sv.port;
            ShowPopup("Connecting to " + string.Join(",", sv.ip));
            Network.Connect(sv);
            _ServersWindow.Hide();
            //_ServersWindow.Ipaddress = _ServersWindow.ServersTable[_ServersWindow.iServersTable];
        }
    }
    private void RefreshMasterServer()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(_Loader.version);
    }
    public void WWWSend(string s, Action<string> a)
    {
        WWWSend(s, null, a);
    }
    public void WWWSend(string s, string xml, Action<string> a)
    {
        s = s + "&r=" + Random.value;
        WWWForm form = new WWWForm();
        form.AddField("hash", Ext.CalculateMD5Hash("/" + s + xml + "er54s4"));
        if (xml != null)
            form.AddField("xml", xml);
        Debug.Log("WWW Sended: " + s);
        var w = new WWW(webserver + s, form);
        _TimerA.AddMethod(() => w.isDone, delegate
        {
            if (w.error == "" || w.error == null)
            {
                Debug.Log("WWW Received: " + w.text);
                a(w.text);
            }
            else
            {
                a(w.error);
                Debug.Log(w.error);
            }
            

        });
    }
    internal static UserView LocalUser { get { return _Loader.UserView; } }
    internal string ip { get { return _ServersWindow.Ipaddress; } set { _ServersWindow.Ipaddress = value; } }
    internal static MapSetting mapSettings { get { return _Loader.mapSettings; } set { _Loader.mapSettings = value; } }
    IEnumerable<string> GetMapsByMode(GameMode gamemode)
    {
        foreach (MapSetting m in _Loader.mapsets)
            if (m.supportedModes.Contains(gamemode))
                yield return m.mapName;
    }
    IEnumerable<string> ParseHosts(HostData[] hosts)
    {
        foreach (var hdp in hosts.Select(a => new { host = a, ping = _Loader.GetPing(a.ip[0]) }).OrderBy(a => a.ping))
        {
            var host = hdp.host;
            string[] data = host.comment.Split(',');
            yield return string.Format(GenerateTable(_ServersWindow.ServersTitle), "", host.gameName, data[0], data[1], host.ip[0], host.connectedPlayers + "/" + host.playerLimit, hdp.ping);
        }
    }
    
}

