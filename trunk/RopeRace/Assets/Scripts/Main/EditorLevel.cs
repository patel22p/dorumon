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
    
    int tooli;
    public Tool tool { get { return (Tool)tooli; } set { tooli = (int)value; } }
    int gridSize;
    int PlanePos;
    public enum Tool { None, Grid, Trail }
    string[] prefabst;
    Transform[] prefabs;
    int prefabsi;
    void OnEnable()
    {         
        prefabs = transform.Find("tools").Cast<Transform>().ToArray();
        prefabst = prefabs.Select(a => a.name).ToArray();
        level = GameObject.Find("level").transform;
    }
    public override void Awake()
    {
    }
    void Start()
    {

    }
    public override void OnEditorGui()
    {        
        gridSize = GUI.IntSlider("GridSize", gridSize, 1, 10);        
        PlanePos = GUI.IntSlider("PlanePos", PlanePos, -10, 10);
        
        tooli = gui.SelectionGrid(tooli, Enum.GetNames(typeof(Tool)), 2);
        prefabsi = gui.SelectionGrid(prefabsi, prefabst, 2);
        base.OnEditorGui();
    }
    Vector3 cursorPos;
    Vector3? lastpos;
    
    float boundsSizeX;
    int axelLock;
    public Event e { get { return Event.current; } }
    public override void OnSceneGUI(SceneView scene, ref bool repaint)
    {
        
        if (tool != Tool.None && !e.alt)
        {
            for (int i = 0; i < prefabst.Length; i++)
                if (prefabsi != i) prefabs[i].SetActive(false);
            SelectedPrefab = prefabs[prefabsi];

            //Plane.transform.position = new Vector3(0, 0, -PlanePos);
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

            if (e.shift && e.delta != Vector2.zero && axelLock == 0)
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
        else
        {
            if (SelectedPrefab.gameObject.active)
                SelectedPrefab.SetActive(false);
        }
    }
    bool visible = false;
    private void UpdateTrail(Event e)
    {
        visible = false;
        
        bool down = e.button == 1 && e.type == EventType.mouseDrag;
        bool up = e.type == EventType.mouseUp;
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
                //Debug.Log(t.name);
                if (t.parent == level || (Selection.activeGameObject != null && t.parent == Selection.activeGameObject.transform))
                    DestroyImmediate(t.gameObject);
            }
            e.Use();
        }
        
    } 
    private void UpdateGrid(SceneView scene)
    {
        var e = Event.current;
        var p = SelectedPrefab.position;
        //SelectedPrefab.position = new Vector3(
        //    axelLock == 1 ? p.x : Mathf.RoundToInt(cursorPos.x),
        //    axelLock == 2 ? p.y : Mathf.RoundToInt(cursorPos.y), cursorPos.z);        
        SelectedPrefab.position = new Vector3(Mathf.RoundToInt(cursorPos.x), Mathf.RoundToInt(cursorPos.y), cursorPos.z);        

        if (gridSize % 2 == 0)
            SelectedPrefab.transform.position += Vector3.one / 2;
        if (e.button == 1 && !e.alt)
        {
            if (LastPrefab == null || !LastPrefab.GetComponentInChildren<Collider>().bounds.Intersects(SelectedPrefab.GetComponentInChildren<Collider>().bounds))
            {
                //Debug.Log(EditorUtility.GetPrefabParent(SelectedPrefab));
                LastPrefab = (Transform)Instantiate(SelectedPrefab);

                //LastPrefab = ((GameObject)EditorUtility.InstantiatePrefab(EditorUtility.GetPrefabParent(SelectedPrefab.gameObject))).transform;
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
