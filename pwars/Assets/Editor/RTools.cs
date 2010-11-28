using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using System.IO;
using System.Collections;

[ExecuteInEditMode]
public partial class RTools : EditorWindow
{
    static float t1;
    string file;
    void OnGUI()
    {
        if (GUI.Button("Init"))
            Init();
        BuildGUI();

    }

    private void Init()
    {

        string cspath = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\scripts\GUI\";
        clearObjects("None");
        clearObjects("zombie");
        foreach (var go in Selection.gameObjects)
            foreach (var g in go.GetComponents<Base>())
            {
                foreach (var f in g.GetType().GetFields())
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

            }

        
        foreach (var g in GameObject.FindGameObjectsWithTag("door"))
        {

            if (g.GetComponent<Door>() == null)
            {                
                g.animation.playAutomatically = false;
                Door d = g.AddComponent<Door>();
                d.Parse();
                NetworkView nw = g.AddComponent<NetworkView>();
                nw.observed = null;
                g.AddComponent<AudioSource>();
            }
        }
        foreach (var g in GameObject.FindGameObjectsWithTag("ammo"))
        {
            if (g.GetComponent<Ammo>() == null)
            {
                Ammo d = g.AddComponent<Ammo>();
                d.Parse();                
                NetworkView nw = g.AddComponent<NetworkView>();
                nw.observed = null;
                g.AddComponent<AudioSource>();
            }
        }
    }
    private void BuildGUI()
    {
        if (!EditorApplication.currentScene.Contains("Game.unity")) return;

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
    void Update()
    {
        if ((t1 -= 1) < 0)
        {
            t1 = 50;
            ewnd.Repaint();
        }
    }
    private void Build()
    {
        file = "Builds/" + DateTime.Now.ToFileTime() + "/";
        Directory.CreateDirectory(file);
        BuildPipeline.BuildPlayer(new[] { "Assets/scenes/Game.unity" }, (file = file + "Game.Exe"), BuildTarget.StandaloneWindows, BuildOptions.Development);
    }
    [MenuItem("RTools/RTools")]
    static void rtoolsclick()
    {
        if (_ewnd == null) _ewnd = EditorWindow.GetWindow<RTools>();
    }
    private static Loader _Loader
    {
        get
        {

            GameObject g = (GameObject)AssetDatabase.LoadAssetAtPath(@"Assets/Resources/Prefabs/loader.prefab", typeof(GameObject));

            Loader l = g.GetComponent<Loader>();
            return l;
        }
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
    private void clearObjects(string name)
    {

        foreach (var spwn in GameObject.FindGameObjectsWithTag(name))
            foreach (var a in spwn.GetComponents<Component>())
                if (!(a is Transform))
                    DestroyImmediate(a);
        
    }
}  