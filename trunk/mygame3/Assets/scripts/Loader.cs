using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

public enum Level { z1login , z2menu, z3labby, z4game }
public class Loader : Base
{
    public static int lastLevelPrefix = 0;
    public int fps;
    public Transform root;    
    float cmx { get { return Screen.height / 2; } }
    public Font font;    
    public string ips = "";
    Menu _GuiConnection { get { return Menu._menu; } }
    public GUIStyle _GUIStyle;

    void Awake()
    {
        
        if (GameObject.FindObjectsOfType(typeof(Loader)).Length == 2)
        {
            enabled = false;
            Destroy(this.gameObject);            
            return;
        }
        if (Application.platform != RuntimePlatform.WindowsWebPlayer)
        {
            //print(Directory.GetCurrentDirectory());
            File.Delete("log.txt");
            Application.RegisterLogCallback(onLog);
        }        
        _Loader = this;
 

        DontDestroyOnLoad(this);
        networkView.group = 1;
        

        localuser = new Vk.user();

        foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            ips += ip + ",";

        
    }
    void Update()
    {        
        WWW2.Update();
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        _TimerA.Update();

    }
    void OnGUI()
    {
        GUI.skin.font = font;
        GUILayout.Label("fps: " + fps);        
        if (lockCursor) return;                

    }
    public void RPCLoadLevel(string level)
    {
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
        _Level = (Level)Enum.Parse(typeof(Level), Application.loadedLevelName);
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }
}

