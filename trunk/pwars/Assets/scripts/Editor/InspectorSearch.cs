#if UNITY_EDITOR && UNITY_STANDALONE_WIN
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

        GUI.BeginHorizontal();       
        //CopyComponent();
        
        //if (GUI.Button("Cap"))
        //    Cap();
        if (GUI.Button("Apply"))
            ApplyAll();
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeGameObject.name))
                instances.Add(Selection.activeGameObject.name);
        GUI.EndHorizontal();
        DrawObjects();
        DrawSearch();
    }
    

    [MenuItem("GameObject/Capture Screenshot")]
    static void Cap()
    {
        if (Selection.activeGameObject == null) return;
        Undo.RegisterSceneUndo("rtools");
        int size = 256;
        var sc = SceneView.lastActiveSceneView.camera;
        GameObject co = (GameObject)Instantiate(Base2.FindAsset<GameObject>("SnapShotCamera"), sc.transform.position, sc.transform.rotation);
        Camera c = co.GetComponentInChildren<Camera>();
        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        c.targetTexture = rt;
        var output = new Texture2D(size, size, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        var g = Selection.activeGameObject;
        var g2 = (GameObject)Instantiate(g, g.transform.position, g.transform.rotation);
        c.cullingMask = 1 << co.layer;
        foreach (var a in g2.GetComponentsInChildren<Transform>())
            a.gameObject.layer = co.layer;
        var r = g2.GetComponentInChildren<Renderer>();

        if (r == null) { Debug.Log("Render is null " + r.name); return; }
        g2.active = true;
        c.Render();
        output.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        output.Apply();
        Color? bk = null;
        for (int x = 0; x < output.width; x++)
            for (int y = 0; y < output.height; y++)
            {
                var px = output.GetPixel(x, y);
                if (bk == null) bk = px;
                px.a = px == bk.Value ? 0 : .6f;
                output.SetPixel(x, y, px);
            }
        var p = AssetDatabase.GetAssetPath(g);


        p = (p == "" ? Application.dataPath + "/materials/Icons/" : Path.GetDirectoryName(p)) + "/" + g.name + ".png";

        File.WriteAllBytes(p, output.EncodeToPNG());
        Debug.Log("Saved: " + p);
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;
        c.targetTexture = null;
        DestroyImmediate(g2);
        DestroyImmediate(co);
    }
    protected virtual void Awake()
    {
        instances = EditorPrefs.GetString(EditorApplication.applicationPath).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    protected virtual void SaveParams()
    {
        EditorPrefs.SetString(EditorApplication.applicationPath, string.Join(",", instances.ToArray()));
    }
    private void DrawSearch()
    {
        search = EditorGUILayout.TextField(search);        
        EditorGUIUtility.LookLikeInspector();
        if (search.Length > 0)
        {
            if ((Selection.activeGameObject != null && Selection.activeGameObject.camera == null) || Selection.activeObject != null)
            {
                IEnumerable<Object> array = new Object[] { Selection.activeObject };
                if (Selection.activeGameObject != null)
                    array = array.Union(Selection.activeGameObject.GetComponents<Component>());                    
                foreach (var m in array)
                {
                    SerializedObject so = new SerializedObject(m);
                    SerializedProperty pr = so.GetIterator();
                    pr.NextVisible(true);
                    do
                    {
                        if (pr.propertyPath.ToLower().Contains(search.ToLower()) 
                            || (pr.propertyType == SerializedPropertyType.String && pr.stringValue.ToLower().Contains(search.ToLower()))
                            || (pr.propertyType == SerializedPropertyType.Enum && pr.enumNames.Length >= 0 && pr.enumNames[pr.enumValueIndex].ToLower().Contains(search.ToLower()))
                            && pr.editable)
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

    private void DrawObjects()
    {        
        List<string> toremove = new List<string>();
        foreach (var inst in instances)
        {
            GUI.BeginHorizontal();
            if (GUI.Button(inst))
            {
                GameObject o = GameObject.Find(inst) != null ? GameObject.Find(inst) : (GameObject)GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)).FirstOrDefault(a => a.name == inst);
                Selection.activeGameObject = o;
                SaveParams();
            }
            if (GUI.Button("X", GUI.ExpandWidth(false)))
                toremove.Add(inst);
            GUI.EndHorizontal();
        }
        foreach (var inst in toremove)
            instances.Remove(inst);

    }
    protected virtual void SetupLevel()
    {
        foreach (Transform g in Selection.activeGameObject.transform)
        {
            bool glow = g.name.Contains("glow");
            if (g.name.Contains("glass") || glow)
            {
                foreach (var t in g.GetTransforms())
                {
                    if (glow && t.collider != null)
                        t.collider.isTrigger = true;
                    Debug.Log(t.gameObject.name);
                    t.gameObject.layer = LayerMask.NameToLayer("Glass");
                    if (t.GetComponent<Renderer>() != null)
                    {
                        t.renderer.castShadows = false;
                    }
                }
            }
        }
    }
    public GameObject selectedGameObject;
    private void CopyComponent()
    {
        if (GUI.Button("CloneComp"))
        {
            selectedGameObject = selectedGameObject == null ? Selection.activeGameObject : null;
        }
        if (selectedGameObject != null)
        {
            foreach (var c in selectedGameObject.GetComponents<Component>())
                if (GUI.Button(c.GetType().Name))
                {
                    foreach (GameObject g in Selection.gameObjects)
                    {
                        var c2 = g.AddComponent(c.GetType());
                        foreach (FieldInfo f in c.GetType().GetFields())
                            f.SetValue(c2, f.GetValue(c));
                        //foreach (PropertyInfo p in c.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        //    if(p.CanRead && p.CanWrite)
                        //        p.SetValue(c2, p.GetValue(c,null),null);
                        //Debug.Log(c.GetType().GetProperties().Length+"+");
                    }
                }
            GUI.Space(10);
        }
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
    
    [MenuItem("GameObject/Child")]
    static void CreateChild()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var nwt = new GameObject(Selection.activeObject.name + "1").transform;
        nwt.position = t.position;
        nwt.rotation = t.rotation;
        nwt.parent = t;
    }
    [MenuItem("GameObject/Parent")]
    static void CreateParent()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var t2 = new GameObject(Selection.activeObject.name + "1").transform;
        t2.position = t.position;
        t2.rotation = t.rotation;
        t2.parent = t.parent;
        t.parent = t2;
    }
    [MenuItem("GameObject/UnParent")]
    static void UnParent()
    {
        Undo.RegisterSceneUndo("rtools");
        Selection.activeTransform.parent = Selection.activeTransform.parent.parent;
    }
    
    [MenuItem("RTools/Rtools")]
    static void rtoolsclick()
    {
        EditorWindow.GetWindow<RTools>();
    }
    [MenuItem("Assets/Add Labels")]
    static void ApplyLabels()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach(var asset in Selection.objects)
        {
            if (AssetDatabase.IsMainAsset(asset))
            {
                var apath = AssetDatabase.GetAssetPath(asset);
                var list = AssetDatabase.GetLabels(asset);
                var nwlist = list.Union(apath.Split('/').Skip(1));
                AssetDatabase.SetLabels(asset,nwlist.ToArray());
                EditorUtility.SetDirty(asset);
            }
        }
    }
    [MenuItem("Assets/Clear Labels")]
    static void ClearLabels()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var asset in Selection.objects)
        {
            if (AssetDatabase.IsMainAsset(asset))
            {
                AssetDatabase.ClearLabels(asset);
                EditorUtility.SetDirty(asset);
            }
        }
    }
    [MenuItem("GameObject/Duplicate Materials")]
    static void Dup()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var a in Selection.activeGameObject.GetComponentsInChildren<Renderer>())
        {
            var ms =a.sharedMaterials;
            for (int i = 0; i < ms.Count(); i++)
            {
                var p = AssetDatabase.GetAssetPath(ms[i]);
                var nwp = p.Substring(0, p.Length - 4)+"D.mat";
                AssetDatabase.DeleteAsset(nwp);
                AssetDatabase.CopyAsset(p, nwp);
                AssetDatabase.Refresh();
                ms[i] = (Material)AssetDatabase.LoadAssetAtPath(nwp, typeof(Material));                
            }
            a.sharedMaterials = ms;
        }
    }
    [MenuItem("GameObject/Create Prefab")]
    static void CreatePrefabs()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (GameObject g in Selection.gameObjects)
        {
            if (!AssetDatabase.IsMainAsset(g))
            {
                Directory.CreateDirectory(Application.dataPath + "/" + g.transform.parent.name);
                var p = EditorUtility.CreateEmptyPrefab("Assets/" + g.transform.parent.name + "/" + g.name + ".prefab");
                EditorUtility.ReplacePrefab(g, p, ReplacePrefabOptions.ConnectToPrefab);
                EditorUtility.SetDirty(g);
            } 
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("GameObject/ApplyAll")]
    static void ApplyAll()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var go in Selection.gameObjects)
        {
            EditorUtility.ReplacePrefab(go, EditorUtility.GetPrefabParent(go), ReplacePrefabOptions.UseLastUploadedPrefabRoot);
            EditorUtility.ResetGameObjectToPrefabState(go);
            EditorUtility.ReconnectToLastPrefab(go);
            AssetDatabase.SaveAssets();
        }
    }
    [MenuItem("GameObject/ReconnectAll")]
    static void ReconnectAll()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var go in Selection.gameObjects)
        {
            EditorUtility.ResetGameObjectToPrefabState(go);            
            EditorUtility.ReconnectToLastPrefab(go);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("GameObject/Look At")]
    static void LookAt()
    {
        Undo.RegisterSceneUndo("rtools");
        if (Selection.activeGameObject != null)
        {
            Selection.activeGameObject.transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
            //var e = Selection.activeGameObject.transform.rotation.eulerAngles;
            //e.y += 90;
            //Selection.activeGameObject.transform.rotation = Quaternion.Euler(e);
        }
    }
    public TimerA _TimerA = new TimerA();
    protected virtual void Update()
    {
        _TimerA.Update();
        SceneView.onSceneGUIDelegate = OnSceneUpdate;
        if (_TimerA.TimeElapsed(320 * 1000))
        {
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused && EditorApplication.currentScene.Contains(".unity"))
            {
                EditorApplication.SaveScene(EditorApplication.currentScene); //autosave
            }
        }
        var ao = Selection.activeObject;
        if (ao != null && !lastUsed.Contains(ao))
            lastUsed.Insert(0, ao);

        if (_TimerA.TimeElapsed(3000))
            this.Repaint();
    }
    
}
#endif