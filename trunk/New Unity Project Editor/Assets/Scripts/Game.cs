using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : bs2 {

	void Start () {
        Plane = GameObject.Find("Plane").transform;
        level = GameObject.Find("level").transform;
        line = gameObject.AddComponent<LineRenderer>();
        line.enabled = false;
        line.SetWidth(0.01f, 0.01f);
        line.material.color = Color.white;

	}
    public Tool SelectedPrefab;
    Tool LastPrefab;
    Transform level;
    Stack<GameObject> stack = new Stack<GameObject>();
    LineRenderer line;
    Transform Plane;
    Vector3? lastpos;
    Vector3 cursorPos;
	void Update () {
        SelectedPrefab = _GameGUI.tools[_GameGUI.tooli];        
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit h;

        foreach (Tool t in _GameGUI.tools)
            t.gameObject.active = false;

        if (Physics.Raycast(r, out h, float.MaxValue) && Input.GetMouseButton(1))
        {
            if (h.transform != Plane)
                Destroy(h.transform.gameObject);
        }
        if (Plane.collider.Raycast(r, out h, float.MaxValue))
            cursorPos = h.point;            

        if (tool == Tools.Zoom)
            DrawCube(h);
        if (tool == Tools.Draw)
        {
            SelectedPrefab.gameObject.active = true;
            SelectedPrefab.gameObject.active = true;
            SelectedPrefab.pos = cursorPos;

            if (down)
            {
                Draw();
            }
        }
        CtrlZ();
        oldms = cursorPos;
    }
    Vector3 oldms;
    void Draw()
    {
        var dist = Vector3.Distance(cursorPos, oldms);
        Draw2();
        for (float i = 0; i < dist ; i += .01f)
        {
            oldms = Vector3.Lerp(oldms, cursorPos, i / dist);
            Draw2();
        }
    }

    private void Draw2()
    {
        bool draw = true;
        if (LastPrefab != null)
        {
            var v = LastPrefab.pos - oldms;
            var sz = SelectedPrefab.collider.bounds.size * .98f;
            draw = Mathf.Abs(v.x) > sz.x || Mathf.Abs(v.y) > sz.y;
        }
        if (draw)
        {
            LastPrefab = (Tool)Instantiate(SelectedPrefab);
            LastPrefab.transform.parent = level;
            LastPrefab.transform.position = oldms;
        }
    }
    private void DrawCube(RaycastHit h)
    {

        
        if (Input.GetMouseButtonDown(0) && !gui)
        {
            lastpos = cursorPos;
            line.enabled = true;
            LastPrefab = (Tool)Instantiate(SelectedPrefab);
            LastPrefab.transform.parent = level;
            LastPrefab.transform.position = cursorPos;

        }
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log(LastPrefab.transform.localScale.magnitude);
            if (LastPrefab.transform.localScale.magnitude < 2f)
                Destroy(LastPrefab.gameObject);
            else
            {
                line.enabled = false;
                lastpos = null;
                stack.Push(LastPrefab.gameObject);
            }
        }

        if (lastpos != null)
        {
            line.SetPosition(0, lastpos.Value);
            line.SetPosition(1, cursorPos);
            var v = lastpos.Value - cursorPos;
            SelectedPrefab.transform.localScale = LastPrefab.transform.localScale = new Vector3(v.x, v.y, 1);
            //Debug.Log("asd");

        }
    }
    void OnMouseEnter()
    {

    }
    private void CtrlZ()
    {
        if (stack.Count > 0 && Input.GetKeyDown(KeyCode.Z))// && Input.GetKey(KeyCode.LeftControl))
        {
            var s = stack.Pop();
            if (s != null)
                Destroy(s);
        }
    }
    
    Tools tool { get { return _GameGUI.brush; } }
    bool down { get { return Input.GetMouseButton(0) && !gui; } }    
    bool gui
    {
        get
        {
            var v = Input.mousePosition;
            return _GameGUI.winRect.Contains(new Vector2(v.x, -v.y + Screen.height));
        }
    }
}
