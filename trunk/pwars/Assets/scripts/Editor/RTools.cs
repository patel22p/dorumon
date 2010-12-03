using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using System.IO;
using System.Collections;
using AstarClasses;

[ExecuteInEditMode]
public partial class RTools : InspectorSearch
{

    string file;
    protected override void OnGUI()
    {
        if (GUI.Button("Init"))
            Init();
        base.OnGUI();
        BuildGUI();

    }
    private void Init()
    {
        string cspath = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\scripts\GUI\";
        Undo.RegisterSceneUndo("SceneInit");
        foreach (UnityEngine.Object go in FindObjectsOfTypeIncludingAssets(typeof(UnityEngine.Object)))
        {
            if (AssetDatabase.IsMainAsset(go))
            {

                if ((go is GameObject && go.hideFlags != HideFlags.NotEditable ) || go is PhysicMaterial || go is MonoScript)
                    AssetDatabase.SetLabels(go, new[] { go.name });
                else
                    AssetDatabase.SetLabels(go, new string[] { });
            }
        }
        if (gameScene)
        {
            GameObject.Find("map").tag = "Level";
            foreach (Transform t in GameObject.FindGameObjectWithTag("Level").GetComponentsInChildren<Transform>())
            {
                GameObject g = t.gameObject;                
                g.layer = LayerMask.NameToLayer("Level");
                g.isStatic = true;
                if (g.name == "path")
                    g.renderer.enabled = false;

                foreach (string s in Enum.GetNames(typeof(MapItemType)))
                {
                    if (t.name.StartsWith(s) && g.GetComponent<MapItem>() == null)
                    {
                        g.AddComponent<NetworkView>();
                        g.AddComponent<AudioSource>();
                        if (g.animation != null && g.animation.clip != null)
                        {
                            
                            AnimationUtility.SetAnimationEvents(g.animation.clip, new[] { 
                                new AnimationEvent() { time = g.animation.clip.length, functionName = "Stop" }, 
                                new AnimationEvent() { time = 0, functionName = "Stop" }
                            });                            
                            
                            g.AddComponent<Rigidbody>();
                            g.isStatic = false;
                            g.networkView.observed = g.animation;
                            g.animation.playAutomatically = true;
                            g.rigidbody.isKinematic = true;
                            g.animation.animatePhysics = true;                            
                        }
                        MapItem mi = g.AddComponent<MapItem>();
                        mi.itemType = (MapItemType)Enum.Parse(typeof(MapItemType), s);                                                                          
                    }
                }
            }
        }
        foreach (var go in Selection.gameObjects)
        {
            foreach (var scr in go.GetComponentsInChildren<Base2>())
            {
                foreach (var pf in scr.GetType().GetFields())
                {
                    InitLoadPath(scr, pf);
                    CreateEnum(cspath, scr, pf);
                }
                scr.Init();
            }
        }
    }
    
