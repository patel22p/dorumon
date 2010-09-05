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

        string[] qs = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
        if (oldquality != (quality = GUILayout.Toolbar(quality, qs)))
        {
            oldquality = quality;            
            QualitySettings.currentLevel = (QualityLevel)quality;
            printC("Set Quality" + QualitySettings.currentLevel);
        }

        if (GUILayout.Button("close")) enabled = false;
        GUI.DragWindow();
    }

}
