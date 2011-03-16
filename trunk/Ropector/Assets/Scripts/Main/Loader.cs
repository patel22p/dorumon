using System;
using UnityEngine;
using System.Collections.Generic;
using doru;

public class Loader : bs //this class entry point class, also it will not removed when new scene loads(all other objects will be destroyed but this after LoadLevel() Call)
{
    
    public int errorcount;
    public int exceptionCount;
    [FindTransform]
    public GUIText info;

    
    TimerA timer = new TimerA();
    public void onLog(string c, string stackTrace, LogType type)
    {
        if (type == LogType.Error) errorcount++;
        if (type == LogType.Exception) exceptionCount++;
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
    List<string> levelName  = new List<string>();
    public void RefreshRecords()
    {
        List<string> fls = new List<string>();
        for (int i = 1; i <9; i++)
        {
            levelName.Add(i + "");
            var f = PlayerPrefs.GetFloat(i + "");
            //fls.Add(i + " Level " + (f == 0 ? "" : "\tRecord " + TimeToSTr(f)));
            var loadProgress = (int)(Application.GetStreamProgressForLevel(i + "") * 100);
            fls.Add(string.Format(CreateTable(_MenuWindow.Tabble), "", i, TimeToSTr(f), loadProgress));
        }
        _MenuWindow.lSelectLevel = fls.ToArray();
    }
    void Action(MenuWindowEnum s) //this function called when menu button pressed
    {        

        if (s == MenuWindowEnum.NewGame)
            LoadLevel("1");
        if (s == MenuWindowEnum.QualitySettings)
            QualitySettings.currentLevel = (QualityLevel)_MenuWindow.iQualitySettings;           
        if (s == MenuWindowEnum.SelectLevel)
            LoadLevel(levelName[_MenuWindow.iSelectLevel]);
        if (s == MenuWindowEnum.DisableMusic)
            audio.Stop();
        if (s == MenuWindowEnum.DisableSounds)
            AudioListener.pause = _MenuWindow.DisableSounds;
    }
    int fps;
    void Update()
    {
        if (_MenuWindow.enabled && timer.TimeElapsed(1000))
            RefreshRecords();

        if(timer.TimeElapsed(1000))
           fps = (int)timer.GetFps() ;

        info.text = "FPS:"+fps + " W:" + errorcount + " E:" + exceptionCount;
        if (Input.GetKeyDown(KeyCode.Escape))
            _MenuWindow.Show(this);
        timer.Update();
    }
    public void LoadLevel(string n)
    {
        _MenuWindow.HideAll();
        Application.LoadLevel(n);
    }
}