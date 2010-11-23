using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Text;

public enum Level { z1login, z2menu, z4game }
public class Loader : Base
{
    public string cmd="";    
    new public UserView[] userViews = new UserView[10];
    public int lastLevelPrefix;
    public Dictionary<string, Ping> hdps = new Dictionary<string, Ping>();
    new public bool build;
    new public bool skip;
    public bool logged;
    new public UserView LocalUserV;
    new public Level _Level;
    new public TimerA _TimerA = new TimerA();
    public List<MapSetting> mapsets = new List<MapSetting>();
    public bool dedicated { get { return _Loader.cmd.Contains("-batchmode"); } }
    new public MapSetting mapSettings = new MapSetting();
    public GUISkin _skin;
    public LayerMask LevelMask;
    new public LayerMask collmask = 1 << 8 | 1 << 9 | 1 << 12 | 1 << 13;
    protected override void Awake()
    {
        LocalUserV = gameObject.AddComponent<UserView>();
        base.Awake();
        _skin = (GUISkin)Resources.Load("Skin/Skin");
        print("loader awake");        
        DontDestroyOnLoad(this.transform.root);
        networkView.group = 1;
        if(!isWebPlayer)
            cmd = joinString(' ', Environment.GetCommandLineArgs());
        mapsets.AddRange(new[]{
            new MapSetting { mapName = "Game", title = "Демо" ,supportedModes = new List<GameMode>() { GameMode.DeathMatch , GameMode.TeamDeathMatch, GameMode.TeamZombieSurvive, GameMode.ZombieSurive } },
            //new MapSetting { mapName = "z5city" , title  = "Город" ,supportedModes = new List<GameMode>() { GameMode.DeathMatch } }
        });
    }
    string curdir { get { return Directory.GetCurrentDirectory(); } }
    void Start()
    {        

        _SettingsWindow.ScreenSize = ToString(Screen.resolutions).ToArray();
        onGraphicQuality();
        if (!isWebPlayer)
        {
            onScreenSize();
            print("Creating Screenshots"+curdir);
            Directory.CreateDirectory(curdir + "/ScreenShots");
            foreach (var a in Directory.GetFiles(curdir + "/ScreenShots").Reverse().Skip(100))
                File.Delete(a);
        }
    }

    void onFullScreen()
    {
        Screen.fullScreen = _SettingsWindow.FullScreen;
    }
    void onGraphicQuality()
    {
        if (_SettingsWindow.iGraphicQuality != -1)
            QualitySettings.currentLevel = (QualityLevel)_SettingsWindow.iGraphicQuality;
    }
    void onScreenSize()
    {
        print(pr);
        if (_SettingsWindow.iScreenSize != -1 && _SettingsWindow.iScreenSize < Screen.resolutions.Length)
        {
            Resolution r = Screen.resolutions[_SettingsWindow.iScreenSize];
            Screen.SetResolution(r.width, r.height, _SettingsWindow.FullScreen);
        }
    }
    void onShadows()
    {
        if (_Cam != null)
            _Cam.onEffect();
    }
    void onReset()
    {
        print(pr);
        PlayerPrefs.DeleteAll();
    }
    void onAtmoSphere()
    {
        if (_Cam != null)
            _Cam.onEffect();
    }
    void onSao()
    {
        if (_Cam != null)
            _Cam.onEffect();
    }
    void onBloomAndFlares()
    {
        if (_Cam != null)
            _Cam.onEffect();
    }
    void onRenderSettings()
    {
        _Cam.onEffect();
    }
    void onOk()
    {
        _PopUpWindow.Hide();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            _Console.enabled = !_Console.enabled;

        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    guiEnabled = !guiEnabled;
        //    if (guiEnabled)
        //        foreach (Base2 m in FindObjectsOfType(typeof(WindowBase)))
        //            m.enabled = m._oldGuiEnabled;
        //    else
        //        foreach (Base2 m in FindObjectsOfType(typeof(WindowBase)))
        //        {                    
        //            m._oldGuiEnabled = m.enabled;
        //            m.enabled = false;
        //        }
        //}
        WWW2.Update();
        AudioListener.volume = _SettingsWindow.SoundVolume;
        if (Network.sendRate != _SettingsWindow.NetworkSendRate) Network.sendRate = _SettingsWindow.NetworkSendRate;
        if(!isWebPlayer && Input.GetKeyDown(KeyCode.F))
            Application.CaptureScreenshot(curdir +"/ScreenShots/Screenshot" + DateTime.Now.ToFileTime() + ".jpg");


        _TimerA.Update();
    }
    

    public void RPCLoadLevel(string level, RPCMode rpcmode)
    {
        print("load Level" + level);
        for (int i = 0; i < 20; i++)
            Network.RemoveRPCsInGroup(i);
        networkView.RPC("LoadLevel", rpcmode, level, lastLevelPrefix + 1);
    }
    [RPC]
    public void LoadLevel(string level, int levelPrefix)
    {
        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);

    }
    void OnLevelWasLoaded(int level)
    {
        _TimerA.Clear();
        foreach (Ping p in hdps.Values)
            p.DestroyPing();
        hdps.Clear();

        try
        {
            _Level = (Level)Enum.Parse(typeof(Level), Application.loadedLevelName);
        }
        catch { _Level = Level.z4game; }
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);

    }    
    
    IEnumerable<string> ToString(Resolution[] t)
    {
        foreach (Resolution a in t)
            yield return a.width + "x" + a.height + " " + a.refreshRate;
    }
    public int GetPing(string ip)
    {
        Ping p;
        if (!hdps.ContainsKey(ip))
            hdps.Add(ip, p = new Ping(ip));
        else
            p = hdps[ip];

        return p.time;
    }

}

