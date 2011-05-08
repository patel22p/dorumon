using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using doru;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class EGame : Gamebs
{

    public new List<Tool> tools = new List<Tool>();
    internal Tool SelectedPrefab;
    internal Tool LastPrefab;
    TimerA timer = new TimerA();
    Transform plane;
    Transform SelectBox;
    Transform spawnTr;
    Vector3? lastpos;
    Vector3 cursorPos;
    public Texture2D[] ToolTextures;
    public Texture2D[] BrushTextures;

    public override void Awake()
    {
        ToolTextures = tools.Select(a => a.texture).ToArray();
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
    internal Vector3 size = Vector3.one * 3;
    public float LineWidth = 1;
    internal bool ShowBrushes;
    void Update()
    {
        
        UpdateOther();
        UpdateCursor();

        if (SelectedPrefab.name == "startpos")
        {
            if (click)
                spawnTr.position = cursorPos;
        }
        else if (SelectedPrefab.GetComponent<PhysAnim>())
        {
            if (click)
            {
                LastPrefab = (Tool)Instantiate(SelectedPrefab, cursorPos, Quaternion.identity);
                LastPrefab.transform.parent = level;
            }
            if (lastpos != null)
            {
                var v = lastpos.Value - cursorPos;
                if (v != Vector3.zero)
                {
                    LastPrefab.rot = Quaternion.LookRotation(v) * Quaternion.Euler(0, 90, 0);
                    LastPrefab.scale = Vector3.one * Vector3.Distance(cursorPos, lastpos.Value);
                }
            }
        }
        else if (SelectedPrefab.GetComponent<Wall>() != null)
        {
            ShowBrushes = true;
            UpdateDrawGrid();
            UpdateDrawTrail();
        }
        else if (SelectedPrefab.GetComponent<TextMesh>() != null || SelectedPrefab.GetComponent<Score>() != null)
        {
            if (click)
            {
                var g = LastPrefab = (Tool)Instantiate(SelectedPrefab, cursorPos, Quaternion.identity);
                g.transform.parent = level;
            }
        }
        timer.Update();
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
        LastPrefab = (Tool)Instantiate(SelectedPrefab);
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
                LastPrefab = (Tool)Instantiate(SelectedPrefab);
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
        ShowBrushes = false;
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
                Destroy(h.transform.GetMonoBehaviorInParrent().gameObject);
        }
        if (plane.collider.Raycast(r, out h, float.MaxValue))
            cursorPos = h.point;
        if (click)
            lastpos = cursorPos;
        if (Input.GetMouseButtonUp(0))
            lastpos = null;
    }

    public void SaveMapToFile()
    {
        _EGame.SaveLevel();
        WWWForm form = new WWWForm();
        form.AddField("map", _EGame.Map);
        var w = new WWW(_Loader.host + "index.php?save=1&mapname=" + _EGameGUI.mapName, form);
        timer.AddMethod(() => w.isDone == true, delegate
        {
            Debug.Log("Saved:" + w.text);
        });
    }
    public void LoadMapFromFile(string filename)
    {
        var w = new WWW(_Loader.host + "index.php?open=1&mapname=" + filename);
        timer.AddMethod(() => w.isDone == true, delegate
        {
            Debug.Log("Loaded:" + w.text);
            if (w.text != "")
                _EGame.Map = w.text;
            _EGame.LoadMap();
        });
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
