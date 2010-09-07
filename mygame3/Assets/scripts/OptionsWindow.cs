using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;

public class OptionsWindow : Base {

    Rect optionsrect = new Rect(0, 0, 300, 300);
    internal int quality = 3;
    int oldquality = 3;
    void Awake()
    {
        _options = this;
    }
    void OnGUI() 
    {
        optionsrect = GUILayout.Window(5, optionsrect, Window, "Options");
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
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1024x768"))
            Screen.SetResolution(1024, 768, Screen.fullScreen);
        if (GUILayout.Button("800x600"))
            Screen.SetResolution(800, 600, Screen.fullScreen);
        if (GUILayout.Button("1280x720"))
            Screen.SetResolution(1280, 720, Screen.fullScreen);
        if (GUILayout.Button("1280x768"))
            Screen.SetResolution(1280, 768, Screen.fullScreen);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("close")) enabled = false;
        GUI.DragWindow();
    }

}
