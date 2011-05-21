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
    string search = "";
    float autosavetm = 0;
    Type[] types = new Type[] { typeof(GameObject), typeof(Material) };
    public bool SetPivot;
    public bool SetCam;
    Vector3 oldPivot;
    protected virtual void OnGUI()
    {
        if (!SetPivot && Selection.activeGameObject) oldPivot = Selection.activeGameObject.transform.position;
        SetPivot = (GUI.Toggle(SetPivot, "Pivot", GUI.ExpandWidth(false)) && Selection.activeGameObject != null);

        SetCam = (GUI.Toggle(SetCam && Camera.main != null, "Cam", GUI.ExpandHeight(false))); //camset        
        QualitySettings.shadowDistance = gui.FloatField("LightmapDist", QualitySettings.shadowDistance);

        OnGUIOther();
        OnGUIMono();
        DrawSearch();
    }

    private void OnSceneUpdate(SceneView scene)
    {
        UpdateOther(scene);
        UpdateSetCam(scene);
    }
    private void UpdateOther(SceneView scene)
    {
        bool repaint = false;
        foreach (var a in GameObject.FindGameObjectsWithTag("EditorGUI"))
            a.GetComponent<Base>().OnSceneGUI(scene, ref repaint);
        if (repaint) { Repaint(); }

        var ago = Selection.activeGameObject;
        if (SetPivot)
        {
            var move = oldPivot - ago.transform.position;
            foreach (Transform t in ago.transform)
                t.position += move;
        }
        if (ago != null)
            oldPivot = ago.transform.position;
    }
    private void UpdateSetCam(SceneView scene)
    {
        if (SetCam)
        {

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                var t = Camera.main.transform;
                scene.LookAt(t.position, t.rotation, 3);
                //Camera.main.GetComponent<GUILayer>().enabled = true;
            }
            else
            {
                var t = Camera.main.transform;
                t.position = scene.camera.transform.position;
                t.rotation = scene.camera.transform.rotation;
                //Camera.main.GetComponent<GUILayer>().enabled = false;
            }
        }
    }
    private static void OnGUIOther()
    {
        if (GUI.Button("Init"))
            foreach (var go in Selection.gameObjects)
                foreach (var scr in go.GetComponents<Base>())
                    scr.Init();
        Base.debug = GUI.Toggle(Base.debug, "debug", GUI.ExpandWidth(false));
    }
    private static void OnGUIMono()
    {
        foreach (var a in GameObject.FindGameObjectsWithTag("EditorGUI"))
            a.GetComponent<Base>().OnEditorGui();
    }
    void OnSelectionChange()
    {

        SetPivot = false;
        search = "";
        this.Repaint();
    }
    protected virtual void Update()
    {
        SceneView.onSceneGUIDelegate = OnSceneUpdate;

        autosavetm += 0.01f;
        if (autosavetm > 60 * 2)
        {
            autosavetm = 0;
            Backup();
        }
    }


    private void DrawSearch()
    {

        search = EditorGUILayout.TextField("search", search);
        EditorGUIUtility.LookLikeInspector();
        var ago = Selection.activeGameObject;
        var ao = Selection.activeObject;
        if (search.Length > 0 && types.Contains(ao.GetType()) && ao != null && !(ago != null && ago.camera != null))
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
    public static bool isPlaying { get { return EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode; } }

    [MenuItem("File/Backup")]
    private static void Backup()
    {
        if (!isPlaying && EditorApplication.currentScene.Contains(".unity"))
        {
            EditorApplication.SaveAssets();
            EditorApplication.SaveScene(EditorApplication.currentScene);
        }
    }
    [MenuItem("Window/Rtools", false, 0)]
    static void rtoolsclick()
    {
        EditorWindow.GetWindow<InspectorSearch>();
    }
    [MenuItem("Assets/TakeScreenshot")]
    static void TakeScreenshot()
    {
        var g = UnityEditor.Selection.activeObject;
        var Texture = UnityEditor.EditorUtility.GetAssetPreview(g);
        File.WriteAllBytes("Assets/" + g.name + ".png", Texture.EncodeToPNG());
    }
    [MenuItem("Edit/Play % ")]
    private static void Play()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPaused = !EditorApplication.isPaused;
        else
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
    }
    [MenuItem("Assets/Create/Prefab", priority = 0)]
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
    [MenuItem("GameObject/CubeMap")]
    private static void SetupMaterials()
    {
        //if (Selection.activeObject is Cubemap)
        {
            Cubemap cb = new Cubemap(64, TextureFormat.RGB24, false);
            var c = SceneView.lastActiveSceneView.camera;
            c.RenderToCubemap(cb);
            AssetDatabase.CreateAsset(cb, "Assets/cb.cubemap");
            //DestroyImmediate(c);
            //Debug.Log("rendered to cubemap");
        }
    }

    [MenuItem("GameObject/Group")]
    static void Group()
    {
        Undo.RegisterSceneUndo("rtools");
        var g = Selection.activeGameObject;

        g = new GameObject("Group");
        var pos = new Vector3(Selection.gameObjects.Average(a => a.transform.position.x), Selection.gameObjects.Average(a => a.transform.position.y), Selection.gameObjects.Average(a => a.transform.position.z));
        g.transform.position = pos;
        g.transform.parent = Selection.activeGameObject.transform.parent;
        foreach (var t in Selection.gameObjects)
            t.transform.parent = g.transform;
    }
    [MenuItem("GameObject/Combine")]
    static void Combine()
    {
        Base.Combine(Selection.activeGameObject);
    }

    [MenuItem("GameObject/Duplicate Animation")]
    static void DupAnim()
    {
        var p = AssetDatabase.GetAssetPath(Selection.activeGameObject.animation.clip);
        var nwp = p.Substring(0, p.Length - 5) + "1.anim";
        AssetDatabase.CopyAsset(p, nwp);
        var anim = (AnimationClip)AssetDatabase.LoadAssetAtPath(nwp, typeof(AnimationClip));
        Selection.activeGameObject.animation = anim;
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
    [MenuItem("GameObject/Create Child")]
    static void CreateChild()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var nwt = new GameObject("Child").transform;
        nwt.position = t.position;
        nwt.rotation = t.rotation;
        nwt.parent = t;
        nwt.name = "Child";
    }
}