    private static void InitLoadPath(Base2 scr, FieldInfo pf)
    {
        LoadPath ap = (LoadPath)pf.GetCustomAttributes(true).FirstOrDefault(a => a is LoadPath);
        if (ap != null)
        {
            //Debug.Log("Found Load Path " + ap.name);
            if (pf.FieldType == typeof(AudioClip))
                pf.SetValue(scr, LoadAudioClip(ap.name));
            else if (pf.FieldType == typeof(GameObject))
                pf.SetValue(scr, LoadPrefab(ap.name));
            else if (pf.FieldType == typeof(AudioClip[]))
                pf.SetValue(scr, LoadAudioClips(ap.name));
            else
                pf.SetValue(scr, LoadAsset(ap.name, pf.FieldType));
        }
    }
    bool gameScene { get { return EditorApplication.currentScene.Contains("Game.unity"); } }
    private void BuildGUI()
    {
        if (!gameScene) return;

        GUI.Space(10);
        if (Application.isPlaying && Application.loadedLevelName.Contains("Game"))
        {
            foreach (Player p in Base2._Game.players)
                if (p != null)
                    if (GUI.Button(p.name + ":" + p.OwnerID))
                        Selection.activeObject = p;
        }

        GUI.BeginHorizontal();
        if (GUILayout.Button("Build"))
        {
            Build();
            return;
        }
        GUI.EndHorizontal();
        GUI.BeginHorizontal();
        if (GUILayout.Button("Server Editor"))
        {
            _Loader.mapSettings.host = true;
            new SerializedObject(_Loader).ApplyModifiedProperties();
            EditorApplication.isPlaying = true;
        }
        if (GUILayout.Button("Server App"))
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + file, "server");
        GUI.EndHorizontal();
        GUI.BeginHorizontal();
        if (GUILayout.Button("Client App"))
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + file, "client");
        if (GUILayout.Button("Client Editor"))
        {
            _Loader.mapSettings.host = false;
            new SerializedObject(_Loader).ApplyModifiedProperties();
            EditorApplication.isPlaying = true;
        }

        GUI.EndHorizontal();
        if (GUILayout.Button("Open Project Folder"))
        {
            System.Diagnostics.Process.Start(@"C:\Users\igolevoc\Documents\PhysxWars");
        }
        _Loader.build = GUI.Toggle(_Loader.build, "build");
        _Loader.disablePathFinding = GUI.Toggle(_Loader.disablePathFinding, "disable path finding");
        _Loader.dontcheckwin = GUI.Toggle(_Loader.dontcheckwin, "dont check win");
    }
    private static void CreateEnum(string cspath, Base2 g, FieldInfo f)
    {
        GenerateEnums ge = (GenerateEnums)f.GetCustomAttributes(true).FirstOrDefault(a => a is GenerateEnums);
        if (ge != null)
        {
            string cs = "";
            Debug.Log("Found!" + ge.name);
            cs += "public enum " + ge.name + ":int{";
            var ie = (IEnumerable)f.GetValue(g);
            foreach (Base o in ie)
                cs += o.name + ",";
            cs = cs.Trim(new[] { ',' });
            cs += "}";
            Debug.Log("geneerated:" + cs);
            File.WriteAllText(cspath + ge.name + ".cs", cs);
        }
    }
    private void Build()
    {
        file = "Builds/" + DateTime.Now.ToFileTime() + "/";
        Directory.CreateDirectory(file);
        BuildPipeline.BuildPlayer(new[] { "Assets/scenes/Game.unity" }, (file = file + "Game.Exe"), BuildTarget.StandaloneWindows, BuildOptions.Development);
    }
    private static Loader _Loader
    {
        get
        {

            Loader l = (Loader)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Loader)).First();
            return l;
        }
    }
    public static GameObject LoadPrefab(string path)
    {
        var g = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/" + path + ".prefab", typeof(GameObject)) ??
        (GameObject)AssetDatabase.LoadAssetAtPath("Assets/" + path + ".prefab", typeof(GameObject));
        if (g == null) Debug.Log("not found prefab " + path);
        return g;
    }
    public static UnityEngine.Object LoadAsset(string path, Type t)
    {
        var o = AssetDatabase.LoadAssetAtPath("Assets/" + path, t);
        if (o == null) Debug.Log("could not load asset " + path);
        return o;
    }
    public static AudioClip LoadAudioClip(string path)
    {
        var ac = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/sounds/" + path + ".wav", typeof(AudioClip));
        if (ac == null)
            ac = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/sounds/" + path + ".mp3", typeof(AudioClip));
        if (ac == null) Debug.Log("not found sound " + path);
        return ac;

    }
    public static AudioClip[] LoadAudioClips(string path)
    {
        path = "Assets/sounds/" + path + "/";
        List<AudioClip> aus = new List<AudioClip>();
        foreach (string s in Directory.GetFiles(path))
        {
            var au = (AudioClip)AssetDatabase.LoadAssetAtPath(s, typeof(AudioClip));
            if (au != null)
                aus.Add(au);
            else
                Debug.Log("not found audio+" + s);
        }
        return aus.ToArray();
    }
    //public static UnityEngine.Object[] LoadPrefabs(string name)
    //{
    //    return AssetDatabase.LoadAllAssetsAtPath("Assets/Prefabs/" + name);
    //} 

}

//[CustomEditor(typeof(LookAtPointEditor))]
//class LookAtPointEditor : Editor {


//    //void OnInspectorGUI () {
//    //    Debug.Log("asdasd");
//    //    //target.lookAtPoint = EditorGUILayout.Vector3Field ("Look At Point", target.lookAtPoint);
//    //    //if (GUI.changed)
//    //    //    EditorUtility.SetDirty (target);
//    //}
//    public override void OnInspectorGUI()
//    {
//        Debug.Log("ads");
//        base.OnInspectorGUI();
//    }
//    public override void OnPreviewGUI(Rect r, GUIStyle background)
//    {
//        Debug.Log("dssdf");
//        base.OnPreviewGUI(r, background);
//    }
//    //void OnSceneGUI () {
//    //    Debug.Log("asdasd");
//    //    //target.lookAtPoint = Handles.PositionHandle (target.lookAtPoint, Quaternion.identity);
//    //    //if (GUI.changed)
//    //    //    EditorUtility.SetDirty (target);
//    //}
//}