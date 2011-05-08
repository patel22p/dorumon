using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using doru;
using System;
using System.Text.RegularExpressions;

public enum Scene { Menu, mapEditor, Game }

public class EGameGUI : bs{

    internal Rect[] winRect = new[] { 
        new Rect(10, 10, 100, 500), 
        new Rect(115, 10, 100, 500)
    };
    
    public Texture2D[] ToolTextures { get { return _EGame.ToolTextures; } }
    public Texture2D[] BrushTextures { get { return _EGame.BrushTextures; } }
    
    public override void Awake()
    {
        
        base.Awake();
    }
    void Start()
    {
    }
    void OnGUI()
    {
        winRect[0] = GUI.Window((int)Wind.EditorTools, winRect[0], Window, "Tools");
        winRect[1] = GUI.Window((int)Wind.EditorProps, winRect[1], Window2, _EGame.SelectedPrefab + "");
    }

    
    internal int brushi;
    internal int tooli;
    public Brushes brush { get { return (Brushes)brushi; } }
    
    TimerA timer = new TimerA();
    void Window2(int id)
    {
        //if (timer.TimeElapsed(100) && _EGame.LastPrefab != null)
        if (_EGame.SelectedPrefab != null)
        {
            var textmesh = _EGame.SelectedPrefab.GetComponent<TextMesh>();
            if (textmesh != null)
            {
                gui.Label("Text");
                textmesh.text = gui.TextField(textmesh.text);
            }
            var score = _EGame.SelectedPrefab.GetComponent<Score>();
            if (score != null)
                score.spawn = gui.Toggle(score.spawn, "CheckPoint");

            var anim = _EGame.SelectedPrefab.GetComponent<AnimHelper>();
            if (anim != null)
            {
                anim.animationSpeedFactor = gui.HorizontalSlider(anim.animationSpeedFactor, 0, 1);
            }
            var physanim = _EGame.SelectedPrefab.GetComponent<PhysAnim>();
            if (physanim != null)
            {
                
            }

            var wall = _EGame.SelectedPrefab.GetComponent<Wall>();
            if (wall != null)
            {
                gui.Label("SpeedTrack");
                wall.SpeedTrackVell = gui.HorizontalSlider(wall.SpeedTrackVell, -5f, 5f);
                float.TryParse(gui.TextField(wall.SpeedTrackVell + ""), out wall.SpeedTrackVell);
            }
            try
            {
                if (wall != null && brush == Brushes.Draw)
                {
                    var s = _EGame.size;
                    gui.Label("Grid Size");
                    s.x = int.Parse(gui.TextField(s.x + ""));
                    s.y = int.Parse(gui.TextField(s.y + ""));
                    _EGame.size = s;
                }
            }
            catch (FormatException) { }
        }
        GUI.DragWindow();
    }
    void Window(int id)
    {
        if (gui.Button("Camera"))
            Camera.main.orthographic = !Camera.main.orthographic;
        
        gui.Label("Map Name");
        mapName = gui.TextField(mapName, 20);
        if (gui.Button("New Map"))
            _EGame.Clear();
        
        if (gui.Button("Save Map") && mapName.Length > 2)
        {
            _EGame.SaveMapToFile();
        }
        if (gui.Button("Load Map") && mapName.Length > 2)
        {
            _EGame.LoadMapFromFile(mapName);

        }
        if (gui.Button("Test"))
            _EGame.TestLevel();
        if (gui.Button("Exit"))
        {
            
            Application.LoadLevel((int)Scene.Menu);
        }
        gui.Label("Tools");
        tooli = gui.SelectionGrid(tooli, ToolTextures, 2);
        if (_EGame.ShowBrushes)
        {
            gui.Label("Brushes");
            brushi = gui.SelectionGrid(brushi, BrushTextures, 2);
        }
        GUI.DragWindow();
    }



    

	void Update () {
        timer.Update();
	}
    internal string mapName
    {
        get
        {
            
            return PlayerPrefs.GetString("mapname", "Map" + UnityEngine.Random.Range(-999, 999));
        }
        set
        {
            value = Regex.Replace(value, "[^qwertyuiopasdfghjklzxcvbnm1234567890]", "", RegexOptions.IgnoreCase);
            PlayerPrefs.SetString("mapname", value);
        }
    }
}
