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
    internal bs MouseOverPrefab;
    internal bs Holder;
    internal Tool LastPrefab;
    TimerA timer = new TimerA();
    Transform plane;    
    Transform spawnTr;
    Vector3? lastpos;
    Vector3 cursorPos;
    internal GUIContent[] PrefabTextures;

    ToolType tool { get { return (ToolType)_EGameGUI.tooli; } set { _EGameGUI.tooli = (int)value; } }

    public override void Awake()
    {
        tools = tools.Where(a => a != null).ToList();
        PrefabTextures = tools.Select(a => a.discription).ToArray();
        _MenuGui.Hide();
        if (_Loader.Map.Length > 0)
             LoadMap();
        else
            _Loader.LoadMap(delegate { LoadMap(); });

        base.Awake();
    }
    void Start()
    {        
        _Loader.EditorTest = true;
        plane = GameObject.Find("Plane").transform;
        
        OnSelectionChanged();
    }
    public override void onMapLoaded()
    {
        spawnTr = transform.Find("spawn").transform;
        spawnTr.gameObject.active = true;
        spawnTr.position = spawn;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            UnityEditor.EditorApplication.isPaused = true;
        UpdateOther();
        if (!gui)
            UpdateCursor();
        UpdateKeyboard();

        if (tool != ToolType.Grid || Input.GetMouseButton(1)) SelectedPrefab.SetActive(false);
        if (SelectedPrefab.name.StartsWith("startpos"))
        {
            if (click)
                spawnTr.position = cursorPos;
            SelectedPrefab.SetActive(false);
        }
        else if (tool == ToolType.Grid)
        {
            UpdateGrid();
        }        
        else if (tool == ToolType.Move)
        {
            if (MouseOverPrefab != null)
            {
                MouseOverPrefab.transform.position = cursorPos;
                if (Input.GetKeyDown(KeyCode.C))
                {
                    var g = (Transform)Instantiate(MouseOverPrefab.transform.GetChild(0), MouseOverPrefab.pos, MouseOverPrefab.rot);
                    g.parent = level;
                }
            }
        }
        else if (tool == ToolType.Rotate)
        {
            if (MouseOverPrefab != null)
                MouseOverPrefab.rotz = (lastpos.Value.x - cursorPos.x) * 40;
        }
        else if (tool == ToolType.Scale)
        {
            if (MouseOverPrefab != null)
                MouseOverPrefab.scale = Vector3.one * (1f + ((cursorPos.x - lastpos.Value.x) / 4f));
        }
        else if (tool == ToolType.Trail)
        {
            UpdateDrawTrail();
        }
        timer.Update();

    }
    private void UpdateDrawTrail()
    {
        if (down && Holder == null)
        {
            if (lastpos == null)
                lastpos = cursorPos;

            Holder = new GameObject("Holder").AddComponent<bs>();
            Holder.position = lastpos.Value;
            LastPrefab = (Tool)Instantiate(SelectedPrefab);
            LastPrefab.scale = Vector3.one * _EGameGUI.scale;
            boundsSizeX = LastPrefab.collider.bounds.size.x;
            LastPrefab.transform.position = Holder.pos + new Vector3(-boundsSizeX / 2, 0, 0);
            //LastPrefab.transform.rotation = Quaternion.Euler(0, 270, 0);
            LastPrefab.transform.parent = Holder.transform;
            
        }
        if (lastpos != null && Holder!=null)
        {
            var v = lastpos.Value - cursorPos;
            var angl = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            var rot = Holder.transform.rotation.eulerAngles;
            rot.z = angl;
            Holder.transform.rotation = Quaternion.Euler(rot);
            if (v.magnitude > boundsSizeX)
            {
                lastpos = lastpos.Value + Holder.transform.rotation * Vector3.left * boundsSizeX;
                LineUp();
            }            
        }
        if (up)
        {
            LineUp();
        }
    }

    float boundsSizeX;


    private void LineUp()
    {
        
        //lastpos = null;
        if (Holder != null)
        {
            Destroy(Holder.gameObject);
            LastPrefab.transform.parent = level;
        }
    }

    private void UpdateGrid()
    {        
        SelectedPrefab.pos2 = new Vector2(Mathf.RoundToInt(cursorPos.x), Mathf.RoundToInt(cursorPos.y));
        SelectedPrefab.scale = Vector3.one * _EGameGUI.scale * .999f;
        if (_EGameGUI.scale % 2 == 0)
            SelectedPrefab.pos2 += Vector2.one / 2;

        if (LastPrefab != null && lastpos != null)
        {
            var v = lastpos.Value - cursorPos;
            var v2 = LastPrefab.GridSize * _EGameGUI.scale;
            
            if (Mathf.Abs(v.x) > v2.x || Mathf.Abs(v.y) > v2.y)
            {
                Inst();
            }
        }
        if (down && (click || LastPrefab == null))
        {
            LastPrefab = (Tool)Instantiate(SelectedPrefab);
            LastPrefab.transform.parent = level;
        }
    }

    private void Inst()
    {
        LastPrefab = (Tool)Instantiate(SelectedPrefab);
        LastPrefab.transform.parent = level;
    }

    private void UpdateKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) tool = ToolType.Grid;
        if (Input.GetKeyDown(KeyCode.Alpha2)) tool = ToolType.Move;
        if (Input.GetKeyDown(KeyCode.Alpha3)) tool = ToolType.Scale;
        if (Input.GetKeyDown(KeyCode.Alpha4)) tool = ToolType.Rotate;
        if (Input.GetKeyDown(KeyCode.Alpha5)) tool = ToolType.Trail;
    }

    public void TestLevel() 
    {
        spawn = spawnTr.position;
        SaveLevel();
        Network.InitializeServer(0, 5300,true);
        Application.LoadLevel("Game");
    }
    private void UpdateOther()
    {
        SelectedPrefab.SetActive(true);
    }
    void OnApplicationQuit()
    {        
        SaveMapToFile();
    }
    
    private void UpdateCursor()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit h;
        var rbtn = Input.GetMouseButton(1);        
        if (Physics.Raycast(r, out h, float.MaxValue))
        {
            Tool t = h.transform.GetComponentInParrent<Tool>();
            if (t != null && t.transform.parent == level)
            {
                
                if (rbtn)
                    Destroy(t.gameObject);
                if (click)
                {
                    MouseOverPrefab = new GameObject("MouseOverPrefab").AddComponent<bs>();
                    MouseOverPrefab.position = cursorPos;
                    MouseOverPrefab.parent = t.transform.parent;
                    t.transform.parent = MouseOverPrefab.transform;
                }                
            }            
        }
        if (up)
        {
            if (MouseOverPrefab != null)
            {

                MouseOverPrefab.transform.GetChild(0).parent = MouseOverPrefab.parent;
                Destroy(MouseOverPrefab.gameObject);
                MouseOverPrefab = null;
                
            }
            
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
        spawn = spawnTr.position;
        _EGame.SaveLevel();
        WWWForm form = new WWWForm();
        Map.Position = 0;
        form.AddField("map", new StreamReader(_EGame.Map).ReadToEnd());
        var w = new WWW(_Loader.host + "index.php?save=1&mapname=" + _EGameGUI.mapName, form);
        timer.AddMethod(() => w.isDone == true, delegate
        {
            if (w.text != "Success")
                _Popup.ShowPopup("Could Not Save Map");
            Debug.Log("Saved:" + w.text);
        });
    }
    bool click { get { return Input.GetMouseButtonDown(0) && !gui; } }
    bool up { get { return Input.GetMouseButtonUp(0); } }    
    bool down { get { return Input.GetMouseButton(0) && !gui; } }    
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

    internal void OnSelectionChanged()
    {
        if (SelectedPrefab != null)
            Destroy(SelectedPrefab.gameObject);
        SelectedPrefab = (Tool)Instantiate(tools[_EGameGUI.prefabi]);
        SelectedPrefab.pos = Vector3.zero;
        tool = ToolType.Grid;
        
    }
}
