using System.Linq;
using doru;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using Object= UnityEngine.Object;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Loader : Bs
{
    public new bool Android = true;
    public GUISkin skin { get { return Android ? AndroidSkin : Defaultskin; } }
    public GUISkin Defaultskin;
    public GUISkin AndroidSkin;    
    public int errorcount;
    public int exceptionCount;
    int lastLevelPrefix;
    public GUIText info;
    string LastError = "";
    int fps;
    Timer timer = new Timer();
    public string[] maps;
    internal bool DebugLevelMode;
    internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }
    
    
    public override void Awake()
    {
        PlayerPrefs.DeleteAll();
        if (Object.FindObjectsOfType(typeof(Loader)).Length > 1)
        {
            Debug.Log("Destroyed Loader Dub");
            DestroyImmediate(this.gameObject);
            return;
        }
   
        Debug.Log("Loader Awake");
        Application.RegisterLogCallback(onLog);
        if (Application.loadedLevel != 0)
        {
            Debug.Log("DebugLevelMode");
            DebugLevelMode = true;            
        }        
        networkView.group = 1;
        DontDestroyOnLoad(transform.root);
        base.Awake();
    }
    

    public void Update()
    {
        fps = (int)timer.GetFps();
        info.text = "FPS:" + fps + " Warnings:" + errorcount + " Errors:" + exceptionCount + " " + LastError;
        timer.Update();
    }

    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
        
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Warning)
            LastError = c.Substring(0, Math.Min(c.Length, 60));
    }

    public void LoadLevel(string level)
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("RPCLoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }
   
    [RPC]
    public IEnumerator RPCLoadLevel(string level, int levelPrefix)
    {
        Debug.Log("LoadLevel: " + level);
        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);
        yield return null;
    }
    public void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected "+name);
        _LoaderGui.enabled = true;
        _LoaderGui.Print("Server closed connection");        
        Application.LoadLevel(0);
        
    }
    public void OnLevelWasLoaded(int level)
    {
        Debug.Log(name + " Level Loaded " + level);
        _Loader.LastError = "";
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }
    void OnConnected()
    {
        Debug.Log("Connected "+name);
    }
    public void OnServerInitialized() { OnConnected(); }
    public void OnConnectedToServer() { OnConnected(); }



}
