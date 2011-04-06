#if UNITY_EDITOR && UNITY_STANDALONE_WIN
using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using gui = UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;
using System.IO;
using doru;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections;


public class InspectorSearch : EditorWindow
{
    bool web;
    protected TimerA _TimerA = new TimerA();
    float autosavetm = 0;    
    List<Object> lastUsed = new List<Object>();
    string search = "";
    Vector3 oldpos;
    public List<string> instances = new List<string>();
    public bool SetPivot;
    public bool SetCam;
    public bool bake;
    public float curentLayerDist;

    float lfactor { get { return EditorPrefs.GetFloat("lightmap" + EditorApplication.currentScene, .2f); } set { EditorPrefs.SetFloat("lightmap" + EditorApplication.currentScene, value); } }
    float dfactor { get { return EditorPrefs.GetFloat("lightmapDT" + EditorApplication.currentScene, .1f); } set { EditorPrefs.SetFloat("lightmapDT" + EditorApplication.currentScene, value); } }
    public virtual void Awake()
    {
        PlayerSettings.runInBackground = true;
        instances = EditorPrefs.GetString(EditorApplication.applicationPath).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    
    protected virtual void OnGUI()
    {
        if (Camera.main == null)
            return;     
        if (!SetPivot && Selection.activeGameObject) oldpos = Selection.activeGameObject.transform.position;
        GUI.BeginHorizontal();
        SetPivot = (GUI.Toggle(SetPivot, "Pivot", GUI.ExpandWidth(false)) && Selection.activeGameObject != null);

        var old = SetCam;
        SetCam = (GUI.Toggle(SetCam && Camera.main != null, "Cam", GUI.ExpandHeight(false))); //camset        
        
        Camera.main.renderingPath = (RenderingPath)gui.EnumPopup(Camera.main.renderingPath);
        if (SetCam != old && SetCam == false)
            ResetCam();
        if (GUI.Button("Apply"))
            ApplyAll();
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeObject.name))
            {
                instances.Add(Selection.activeObject.name);
                SaveParams();
            }
        if (GUI.Button("Init"))
            Inits();            
        GUI.EndHorizontal();
        QualitySettings.shadowDistance = gui.FloatField("LightmapDist", QualitySettings.shadowDistance);
        if (Selection.activeGameObject != null) // Layer Distances
        {
            var ls = Camera.main.layerCullDistances;
            var lr = Selection.activeGameObject.layer;
            var oldv = ls[lr];
            ls[lr] = gui.FloatField("LayerDist", ls[lr]);
            if (oldv != ls[lr])
                Camera.main.layerCullDistances = ls;
        }
        GUI.BeginHorizontal();
        bake = GUI.Toggle(bake, "Bake", GUI.ExpandWidth(false));
        
