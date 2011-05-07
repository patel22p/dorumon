using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class EGame : Gamebs {

    public new List<Tool> tools = new List<Tool>();
    internal bs SelectedPrefab;
    internal bs LastPrefab;
    
    Transform plane;
    Transform SelectBox;
    Transform spawnTr;
    Vector3? lastpos;
    Vector3 cursorPos;
    public override void Awake()
    {
        _MyGui.Hide();
        base.Awake();
    }

    
    void Start()
    {
        _Loader.EditorTest = true;
        plane = GameObject.Find("Plane").transform;
        
        spawnTr = GameObject.Find("spawn").transform;
        spawnTr.position = spawn;
        SelectBox = transform.Find("SelectBox");

    }
    internal Vector3 size = Vector3.one*3;
    public float LineWidth = 1;
    void Update () {
        UpdateOther();
        UpdateCursor();
        if (SelectedPrefab.GetComponent<Wall>() != null)
        {
            UpdateDrawGrid();
            UpdateDrawTrail();
        }
        if (SelectedPrefab.GetComponent<TextMesh>() != null || SelectedPrefab.GetComponent<Score>() != null)
        {
            if (click)
            {
                var g = LastPrefab = (bs)Instantiate(SelectedPrefab, cursorPos, Quaternion.identity);
                g.transform.parent = level;
            }
        }
        if (brush == Brushes.Spawn)
        {
            if (click)
                spawnTr.position = cursorPos;
        }
    }

    private void UpdateDrawTrail()
    {
        if (brush == Brushes.Line || brush == Brushes.Trail)
        {

            if (up)
                LineUp();
            if (click)
                LineDown();
            if (lastpos != null)
            {
                Holder.LookAt(cursorPos);
                var s = Holder.localScale;
                s.z = Vector3.Distance(cursorPos, lastpos.Value);
                Holder.localScale = s;
                if (brush == Brushes.Line) LineWidth = s.z;
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
        LastPrefab = (bs)Instantiate(SelectedPrefab);
        LastPrefab.transform.localScale= Vector3.one;
        LastPrefab.transform.position = cursorPos + new Vector3(0, 0, .5f);
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
        if (brush == Brushes.Draw)
        {
            SelectBox.gameObject.active = true;
            var s = new Vector3(size.x, size.y, 1);
            var p = cursorPos - s / 2;
            p = new Vector3(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), 0);

            SelectBox.position = SelectedPrefab.transform.position = p + (s-Vector3.forward) / 2 ;
            
            SelectBox.localScale = SelectedPrefab.transform.localScale = s * .999f;            
            if (hold && (LastPrefab == null || LastPrefab.collider == null || !LastPrefab.collider.bounds.Intersects(SelectBox.collider.bounds)))
            {
                LastPrefab = (bs)Instantiate(SelectedPrefab);
                LastPrefab.transform.parent = level;
            }
        }
    }
    
    public void TestLevel()
    {
        spawn = spawnTr.position;
        SaveLevel();
        Application.LoadLevel("Game");
    }


    

    Transform Holder;
    private void UpdateOther()
    {
        SelectedPrefab = tools[_EGameGUI.tooli];
        foreach (bs t in tools)
            t.gameObject.active = false;
        SelectBox.gameObject.active = false;
    }
    void OnApplicationQuit()
    {
        SaveLevel();
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
        if (click)
            lastpos = cursorPos;
        if (Input.GetMouseButtonUp(0))
            lastpos = null;
    }

    public Brushes brush { get { return _EGameGUI.brush; } }
    bool click { get { return Input.GetMouseButtonDown(0) && !gui; } }
    bool up { get { return Input.GetMouseButtonUp(0) && !gui; } }    
    bool hold { get { return Input.GetMouseButton(0) && !gui; } }    
    bool gui
    {
        get
        {
            var v = Input.mousePosition;
            return _EGameGUI.winRect.Any(a => a.Contains(new Vector2(v.x, -v.y + Screen.height)));
        }
    }

    internal void Clear()
    {
        foreach (Transform t in level)
            Destroy(t.gameObject);
    }
}
