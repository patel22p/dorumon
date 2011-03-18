using System;
using UnityEngine;
using System.Collections.Generic;
using doru;

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
        _MenuWindow.lQualitySettings = Enum.GetNames(typeof(QualityLevel));
        RefreshRecords();
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this.transform.root);
    }
    int[] lvl = new int[] { 9,10, 1, 2, 3, 4, 5, 6, 7, 8 };
    public void RefreshRecords()
    {
        List<string> fls = new List<string>();
        for (int n = 0; n < lvl.Length; n++)
        {
            var i = lvl[n];
            var f = PlayerPrefs.GetFloat(i + "");
            var loadProgress = (int)(Application.GetStreamProgressForLevel(i + "") * 100);
            fls.Add(string.Format(CreateTable(_MenuWindow.Tabble), "", n + 1, TimeToSTr(f), loadProgress));
        }        
        _MenuWindow.lSelectLevel = fls.ToArray();
    }
 
    void Action(MenuWindowEnum s) //this function called when menu button pressed
    {        
        if (s == MenuWindowEnum.NewGame)
            LoadLevel(0);
        if (s == MenuWindowEnum.QualitySettings)
            QualitySettings.currentLevel = (QualityLevel)_MenuWindow.iQualitySettings;           
        if (s == MenuWindowEnum.SelectLevel)
            LoadLevel(_MenuWindow.iSelectLevel);
        if (s == MenuWindowEnum.DisableMusic)
        {
            Music.audio.Stop();
            Music.enabled = false;
        }
        if (s == MenuWindowEnum.DisableSounds)
            AudioListener.volume = _MenuWindow.DisableSounds ? 0 : 1;
    }
    int fps;
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
            Console.enabled = !Console.enabled;
            Debug.Log("sad");
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

        if (_MenuWindow.enabled && timer.TimeElapsed(1000))
            RefreshRecords();

        if(timer.TimeElapsed(1000))
           fps = (int)timer.GetFps() ;

        info.text = "FPS:"+fps + " W:" + errorcount + " E:" + exceptionCount+ " "+ LastError;
        if (Input.GetKeyDown(KeyCode.Escape))
            _MenuWindow.Show(this);
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
        _MenuWindow.HideAll();
        Application.LoadLevel(lvl[n] + "");
    }
}