        lfactor = EditorGUILayout.FloatField(lfactor, GUI.Width(30));
        GUI.Label("ambient", GUI.Width(30));
        dfactor = EditorGUILayout.FloatField(dfactor, GUI.Width(30));
        GUI.Label("Directional", GUI.Width(30));
        //if (GUI.Button("SetupLevel"))
        //    LevelSetup();
        GUI.EndHorizontal();
        GUI.BeginHorizontal();
        EditorPrefs.SetBool("Debug", GUI.Toggle(EditorPrefs.GetBool("Debug"), "debug", GUI.ExpandWidth(false)));
        web = GUI.Toggle(web, "web", GUI.ExpandWidth(false));
        GUI.EndHorizontal();
        if (GUI.Button("Build"))
        {
            Build();
            return;
        }
        if (Selection.activeGameObject != null)
        {
            var bs2 = Selection.activeGameObject.GetComponent<Base>();
            if (bs2 != null)
                bs2.OnEditorGui();
        }
        DrawObjects();
        DrawSearch();
    }
    class DTR
    {
        public Transform transform;
        public string path = "";
    }
    static IEnumerable<DTR> GetTransforms2(DTR ts)
    {
        yield return ts;
        foreach (Transform t in ts.transform)
        {
            foreach (var t2 in GetTransforms2(new DTR { path = (ts.path + "/" + t.name), transform = t }))
                yield return t2;
        }
    }
    [MenuItem("File/SetupLevel")]    
    private void LevelSetup()
    {
        Undo.RegisterSceneUndo("rtools");

        var Level = GameObject.Find("Level");
        var oldl = Level.transform.Find("level");
        if (oldl != null)
            DestroyImmediate(oldl.gameObject);
        string path = EditorApplication.currentScene.Split('.')[0] + "/";
        path = path.Substring("Assets/".Length);
        Debug.Log("setup level: " + path);
        var nl = (GameObject)EditorUtility.InstantiatePrefab(GetAssets<GameObject>(path, "*.FBX").FirstOrDefault());
        nl.transform.parent = Level.transform;
        nl.transform.position = Level.transform.position;
        nl.name = "level";
        Selection.activeGameObject = nl;


        foreach (var d in GetTransforms2(new DTR { transform = Selection.activeGameObject.transform }))
        {
            var g = d.transform.gameObject;
            var pathA = d.path.TrimStart(new char[] { '/' });
            var p = Selection.activeGameObject.transform.parent.Find(pathA);
            if (p != null)
            {
                var mt = p.GetComponent<Base>();
                if (!(mt != null && mt.dontResetPos))
                {
                    p.transform.position = g.transform.position;
                    p.transform.rotation = g.transform.rotation;
                    p.transform.localScale = g.transform.localScale;
                }
                Clear(g, true);
            }
        }
        foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = LayerMask.NameToLayer("Level");
            t.gameObject.isStatic = true;
        }
        Inits();

        _TimerA.AddMethod(delegate
        {
            if (bake)
            {
                var old = RenderSettings.ambientLight;
                RenderSettings.ambientLight = Color.white * lfactor;
                var en = new Queue<LightShadows>();
                var q = new Queue<float>();
                foreach (Light a in GameObject.FindObjectsOfType(typeof(Light)))
                {

                    q.Enqueue(a.intensity);
                    en.Enqueue(a.shadows);
                    if (a.type == LightType.Directional)
                        a.intensity = dfactor;
                    a.shadows = LightShadows.Soft;
                }
                Lightmapping.BakeAsync();
                foreach (Light a in GameObject.FindObjectsOfType(typeof(Light)))
                {
                    a.intensity = q.Dequeue();
                    a.shadows = en.Dequeue();
                }
                RenderSettings.ambientLight = old;
            }
        });
    }
    private static void CrDir(string pt)
    {
        if (Directory.Exists(pt)) Directory.Delete(pt, true);
        Directory.CreateDirectory(pt);
    }
    public void Inits()
    {
        InitTransforms();
        foreach (var go in Selection.gameObjects)
        {
            foreach (Animation anim in go.GetComponentsInChildren<Animation>().Cast<Animation>().ToArray())
                if (anim.clip == null) DestroyImmediate(anim);
            foreach (var scr in go.GetComponents<Base>())
            {
                foreach (var pf in scr.GetType().GetFields())
                {
                    FindAsset(scr, pf);
                    //FindTransform(scr, pf);
                }                
                scr.Init();
            }
        }
    }
    static void InitTransforms()
    {
        foreach (Base bs in GameObject.FindObjectsOfType(typeof(Base)))
        {
            //if (bs.name == "Game") Debug.Log("found");
            foreach (var pf in bs.GetType().GetFields())
            {
                FindTransform(bs, pf);
            }
        }
    }

    private static void FindTransform(Base scr, FieldInfo pf)
    {
        FindTransform atr = (FindTransform)pf.GetCustomAttributes(true).FirstOrDefault(a => a is FindTransform);
        if (atr != null)
        {
            string name = (atr.name == null) ? pf.Name : atr.name;
            Transform g;
            try
            {
                g = atr.self ? scr.transform : scr.transform.GetTransforms().FirstOrDefault(a => a.name == name);
                //if (g == null) g = GameObject.Find(name).transform;
                if (g == null) throw new Exception();
                if (pf.FieldType == typeof(GameObject))
                    pf.SetValue(scr, g.gameObject);
                else
                {
                    var c = g.GetComponent(pf.FieldType);
                    if (c == null) throw new Exception();
                    pf.SetValue(scr, c);
                }
            }
            catch { Debug.Log(scr.name + " cound not find path " + scr.name + "+" + name); }
        }
    }
    private static void FindAsset(Base scr, FieldInfo pf)
    {
        FindAsset ap = (FindAsset)pf.GetCustomAttributes(true).FirstOrDefault(a => a is FindAsset);
        if (ap != null)
        {
            string name = (ap.name == null) ? pf.Name : ap.name;
            object value = pf.GetValue(scr);
            if (ap.overide || (value == null || value.Equals(null)) || (value is IEnumerable && ((IEnumerable)value).Cast<object>().Count() == 0))
            {
                if (value is Array)
                {
                    Debug.Log("FindAsset " + name);
                    var type = value.GetType().GetElementType();
                    var q = Base.GetFiles().Where(a => a.Contains(name)).Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, type)).Where(a => a != null);
                    if (q.Count() == 0)
                        Debug.Log("could not find folder " + name);

                    pf.SetValue(scr, Cast(q, type));
                }
                else
                {
                    Debug.Log("FindAsset " + name);
                    try
                    {
                        pf.SetValue(scr, Base.FindAsset(name, pf.FieldType));
                    }
                    catch (Exception e) { Debug.Log(scr.name + ":" + e.Message); }
                }
            }
        }
    }
    private static object Cast<T>(IEnumerable<T> objectList, Type t)
    {
        object a = typeof(Enumerable)
            .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
            .MakeGenericMethod(t)
            .Invoke(null, new[] { objectList });
        var b = typeof(Enumerable)
            .GetMethod("ToArray", BindingFlags.Public | BindingFlags.Static)
            .MakeGenericMethod(t)
            .Invoke(null, new[] { a });
        return b;
    }
    Type[] types = new Type[] { typeof(GameObject), typeof(Material) };
    private void DrawObjects()
    {
        List<string> toremove = new List<string>();
        foreach (var inst in instances)
        {
            GUI.BeginHorizontal();
            if (GUI.Button(inst))
            {
                Object o = GameObject.Find(inst) != null ? GameObject.Find(inst) : GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)).FirstOrDefault(a => a.name == inst);
                Selection.activeObject = o;
            }
            if (GUI.Button("X", GUI.ExpandWidth(false)))
                toremove.Add(inst);
            GUI.EndHorizontal();
        }
        foreach (var inst in toremove)
        {
            instances.Remove(inst);
            SaveParams();
        }

    }
    private void DrawSearch()
    {

        search = EditorGUILayout.TextField("search",search);
        EditorGUIUtility.LookLikeInspector();
        
        var ago = Selection.activeGameObject;
        var ao = Selection.activeObject;
        if (types.Contains(ao.GetType()) && ao != null && !(ago != null && ago.camera != null) && search.Length > 0)
        {
            IEnumerable<Object> array = new Object[] { ao };
            if (ago != null)
            {
                array = array.Union(ago.GetComponents<Component>());
                if (ago.renderer != null)
                    array = array.Union(new[] { ago.renderer.sharedMaterial });
            }
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
                        //Debug.Log(pr.name);
                        SetMultiSelect(m, pr);
                    }
                }
                while (pr.NextVisible(true));
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
            case SerializedPropertyType.Enum:
                MySetValue(m, pr.enumValueIndex, pr.propertyPath, pr.propertyType);
                break;
        }
    }
    void MySetValue(Object c, object value, string prName, SerializedPropertyType type)
    {
        var array = Selection.gameObjects.Select(a => a.GetComponent(c.GetType())).Cast<Object>().Union(Selection.objects.Where(a => !(a is GameObject)));
        if (c is Material)
        {
            var d = Selection.gameObjects.Select(a => a.renderer).SelectMany(a => a.sharedMaterials).Distinct();
            array = array.Union(d.Cast<Object>());
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
                    case SerializedPropertyType.Enum:
                        pr.enumValueIndex = (int)value;
                        break;
                }

                so.ApplyModifiedProperties();
            }
        }
    }
    public void ResetCam()
    {
        SetCam = false;
        if (Camera.main != null && Camera.main.transform.parent != null)
        {
            Camera.main.transform.position = Camera.main.transform.parent.position;
            Camera.main.transform.rotation = Camera.main.transform.parent.rotation;
        }
    }
    protected virtual void SaveParams()
    {
        EditorPrefs.SetString(EditorApplication.applicationPath, string.Join(",", instances.ToArray()));
    }
    public GameObject selectedGameObject;
    public DateTime idletime;
    private void OnSceneUpdate(SceneView scene)
    {
        if (Event.current.isMouse) idletime = DateTime.Now;
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
        var scenecam = scene.camera;
        if (SetCam)
        {
            //if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            //{
            //    var t = Camera.main.transform;
            //    scene.LookAt(t.position,t.rotation,3);
            //    Camera.main.GetComponent<GUILayer>().enabled = true;
            //}
            //else
            {
                var t = Camera.main.transform;
                t.position = scene.camera.transform.position;
                t.rotation = scene.camera.transform.rotation;
                Camera.main.GetComponent<GUILayer>().enabled = false;
            }
        }
        var e = Event.current;
        var p = e.mousePosition;
        if (e.keyCode == KeyCode.G && e.type == EventType.KeyUp)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(new Vector2(p.x, p.y));
            RaycastHit h;

            if (Physics.Raycast(r, out h))
                scene.LookAt(h.point - 5 * r.direction, scenecam.transform.rotation, 5);
            if (e.modifiers == EventModifiers.Control && Selection.activeGameObject != null)
            {
                Undo.RegisterSceneUndo("rtools");
                var o = (GameObject)EditorUtility.InstantiatePrefab(Selection.activeGameObject);
                o.transform.localPosition = Vector3.zero;
                o.transform.position = h.point;
                o.transform.rotation = Quaternion.AngleAxis(90, Vector3.up) * Quaternion.LookRotation(h.normal);
                //* Quaternion.LookRotation(h.point - SceneView.lastActiveSceneView.camera.transform.position);
            }
        }
    }
    #region menuitems    
    [MenuItem("Edit/Play % ")]    
    private static void Play()
    {
        Debug.Log("PLay");
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
            //if (!EditorApplication.isPaused)
            //{
            //    Screen.lockCursor = true;                
            //}
        }
        else
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
        
    }
    [MenuItem("File/Backup")]    
    private static void Backup()
    {
        if (!isPlaying && EditorApplication.currentScene.Contains(".unity"))
        {
            EditorApplication.SaveAssets();
            EditorApplication.SaveScene(EditorApplication.currentScene); //autosave
            //var cs = EditorApplication.currentScene;
            //var dir = Path.GetDirectoryName(cs) + "/" + Path.GetFileNameWithoutExtension(cs) + "/Backups/";
            //if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //var p = dir + Path.GetFileNameWithoutExtension(cs) + datetime + Path.GetExtension(cs);
            //Debug.Log("backup: " + p);
            //EditorApplication.SaveScene(p);
        }
    }
    [MenuItem("Edit/Capture Screenshot %e")]
    static void Capture()
    {
        var c = EditorApplication.currentScene;
        var dir = Path.GetDirectoryName(c) + "/" + Path.GetFileNameWithoutExtension(c) + "/ScreenShots/";
        Directory.CreateDirectory(dir);
        var file = dir + datetime + ".png";
        Debug.Log("saved to: " + file);
        Application.CaptureScreenshot(file);
    }    
    [MenuItem("GameObject/Attach Prefab")]
    static void AttachPrefabToSelectedObjects()
    {
        var b = Selection.gameObjects.FirstOrDefault(a => AssetDatabase.IsMainAsset(a));
        foreach (var g in Selection.gameObjects.ToArray())
        {
            if (g != b)
            {
                var p = EditorUtility.InstantiatePrefab(b);
                var d = (GameObject)p;
                d.transform.parent = g.transform;
                d.transform.position = g.transform.position;
            }
        }
    }
    
    [MenuItem("GameObject/Duplicate100")]
    static void Duplicate()
    {
        Undo.RegisterSceneUndo("rtools");
        var store = new GameObject("test");
        store.transform.position = Selection.activeGameObject.transform.position;
        for (int i = 0; i < 50; i++)
        {
            var g = (GameObject)EditorUtility.InstantiatePrefab(EditorUtility.GetPrefabParent(Selection.activeGameObject));
            g.transform.rotation = Random.rotation;
            g.transform.parent = store.transform;
        }
    }
    [MenuItem("GameObject/CubeMap")]
    private static void SetupMaterials()
    {
        if (Selection.activeObject is Cubemap)
        {
            Cubemap cb = (Cubemap)Selection.activeObject;            
            var c = SceneView.lastActiveSceneView.camera;
            
            c.RenderToCubemap(cb);
            //DestroyImmediate(c);
            //Debug.Log("rendered to cubemap");
        }
    }
    //[MenuItem("File/Refresh")]
    //static void RefreshProject()
    //{
    //    if (!isPlaying)
    //    {
    //        //var c = EditorApplication.currentScene;
    //        //EditorApplication.SaveScene(c);
    //        ////EditorApplication.NewScene();
    //        //var s = @"/Assets/scenes/2.unity";
    //        ////EditorApplication.SaveScene("tempscene");
    //        //for (int i = 0; i < 2; i++)
    //        //{
    //        //    EditorApplication.OpenScene(s);                                                   
    //        //    EditorApplication.OpenScene(EditorApplication.currentScene);                                             
    //        //}
    //        //File.Delete("tempscene");
    //        //EditorApplication.OpenScene(c);

    //        //if (!isPlaying)
    //        {
    //            var c = EditorApplication.currentScene;
    //            EditorApplication.SaveScene(c);
    //            for (int i = 0; i < 4; i++)
    //            {
    //                EditorApplication.OpenScene("Assets/scenes/1.unity");
    //                EditorApplication.OpenScene("Assets/scenes/2.unity");
    //            }
    //            EditorApplication.OpenScene(c);
    //            return;
    //        }
            
    //    }        
    //}
    [MenuItem("GameObject/Combine")]
    static void Combine()
    {
        Undo.RegisterSceneUndo("rtools");
        
        var g = Selection.activeGameObject;
        Base.Combine(g);
    }

    
    [MenuItem("GameObject/Parent")]
    static void CreateParent()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var t2 = new GameObject("Parent").transform;
        t2.position = t.position;
        t2.rotation = t.rotation;
        t2.parent = t.parent;
        t.parent = t2;
        t2.name = t.name;
    }
    [MenuItem("GameObject/Child")]
    static void CreateChild()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var nwt = new GameObject("Child").transform;
        nwt.position = t.position;
        nwt.rotation = t.rotation;
        nwt.parent = t;
        nwt.name = t.name;
    }

    [MenuItem("GameObject/UnParent")]
    static void UnParent()
    {
        Undo.RegisterSceneUndo("rtools");
        Selection.activeTransform.parent = Selection.activeTransform.parent.parent;
    }
    [MenuItem("Window/Rtools", false, 0)]
    static void rtoolsclick()
    {
        EditorWindow.GetWindow<InspectorSearch>();
    }
    [MenuItem("Assets/Add Labels %t")]
    static void ApplyLabels()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var asset in Selection.objects)
        {
            if (AssetDatabase.IsMainAsset(asset))
            {
                var apath = AssetDatabase.GetAssetPath(asset);
                var list = AssetDatabase.GetLabels(asset);
                var nwlist = list.Union(apath.Split('/').Skip(1));
                AssetDatabase.SetLabels(asset, nwlist.ToArray());
                EditorUtility.SetDirty(asset);
            }
        }
    }
    [MenuItem("GameObject/toCloth")]
    static void ToCloth()
    {
        Undo.RegisterSceneUndo("rtools");
        //var cols = Selection.gameObjects.Where(a => clothcollider(a));
        foreach (var g in Selection.gameObjects)
            if (!clothcollider(g))
            {
                Material mt = null;
                var mf = g.GetComponent<MeshFilter>();
                var cl = g.AddOrGet<InteractiveCloth>();
                var r = g.AddOrGet<ClothRenderer>();
                if (mf != null)
                {
                    var me = mf.sharedMesh;
                    mt = g.renderer.sharedMaterial;
                    r.sharedMaterial = mt;
                    cl.mesh = me;
                    cl.randomAcceleration = Vector3.one * 10;
                    DestroyImmediate(mf);
                    DestroyImmediate(g.renderer);
                    DestroyImmediate(g.collider);
                }
                //foreach (var col in cols)
                //    cl.AttachToCollider(col.collider,false,false);
            }

    }
    private static bool clothcollider(GameObject g)
    {
        return g.GetComponent<Base>() != null && g.GetComponent<Base>().ObjectType == ObjectType.clothCollider;
    }
    [MenuItem("Assets/Clear Labels %y")]
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
        var n = "D" + Random.Range(10, 99) + ".mat";
        foreach (var m in Selection.gameObjects.Select(a => a.renderer).SelectMany(a => a.sharedMaterials))
        {
            var p = AssetDatabase.GetAssetPath(m);
            var nwp = p.Substring(0, p.Length - 4) + n;
            AssetDatabase.CopyAsset(p, nwp);
            AssetDatabase.Refresh();
        }
        foreach (var a in Selection.gameObjects.Select(a => a.renderer))
        {
            var ms = a.sharedMaterials;
            for (int i = 0; i < ms.Count(); i++)
            {
                var p = AssetDatabase.GetAssetPath(ms[i]);
                var nwp = p.Substring(0, p.Length - 4) + n;
                ms[i] = (Material)AssetDatabase.LoadAssetAtPath(nwp, typeof(Material));
            }
            a.sharedMaterials = ms;
        }
    }
    [MenuItem("GameObject/Create Other/Prefab", priority =-1)]
    static void CreatePrefabs()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (GameObject g in Selection.gameObjects)
        {
            if (!AssetDatabase.IsMainAsset(g))
            {
                var p = EditorUtility.CreateEmptyPrefab("Assets/" + g.name + ".prefab");
                EditorUtility.ReplacePrefab(g, p, ReplacePrefabOptions.ConnectToPrefab);
                EditorUtility.SetDirty(g);
            }
            AssetDatabase.Refresh();
        }
    }
    //[MenuItem("GameObject/Clear")]
    //static void Clear()
    //{
    //    Undo.RegisterSceneUndo("rtools");
    //    foreach (var g in Selection.gameObjects)
    //        Clear(g, false);
    //}
    [MenuItem("File/Open Log")]
    private static void ParseLog()
    {
        string path = EditorUtility.OpenFilePanel("log", "", "txt");
        string s = File.ReadAllText(path);
        Debug.Log("Parsing Log: " + s.Length);
        var ms = Regex.Matches(s, @"(?s).*?\(.*?Line: (-?\d+)\)");
        foreach (Match m in ms.Cast<Match>())
        {
            var id = m.Groups[1].Value;
            var t = m.Value.Trim();
            if (id == "2528")
                Debug.Log(t);
            else if (id == "-1")
                Debug.LogError(t);
            else
                Debug.LogWarning(t);
        }
    }
    [MenuItem("GameObject/FixShaders")]
    public static void FixShaders()
    {
        foreach (MeshRenderer r in FindObjectsOfType(typeof(MeshRenderer)))
        {
            //var rs = g.Select(a => a.GetComponentsInChildren<MeshRenderer>());
            //foreach (var r in rs)
            {
                foreach (var m in r.sharedMaterials)
                {
                    m.shader = Shader.Find("Diffuse");
                }

            }
        }
    }
    [MenuItem("GameObject/FixMaterials")]
    public static void FixMaterials()
    {
        Undo.RegisterSceneUndo("SceneInit");
        var rs = Selection.gameObjects.SelectMany(a => a.GetComponentsInChildren<MeshRenderer>());
        foreach (var r in rs)
        {
            var sm = r.sharedMaterials;
            for (int i = 0; i < sm.Length; i++)
            {
                if (AssetDatabase.GetAssetPath(sm[i]) == "")
                {
                    var n = sm[i].name;
                    Debug.Log("Fixing Material " + r.name + "." + n);
                    n = Regex.Match(n, @"[\w\d- ]+").Value.Trim();
                    var pf = r.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(pf)) + "/Materials/" + n + ".mat";
                    //Debug.Log(path);
                    var m = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                    if (m != null)
                        sm[i] = m;
                    else
                        Debug.Log("material could not be found");
                }
            }
            r.sharedMaterials = sm;
        }
    }
    [MenuItem("GameObject/ClearCollider")]
    static void ClearColl()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var g in Selection.gameObjects)
            foreach (var c in g.GetComponentsInChildren<Collider>().ToArray())
                DestroyImmediate(c);
    }
    [MenuItem("GameObject/ClearAll")]
    static void ClearAll()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var g in Selection.gameObjects)
            Clear(g, true);
    }
    public static void Clear(GameObject g, bool all)
    {
        foreach (var c in g.GetComponents<Component>())
        {
            if (!(c is Transform || (c is Collider && !all)))
                DestroyImmediate(c);
            else if (c is Collider)
                ((Collider)c).isTrigger = true;
        }
    }
    [MenuItem("Assets/Reset pitch")]
    static void Pitch()
    {
        foreach (AudioSource a in FindObjectsOfTypeIncludingAssets(typeof(AudioSource)))
            a.pitch = 1;
    }
    [MenuItem("GameObject/ApplyAll")]
    static void ApplyAll()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var go in Selection.gameObjects)
        {
            if (EditorUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
            {
                EditorUtility.ReplacePrefab(go, EditorUtility.GetPrefabParent(go), ReplacePrefabOptions.UseLastUploadedPrefabRoot);
                EditorUtility.ResetGameObjectToPrefabState(go);
                EditorUtility.ReconnectToLastPrefab(go);
                Debug.Log("applied Prefab " + go.name);
            }
        }
        AssetDatabase.SaveAssets();
    }
    static Object[] tocopy;
    static bool move;
    [MenuItem("Assets/Copy")]
    static void CopyAsset()
    {
        tocopy = Selection.objects;
        move = false;
    }
    [MenuItem("Assets/Cut")]
    static void CutAsset()
    {
        tocopy = Selection.objects;
        move = true;
    }
    [MenuItem("Assets/Paste")]
    static void PasteAsset()
    {
        Undo.RegisterSceneUndo("rtools");
        if (tocopy == null) Debug.Log("null");
        var to = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (to == "") to = "Assets/";
        foreach (var a in tocopy.Select(a => AssetDatabase.GetAssetPath(a)))
        {
            if (!Directory.Exists(to))
                to = Path.GetDirectoryName(to);
            var b = to + "/" + Path.GetFileName(a);
            if (File.Exists(b))
                b = to + "/" + Path.GetFileNameWithoutExtension(a) + Random.Range(10, 99) + Path.GetExtension(a);
            Debug.Log("moving " + a + " to " + b + ":" + CopyAsset(a, b, move));
        }
        AssetDatabase.Refresh();
    }
    private static string CopyAsset(string a, string b, bool move)
    {

        if (move)
            return AssetDatabase.MoveAsset(a, b);
        else
            return AssetDatabase.CopyAsset(a, b) ? "success" : "failed";
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
    static Queue<Transform> trs;
    [MenuItem("GameObject/PushTransforms")]
    static void PushTransforms()
    {
        Undo.RegisterSceneUndo("rtools");
        trs = new Queue<Transform>();
        foreach (var g in Selection.gameObjects)
            trs.Enqueue(g.transform);
        Debug.Log(trs.Count);
    }
    [MenuItem("GameObject/PopTransforms")]
    static void PopTransforms()
    {
        Debug.Log(Selection.gameObjects.Length);
        if (trs != null)
        {
            Undo.RegisterSceneUndo("rtools");
            foreach (var g in Selection.gameObjects)
            {
                if (trs.Count == 0)
                    break;
                var t = trs.Dequeue();
                g.transform.position = t.position;
                g.transform.rotation = t.rotation;
            }
        }
    }
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


    const string duplicatePostfix = "_copy";

    static void CopyClip(string importedPath, string copyPath)
    {
        AnimationClip src = AssetDatabase.LoadAssetAtPath(importedPath, typeof(AnimationClip)) as AnimationClip;
        AnimationClip newClip = new AnimationClip();
        newClip.name = src.name + duplicatePostfix;
        AssetDatabase.CreateAsset(newClip, copyPath);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Copy Animation Clip")]
    static void CopyCurvesToDuplicate()
    {
        AnimationClip imported = Selection.activeObject as AnimationClip;
        if (imported == null) return;

        string importedPath = AssetDatabase.GetAssetPath(imported);
        string copyPath = importedPath.Substring(0, importedPath.LastIndexOf("/"));
        copyPath += "/" + imported.name + duplicatePostfix + ".anim";
        CopyClip(importedPath, copyPath);

        AnimationClip copy = AssetDatabase.LoadAssetAtPath(copyPath, typeof(AnimationClip)) as AnimationClip;
        if (copy == null)
            throw new Exception("No copy found at " + copyPath);
        
        AnimationClipCurveData[] curveDatas = AnimationUtility.GetAllCurves(imported, true);
        for (int i = 0; i < curveDatas.Length; i++)
        {
            AnimationUtility.SetEditorCurve(
                copy,
                curveDatas[i].path,
                curveDatas[i].type,
                curveDatas[i].propertyName,
                curveDatas[i].curve
            );
        }

        Debug.Log("Copying curves into " + copy.name + " is done");
    }

    //[MenuItem("GameObject/cacheMaterials")]
    //public static void CacheMaterials()
    //{
    //    mdir database = new mdir();
    //    XmlSerializer xs = new XmlSerializer(typeof(mdir));
    //    string str = EditorPrefs.GetString("matDatabase");
    //    if (str != null && str != "")
    //        try
    //        {
    //            using (var sr = new StringReader(str))
    //                database = (mdir)xs.Deserialize(sr);
    //        } catch (Exception e) { Debug.Log(e.Message); }
    //    Debug.Log("db" + database.Count);
    //    foreach (var render in Selection.gameObjects.SelectMany(a => a.GetComponentsInChildren<Renderer>()))
    //    {
    //        string[] foundvalue;
    //        var paths = (render.sharedMaterials.Select(m => Path.GetFileName(AssetDatabase.GetAssetPath(m))).ToArray());                       
    //        var mats = render.sharedMaterials;
    //        var key = render.transform.parent + "/" + render.name;
    //        if (database.TryGetValue(key, out foundvalue))
    //        {                
    //            for (int i = 0; i < paths.Length; i++)
    //            {
    //                if (mats[i] == null)
    //                {
    //                    paths[i] = foundvalue[i];
    //                    var p = Path.GetDirectoryName(AssetDatabase.GetAssetPath(EditorUtility.GetPrefabParent(render))) + "/Materials/" + paths[i];
    //                    Debug.Log("set material" + key + " " + i + " " + p);                        
    //                    mats[i] = (Material)AssetDatabase.LoadAssetAtPath(p, typeof(Material));

    //                }
    //            }
    //            render.sharedMaterials = mats;
    //        }
    //        database[key] = paths;
    //    }
    //    EditorPrefs.SetString(EditorApplication.currentScene + "mat", bs.SerializeToStr(database, xs));

    //}
    //[MenuItem("Edit/Capture Screenshot Custom")]
    //static void Cap()
    //{
    //    if (Selection.activeGameObject == null) return;
    //    Undo.RegisterSceneUndo("rtools");
    //    int size = 256;
    //    var sc = SceneView.lastActiveSceneView.camera;
    //    GameObject co = (GameObject)Instantiate(Base.FindAsset<GameObject>("SnapShotCamera"), sc.transform.position, sc.transform.rotation);
    //    Camera c = co.GetComponentInChildren<Camera>();
    //    RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
    //    c.targetTexture = rt;
    //    var output = new Texture2D(size, size, TextureFormat.ARGB32, false);
    //    RenderTexture.active = rt;
    //    var g = Selection.activeGameObject;
    //    var g2 = (GameObject)Instantiate(g, g.transform.position, g.transform.rotation);
    //    c.cullingMask = 1 << co.layer;
    //    foreach (var a in g2.GetComponentsInChildren<Transform>())
    //        a.gameObject.layer = co.layer;
    //    var r = g2.GetComponentInChildren<Renderer>();

    //    if (r == null) { Debug.Log("Render is null " + r.name); return; }
    //    g2.active = true;
    //    c.Render();
    //    output.ReadPixels(new Rect(0, 0, size, size), 0, 0);
    //    output.Apply();
    //    Color? bk = null;
    //    for (int x = 0; x < output.width; x++)
    //        for (int y = 0; y < output.height; y++)
    //        {
    //            var px = output.GetPixel(x, y);
    //            if (bk == null) bk = px;
    //            px.a = px == bk.Value ? 0 : .6f;
    //            output.SetPixel(x, y, px);
    //        }
    //    var p = AssetDatabase.GetAssetPath(g);


    //    p = (p == "" ? Application.dataPath + "/materials/Icons/" : Path.GetDirectoryName(p)) + "/" + g.name + ".png";

    //    File.WriteAllBytes(p, output.EncodeToPNG());
    //    Debug.Log("Saved: " + p);
    //    RenderTexture.ReleaseTemporary(rt);
    //    RenderTexture.active = null;
    //    c.targetTexture = null;
    //    DestroyImmediate(g2);
    //    DestroyImmediate(co);
    //}
    #endregion
    void OnSelectionChange()
    {
        this.Repaint();
        //Debug.Log(Selection.activeGameObject.GetComponent<bs>());

        //Update();
    }
    protected virtual void Update()
    {
        
        //EditorApplication.playmodeStateChanged = OnPlaymodeStateChanged;
        autosavetm += 0.01f;
        _TimerA.Update();
        SceneView.onSceneGUIDelegate = OnSceneUpdate;
        bool autosave = (DateTime.Now - idletime) < TimeSpan.FromMinutes(10) && autosavetm > 60 * 20;

        if (autosave)
        {
            autosavetm = 0;
            Backup();
        }
        var ao = Selection.activeObject;
        if (ao != null && !lastUsed.Contains(ao))
            lastUsed.Insert(0, ao);

        if (_TimerA.TimeElapsed(3000))
            this.Repaint();
    }
    public static bool isPlaying { get { return EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode; } }
    public static string datetime
    {
        get
        {
            return DateTime.Now.Ticks + "";
            //return DateTime.Now.ToString("yyyy-MM-dd hh-mm");
        }
    }
    IEnumerable<T> GetAssets<T>(string path, string pattern) where T : Object
    {
        foreach (string f2 in Directory.GetFiles("Assets/" + path, pattern, SearchOption.AllDirectories))
        {
            string f = f2.Replace(@"\", "/").Replace("//", "/");
            var a = (T)AssetDatabase.LoadAssetAtPath(f, typeof(T));
            if (a != null)
                yield return a;
        }
    }
    
    private void Build()
    {
        PlayerSettings.productName = "Arena Build " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        if (!web)
        {
            var fn = "game.exe";
            var path = @"Builds/";
            CrDir(path);
            BuildPipeline.BuildPlayer(Base.scenes, path + fn, BuildTarget.StandaloneWindows, BuildOptions.Development);
            Debug.Log("Stand Alone Bulid success");
        }
        if (web)
        {
            //var d = DateTime.Now;
            //var dt = d.Year + "-" + d.Month + "-" + d.Day + " " + d.Hour + "-" + d.Minute + "-" + d.Second + "/";
            //var folder = "Web/" + dt;
            //if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);            
            if (Directory.Exists("Index")) Directory.Delete("Index",true);
            BuildPipeline.BuildPlayer(Base.scenes, "Index", BuildTarget.WebPlayerStreamed, BuildOptions.None);
            CrDir("t2");
            BuildPipeline.BuildPlayer(new[] { Base.scenes[0] }, "t2/asd", BuildTarget.StandaloneWindows, BuildOptions.Development);
            Directory.Delete("t2", true);
            Debug.Log("Web Bulid success");
        }
    }
}
#endif