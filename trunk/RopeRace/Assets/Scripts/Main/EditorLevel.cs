#if (UNITY_EDITOR)
using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using GUI = UnityEditor.EditorGUILayout;
using System;
using UnityEditor;
[ExecuteInEditMode]
public class EditorLevel : bs {
    
    public Collider Plane;
    public Transform SelectedPrefab;
    Transform LastPrefab;
    //SerializedObject so;
    internal Transform Holder;
    Transform level;
    
    //int tool;
    public Tool tool; //{ get { return (Tool)tool; } set { tool = (int)value; } }
    public int gridSize = 3;
    public int PlanePos;
    public enum Tool { None, Grid, Trail }
    string[] prefabst;
    Transform[] prefabs;
    int prefabsi;
    public override void Awake()
    {
        //InitLoader
    }
    void OnEnable()
    {         
        level = GameObject.Find("level").transform;
    }
    void Start()
    {

    }
    public override void OnEditorGui()
    {
        if (gui.Button("InitLevel"))
        {
            foreach (var a in  level.GetComponentsInChildren<AnimHelper>())
                a.Init();
        }
        prefabs = transform.Find("tools").Cast<Transform>().ToArray();
        prefabst = prefabs.Select(a => a.name).ToArray();

        gridSize = GUI.IntSlider("GridSize", gridSize, 1, 10);        
        PlanePos = GUI.IntSlider("PlanePos", PlanePos, -10, 10);
        
        tool = (Tool)gui.SelectionGrid((int)tool, Enum.GetNames(typeof(Tool)), 2);
        if (tool != oldtool)
        {
            //Debug.Log("Chagne");
            SelectedPrefab.gameObject.active = false;
        }
        oldtool = tool;
        //oldtooli = tooli;
        prefabsi = gui.SelectionGrid(prefabsi, prefabst, 2);
        base.OnEditorGui();
    }
    Tool oldtool;
    Vector3 cursorPos;
    Vector3? lastpos;
    
