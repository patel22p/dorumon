
using Random = UnityEngine.Random;
using System;
using UnityEngine;
using System.Collections.Generic;
using doru;
using System.Collections;
using System.Linq;
using System.IO;
//using System.Net;
public class Loader : bs 
{
    //public MemoryStream map = new MemoryStream();    
    public string host = "http://127.0.0.1:5600/";
    public int errorcount;
    public int exceptionCount;
    public int totalScores;    
    public GUIText info;        
    public TimerA timer = new TimerA();
    public string nick;
    public bool EditorTest;    
    public int fps = 100;
    static string LastError = "";
    
    public override void Awake()
    {        
        Debug.Log("Loader Load");
        nick = "Guest" + " (" + Random.Range(0, 99) + ")";
        networkView.group = 1;
        Application.RegisterLogCallback(onLog);
        Debug.Log("Loader Awake");
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this.transform.root);
    }
    
    void Update()
    {
        if (timer.TimeElapsed(1000))
            fps = (int)timer.GetFps();
        UpdateOther();
        timer.Update();
    }
    void UpdateOther()
    {
        info.text = "FPS:" + fps + " Warnings:" + errorcount + " Errors:" + exceptionCount + " " + LastError;
    }
    public void NextLevel()
    {
        if (_MyGui.LoopSameLevel)
            LoadLevel(Application.loadedLevel);
        else 
        {
            var i = Application.loadedLevel + 1;
            if (i > Application.levelCount - 1) i = 1;
            LoadLevel(i);
        }
    }
    
    int lastLevelPrefix;
    public void LoadLevel(int level)
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("RPCLoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
    }
    [RPC]
    public IEnumerator RPCLoadLevel(int level, int levelPrefix)
    {
        Debug.Log("LoadLevel: " + level);
        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);
        yield return null;
    }
    void OnLevelWasLoaded(int level)
    {
        Debug.Log("Level Loaded");
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }

    
    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
        if (type == LogType.Exception)
            LastError = c + stackTrace;
    }
}