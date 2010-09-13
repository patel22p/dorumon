using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;

public class OptionsWindow : WindowBase {

    
    internal int quality = 3;
    int oldquality = 3;
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
        base.Awake();
    }    
    protected override void OnGUI() 
    {
        
        rect = GUILayout.Window(id, rect, Window, "Options", GUILayout.Height(300), GUILayout.Width(300));
        base.OnGUI();
    }
    private void Window(int id)
    {
        GUILayout.Label("quality:");
        
        string[] qs = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
        if (oldquality != (quality = GUILayout.Toolbar(quality, qs)))
        {
            oldquality = quality;            
            QualitySettings.currentLevel = (QualityLevel)quality;
            printC("Set Quality" + QualitySettings.currentLevel);
        }
        GUILayout.Label("screen resolution:");
        //GUILayout.BeginHorizontal();        
        //if (GUILayout.Button("800x600"))
        //    Screen.SetResolution(800, 600, Screen.fullScreen);
        //if (GUILayout.Button("1024x768"))
        //    Screen.SetResolution(1024, 768, Screen.fullScreen);
        //if (GUILayout.Button("1280x720"))
        //    Screen.SetResolution(1280, 720, Screen.fullScreen);
        //if (GUILayout.Button("1280x768"))
        //    Screen.SetResolution(1280, 768, Screen.fullScreen);
        int old = selres;
        selres = GUILayout.SelectionGrid(selres, rs,3);

        if (old != selres)
        {
            Resolution r = Screen.resolutions[selres];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            
        }

        //GUILayout.EndHorizontal();
        GUILayout.Label("Camera X Offset:" + xofset);
        xofset = GUILayout.HorizontalSlider(xofset, 0, 20);
        GUILayout.Label("Camera Y Offset:" + yofset);
        yofset = GUILayout.HorizontalSlider(yofset, 0, 20);
        GUILayout.Label("Camera fieldOfView:" + fieldof);
        fieldof = GUILayout.HorizontalSlider(fieldof, 40, 90);        
        if (GUILayout.Button("close")) enabled = false;
        GUI.DragWindow();
    }
    public int selres { get { return PlayerPrefs.GetInt("resolution", -1); } set { PlayerPrefs.SetInt("resolution", value); } }
    public float xofset { get { return PlayerPrefs.GetFloat("xofset", 3); } set { PlayerPrefs.SetFloat("xofset", value); } }
    public float yofset { get { return PlayerPrefs.GetFloat("yofset", 2); } set { PlayerPrefs.SetFloat("yofset", value); } }
    public float fieldof { get { return PlayerPrefs.GetFloat("fieldof", 70); } set { PlayerPrefs.SetFloat("fieldof", value); } }
}
