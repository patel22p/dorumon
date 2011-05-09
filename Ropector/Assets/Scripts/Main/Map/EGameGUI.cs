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
        new Rect(10, 10, 100, 550), 
        new Rect(115, 10, 100, 550)
    };
    GUIText tip;
    GUIContent[] ToolTextures { get { return _EGame.ToolTextures; } }
    GUIContent[] BrushTextures { get { return _EGame.BrushTextures; } }
    
    public override void Awake()
    {
        tip = transform.Find("Tip").gameObject.GetComponent<GUIText>();
        base.Awake();
    }
    void Start()
    {
    }
    void OnGUI()
    {
        tip.text = "";
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
        if (GUI.tooltip != "")
            tip.text = GUI.tooltip;
    }
    void Window(int id)
    {
        if (gui.Button(new GUIContent("Camera", "switch camera to orthographic")))
            Camera.main.orthographic = !Camera.main.orthographic;
        
        gui.Label("Map Name");
        mapName = gui.TextField(mapName, 20);
        if (gui.Button(new GUIContent("New Map", "Clear Level")))
            _EGame.Clear();

        if (gui.Button(new GUIContent("Save Map", "Save map to web server")))
        {
            if (mapName.Length > 2)
                _EGame.SaveMapToFile();
            else
                _Popup.ShowPopup("Map Name is empty");
        }
        if (gui.Button(new GUIContent("Load Map", "Load map from web server")))
        {
            if (mapName.Length > 2)
                _Loader.LoadMap(delegate
                {
                    _EGame.LoadMap();
                });
            else
                _Popup.ShowPopup("Map Name is empty");
        }
        if (gui.Button(new GUIContent("Test", "Test Level")))
            _EGame.TestLevel();
        if (gui.Button("Exit"))
        {
            Application.LoadLevel((int)Scene.Menu);
        }
        gui.Label( new GUIContent("Tools" , "Objects that you put on scene"));
        tooli = gui.SelectionGrid(tooli, ToolTextures, 2);
        if (_EGame.ShowBrushes)
        {
            gui.Label(new GUIContent("Brushes", "Brushes to draw Objects"));
            brushi = gui.SelectionGrid(brushi, BrushTextures, 2);
        }
        GUI.DragWindow();
        if (GUI.tooltip != "")
            tip.text = GUI.tooltip;
    }



    

	void Update () {
        timer.Update();
	}
    internal string mapName { get { return _Loader.mapName; } set { _Loader.mapName = value; } }
}
