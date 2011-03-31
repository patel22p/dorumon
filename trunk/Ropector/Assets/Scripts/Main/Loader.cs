
using System;
using UnityEngine;
using System.Collections.Generic;
using doru;
using System.Collections;
using System.Linq;
public class Loader : bs //this class entry point class, also it will not removed when new scene loads(all other objects will be destroyed but this after LoadLevel() Call)
{
    public int currentLevel = -1;
    public int errorcount;
    public int exceptionCount;
    [FindTransform]
    public GUIText info;        
    TimerA timer = new TimerA();

    
    static string LastError = "";
    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
        if (type == LogType.Error || type == LogType.Exception)
            LastError = c + stackTrace;
    }
    public override void Awake()
    {
        Application.RegisterLogCallback(onLog);
        Debug.Log("Loader Awake");
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this.transform.root);
    }
    static string[] lvl { get { return Base.scenes; } }
 
    List<float> avverageFps = ResetFps();
    public static List<float> ResetFps()
    {
        return Enumerable.Repeat(100f, 4).ToList();
    }
    public int fps = 100;
    void Update()
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

        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
            Console.enabled = !Console.enabled;
        }
        if (QualitySettings.currentLevel == QualityLevel.Fastest && Camera.main.renderingPath != RenderingPath.VertexLit)
        {
            Debug.Log("Vertex Lit");
            Camera.main.renderingPath = RenderingPath.VertexLit;
        }
        else if (Camera.main.renderingPath == RenderingPath.VertexLit && QualitySettings.currentLevel != QualityLevel.Fastest)
        {
            Debug.Log("Deffered");
            Camera.main.renderingPath = RenderingPath.DeferredLighting;
        }

        info.text = "FPS:" + fps + " W:" + errorcount + " E:" + exceptionCount + " " + LastError;
        timer.Update();
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
        Application.LoadLevelAsync(lvl[n] + "");
    }
}