    float boundsSizeX;
    int axelLock;
    public Event e { get { return Event.current; } }
    public override void OnSceneGUI(SceneView scene, ref bool repaint)
    {
        if (e.type == EventType.keyDown)
            repaint = true;
        if (e.keyCode == KeyCode.Alpha1)
            tool = Tool.None;            
        if (e.keyCode == KeyCode.Alpha2)
            tool = Tool.Grid;
        if (e.keyCode == KeyCode.Alpha3)
            tool = Tool.Trail;
        if (tool != Tool.None && !e.alt)
        {
            for (int i = 0; i < prefabst.Length; i++)
                if (prefabsi != i) prefabs[i].SetActive(false);
            SelectedPrefab = prefabs[prefabsi];

            if (e.type == EventType.scrollWheel)
            {
                var i = (int)(Mathf.Clamp(e.delta.y, -1, 1));

                if (e.modifiers == EventModifiers.Control)
                    gridSize -= i;
                else if (e.modifiers == EventModifiers.Shift)
                    prefabsi = Mathf.Clamp(prefabsi + i, 0, prefabs.Length - 1);
                else
                    PlanePos += i;
                    
                repaint = true;
                e.Use();
            }

            if (e.shift && e.delta.magnitude > 1 && axelLock == 0)
            {
                if (Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y))
                    axelLock = 1;
                else
                    axelLock = 2;
            }
            if (!e.shift)
                axelLock = 0;

            if (e.isMouse)
            {       
                if (e.type == EventType.mouseDown && tool != Tool.None)
                    Undo.RegisterSceneUndo("rtools");
                if (e.button != 2)
                    visible = true;
                else
                    visible = false;
            }
            SelectedPrefab.transform.localScale = Vector3.one * .999f * gridSize;
            UpdateCursor();
            if (tool == Tool.Grid)
            {
                UpdateGrid(scene);
            }
            if (tool == Tool.Trail)
            {
                UpdateTrail(e);
            }
            UpdateRemove(e);

            SelectedPrefab.SetActive(visible);
        }
        //else
        //{
        //    if (SelectedPrefab.gameObject.active)
        //        SelectedPrefab.SetActive(false);
        //}
    }
    bool visible = false;
    private void UpdateTrail(Event e)
    {
        visible = false;
        
        bool down = e.button == 1 && e.type == EventType.mouseDrag;
        bool up = e.type == EventType.mouseUp;
        var cursorPos = this.cursorPos + (gridSize % 2 == 0 ? new Vector3(0, 0, 1) / 2 : Vector3.zero);
        
        if (down && Holder == null)
        {
            if (lastpos == null)
                lastpos = cursorPos; 
            Holder = new GameObject("Holder").transform;
            Holder.position = lastpos.Value;
            LastPrefab = (Transform)Instantiate(SelectedPrefab);
            //LastPrefab = ((GameObject)EditorUtility.InstantiatePrefab(EditorUtility.GetPrefabParent(SelectedPrefab.gameObject))).transform;            
            LastPrefab.name = "Cube";            
            LastPrefab.localScale = Vector3.one * gridSize;
            boundsSizeX = LastPrefab.collider.bounds.size.x;
            LastPrefab.transform.position = Holder.transform.position + new Vector3(-boundsSizeX / 2, 0, 0);
            LastPrefab.transform.parent = Holder.transform;
        }
        else if (lastpos != null && Holder != null)
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
        else
        {
            visible = true;
            SelectedPrefab.transform.position = cursorPos;
        }

        if (up)
        {
            lastpos = null;
            LineUp();
        }
        if (down)
            e.Use();
    }
    private void LineUp()
    {
        if (Holder != null)
        {
             
            LastPrefab.transform.parent = level;
            DestroyImmediate(Holder.gameObject);            
        }
    }
    private void UpdateRemove(Event e)
    {
        if (e.button == 2 && !e.alt)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit rhit;
            if (Physics.Raycast(r, out rhit, Mathf.Infinity))
            {
                var t = rhit.transform;
                //t.GetComponentInParrent<Wall>();
                var p = t.Parent().FirstOrDefault(a => a.parent == level && a.GetComponent<bs>() != null);
                if(p!=null)
                    DestroyImmediate(p.gameObject);  
                //if (p != null && p.parent == level)
                                      
            }
            e.Use();
        }
        
    } 
    private void UpdateGrid(SceneView scene)
    {
        var e = Event.current;
        SelectedPrefab.position = new Vector3(Mathf.RoundToInt(cursorPos.x), Mathf.RoundToInt(cursorPos.y), cursorPos.z);        

        if (gridSize % 2 == 0)
            SelectedPrefab.transform.position +=  (Vector3.one / 2);
        if (e.button == 1 && !e.alt)
        {
            if (LastPrefab == null || !LastPrefab.GetComponentInChildren<Collider>().bounds.Intersects(SelectedPrefab.GetComponentInChildren<Collider>().bounds) )
            {
                LastPrefab = (Transform)Instantiate(SelectedPrefab);
                LastPrefab.name = "Cube";
                LastPrefab.transform.position = SelectedPrefab.transform.position;
                LastPrefab.transform.parent = level;
                LastPrefab.transform.localScale = Vector3.one * gridSize;
            }
            e.Use();
        }

    }
    void UpdateCursor()
    {
        var e = Event.current;
        //Selection.activeGameObject = null;
        //if (e.type == EventType.mouseDown && e.button == 0) { tool = Tool.None; }
        Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit rhit;
        if (Plane.Raycast(r, out rhit, Mathf.Infinity))
        {
            var old = cursorPos;
            cursorPos = rhit.point - new Vector3(0, 0, PlanePos);
            if (axelLock == 2) cursorPos.x = old.x;
            if (axelLock == 1) cursorPos.y = old.y;
        }
    }
}
#endif