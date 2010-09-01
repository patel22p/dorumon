using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using doru;
using System.IO;

public class Loader : Base
{
    public static bool Online;
    public new String disconnectedLevel;
    public static int lastLevelPrefix = 0;
    public int fps;
    public Transform root;
    public string[] supportedNetworkLevels;
    float cmx { get { return Screen.height / 2; } }
    void onLog(string condition, string stackTrace, LogType type)
    {
        
        StreamWriter a;
        using (a = new StreamWriter(File.Open("log.txt", FileMode.Append, FileAccess.Write)))
            a.WriteLine("fps:" + fps + "Type:" + type + "\r\n" + condition + "\r\n" + stackTrace + "\r\n");
    }
    void OnLevelWasLoaded(int level)
    {
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }    
    void Awake()
    {
        if (GameObject.FindObjectsOfType(typeof(Loader)).Length == 2)
        {
            Destroy(this.gameObject);
            return;
        }
        if (Application.platform != RuntimePlatform.WindowsWebPlayer)
        {
            File.Delete("log.txt");
            Application.RegisterLogCallback(onLog);
        }        
        _Loader = this;
        DontDestroyOnLoad(this);
        networkView.group = 1;

        
        if (Application.loadedLevel == 0)
        {
            Application.LoadLevel(disconnectedLevel);
            Online = true;
        }
    }
    void Update()
    {

        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
        _TimerA.Update();

    }
    void OnGUI()
    {
        GUILayout.Label("fps: " + fps);        
        if (lockCursor) return;                

    }
    GuiConnection _GuiConnection { get { return GuiConnection._This; } }
    public GUIStyle _GUIStyle;
    
    public void LoadLevelRPC(string level)
    {
        for (int i = 0; i < 20; i++)
            Network.RemoveRPCsInGroup(i);
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }
    void OnServerInitialized()
    {
        rpcwrite("Player joined " + GuiConnection.Nick);
    }
    
    
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        rpcwrite("Player disconnected " + GuiConnection.Nick);
        write("Disconnected from game:"+info);
        Application.LoadLevel(disconnectedLevel);
    }
    void OnConnectedToServer()
    {
        rpcwrite("Player joined " + GuiConnection.Nick);
    }
    
    [RPC]
    void LoadLevel(String level, int levelPrefix)
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

}

