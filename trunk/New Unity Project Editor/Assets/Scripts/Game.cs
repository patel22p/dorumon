using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : bs2 {

	
    public Tool SelectedPrefab;
    Tool LastPrefab;
    Transform level;
    
    Stack<GameObject> stack = new Stack<GameObject>();
    LineRenderer line;
    Transform Plane;
    Transform SelectBox;
    Vector3? lastpos;
    Vector3 cursorPos;
    void Start()
    {        
        Plane = GameObject.Find("Plane").transform;
        level = GameObject.Find("level").transform;
        //SelectBox = GameObject.Find("SelectBox").transform;
        SelectBox = transform.Find("SelectBox");
        line = gameObject.AddComponent<LineRenderer>();
        line.enabled = false;
        line.SetWidth(0.01f, 0.01f);
        line.material.color = Color.white;

    }
    Rect g { get { return new Rect(SelectedPrefab.x, SelectedPrefab.y, SelectedPrefab.scale.x, SelectedPrefab.scale.y); } }
    void Update () {
        UpdateOther();
        UpdateCursor();
        

        if (tool == Tools.SetGrid)
        {
            if (lastpos != null)
            {
                SelectBox.gameObject.active = true;
                var v = (lastpos.Value - cursorPos);
                SelectBox.transform.localScale = new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), 1);
                
                var p = SelectBox.transform.position = cursorPos + (v/2);
                //g = new Rect(p.x, p.y, v.x, v.y);
                SelectedPrefab.pos = p;
                SelectedPrefab.scale = v;
            }
        }
        if (tool == Tools.Draw)
        {            
            SelectBox.gameObject.active = true;
            var p = cursorPos;
            p = new Vector3(p.x - p.x % g.width, p.y - p.y % g.height,0);
            p += new Vector3(g.x % g.width, g.y % g.height);
            SelectBox.position = SelectedPrefab.pos = p;
            SelectBox.localScale = SelectedPrefab.scale = new Vector3(g.width, g.height, 1);
            if (hold && !level.Cast<Transform>().Any(a => a.position == p))
            {
                var t = (Tool)Instantiate(SelectedPrefab);
                t.transform.parent = level;
            }
        }
        if (tool == Tools.Line)
        {
            if (down)
            {
                Holder = new GameObject("Holder").transform;
                Holder.position = cursorPos;
                LastPrefab = (Tool)Instantiate(SelectedPrefab);
                LastPrefab.scale = Vector3.one;
                LastPrefab.pos = cursorPos + new Vector3(0, 0, .5f);
                LastPrefab.transform.parent = Holder;
            }
            if (lastpos != null)
            {
                Holder.LookAt(cursorPos);
                var s = Holder.localScale;
                s.z = Vector3.Distance(cursorPos, lastpos.Value);
                Holder.localScale = s;
            }
            if (up)
            {
                LastPrefab.transform.parent = level;
                var s = SelectedPrefab.scale;
                s.y = LastPrefab.scale.z;                
                SelectedPrefab.scale = s;
                Destroy(Holder.gameObject);
            }
        }
    }
    

    Transform Holder;
    private void UpdateOther()
    {
        SelectedPrefab = _GameGUI.tools[_GameGUI.tooli];
        foreach (Tool t in _GameGUI.tools)
            t.gameObject.active = false;
        SelectBox.gameObject.active = false;
    }

    private void UpdateCursor()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit h;
        if (Physics.Raycast(r, out h, float.MaxValue) && Input.GetMouseButton(1))
        {
            if (h.transform != Plane)
                Destroy(h.transform.gameObject);
        }
        if (Plane.collider.Raycast(r, out h, float.MaxValue))
            cursorPos = h.point;
        if (down)
            lastpos = cursorPos;
        if (Input.GetMouseButtonUp(0))
            lastpos = null;
    }

    Tools tool { get { return _GameGUI.brush; } }
    bool down { get { return Input.GetMouseButtonDown(0) && !gui; } }
    bool up { get { return Input.GetMouseButtonUp(0) && !gui; } }    
    bool hold { get { return Input.GetMouseButton(0) && !gui; } }    
    bool gui
    {
        get
        {
            var v = Input.mousePosition;
            return _GameGUI.winRect.Contains(new Vector2(v.x, -v.y + Screen.height));
        }
    }
}
