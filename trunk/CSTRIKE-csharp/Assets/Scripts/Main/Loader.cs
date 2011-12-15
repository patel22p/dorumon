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
    public Timer timer = new Timer();
    public string[] maps;
    internal bool DebugLevelMode;

    internal string playerName { get { return PhotonNetwork.playerName; } set { PhotonNetwork.playerName = value; } }
    //internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }
    
    
    public override void Awake()
    {                
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
        photonView.group = 1;
        DontDestroyOnLoad(transform.root);
        base.Awake();
    }

    public void OnFailedToConnect_OBSELETE(NetworkConnectionError error)
    {
        Debug.LogWarning("Could not connect to server: " + error);
        _LoaderGui.ShowLoading(false);
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
        //PhotonNetwork.RemoveRPCsInGroup(0);
        //PhotonNetwork.RemoveRPCsInGroup(1);
        photonView.RPC("RPCLoadLevel", PhotonTargets.AllBuffered, level, lastLevelPrefix + 1);
    }
   
    [RPC]
    public IEnumerator RPCLoadLevel(string level, int levelPrefix)
    {
        Debug.Log("LoadLevel: " + level);
        //lastLevelPrefix = levelPrefix;
        //PhotonNetwork.SetSendingEnabled(0, false);
        PhotonNetwork.isMessageQueueRunning = false;
        //PhotonNetwork.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);
        yield return null;
    }
    public void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected "+name);
        _LoaderGui.enabled = true;
        lockCursor = false;
        _LoaderGui.Print("Server closed connection");        
        Application.LoadLevel(0);        
    }
    public void OnLevelWasLoaded(int level)
    {
        Debug.Log(name + " Level Loaded " + level);
        _Loader.LastError = "";
        PhotonNetwork.isMessageQueueRunning = true;
        PhotonNetwork.SetSendingEnabled(0, true);
    }
    void OnConnected()
    {
        Debug.Log("Connected "+name);
    }
    public void OnCreatedRoom() { OnConnected(); }
    public void OnJoinedRoom() { OnConnected(); }



}
