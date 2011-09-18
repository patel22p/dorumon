using System.Linq;
using doru;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;

public class Loader : Bs
{
    public int errorcount;
    public int exceptionCount;
    int lastLevelPrefix;
    public GUIText info;
    static string LastError = "";
    int fps;
    Timer timer = new Timer();
    public string[] maps;

    internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }

    public override void Awake()
    {
        if (Object.FindObjectsOfType(typeof(Loader)).Length > 1) {
            DestroyImmediate(this.gameObject);
            return;
        }
        Application.RegisterLogCallback(onLog);
        Debug.Log("Loader Awake");
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
    public void OnLevelWasLoaded(int level)
    {
        Debug.Log("Level Loaded "+name);
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
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
