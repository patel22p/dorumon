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
        new Rect(115, 10, 150, 550)
    };
    GUIText tip;
    GUIContent[] PrefabTextures { get { return _EGame.PrefabTextures; } }
    
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

    
    internal int prefabi;
    internal int tooli;
    new internal int scale = 1;
    TimerA timer = new TimerA();
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
              
        
        GUI.DragWindow();
        if (GUI.tooltip != "")
            tip.text = GUI.tooltip;
    }
    void Window2(int id)
    {
        gui.Label(new GUIContent("Prefabs", "Objects that you put on scene"));
        var old = prefabi;
        prefabi = gui.SelectionGrid(prefabi, PrefabTextures, 2);
        if (old != prefabi)
            _EGame.OnSelectionChanged();
        gui.Label(new GUIContent("Tools", "Move, Scale and other tools"));
        tooli = gui.SelectionGrid(tooli, Enum.GetNames(typeof(ToolType)), 2);  

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

            var anim = _EGame.SelectedPrefab.GetComponentInChildren<AnimHelper>();
            if (anim != null)
            {
                gui.Label("anim speed");
                anim.animationSpeedFactor = gui.HorizontalSlider(anim.animationSpeedFactor, -1, 1);
                gui.Label("anim offset");
                anim.TimeOffsetFactor = gui.HorizontalSlider(anim.TimeOffsetFactor, 0, 1);
            }
            

        }
        gui.BeginHorizontal();
        if (gui.Button("<")) scale--;
        gui.Label(""+scale);
        if (gui.Button(">")) scale++;
        scale = Mathf.Clamp(scale, 1, 10);
        gui.EndHorizontal();
        

        GUI.DragWindow();
        if (GUI.tooltip != "")
            tip.text = GUI.tooltip;
    }
    

	void Update () {
        timer.Update();
	}
    internal string mapName { get { return _Loader.mapName; } set { _Loader.mapName = value; } }
}
