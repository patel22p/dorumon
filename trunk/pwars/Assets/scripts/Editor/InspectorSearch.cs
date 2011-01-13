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

        GUI.BeginHorizontal();
        if (GUI.Button("ApplyAll"))
            foreach (var go in Selection.gameObjects)
            {
                EditorUtility.ReplacePrefab(go, EditorUtility.GetPrefabParent(go), ReplacePrefabOptions.UseLastUploadedPrefabRoot);
                EditorUtility.ResetGameObjectToPrefabState(go);
                AssetDatabase.SaveAssets();
            }
        if (GUI.Button("Replace"))
        {
            Undo.RegisterSceneUndo("Replace");
            var asset = Selection.gameObjects.FirstOrDefault(a => AssetDatabase.IsMainAsset(a));
            if (asset != null)
            {
                foreach (var a in Selection.gameObjects)
                    if (!AssetDatabase.IsMainAsset(a))
                    {
                        var nw = (GameObject)Instantiate(asset, a.transform.position, a.transform.rotation);
                        nw.transform.parent = a.transform.root;
                        nw.name = a.name;
                        DestroyImmediate(a);
                    }
            }
            
        }
        //CopyComponent();
        CapturePrefabs();        
        if (GUI.Button("AddToList"))
            if (!instances.Contains(Selection.activeGameObject.name))
                instances.Add(Selection.activeGameObject.name);
        GUI.EndHorizontal();
        DrawObjects();
        DrawSearch();
    }
    private void CapturePrefabs()
    {
        if (GUI.Button("Cap") && Selection.activeGameObject != null)
        {
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
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            GameObject g = t.gameObject;
            bool glow = t.name.Contains("glow");
            if (t.name.Contains("glass") || glow)
            {
                foreach (var t2 in t.GetComponentsInChildren<Transform>())
                {
                    if (glow && t2.collider != null)
                        t2.collider.isTrigger = true;

                    if (t2.GetComponent<Renderer>() != null)
                        t2.renderer.castShadows = false;
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
        //if (_TimerA.TimeElapsed(60 * 1000) && !EditorApplication.isPlaying && !EditorApplication.isPaused && EditorApplication.currentScene.Contains(".scene"))
        //{
        //    Debug.Log("autosave");
        //    EditorApplication.SaveScene(EditorApplication.currentScene);
        //}
        var ao = Selection.activeObject;
        if (ao != null && !lastUsed.Contains(ao))
            lastUsed.Insert(0, ao);

        if (_TimerA.TimeElapsed(3000))
            ewnd.Repaint();
    }
    public static EditorWindow _ewnd;
    protected virtual EditorWindow ewnd
    {
        get
        {
            if (_ewnd == null) _ewnd = EditorWindow.GetWindow(this.GetType());
            return _ewnd;
        }
    }
}
#endif