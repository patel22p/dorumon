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
    public string[] ipaddress;
    public int port = 5300;
    public bool debugPath;
    public bool disablePathFinding= true;
    public bool loggedin;
    internal string password;
    public bool host;
    public UserView UserView;//{ get { return userView; } set { CopyS(value, userView); } }
    new public Level _Level;
    new public TimerA _TimerA = new TimerA();
    public List<MapSetting> mapsets = new List<MapSetting>();
    public bool dedicated { get { return _Loader.cmd.Contains("-batchmode"); } }
    new public MapSetting mapSettings { get { return mapsets[currentmap]; } set { mapsets[currentmap] = value; } }
    public int currentmap;
    [FindAsset("Skin/Skin.guiskin")]
    public GUISkin Skin;
    public override void Awake()
    {
        
        Debug.Log("loader Awake");
        base.Awake();
        enabled = true;
        Application.targetFrameRate = 60;                
        for (int i = 0; i < mapsets.Count; i++)
        {
            if (mapsets[i].mapName == Application.loadedLevelName)
                currentmap = i;
        }
        DontDestroyOnLoad(this.transform.root);
        networkView.group = 1;
        if (!isWebPlayer)
            cmd = joinString(' ', Environment.GetCommandLineArgs());
        else
            cmd = Application.absoluteURL;
    }
    string mapspath { get { return Application.dataPath + "/../maps.xml"; } }
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    SerializedObject serializedObject;
    public SerializedObject SerializedObject { get { if (serializedObject == null) serializedObject = new SerializedObject(this); return serializedObject; } }
    public override void Init()
    {
        foreach (var a in mapsets)
        {
            for (int i = 0; i < a.patrons.Length; i++)
                a.patrons[i] = -1;
            a.patrons[(int)GunType.physxgun] = 50;
            a.patrons[(int)GunType.pistol] = 20;
            a.patrons[(int)GunType.shotgun] = 20;
        }

        version = DateTime.Now.ToString();
        base.Init();
    }
#endif
    string curdir { get { return Directory.GetCurrentDirectory(); } }
    protected override void Start()
    {
        print("Version " + version);
        _SettingsWindow.lScreenSize = ToString(Screen.resolutions).ToArray();
        _SettingsWindow.lGraphicQuality = Enum.GetNames(typeof(QualityLevel));
        _SettingsWindow.lRenderSettings = Enum.GetNames(typeof(RenderingPath));
        Action("onGraphicQuality");
        if (!isWebPlayer)
        {
            Action("onScreenSize");            
            Directory.CreateDirectory(curdir + "/ScreenShots");
            foreach (var a in Directory.GetFiles(curdir + "/ScreenShots").Reverse().Skip(100))
                File.Delete(a);
        }
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
    
    public void Action(string s)
    {
        Debug.Log("Loader Action: " + s);
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
            _SettingsWindow.Close(); ;
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
        if (s == "Close" && _Menu != null)
            _MenuWindow.Show();
    }
}

