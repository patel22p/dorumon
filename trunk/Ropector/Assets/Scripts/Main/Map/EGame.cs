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
    Transform spawnTr;
    Vector3? lastpos;
    Vector3 cursorPos;
    internal GUIContent[] ToolTextures;
    public override void Awake()
    {
        ToolTextures = tools.Select(a => a.discription).ToArray();
        _MenuGui.Hide();
        base.Awake();
    }
    void Start()
    {
        
        _Loader.EditorTest = true;
        plane = GameObject.Find("Plane").transform;
        spawnTr = transform.Find("spawn").transform;
        spawnTr.position = spawn;
        OnSelectionChanged();
    }
    Vector2 offset;
    void Update()
    {
        
        UpdateOther();
        UpdateCursor();

        if (SelectedPrefab.toolType == ToolType.Grid)
        {
            if (SelectedPrefab.name == "startpos")
            {
                if (click)
                    spawnTr.position = cursorPos;
            }

            SelectedPrefab.pos2 = new Vector2(Mathf.RoundToInt(cursorPos.x), Mathf.RoundToInt(cursorPos.y)) + offset;
            if (!gui)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                    offset.x -= Time.deltaTime;
                if (Input.GetKey(KeyCode.RightArrow))
                    offset.x += Time.deltaTime;
                if (Input.GetKey(KeyCode.UpArrow))
                    offset.y += Time.deltaTime;
                if (Input.GetKey(KeyCode.DownArrow))
                    offset.y -= Time.deltaTime;                                
            }
            SelectedPrefab.scale = Vector3.one * _EGameGUI.scale * .999f;
            if (_EGameGUI.scale % 2 == 0)
                SelectedPrefab.pos2 += Vector2.one / 2;
        }
        if (down && (LastPrefab == null || LastPrefab.collider == null || !LastPrefab.collider.bounds.Intersects(SelectedPrefab.collider.bounds)))
        {
            LastPrefab = (Tool)Instantiate(SelectedPrefab);
            LastPrefab.transform.parent = level;
        }
        timer.Update();
    }
    public void TestLevel()
    {
        spawn = spawnTr.position;
        SaveLevel();
        Application.LoadLevel("Game");
    }
    
    private void UpdateOther()
    {
        //SelectedPrefab = tools[_EGameGUI.tooli];
        //foreach (bs t in tools)
        //    t.gameObject.active = false;
    }
    void OnApplicationQuit()
    {
        SaveLevel();
    }
    private void UpdateCursor()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit h;

        var rbtn = Input.GetMouseButton(1);
        SelectedPrefab.SetActive(!rbtn);        
        if (Physics.Raycast(r, out h, float.MaxValue) && rbtn)
        {
            var t = h.transform.GetComponentInParrent<Tool>();
            Debug.Log(h.transform.name);
            if (t != null && t.transform.parent == level)
                Destroy(t.gameObject);
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
    bool up { get { return Input.GetMouseButtonUp(0) && !gui; } }    
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
        Debug.Log("Selection Changed");
        if (SelectedPrefab != null)
            Destroy(SelectedPrefab.gameObject);
        SelectedPrefab = (Tool)Instantiate(tools[_EGameGUI.tooli]);
    }
}
