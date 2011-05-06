using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class EGame : bs2 {

	
    internal Tool SelectedPrefab;
    Tool LastPrefab;
    Transform level;
    Transform plane;
    Transform SelectBox;
    Transform spawn;
    Vector3? lastpos;
    Vector3 cursorPos;
    void Start()
    {        
        plane = GameObject.Find("Plane").transform;
        level = GameObject.Find("level").transform;
        spawn = GameObject.Find("spawn").transform;
        SelectBox = transform.Find("SelectBox");
    }
    public Vector3 size = Vector3.one;
    //public Vector3 size2 = Vector3.one;
    public float LineWidth = 1;
    void Update () {
        UpdateOther();
        UpdateCursor();
        UpdateDrawGrid();
        UpdateDrawTrail();
        if (tool == Tools.Spawn)
        {
            if (down)
                spawn.position = cursorPos;
        }
    }

    private void UpdateDrawTrail()
    {
        if (tool == Tools.Line || tool == Tools.Trail)
        {

            if (up)
                LineUp();
            if (down)
                LineDown();
            if (lastpos != null)
            {
                Holder.LookAt(cursorPos);
                var s = Holder.localScale;
                s.z = Vector3.Distance(cursorPos, lastpos.Value);
                Holder.localScale = s;
                if (tool == Tools.Line) LineWidth = s.z;
                else
                {
                    if (LineWidth < s.z)
                    {
                        LineUp();
                        LineDown();
                    }
                }
            }
        }
    }

    private void LineDown()
    {
        
        Holder = new GameObject("Holder").transform;
        Holder.position = cursorPos;
        LastPrefab = (Tool)Instantiate(SelectedPrefab);
        LastPrefab.scale = Vector3.one;
        LastPrefab.pos = cursorPos + new Vector3(0, 0, .5f);
        LastPrefab.transform.parent = Holder;
        lastpos = cursorPos;
    }

    private void LineUp()
    {
        LastPrefab.transform.parent = level;
        Destroy(Holder.gameObject);
    }

    private void UpdateDrawGrid()
    {
        if (tool == Tools.Draw)
        {
            SelectBox.gameObject.active = true;
            var s = new Vector3(size.x, size.y, 1);
            var p = cursorPos - s / 2;
            p = new Vector3(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), 0);

            SelectBox.position = SelectedPrefab.pos = p + (s-Vector3.forward) / 2 ;
            
            SelectBox.localScale = SelectedPrefab.scale = s * .999f;
            if (hold && (LastPrefab == null || !LastPrefab.collider.bounds.Intersects(SelectBox.collider.bounds)))
            {
                LastPrefab = (Tool)Instantiate(SelectedPrefab);
                LastPrefab.transform.parent = level;
            }
        }
    }
    
    public void TestLevel()
    {
        foreach (Transform t in level)
        {
            var db = new DB();
            db.tools.Add(t.GetComponent<Tool>().Save());
            MemoryStream ms = new MemoryStream();
            DB.xml.Serialize(ms, db);
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
            if (h.transform != plane)
                Destroy(h.transform.gameObject);
        }
        if (plane.collider.Raycast(r, out h, float.MaxValue))
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
