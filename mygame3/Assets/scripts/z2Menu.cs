﻿using UnityEngine;
using System.Collections;


using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
[assembly: AssemblyVersion("1.0.*")]

public class z2Menu : Base
{
    public string gamename = "Swiborg";
    bool guiloaded;
    public Rect r = new Rect(0, 0, 200, 200);
    public Rect r2;
    internal int port = 5300;
    
    
    public string ip { get { return PlayerPrefs.GetString("ip"); } set { PlayerPrefs.SetString("ip", value); } }
    public string masterip { get { return PlayerPrefs.GetString("ip2"); } set { PlayerPrefs.SetString("ip2", value); } }
    
    public static Dictionary<string, Ping> hdps = new Dictionary<string, Ping>();
    
    

    void Awake()
    {
        Online = true;
        _menu = this;
    }    
    void Start()
    {
        if (!build) GameObject.Find("menu").active = false;
        print("menu start");
        MasterServer.RequestHostList(gamename);

        if (!music.audio.isPlaying) music.audio.Play();
        if (logged)
        {
            //_vk.SetStatus("");                    
            _vk.enabled = _Vkontakte.enabled = true;
            _vk.SetLocalVariable((int)z0Vk.Keys.playerstats, tostring(new object[] { localuser.totalkills, localuser.totaldeaths, localuser.totalzombiekills, localuser.totalzombiedeaths }));
            _vk.KillsTop(true);
            _vk.KillsTop(false);
        }
        else
        {
            _vk.enabled = _Vkontakte.enabled = false;
            if (Nick.Length < 2 || !build) Nick = lc.guest.ToString() + UnityEngine.Random.Range(0, 99);
            
        }
        r2 = CenterRect(.6f, .5f);
        
        //printC(Network.incomingPassword);
    }
    void OnGUI()
    {
        try
        {

            r = GUILayout.Window(2, r, Window, lc.connwin .ToString());
            r2 = GUILayout.Window(1, r2, ServerListWindow, lc.serv .ToString());
            if (!guiloaded)
            {
                GUI.BringWindowToFront(2);
                GUI.BringWindowToFront(1);
                guiloaded = true;
            }
        }
        catch (Exception e) { print(e); }
    }
    int begintime = 20;
    bool isnottime { get { return (_vk.time.Hour < begintime || _vk.time.Hour > begintime + 2); } }
    bool isguest { get { return _vk._Status == z0Vk.Status.disconnected; } }
    public void Window(int id)
    {


        GUILayout.Label(lc.ipaddress.ToString());
        GUILayout.BeginHorizontal();
        ip = GUILayout.TextField(ip);

        int.TryParse(GUILayout.TextField(port.ToString(), 10), out port);
        GUILayout.EndHorizontal();

        if (isnottime &&  timeLimit &&!isguest)
        {
            int timeleft = begintime - _vk.time.Hour;
            if (timeleft < 0) timeleft += 24;
            GUILayout.Label(String.Format(lc.time .ToString(), begintime, (begintime + 2), timeleft));
        }

        if (GUILayout.Button(lc.connect.ToString()))
        {
            if (isguest && build && timeLimit)
                printC(lc.mustlogin);
            else if (isnottime && timeLimit && build)
                printC(lc.timelimit);
            else if (Nick.Length == 0)
                printC(lc.firstname);
            else
            {
                printC(lc.connectingto + ip);
                string[] ips = ip.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Network.Connect(ips, port, Network.incomingPassword);                
            }
        }
        if (!build)
        {
            GUILayout.Label(lc.MasterServer.ToString());
            masterip = GUILayout.TextField(masterip, 20);
        }
        if (!logged)
        {
            GUILayout.Label(lc.name.ToString());
            Nick = GUILayout.TextField(Nick, 20);
        }
        if (GUILayout.Button(lc.host.ToString()) || skip)
            if (Nick.Length > 0)
                _hw.enabled= true;
            else printC(lc.enternickname.ToString());
        GUI.DragWindow();
    }
    
    
    
    int GetPing(string ip)
    {
        Ping p;
        if (!hdps.ContainsKey(ip))
            hdps.Add(ip, p = new Ping(ip));
        else
            p = hdps[ip];

        return p.time;
    }
    void ServerListWindow(int q)
    {
        GUILayout.BeginScrollView(new Vector2());
        GUILayout.Label(lc.yourversionis .ToString() + version);
        if (GUILayout.Button(lc.refreshserver.ToString()))
        {
            if (masterip != "") MasterServer.ipAddress = masterip;            
            MasterServer.RequestHostList(gamename);
        }
        HostData[] data = MasterServer.PollHostList();        
        
        foreach (HostData element in data)
        {
            GUILayout.BeginHorizontal();
            string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
            GUILayout.Label(name);
            GUILayout.Space(5);

            string hostInfo = "[";
            foreach (string host in element.ip)
                hostInfo = hostInfo + host + ":" + element.port + " ";
            hostInfo = hostInfo + "]";
            GUILayout.Label(hostInfo);
            GUILayout.Space(5);
            
            GUILayout.Label(lc.ping .ToString() + GetPing(element.ip[0]));

            
            GUILayout.Space(5);


            if (GUILayout.Button(lc.copyip.ToString()))
            {
                try
                {                                                                
                    ip = string.Join(",", element.ip);
                    port = element.port;
                }
                catch { printC("cannot parse xml"); }

            }
            GUILayout.EndHorizontal();

        }
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
    void OnLevelWasLoaded(int level)
    {
        foreach (Ping p in hdps.Values)
            p.DestroyPing();
        hdps.Clear();
    }
    void OnFailedToConnect(NetworkConnectionError error)
    {
        printC(lc.clnt.ToString() + error.ToString().Replace("InvalidPassword", lc.gamealreadystarted.ToString()));
    }
    void OnFailedToConnectToMasterServer(NetworkConnectionError error)
    {
        printC(lc.clnt.ToString() + error);
    }
}

