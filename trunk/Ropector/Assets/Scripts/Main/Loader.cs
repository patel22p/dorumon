
using System;
using UnityEngine;
using System.Collections.Generic;
using doru;
using System.Collections;
using System.Linq;
public class Loader : bs 
{
    public int currentLevel = -1;
    public int errorcount;
    public int exceptionCount;
    [FindTransform]
    public GUIText info;        
    TimerA timer = new TimerA();

    List<float> avverageFps = ResetFps();
    public int fps = 100;
    static string LastError = "";
    
    public override void Awake()
    {
        Application.RegisterLogCallback(onLog);
        Debug.Log("Loader Awake");
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this.transform.root);
    }
    
    void Update()
    {
        UpdateFpsInfo();
        UpdateOther();
        timer.Update();
    }
    void UpdateOther()
    {
        info.text = "FPS:" + fps + " W:" + errorcount + " E:" + exceptionCount + " " + LastError;
    }
    private void UpdateFpsInfo()
    {
        if (timer.TimeElapsed(1000))
        {
            avverageFps.RemoveAt(0);
            avverageFps.Add((float)timer.GetFps());
            fps = (int)avverageFps.Average();
            if (fps < 40 && QualitySettings.currentLevel != QualityLevel.Fastest)
            {
                Debug.Log("Low");
                avverageFps = ResetFps();
                QualitySettings.DecreaseLevel();
            }
        }
        
    }
    public void NextLevel()
    {
        LoadLevel(currentLevel+1);
    }
    public void ResetLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    public void LoadLevel(int n)
    {
        currentLevel = n;
        Application.LoadLevelAsync(n);
    }
    public static List<float> ResetFps()
    {
        return Enumerable.Repeat(100f, 4).ToList();
    }
    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
        if (type == LogType.Error || type == LogType.Exception)
            LastError = c + stackTrace;
    }
}