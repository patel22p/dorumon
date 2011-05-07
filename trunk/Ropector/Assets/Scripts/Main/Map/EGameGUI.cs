using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using doru;

public enum Scene { Menu, mapEditor, Game }
public class EGameGUI : bs{
    internal Rect[] winRect = new[] { 
        new Rect(10, 10, 100, 500), 
        new Rect(115, 10, 100, 500)
    };
    
    public Texture2D[] brushTextures ;
    void Start()
    {        
	}
    void OnGUI()
    {
        winRect[0] = GUI.Window(0, winRect[0], Window, "Tools");

        winRect[1] = GUI.Window(1, winRect[1], Window2, _EGame.SelectedPrefab + "");
        
    }
    internal int brushi;
    internal int tooli;
    public Brushes brush { get { return (Brushes)brushi; } }    
    public Texture2D[] toolTextures;
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
            var wall = _EGame.SelectedPrefab.GetComponent<Wall>();
            if (wall != null)
            {
                gui.Label("SpeedTrack");
                float.TryParse(gui.TextField(wall.SpeedTrackVell + ""), out wall.SpeedTrackVell);
            }

            if (wall != null && brush == Brushes.Draw)
            {
                var s = _EGame.size;
                gui.Label("Grid Size");
                s.x = int.Parse(gui.TextField(s.x + ""));
                s.y = int.Parse(gui.TextField(s.y + ""));
                _EGame.size = s;
            }
        }
        GUI.DragWindow();
    }
    void Window(int id)
    {
        if (gui.Button("Camera"))
            Camera.main.orthographic = !Camera.main.orthographic;
        gui.Label("Brushes");
        brushi = gui.SelectionGrid(brushi, toolTextures, 2);
        gui.Label("Tools");
        tooli = gui.SelectionGrid(tooli, brushTextures, 2);
        
        

        if (gui.Button("Test"))
            _EGame.TestLevel();
        if (gui.Button("Menu"))
            Application.LoadLevel((int)Scene.Menu);
        if (gui.Button("Clear"))
            _EGame.Clear();
        
        GUI.DragWindow();
    }

	void Update () {
        timer.Update();
	}
}
