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

    //internal string playerName { get { return PhotonNetwork.playerName; } set { PhotonNetwork.playerName = value; } }
    internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "User" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }

    public bool EnableBlood { get { return PlayerPrefs.GetInt("EnableBlood", 1) == 1; } set { PlayerPrefs.SetInt("EnableBlood", value ? 1 : 0); } }
    public bool EnableHighQuality { get { return PlayerPrefs.GetInt("EnableHighQuality", 0) == 1; } set { PlayerPrefs.SetInt("EnableHighQuality", value ? 1 : 0); } }
    public float SensivityX { get { return PlayerPrefs.GetFloat("sx", 1); } set { PlayerPrefs.SetFloat("sx", value); } }
    public float SensivityY { get { return PlayerPrefs.GetFloat("sy", 1); } set { PlayerPrefs.SetFloat("sy", value); } }
    public float SoundVolume { get { return PlayerPrefs.GetFloat("SoundVolume ", 1); } set { PlayerPrefs.SetFloat("SoundVolume ", value); } }

    
    
    
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
        Debug.Log("LoadLevel: " + level);
        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel(level);
    }


    public void OnLevelWasLoaded(int level)
    {
        Debug.Log(name + " Level Loaded " + level);
        PhotonNetwork.isMessageQueueRunning = true;
    }
    
    



}
