using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
public enum Tools { Zoom, Draw }
public class GameGUI : MonoBehaviour {
    internal Rect winRect= new Rect(10, 10, 100, 200);
    internal List<Tool> tools = new List<Tool>();
    Texture2D[] brushTextures ;
	void Start () {
        List<Texture2D> brushTextures = new List<Texture2D>();
        foreach (Transform t in GameObject.Find("Tools").transform)
        {
            var t2 = t.GetComponent<Tool>();
            tools.Add(t2);
            brushTextures.Add(t2.Texture);
        }
        this.brushTextures = brushTextures.ToArray();
	}
    void OnGUI()
    {
        GUI.Window(1, winRect, Window, "Tools");
    }
    internal int brushi;
    internal int tooli;
    public Tools brush { get { return (Tools)brushi; } }    
    public Texture2D[] toolTextures;
    void Window( int id)
    {
        if (gui.Button("Camera"))
            Camera.main.orthographic = !Camera.main.orthographic; 
        brushi = gui.Toolbar(brushi, toolTextures);
        gui.Space(1);
        tooli = gui.Toolbar(tooli, brushTextures);
    }
	void Update () {
	
	}
}
