using System;
using UnityEngine;
using System.Collections.Generic;

public class Loader : bs
{
    bool m_debug;
    internal new bool debug { get { return m_debug && !Application.isEditor; } set { m_debug = value; } }
    public override void Awake()
    {
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
            fls.Add(i + " Level " + (f == 0 ? "" : "\tRecord " + TimeToSTr(f)));
        }
        _MenuWindow.lSelectLevel = fls.ToArray();
    }
    void Action(MenuWindowEnum s)
    {        
        if (s == MenuWindowEnum.NewGame)
            LoadLevel("1");
        if (s == MenuWindowEnum.QualitySettings)
        {
            QualitySettings.currentLevel = (QualityLevel)_MenuWindow.iQualitySettings;           
        }
        if (s == MenuWindowEnum.SelectLevel)
            LoadLevel(levelName[_MenuWindow.iSelectLevel]);
        if (s == MenuWindowEnum.DisableMusic)
            audio.Stop();
        if (s == MenuWindowEnum.DisableSounds)
            AudioListener.pause = _MenuWindow.DisableSounds;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _MenuWindow.Show(this);
    }
    public void LoadLevel(string n)
    {
        _MenuWindow.HideAll();
        Application.LoadLevel(n);
    }
}