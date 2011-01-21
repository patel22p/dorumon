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

//[assembly: AssemblyVersion("1.0.*")] // C:\Documents and Settings\<USERNAME>\Local Settings\temp\UnityWebPlayer\log

public class Menu : Base
{
    public string gameVersionName = "Swiborg3";
    public string ip { get { return _ServersWindow.Ipaddress; } set { _ServersWindow.Ipaddress = value; } }
    public bool enableIrc;
    
    public override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
 
        lockCursor = false;
        if (_Loader.dedicated)
        {
            Dedicated();
            return;
        }
        foreach (WindowBase o in Component.FindObjectsOfType(typeof(WindowBase))) o.SendMessage("HideWindow", SendMessageOptions.DontRequireReceiver);
        if (_Loader.logged)
            onLogin();
        else
            _LoginWindow.Show(this);
        onRefresh();
        Debug.Log(GameObject.Find("RenderCam") == null);
        Debug.Log(_LoginWindow == null);
        Debug.Log(_LoginWindow.ImageImage8 == null);
        _LoginWindow.ImageImage8 = GameObject.Find("RenderCam").camera.targetTexture;
    }
    
    private void Dedicated()
    {
        nick = "Server";
        mapSettings = TakeRandom(_Loader.mapsets);
        mapSettings.gameMode = TakeRandom(mapSettings.supportedModes);
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(mapSettings.maxPlayers, mapSettings.port, useNat);
        MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(gameVersionName, "DedicatedServer", mapSettings.mapName + "," + _HostWindow.GameMode[_HostWindow.iGameMode]);
        _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
    }
    public HostData[] hosts { get { return MasterServer.PollHostList(); } }
    void Update()
    {
        if (_TimerA.TimeElapsed(1000))
            _ServersWindow.ServersTable = ParseHosts(hosts).ToArray();
    }
    bool logged;
    [FindTransform(scene = true)]
    public GameObject Sphere;
    int sl;
    public void Action(string n)
    {
        if (n == "Next")
            sl++;
        if (n == "Prev")
            sl--;
        var pt =_Loader.playerTextures;
        Sphere.renderer.material = pt[Math.Abs(sl) % pt.Length];

        if (n == "About")
            _AboutWindow.Show(this);
        if (n == "LogOut")
            _LoginWindow.Show(this);
        if (n == "EnterAsGuest")
            onEnterAsGuest();
        if (n == "Login")
            onLogin();
        if (n == "Close")
            onLogin();
        if (n == "Create")
        {
            _HostWindow.Show(this);
            onGameMode();
        }
        if (n == "GameMode")
            onGameMode();
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
            _ServersWindow.Ipaddress = hosts[_ServersWindow.iServersTable].ip[0];
            _ServersWindow.Port = hosts[_ServersWindow.iServersTable].port;
            //_ServersWindow.Ipaddress = _ServersWindow.ServersTable[_ServersWindow.iServersTable];
        }
    }

    private void onEnterAsGuest()
    {
        if (_LoginWindow.Nick.Length < 4)
            ShowPopup("Your nick is to short");
        else
        {
            nick = _LoginWindow.Nick;
            _Loader.logged = true;
            onLogin();
        }
    }

    private void onStartServer()
    {
        if (_HostWindow.Name.Length < 4) ShowPopup("Game name is to short");
        else
        {
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == _HostWindow.Map[_HostWindow.iMap]);
            mapSettings.gameMode = GetGameMode();
            mapSettings.fragZLimit = _HostWindow.MaxFrags;
            mapSettings.maxPlayers = _HostWindow.MaxPlayers;
            mapSettings.timeLimit = _HostWindow.MaxTime;
            mapSettings.port = _HostWindow.Port;
            mapSettings.host = true;
            bool useNat = !Network.HavePublicAddress();
            Network.InitializeServer(mapSettings.maxPlayers, mapSettings.port, useNat);
            MasterServer.RegisterHost(gameVersionName, _HostWindow.Name, mapSettings.mapName + "," + _HostWindow.GameMode[(int)mapSettings.gameMode]);
            _Loader.RPCLoadLevel(mapSettings.mapName, RPCMode.AllBuffered);
        }
    }

    private static void onConnect()
    {
        mapSettings.ipaddress = _ServersWindow.Ipaddress.Split(',');
        mapSettings.port = _ServersWindow.Port;
        mapSettings.host = false;
        Network.Connect(mapSettings.ipaddress, _ServersWindow.Port);
        _ServersWindow.Hide();
    }

    private void onRefresh()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(gameVersionName);
    }

    private void onGameMode()
    {
        _HostWindow.Map = GetMaps(GetGameMode()).ToArray();
        _HostWindow.iMap = 0;
    }

    private void onLogin()
    {
        if (!logged)
        {
            _Irc.ircNick = nick;
            if (enableIrc) _Irc.enabled = true;
        }
        logged = true;
        _MenuWindow.Show(this);
    }
    GameMode GetGameMode()
    {
        return GameMode.DeathMatch; //(GameMode)Enum.GetValues(typeof(GameMode)).GetValue_HostWindow.iGameMode);
    }
    IEnumerable<string> GetMaps(GameMode gamemode)
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

    //void OnFailedToConnect(NetworkConnectionError error)
    //void OnFailedToConnectToMasterServer(NetworkConnectionError error)
}

