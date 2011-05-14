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
    internal Transform MouseOverPrefab;    
    internal Tool LastPrefab;
    TimerA timer = new TimerA();
    Transform plane;    
    Transform spawnTr;
    Vector3? lastpos;
    Vector3 cursorPos;
    internal GUIContent[] PrefabTextures;
    
    ToolType tool { get { return (ToolType)_EGameGUI.tooli; } }

    public override void Awake()
    {
        PrefabTextures = tools.Select(a => a.discription).ToArray();
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
    void Update()
    {

        UpdateOther();
        if (!gui)
            UpdateCursor();

        if (SelectedPrefab.name == "startpos")
        {
            if (click)
                spawnTr.position = cursorPos;
        }
        else if (tool == ToolType.Grid)
        {
            if (!Input.GetMouseButton(1))
                SelectedPrefab.SetActive(true); 
            SelectedPrefab.pos2 = new Vector2(Mathf.RoundToInt(cursorPos.x), Mathf.RoundToInt(cursorPos.y));
            SelectedPrefab.scale = Vector3.one * _EGameGUI.scale * .999f;
            if (_EGameGUI.scale % 2 == 0)
                SelectedPrefab.pos2 += Vector2.one / 2;

            if (down && (LastPrefab == null || LastPrefab.collider == null || !LastPrefab.collider.bounds.Intersects(SelectedPrefab.collider.bounds)))
            {
                LastPrefab = (Tool)Instantiate(SelectedPrefab);
                LastPrefab.transform.parent = level;
            }
        }
        else if (tool == ToolType.Move)
        {
            
            if (MouseOverPrefab != null)
            {
                MouseOverPrefab.transform.position= cursorPos;
            }
        }
        else if (tool == ToolType.Rotate)
        {

            if (MouseOverPrefab != null)
            {
                //Debug.Log(Quaternion.Looka(MouseOverPrefab.pos - cursorPos).eulerAngles);
                MouseOverPrefab.transform.LookAt(cursorPos);
            }
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
        SelectedPrefab.SetActive(false);
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
        if (Physics.Raycast(r, out h, float.MaxValue))
        {
            Tool t = h.transform.GetComponentInParrent<Tool>();
            if (t != null && t.transform.parent == level)
            {
                
                if (rbtn)
                    Destroy(t.gameObject);
                if (click)
                {
                    MouseOverPrefab = new GameObject("MouseOverPrefab").transform;
                    MouseOverPrefab.position = cursorPos;
                    MouseOverPrefab.parent = t.transform.parent;
                    t.transform.parent = MouseOverPrefab;
                }                
            }            
        }
        if (up)
        {
            if (MouseOverPrefab != null)
            {
                //MouseOverPrefab.transform.GetChild(0).parent = MouseOverPrefab.parent;
                //Destroy(MouseOverPrefab.gameObject);
                //MouseOverPrefab = null;
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
        if (SelectedPrefab != null)
            Destroy(SelectedPrefab.gameObject);
        SelectedPrefab = (Tool)Instantiate(tools[_EGameGUI.prefabi]);
        SelectedPrefab.pos = Vector3.zero;
    }
}
