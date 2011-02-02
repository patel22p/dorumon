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
public class Loader : bs
{
    Dictionary<string, Ping> hdps = new Dictionary<string, Ping>();
    int currentmap;
    internal string cmd = ""; 
    string pass;
    internal MapSetting mapSettings = new MapSetting();
    internal bool loaded;
    internal int lastLevelPrefix;
    public bool disableSounds;
    public string version = "PhysxWars";
    public bool stopZombies;
    public bool dontcheckwin;
    new public bool build;
    public string[] ipaddress;
    public int port = 5300;
    [FindAsset]
    public Material[] playerTextures;
    public bool debugPath;
    public bool disablePathFinding= true;
    public bool loggedin;    
    public bool host;
    public UserView UserView = new UserView();
    new public Level _Level;
    new public TimerA _TimerA = new TimerA();
    public MapSetting[] mapsets = new MapSetting[1];
    [FindAsset("Skin/Skin.guiskin")]
    public GUISkin Skin;
    public override void Awake()
    {        
        Debug.Log("loader Awake");
        base.Awake();
        enabled = true;
        Application.targetFrameRate = 60;                
        for (int i = 0; i < mapsets.Length; i++)
            if (mapsets[i].mapName == Application.loadedLevelName)
            {
                currentmap = i; break;
            }
        DontDestroyOnLoad(this.transform.root);
        networkView.group = 1;
        if (!isWebPlayer)
            cmd = joinString(' ', Environment.GetCommandLineArgs());
        else
            cmd = Application.absoluteURL;
    }
    public void Start()
    {
        _TimerA.AddMethod(100, delegate { loaded = true; });
        print("Version " + version);
        Debug.Log("App Path" + curdir);        
        _SettingsWindow.lScreenSize = ToString(Screen.resolutions).ToArray();
        _SettingsWindow.lGraphicQuality = Enum.GetNames(typeof(QualityLevel));
        _SettingsWindow.lRenderSettings = Enum.GetNames(typeof(RenderingPath));
        if (_Cam != null)
            _Cam.onEffect();
        if (!isWebPlayer && !Application.isEditor)
        {
            Action("ScreenSize");            
            Directory.CreateDirectory(curdir + "/ScreenShots");
            foreach (var a in Directory.GetFiles(curdir + "/ScreenShots").Reverse().Skip(100))
                File.Delete(a);
        }
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            _Console.enabled = !_Console.enabled;

        _Music.audio.volume = _SettingsWindow.MusicVolume;
        AudioListener.volume = disableSounds ? 0 : _SettingsWindow.SoundVolume;        
        if (Network.sendRate != _SettingsWindow.NetworkSendRate) Network.sendRate = _SettingsWindow.NetworkSendRate;
        if (!isWebPlayer && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
        {
            var path = curdir + "/ScreenShots/Screenshot" + DateTime.Now.ToFileTime() + ".jpg";
            Debug.Log("sceenshot saved " + path);
            Application.CaptureScreenshot(path);
        }
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

        return p.time == -1 ? 999 : p.time;
    }
    public void Action(string s)
    {
        Debug.Log("Loader Action: " + s);
        if (s == "FullScreen")
            Screen.fullScreen = _SettingsWindow.FullScreen;
        
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
        if (s == "Reset")//reset settings
            PlayerPrefs.DeleteAll();
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
        if (s == "ShowKeyboard")
            _KeyboardWindow.Show(this);
        if (s == "Close" && _Menu != null)
            _MenuWindow.Show();
    }
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    SerializedObject serializedObject;
    public SerializedObject SerializedObject { get { if (serializedObject == null) serializedObject = new SerializedObject(this); return serializedObject; } }
    public override void InitValues()
    {
        mapsets = new MapSetting[]
        {
            new MapSetting(){ title = "Jamo map", mapName = "Pitt"},
            new MapSetting(){ title = "Test map", mapName = "test" }
        };

        foreach (var a in mapsets)
        {
            for (int i = 0; i < a.patrons.Length; i++)
                a.patrons[i] = -1;
            a.patrons[(int)GunType.physxgun] = 40;
            a.patrons[(int)GunType.pistol] = 30;
        }
        base.InitValues();
    }
    public override void Init()
    {
        version = DateTime.Now.ToString();
        base.Init();
    }
#endif
    public string curdir { get { return Application.isWebPlayer ? Application.absoluteURL : Directory.GetCurrentDirectory(); } }
    public bool dedicated { get { return _Loader.cmd.Contains("-batchmode"); } }
    public string nickpref { get { return PlayerPrefs.GetString(Application.platform + "nick"); } set { PlayerPrefs.SetString(Application.platform + "nick", value); } }
    public string passpref { get { return pass ?? PlayerPrefs.GetString(Application.platform + "passw"); } set { PlayerPrefs.SetString(Application.platform + "passw", value); pass = value; } }
    public bool guestpref { get { return PlayerPrefs.GetInt(Application.platform + "guest").toBool(); } set { PlayerPrefs.SetInt(Application.platform + "guest", value.toInt()); } }
    internal string passwordHash { get { return Ext.CalculateMD5Hash(passpref); } }
    string mapspath { get { return Application.dataPath + "/../maps.xml"; } }

}

