//script by igor levochkin
using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using Object = UnityEngine.Object;
using System.IO;
using doru;

[ExecuteInEditMode]
public class InspectorSearch : EditorWindow
{
    public List<string> instances = new List<string>();
    List<Object> lastUsed = new List<Object>();
    string search = "";
    public bool SetPivot;
    Vector3 oldpos;
    protected virtual void OnGUI()
    {
        SetPivot = (GUI.Toggle(SetPivot, "Set Pivot") && Selection.activeGameObject != null);
        if (!SetPivot && Selection.activeGameObject) oldpos = Selection.activeGameObject.transform.position;
        DrawObjects();
        DrawSearch();
    }
    protected virtual void Awake()
    {
        instances = EditorPrefs.GetString("Favs").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    protected virtual void SaveParams()
    {
        EditorPrefs.SetString("Favs", string.Join(",", instances.ToArray()));
    }
    private void DrawSearch()
    {
        search = EditorGUILayout.TextField(search);
        EditorGUIUtility.LookLikeInspector();
        if (search.Length > 0)
        {
            if ((Selection.activeGameObject != null && Selection.activeGameObject.camera == null) || Selection.activeObject is Material)
            {
                IEnumerable<Object> array = new Object[] { Selection.activeObject };
                if (Selection.activeGameObject != null)
                {
                    array = array.Union(Selection.activeGameObject.GetComponents<Component>());
                    if (Selection.activeGameObject.renderer != null)
                        array = array.Union(new[] { Selection.activeGameObject.renderer.sharedMaterial });
                }
                foreach (var m in array)
                {
                    SerializedObject so = new SerializedObject(m);
                    SerializedProperty pr = so.GetIterator();
                    pr.NextVisible(true);
                    do
                    {
                        if (pr.propertyPath.ToLower().Contains(search.ToLower()) && pr.editable)
                            EditorGUILayout.PropertyField(pr);
                        if (so.ApplyModifiedProperties())
                        {
                            SetMultiSelect(m, pr);
                        }
                    }
                    while (pr.NextVisible(true));
                }
            }
        }
    }
    private void DrawObjects()
    {
        int i=0;
        foreach (var a in lastUsed.Where(a => a != null).Take(3))
        {
            i++;
            if (GUI.Button(a.name))
                Selection.activeObject = a;
        }
        
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeGameObject.name))
                instances.Add(Selection.activeGameObject.name);
        List<string> toremove = new List<string>();
            foreach (var inst in instances)
            {
                GUI.BeginHorizontal();
                if (GUI.Button(inst))
                {
                    GameObject o = GameObject.Find(inst) ?? (GameObject)GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)).FirstOrDefault(a => a.name == inst);
                    Selection.activeGameObject = o;
                    //var c = SceneView.lastActiveSceneView.camera;
                    //Debug.Log(c.transform.position);
                    //c.transform.localPosition = o.transform.position;
                    SaveParams();
                }
                if (GUI.Button("X", GUI.ExpandWidth(false)))
                    toremove.Add(inst);
                GUI.EndHorizontal();
            }
            foreach (var inst in toremove)
                instances.Remove(inst);

    }
    private void OnSceneUpdate(SceneView s)
    {
        
        var ago = Selection.activeGameObject;
        

        if (SetPivot)
        {
            var move = oldpos - ago.transform.position;
            foreach (Transform t in ago.transform)
            {
                t.position += move;
            }
        }
        
        if (ago != null)
            oldpos = ago.transform.position;



        var c = s.camera;
        var e = Event.current;
        var p = e.mousePosition;
        if (e.keyCode == KeyCode.G && e.type == EventType.KeyUp)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(new Vector2(p.x, p.y));
            RaycastHit h;
            if (Physics.Raycast(r, out h))
                s.LookAt(h.point - 5 * r.direction, c.transform.rotation, 5);

        }
    }
    private void SetMultiSelect(Object m, SerializedProperty pr)
    {
        switch (pr.propertyType)
        {
            case SerializedPropertyType.Float:
                MySetValue(m, pr.floatValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Boolean: 
                MySetValue(m, pr.boolValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Integer:
                MySetValue(m, pr.intValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.String:
                MySetValue(m, pr.stringValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Color:
                MySetValue(m, pr.colorValue, pr.propertyPath, pr.propertyType);
                break;
        }
    }
    void MySetValue(Object c, object value, string prName, SerializedPropertyType type)
    {
        var array = Selection.gameObjects.Select(a => a.GetComponent(c.GetType())).Cast<Object>().Union(Selection.objects.Where(a => !(a is GameObject)));
        if (Selection.activeGameObject.renderer != null && c is Material)
        {            
            array = array.Union(Selection.activeGameObject.renderer.sharedMaterials);
        }

        foreach (var nc in array) //êîìïîíåíòû gameobjectîâ è âûáðàíûå Objectû
        {            
            if (nc != null && nc != c)
            {
                SerializedObject so = new SerializedObject(nc);
                var pr = so.FindProperty(prName);
                switch (type)
                {
                    case SerializedPropertyType.Float:
                        pr.floatValue = (float)value;
                        break;
                    case SerializedPropertyType.Boolean:
                        pr.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.String:
                        pr.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.Integer:
                        pr.intValue = (int)value;
                        break;
                    case SerializedPropertyType.Color:
                        pr.colorValue = (Color)value;
                        break;
                }

                so.ApplyModifiedProperties();
            }
        }
    }
    [MenuItem("GameObject/Child")]
    static void CreateChild()
    {
        var t = Selection.activeTransform;
        var nwt = new GameObject(Selection.activeObject.name + "1").transform;
        nwt.position = t.position;
        nwt.rotation = t.rotation;
        nwt.parent = t;
    }
    [MenuItem("GameObject/Parent")]
    static void CreateParent()
    {
        var t = Selection.activeTransform;
        var t2 = new GameObject(Selection.activeObject.name + "1").transform;
        t2.position = t.position;
        t2.rotation = t.rotation;
        t2.parent = t.parent;
        t.parent = t2;
    }
    [MenuItem("RTools/Rtools")]
    static void rtoolsclick()
    {
        if (_ewnd == null) _ewnd = EditorWindow.GetWindow<RTools>();
    }
    public TimerA _TimerA = new TimerA();
    protected virtual void Update()
    {        
        _TimerA.Update();
        SceneView.onSceneGUIDelegate = OnSceneUpdate;
        if (_TimerA.TimeElapsed(60 * 1000) && !EditorApplication.isPlaying && !EditorApplication.isPaused && EditorApplication.currentScene.Contains(".scene"))
        {
            EditorApplication.SaveScene(EditorApplication.currentScene);
        }
        var ao = Selection.activeObject;
        if (ao != null && !lastUsed.Contains(ao))
            lastUsed.Insert(0,ao);

        if (_TimerA.TimeElapsed(3000))
            ewnd.Repaint();
    }
    static EditorWindow _ewnd;
    static EditorWindow ewnd
    {
        get
        {
            if (_ewnd == null) _ewnd = EditorWindow.GetWindow<RTools>();
            return _ewnd;
        }
    }
}