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
    //todo version
    
    public int errorcount;
    public int exceptionCount;
    int lastLevelPrefix;
    public GUIText info;
    static string LastError = "";
    int fps;
    Timer timer = new Timer();
    public string[] maps;
    internal bool DebugLevelMode;
    string NickNameEditor = "Editor" + new System.Random().Next(99);
    internal string playerName { get { return isEditor ? NickNameEditor : PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }

    public override void Awake()
    {
        MasterServer.ipAddress = "127.0.0.1";
        MasterServer.port = 23466;
        if (Object.FindObjectsOfType(typeof(Loader)).Length > 1)
        {
            Debug.Log("Destroyed Loader Dub");
            DestroyImmediate(this.gameObject);
            return;
        }
        Debug.Log("Loader Awake");
        if (Application.loadedLevel != 0)
        {
            Debug.Log("DebugLevelMode");
            DebugLevelMode = true;
            return;
        }
        Application.RegisterLogCallback(onLog);
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
        if (type == LogType.Exception)
            LastError = c + stackTrace;
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
        Application.LoadLevel(0);
        
    }
    void OnConnected()
    {
        Debug.Log("Connected "+name);
    }
    public void OnServerInitialized() { OnConnected(); }
    public void OnConnectedToServer() { OnConnected(); }



}
