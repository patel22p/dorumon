using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindowBase:Base
{
    bool show;
    public new bool enabled
    {
        get { return base.enabled; }
        set
        {
            base.enabled = value;
            if (!value)                        
                show = true;
        }
    }
    
    public Rect rect;
    internal int id;
    protected virtual void Awake()
    {
        id = Random.Range(0, int.MaxValue);
        rect = new Rect(Random.Range(0, 100), Random.Range(0, 100), 0, 0);
        show = true;
    }
    public string title;
    public Vector2 size = new Vector2(200,200);
    protected virtual void OnGUI()
    {
        
        rect = GUILayout.Window(id, rect, Window, title, GUILayout.Width(size.x), GUILayout.Height(size.y));
        GUI.skin = _Loader.skin;
        if (show) { GUI.BringWindowToFront(id); GUI.FocusWindow(id); show = false; }
    }
    protected virtual void Window(int id)
    {

    }
    

}
