using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using System.IO;
[ExecuteInEditMode]
public class RTools : EditorWindow
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
        foreach (var g in GameObject.FindGameObjectsWithTag("door"))
        {
            if (g.GetComponent<Door>() == null)
            {
                Door d = g.AddComponent<Door>();
                try
                {
                    d.score = int.Parse(d.name.Split(',')[1]);
                }
                catch { }
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
        if (GUILayout.Button("Client Editor"))
        {
            _Loader.mapSettings.host = false;
            new SerializedObject(_Loader).ApplyModifiedProperties();
            EditorApplication.isPlaying = true;
        }
        GUI.EndHorizontal();
        GUI.BeginHorizontal();
        if (GUILayout.Button("Client App"))
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + file, "client");
        if (GUILayout.Button("Server App"))
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + file, "server");
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
    private static Loader _Loader
    {
        get
        {
            
            GameObject g = (GameObject)AssetDatabase.LoadAssetAtPath(@"Assets/Resources/Prefabs/loader.prefab", typeof(GameObject));
            
            Loader l = g.GetComponent<Loader>();
            return l;
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