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
using Random = UnityEngine.Random;
public class InspectorSearch : EditorWindow
{
    public List<string> instances = new List<string>();
    List<Object> lastUsed = new List<Object>();
    string search = "";
    public bool SetPivot;
    public bool SetCam;
    Vector3 oldpos;
    public static string datetime { get { return DateTime.Now.ToString("yyyy-MM-dd hh-mm"); } }
    public virtual void Awake()
    {
        instances = EditorPrefs.GetString(EditorApplication.applicationPath).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    protected virtual void OnGUI()
    {

        if (!SetPivot && Selection.activeGameObject) oldpos = Selection.activeGameObject.transform.position;        
        GUI.BeginHorizontal();       
        SetPivot = (GUI.Toggle(SetPivot, "Pivot",GUI.ExpandWidth(false)) && Selection.activeGameObject != null);
        var old = SetCam;
        SetCam = (GUI.Toggle(SetCam && Camera.main != null, "Cam", GUI.ExpandHeight(false))); //camset
        if (SetCam != old && SetCam == false) 
            ResetCam();
        if (GUI.Button("Apply"))
            ApplyAll();
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeGameObject.name))
                instances.Add(Selection.activeGameObject.name);
        GUI.EndHorizontal();
        DrawObjects();
        DrawSearch();
    }
    Type[] types = new Type[] { typeof(GameObject), typeof(Material) };
    private void DrawSearch()
    {
        search = EditorGUILayout.TextField(search);
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
        if (Camera.main != null)
        {
            Camera.main.transform.position = Camera.main.transform.parent.position;
            Camera.main.transform.rotation = Camera.main.transform.parent.rotation;
        }
    }
    
    protected virtual void SaveParams()
    {
        EditorPrefs.SetString(EditorApplication.applicationPath, string.Join(",", instances.ToArray()));
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
            bool glow = g.tag == "Glow";
            if (g.tag == "glass" || glow)
            {
                foreach (var t in g.GetTransforms())
                {
                    if (glow && t.collider != null)
                        t.collider.isTrigger = true;
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
    void OnInspectorGUI()
    {
        
    }
    public DateTime idletime;
    private void OnSceneUpdate(SceneView s)
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
        var c = s.camera;
        if (SetCam)
        {
            Camera.main.transform.position = s.camera.transform.position;
            Camera.main.transform.rotation = s.camera.transform.rotation;
        }
        var e = Event.current;
        var p = e.mousePosition;
        if (e.keyCode == KeyCode.G && e.type == EventType.KeyUp)
        {

            Ray r = HandleUtility.GUIPointToWorldRay(new Vector2(p.x, p.y));
            RaycastHit h;
            if (Physics.Raycast(r, out h))
                s.LookAt(h.point - 5 * r.direction, c.transform.rotation, 5);
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
    [MenuItem("Edit/Capture Screenshot %e")]
    static void Capture()
    {
        var c = EditorApplication.currentScene;
        var dir = Path.GetDirectoryName(c) + "/" + Path.GetFileNameWithoutExtension(c) + "/ScreenShots/";
        Directory.CreateDirectory(dir);
        var file = dir + datetime + ".jpg";
        Debug.Log("saved to: " + file);
        Application.CaptureScreenshot(file);
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
        EditorWindow.GetWindow<ETools>();
    }
    [MenuItem("Assets/Add Labels %t")]
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
        foreach (var m in Selection.gameObjects.Select(a => a.renderer).SelectMany(a => a.sharedMaterials))
        {
            var p = AssetDatabase.GetAssetPath(m);
            var nwp = p.Substring(0, p.Length - 4) + "D.mat";
            AssetDatabase.DeleteAsset(nwp);
            AssetDatabase.CopyAsset(p, nwp);
            AssetDatabase.Refresh();
        }
        foreach (var a in Selection.gameObjects.Select(a=>a.renderer))
        {
            var ms = a.sharedMaterials;
            for (int i = 0; i < ms.Count(); i++)
            {
                var p = AssetDatabase.GetAssetPath(ms[i]);
                var nwp = p.Substring(0, p.Length - 4) + "D.mat";                
                ms[i] = (Material)AssetDatabase.LoadAssetAtPath(nwp, typeof(Material));
            }
            a.sharedMaterials = ms;
        }
    }
    //[MenuItem("GameObject/Create Prefab")]
    //static void CreatePrefabs()
    //{
    //    Undo.RegisterSceneUndo("rtools");
    //    foreach (GameObject g in Selection.gameObjects)
    //    {
    //        if (!AssetDatabase.IsMainAsset(g))
    //        {
    //            Directory.CreateDirectory(Application.dataPath + "/" + g.transform.parent.name);
    //            var p = EditorUtility.CreateEmptyPrefab("Assets/" + g.transform.parent.name + "/" + g.name + ".prefab");
    //            EditorUtility.ReplacePrefab(g, p, ReplacePrefabOptions.ConnectToPrefab);
    //            EditorUtility.SetDirty(g);
    //        }
    //        AssetDatabase.Refresh();
    //    }
    //}
    [MenuItem("GameObject/Clear")]
    static void Clear()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var g in Selection.gameObjects)
            Clear(g, false);
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
    [MenuItem("GameObject/ApplyAll")]
    static void ApplyAll()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (var go in Selection.gameObjects)
        {
            Debug.Log("Applied:" + go.name);
            EditorUtility.ReplacePrefab(go, EditorUtility.GetPrefabParent(go), ReplacePrefabOptions.UseLastUploadedPrefabRoot);
            EditorUtility.ResetGameObjectToPrefabState(go);
            EditorUtility.ReconnectToLastPrefab(go);
        }
        AssetDatabase.SaveAssets();
    }
    static Object[] tocopy;
    static bool move;
    [MenuItem("Assets/Copy #c")]
    static void CopyAsset()
    {        
        tocopy = Selection.objects;
        move = false;
    }
    [MenuItem("Assets/Paste #v")]
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
    [MenuItem("Assets/Move #x")]
    static void MoveAsset()
    {        
        tocopy = Selection.objects;
        move = true;
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
    public TimerA _TimerA = new TimerA();
    float autosavetm = 0;

    protected virtual void Update()
    {
        autosavetm += 0.01f;
        _TimerA.Update();        
        SceneView.onSceneGUIDelegate = OnSceneUpdate;
        bool autosave = (DateTime.Now - idletime) < TimeSpan.FromMinutes(10) && autosavetm >360;

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
    [MenuItem("File/Backup")]
    private static void Backup()
    {
        if (!isPlaying && EditorApplication.currentScene.Contains(".unity"))
        {            
            EditorApplication.SaveAssets();
            EditorApplication.SaveScene(EditorApplication.currentScene); //autosave
            var cs = EditorApplication.currentScene;
            var dir = Path.GetDirectoryName(cs) + "/" + Path.GetFileNameWithoutExtension(cs) + "/Materials/";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            EditorApplication.SaveScene(dir + Path.GetFileNameWithoutExtension(cs) +datetime+ Path.GetExtension(cs));
        }
    }
    
    [MenuItem("Edit/Capture Screenshot Custom")]
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
}
#endif