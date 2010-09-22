using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;

public class OptionsWindow : WindowBase {

    string[] rs;
    protected override void Awake()
    {
        
        List<string> rs = new List<string>();
        foreach (Resolution r in Screen.resolutions)
            rs.Add(r.width + "x" + r.height);        
        this.rs = rs.ToArray();
        if (selres != -1 && Screen.resolutions.Length > 1)
        {
            Resolution r = Screen.resolutions[selres];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        }
        
        if (quality != -1)
            QualitySettings.currentLevel = (QualityLevel)quality;
        base.Awake();
    }
    void Start()
    {
        size = new Vector2(400, 300);
        title = lc.options.ToString();
    }
    
    protected override void Window(int id)
    {
        GUILayout.Label(lc.quality .ToString());
        int oldquality = quality;
        string[] qs = new string[] { lc.fastest .ToString(), lc.fast .ToString(), lc.simple .ToString(), lc.good .ToString(), lc.beateful .ToString(), lc.fantastic .ToString() };
        if (oldquality != (quality = GUILayout.Toolbar(quality, qs)))
        {
            oldquality = quality;            
            QualitySettings.currentLevel = (QualityLevel)quality;
            printC(lc.setquality.ToString() + QualitySettings.currentLevel);
        }
        GUILayout.Label(lc.setres.ToString());

        int old = selres;
        selres = GUILayout.SelectionGrid(selres, rs,3);

        if (old != selres)
        {
            Resolution r = Screen.resolutions[selres];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            
        }
        
        enableMusic = GUILayout.Toggle(enableMusic, lc.enablemusci.ToString());
        //GUILayout.EndHorizontal();
        GUILayout.Label(lc.xof .ToString() + xofset);
        xofset = GUILayout.HorizontalSlider(xofset, 0, 20);
        GUILayout.Label(lc.yof .ToString()+ yofset);
        yofset = GUILayout.HorizontalSlider(yofset, 0, 20);
        GUILayout.Label(lc.camf .ToString()+ fieldof);
        fieldof = GUILayout.HorizontalSlider(fieldof, 40, 90);        
        if (GUILayout.Button(lc.close.ToString())) enabled = false; 
        GUI.DragWindow();
    }
    public static bool enableMusic { get { return PlayerPrefs.GetInt("enableMusic",1) == 1; } set { PlayerPrefs.SetInt("enableMusic", value ? 1 : 0); } }

    public static bool secondrun { get { return PlayerPrefs.GetInt("firstrun") == 1; } set { PlayerPrefs.SetInt("firstrun", value ? 1 : 0); } }
    public static bool ruslang { get { return PlayerPrefs.GetInt("ruslang") == 1; } set { PlayerPrefs.SetInt("ruslang", value ? 1 : 0); } }
    public int quality { get { return PlayerPrefs.GetInt("quality", -1); } set { PlayerPrefs.SetInt("quality", value); } }
    public int selres { get { return PlayerPrefs.GetInt("resolution", -1); } set { PlayerPrefs.SetInt("resolution", value); } }
    public float xofset { get { return PlayerPrefs.GetFloat("xofset", 3); } set { PlayerPrefs.SetFloat("xofset", value); } }
    public float yofset { get { return PlayerPrefs.GetFloat("yofset", 2); } set { PlayerPrefs.SetFloat("yofset", value); } }
    public float fieldof { get { return PlayerPrefs.GetFloat("fieldof", 70); } set { PlayerPrefs.SetFloat("fieldof", value); } }
}
