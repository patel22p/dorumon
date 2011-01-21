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
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
using UnityEditor;
#endif
using System.Runtime.Serialization.Formatters.Binary;
public enum Level { z1login, z2menu, z4game }
public class Loader : Base
{
    public string version;
    public string cmd="";    
    public int lastLevelPrefix;
    public Dictionary<string, Ping> hdps = new Dictionary<string, Ping>();
    new public bool build;
    new public bool skip;    
    public bool dontcheckwin;
    [FindAsset]
    public Material[] playerTextures;
    public bool debugPath;
    public bool disablePathFinding= true;
    public bool logged;
    public UserView userView = new UserView();
    new public Level _Level;
    new public TimerA _TimerA = new TimerA();
    public List<MapSetting> mapsets = new List<MapSetting>();
    public bool dedicated { get { return _Loader.cmd.Contains("-batchmode"); } }
    new public MapSetting mapSettings = new MapSetting();
    [FindAsset("Skin/Skin.guiskin")]
    public GUISkin Skin;
    //new public LayerMask collmask = 1 << 8 | 1 << 9 | 1 << 12 | 1 << 13;
    public override void Awake()
    {
        Debug.Log("loader Awake");
        Application.targetFrameRate = 60;                
        base.Awake();        
        DontDestroyOnLoad(this.transform.root);
        networkView.group = 1;
        if (!isWebPlayer)
            cmd = joinString(' ', Environment.GetCommandLineArgs());
        else
            cmd = Application.absoluteURL;
        if (dedicated)
            using (var s = File.OpenRead(mapspath))
                mapsets = (List<MapSetting>)xml.Deserialize(s);
    }
    string mapspath { get { return Application.dataPath + "/../maps.xml"; } }
    XmlSerializer xml = new XmlSerializer(typeof(List<MapSetting>), new Type[] { typeof(MapSetting) });
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    public override void Init()
    {
        mapsets.Clear();
        mapsets.AddRange(new[]{
            new MapSetting { mapName = "Arena", title = "Arena" ,supportedModes = new []{ GameMode.DeathMatch } },
            new MapSetting { mapName = "Pitt", title = "Pitt" ,supportedModes = new []{ GameMode.DeathMatch , GameMode.TeamDeathMatch, GameMode.ZombieSurive } },
            new MapSetting { mapName = "test", title = "test" ,supportedModes = new []{ GameMode.DeathMatch , GameMode.TeamDeathMatch, GameMode.ZombieSurive } },
        });

        using (var s = File.Open(mapspath, FileMode.Create))
            xml.Serialize(s, mapsets);

        version = DateTime.Now.ToString();
        base.Init();
    }
#endif
    string curdir { get { return Directory.GetCurrentDirectory(); } }
    protected override void Start()
    {
        print("Version " + version);
        _SettingsWindow.ScreenSize = ToString(Screen.resolutions).ToArray();
        Action("onGraphicQuality");
        if (!isWebPlayer)
        {
            Action("onScreenSize");            
            Directory.CreateDirectory(curdir + "/ScreenShots");
            foreach (var a in Directory.GetFiles(curdir + "/ScreenShots").Reverse().Skip(100))
                File.Delete(a);
        }
    }
    public void Action(string s)
    {
        if (s == "FullScreen")
            Screen.fullScreen = _SettingsWindow.FullScreen;
        if (s == "GraphicQuality")
            if (_Cam != null)
                _Cam.onEffect();
        if (s == "ScreenSize")
            if (_SettingsWindow.iScreenSize != -1 && _SettingsWindow.iScreenSize < Screen.resolutions.Length)
            {
                print(pr);
                Resolution r = Screen.resolutions[_SettingsWindow.iScreenSize];
                Screen.SetResolution(r.width, r.height, _SettingsWindow.FullScreen);
            }
        if (s == "Shadows")
            if (_Cam != null)
                _Cam.onEffect();
        if (s == "Reset")
        {
            PlayerPrefs.DeleteAll();
            _SettingsWindow.enabled = false;
        }
        if (s == "AtmoSphere")
            if (_Cam != null)
                _Cam.onEffect();
        if (s == "Sao")
            if (_Cam != null)
                _Cam.onEffect();
        if (s == "BloomAndFlares")
            if (_Cam != null)
                _Cam.onEffect();
        if (s == "RenderSettings")
            _Cam.onEffect();
        if (s == "Ok")
            _PopUpWindow.Hide();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            _Console.enabled = !_Console.enabled;

        AudioListener.volume = _SettingsWindow.SoundVolume;
        if (Network.sendRate != _SettingsWindow.NetworkSendRate) Network.sendRate = _SettingsWindow.NetworkSendRate;
        if (!isWebPlayer && Input.GetKeyDown(KeyCode.E))
            Application.CaptureScreenshot(curdir + "/ScreenShots/Screenshot" + DateTime.Now.ToFileTime() + ".jpg");

        _TimerA.Update();
        //WWW2.Update();
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

