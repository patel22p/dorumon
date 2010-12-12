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

[assembly: AssemblyVersion("1.0.*")] // C:\Documents and Settings\<USERNAME>\Local Settings\temp\UnityWebPlayer\log

public class Menu : Base
{
    public string gameVersionName = "Swiborg2";
    public string ip { get { return _ServersWindow.Ipaddress; } set { _ServersWindow.Ipaddress = value; } }
    public bool enableIrc;    
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
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
        _IrcChatWindow.Msgs = _Irc.msg;
        _IrcChatWindow.Users = _Irc.users;
        if (_TimerA.TimeElapsed(1000))
        {
            _ServersWindow.ServersTable = ParseHosts(hosts).ToArray();
        }
    }
    void onAbout()
    {
        _AboutWindow.Show(this);
    }
    void onIrcChat()
    {
        _IrcChatWindow.Show(this);
    }
    void onIrcSend()
    {
        _Irc.SendIrcMessage(_IrcChatWindow.Input.Trim());        
        _IrcChatWindow.Input = "";
    }
    void onLogOut()
    {
        _LoginWindow.Show(this);
    }
    void onEnterAsGuest()
    {
        if (_LoginWindow.Nick.Length < 4)
            ShowPopup("Ваш ник слишком короток");
        else
        {
            nick = _LoginWindow.Nick;
            _Loader.logged = true;
            onLogin();
        }
    }
    bool logged;
    void onLogin()
    {
        print(pr);
        if (!logged)
        {
            _Irc.ircNick = nick;
            if (enableIrc) _Irc.enabled = true;
        }
        logged = true;
        _MenuWindow.Show(this);
    }
    
    void onClose()
    {
        print(pr);
        onLogin();        
    }
    void onCreate()
    {
        _HostWindow.Show(this);        
        onGameMode();
    }
    void onGameMode()
    {
        _HostWindow.Map = GetMaps(GetGameMode()).ToArray();                
        _HostWindow.iMap = 0;        
    }
    GameMode GetGameMode()
    {
        return (GameMode)Enum.GetValues(typeof(GameMode)).GetValue(_HostWindow.iGameMode);
    }
    void onStartServer()
    {
        if (_HostWindow.Name.Length < 4) ShowPopup("Название игры слишком короткое");
        else
        {
            mapSettings = _Loader.mapsets.FirstOrDefault(a => a.mapName == _HostWindow.Map[_HostWindow.iMap]);
            mapSettings.gameMode = GetGameMode();
            mapSettings.fragLimit = _HostWindow.MaxFrags;
            mapSettings.maxPlayers = _HostWindow.MaxPlayers;
            mapSettings.timeLimit = _HostWindow.MaxTime;
            mapSettings.port = _HostWindow.Port;
            mapSettings.host = true;            
            bool useNat = !Network.HavePublicAddress();            
            Network.InitializeServer(mapSettings.maxPlayers, mapSettings.port, useNat);
            MasterServer.RegisterHost(gameVersionName, _HostWindow.Name, mapSettings.mapName + "," + _HostWindow.GameMode[(int)mapSettings.gameMode]);
            _Loader.RPCLoadLevel(mapSettings.mapName,RPCMode.AllBuffered);            
        }
    }
    void onConnect()
    {
        mapSettings.ipaddress = _ServersWindow.Ipaddress.Split(',');
        mapSettings.port = _ServersWindow.Port;
        mapSettings.host = false;
        Network.Connect(mapSettings.ipaddress, _ServersWindow.Port);
        _ServersWindow.Hide();
        
    }
    IEnumerable<string> GetMaps(GameMode gamemode)
    {
        foreach (MapSetting m in _Loader.mapsets)
            if (m.supportedModes.Contains(gamemode))
                yield return m.mapName;
    }
    void onSettings()
    {
        _SettingsWindow.Show(_Loader);        
    }
    void onServers()
    {
        _ServersWindow.Show(this);

    }
    IEnumerable<string> ParseHosts(HostData[] hosts)
    {
        foreach (HostData host in hosts)
        {
            string[] data =host.comment.Split(',');
            yield return string.Format(GenerateTable(_ServersWindow.ServersTitle), "", host.gameName, data[0], data[1], host.connectedPlayers + "/" + host.playerLimit, _Loader.GetPing(host.ip[0]));
        }
    }
    void onRefresh()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(gameVersionName);
    }
    
    void onServersTable()
    {
        _ServersWindow.Ipaddress = hosts[_ServersWindow.iServersTable].ip[0];
        _ServersWindow.Port = hosts[_ServersWindow.iServersTable].port;
        //_ServersWindow.Ipaddress = _ServersWindow.ServersTable[_ServersWindow.iServersTable];

    }
        //    if (masterip != "") MasterServer.ipAddress = masterip;            
        
        //
    

    //void OnFailedToConnect(NetworkConnectionError error)
    //void OnFailedToConnectToMasterServer(NetworkConnectionError error)
}

