
using Random = UnityEngine.Random;
using System;
using UnityEngine;
using System.Collections.Generic;
using doru;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
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
    
    public bool EditorTest;    
    public int fps = 100;
    static string LastError = "";
    

    public override void Awake()
    {        
        Debug.Log("Loader Load");        
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
    

    

    int lastLevelPrefix;
    public void StartGame(MemoryStream mapdata)
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        //StringWriter sw= new StringWriter();        
        networkView.RPC("RPCLoadLevel", RPCMode.AllBuffered, mapdata.ToArray(), lastLevelPrefix + 1);
    }
    [RPC]
    public IEnumerator RPCLoadLevel(byte[] mapdata, int levelPrefix)
    {
        Map.Dispose();
        Map =  new MemoryStream(mapdata);     
        lastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel((int)Scene.Game);        
        yield return null;
    }
    void OnLevelWasLoaded(int level)
    {
        Debug.Log("Level Loaded");
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
    }
    public void LoadMap(Action onLoaded)
    {
        var w = new WWW(_Loader.host + "index.php?open=1&mapname=" + _Loader.mapName + "&r=" + Random.Range(0, 999));
        timer.AddMethod(() => w.isDone == true, delegate
        {
            Debug.Log("Loaded:" + w.text);
            if (w.text == "" || w.text == "NotFound" || w.error != null)
            {
                _Popup.ShowPopup("Map Not Found");
                Network.Disconnect();
            }
            else
            {
                _Loader.Map.Dispose();
                _Loader.Map = new MemoryStream(w.bytes);
                onLoaded();
            }            
        });
    }
    
    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
        if (type == LogType.Exception)
            LastError = c + stackTrace;
    }
    public string nick
    {
        get
        {
            return PlayerPrefs.GetString(Application.dataPath + "nick", "Nick" + UnityEngine.Random.Range(0, 99));
        }
        set
        {
            value = Regex.Replace(value, "[^qwertyuiopasdfghjklzxcvbnm1234567890]", "", RegexOptions.IgnoreCase);
            PlayerPrefs.SetString(Application.dataPath + "nick", value);
        }
    }
    internal string mapName
    {
        get
        {
            return PlayerPrefs.GetString(Application.dataPath + "mapname");
        }
        set
        {
            value = Regex.Replace(value, "[^qwertyuiopasdfghjklzxcvbnm1234567890]", "", RegexOptions.IgnoreCase);
            PlayerPrefs.SetString(Application.dataPath + "mapname", value);
        }
    }
    public MemoryStream Map = new MemoryStream();
}