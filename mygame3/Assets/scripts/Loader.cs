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

public enum Level { z1login, z2menu, z3labby, z4game }
public class Loader : Base
{
    public static int lastLevelPrefix = 0;
    public int fps;
    public Transform root;
    float cmx { get { return Screen.height / 2; } }
    public Font font; 
    public string ips = "";
    Menu _GuiConnection { get { return Menu._menu; } }

    XmlSerializer xml = new XmlSerializer(typeof(Localize));

    void Awake()
    {
        if (Duplicate()) return;
        print("loader awake");
        if (isWebPlayer)
        {
            new WWW2("dict.xml").done += delegate(WWW2 w)
            {
                try
                {
                    using (Stream s = new MemoryStream(w.bytes))
                        lc = (Localize)xml.Deserialize(s);
                }
                catch
                {
                    printC("Dictionary could not be loaded");
                }
                dictLoaded();
            };
        }
        else
        {
            print(Directory.GetCurrentDirectory());
            try
            {
                using (Stream s = File.Open("dict.xml", FileMode.Open))
                    lc = (Localize)xml.Deserialize(s);
                print("dict loaded");
            }
            catch
            {
                using (Stream s = File.Open("dict.xml", FileMode.Create))
                    xml.Serialize(s, lc);
                print("dict created");
            }            
            File.Delete("log.txt");
            Application.RegisterLogCallback(onLog);
            dictLoaded();
        }        
        _Loader = this;
        _options = this.GetComponent<OptionsWindow>();

        DontDestroyOnLoad(this);
        networkView.group = 1;


        localuser = new Vk.user();

        foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            ips += ip + ",";
    }

    private void dictLoaded()
    {
        ConsoleWindow.output += lc.onload;
    }


    void Start()
    {


    }
    void Update()
    {

        WWW2.Update();
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        _TimerA.Update();

    }
    public GUISkin skin;
    void OnGUI()
    {

        GUI.skin.font = font;
        GUILayout.Label(lc.fps .ToString()+ fps);
        if (lockCursor) return;

    }
    public void RPCLoadLevel(string level)
    {
        print("load Level" + level);
        for (int i = 0; i < 20; i++)
            Network.RemoveRPCsInGroup(i);
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }

    [RPC]
    void LoadLevel(string level, int levelPrefix)
    {
        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);

    }


    public Rect dockup()
    {
        Vector2 c = new Vector2(Screen.width, Screen.height);
        return new Rect(0, 0, c.x, c.y - cmx);
    }
    public Rect dockdown()
    {
        Vector2 c = new Vector2(Screen.width, Screen.height);
        return new Rect(0, c.y - cmx, c.x, c.y);
    }
    void onLog(string condition, string stackTrace, LogType type)
    {
        StreamWriter a;
        using (a = new StreamWriter(File.Open("log.txt", FileMode.Append, FileAccess.Write)))
            a.WriteLine("fps:" + fps + "Type:" + type + "\r\n" + condition + "\r\n" + stackTrace + "\r\n");

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
}

