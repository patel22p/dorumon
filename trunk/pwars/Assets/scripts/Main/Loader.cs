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
    [FindTransform]
    public GUIText infoText;
    internal string cmd = ""; 
    internal MapSetting mapSettings = new MapSetting();
    internal bool loaded;
    internal int lastLevelPrefix;
    internal int rpcCount;
    public bool disableSounds;
    public float Version = 1;
    public bool stopZombies;
    internal bool Kongregate;
    public bool dontcheckwin;
    public bool completeBuild;
    public bool proxy;
    new public bool build;
    public int buildVersion;
    public string[] ipaddress;
    internal int hostport = 5300;    
    [FindAsset]
    public Material[] playerTextures;
    public bool debugPath;
    public bool disablePathFinding= true;
    public bool loggedin;    
    public bool host;
    [FindAsset]
    public AudioClip ForceField;
    public UserView UserView = new UserView();
    new public Level _Level;
    new public TimerA _TimerA = new TimerA();
    
    public MapSetting[] mapsets = new MapSetting[1];
    [FindAsset("Skin/Skin.guiskin")]
    public GUISkin Skin;
    public override void InitValues()
    {
        mapsets = new MapSetting[]
        {
            new MapSetting(){ title = "Jamo map", mapName = "Pitt"},
            new MapSetting(){ title = "Test map", mapName = "test" }
        };

        foreach (var m in mapsets)
        {
            for (int i = 0; i < m.patrons.Length; i++)
                m.patrons[i] = -1;
            m.patrons[(int)GunType.physxgun] = 30;
            m.patrons[(int)GunType.pistol] = 100;
            m.timeLimit = 99;
            m.zombieDamage = 8;
            m.pointsPerZombie = 2;
            m.haveALaser = false;
            m.pointsPerPlayer = 20;
            m.slow = false;
            m.zombiesAtStart = 20;
            m.StartMoney = 50;
            m.pointsPerStage = 5;
            m.gameMode = GameMode.DeathMatch; 
            m.stage = 0;
        }        
        base.InitValues();
    }
    public override void Awake()
    {
        Application.RegisterLogCallback(_Console.onLog);
        Debug.Log("loader Awake");        
        base.Awake();
        enabled = true;
        Application.targetFrameRate = 60;                
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
        _LoginWindow.Build = "Build " + buildVersion + " Version " + Version;
        Debug.Log(_LoginWindow.Build);        
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
    internal int fps;
    internal float pingAverage;
    void Update()
    {
        infoText.text = "Fps: " + fps + " Errors:" + _Console.exceptionCount + " Ping: " + (int)pingAverage;
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            _Console.enabled = !_Console.enabled;

        _Music.audio.volume = _SettingsWindow.MusicVolume;
        AudioListener.volume = disableSounds ? 0 : _SettingsWindow.SoundVolume;        
        //if (Network.sendRate != _SettingsWindow.NetworkSendRate) Network.sendRate = _SettingsWindow.NetworkSendRate;
        if (!isWebPlayer && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
        {
            var path = curdir + "/ScreenShots/Screenshot" + DateTime.Now.ToFileTime() + ".png";
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
        LoadLevel(level);    

    }

    private void LoadLevel(string level)
    {
        foreach (bs bs in GameObject.FindObjectsOfType(typeof(bs)))
            bs.OnLevelLoading();
        Application.LoadLevel(level);
    }

    
    void OnLevelWasLoaded(int level)
    {        
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
    
    public void Action(string n)
    {
        Debug.Log("Loader Action: " + n);
        if (n == "FullScreen")
            Screen.fullScreen = _SettingsWindow.FullScreen;
        
        if (n == "ScreenSize")
            if (_SettingsWindow.iScreenSize != -1 && _SettingsWindow.iScreenSize < Screen.resolutions.Length)
            {
                print(pr);
                Resolution r = Screen.resolutions[_SettingsWindow.iScreenSize];
                Screen.SetResolution(r.width, r.height, _SettingsWindow.FullScreen);
            }
        if (n == "Shadows")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "Reset")//reset settings
            PlayerPrefs.DeleteAll();
        if (n == "AtmoSphere")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "Sao")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "BloomAndFlares")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "RenderSettings")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "GraphicQuality")
            if (_Cam != null)
                _Cam.onEffect();
        if (n == "Ok")
            _PopUpWindow.Hide();
        if (n == "ShowKeyboard")
            Application.OpenURL("https://picasaweb.google.com/dorumonstr/PhysicsWarsHelp#slideshow/5571650111087233570");

        if (n == "Close" && _Menu != null)
            _MenuWindow.Show();
    }
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    SerializedObject serializedObject;
    public SerializedObject SerializedObject { get { if (serializedObject == null) serializedObject = new SerializedObject(this); return serializedObject; } }
    
    public override void Init()
    {
        Version += 0.01f;
        base.Init();
    }
#endif
    public string curdir { get { return Application.isWebPlayer ? Application.absoluteURL : Directory.GetCurrentDirectory(); } }
    public bool dedicated { get { return _Loader.cmd.Contains("-batchmode"); } }
    public string nickpref { get { return PlayerPrefs.GetString(Application.platform + "nick"); } set { PlayerPrefs.SetString(Application.platform + "nick", value); } }
    string pass;
    public string passpref
    {
        get {
            if (pass == "") Debug.LogWarning("wtf");
            return pass ?? PlayerPrefs.GetString(Application.platform + "passw"); }
        set
        {
            
            PlayerPrefs.SetString(Application.platform + "passw", value);
            pass = value;
            if (value == "" || value == null) Debug.LogWarning("password is set to null");
        }
    }
    public bool guestpref { get { return PlayerPrefs.GetInt(Application.platform + "guest").toBool(); } set { PlayerPrefs.SetInt(Application.platform + "guest", value.toInt()); } }
    internal string passwordHash { get { return Ext.CalculateMD5Hash(passpref); } }
    string mapspath { get { return Application.dataPath + "/../maps.xml"; } }

}

