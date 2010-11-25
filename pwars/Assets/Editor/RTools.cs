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
    public Dictionary<SerializedObject, List<SerializedProperty>> prlist = new Dictionary<SerializedObject, List<SerializedProperty>>();
    string search = "";
    string file;
    GameObject Find(string path)
    {
        foreach (GameObject g in FindObjectsOfTypeIncludingAssets(typeof(GameObject)))
            if (GetPath(g.transform) == path)
            {
                return g;
            }
        return null;
    }
    string GetPath(Transform t)
    {
        string path = "/" + t.name;
        while (t.parent != null && t.parent != t)
        {            
            path += t.parent.name;
            t = t.parent;
        }
        return path;
    }

    void OnGUI()
    {
        DrawObjects();
        DrawSearch();        
        pw();
    }
    private void pw()
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
            EditorApplication.isPlaying = true;
        }
        if (GUILayout.Button("Client Editor"))
        {
            _Loader.mapSettings.host = false;
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
    private void DrawSearch()
    {
        string oldsr = search;
        search = EditorGUILayout.TextField(search);

        if (oldsr != search && search.Length > 1)
        {
            prlist.Clear();
            foreach (var m in Selection.activeGameObject.GetComponents<Component>())
            {
                SerializedObject so = new SerializedObject(m);
                SerializedProperty pr = so.GetIterator();
                pr.NextVisible(true);
                prlist.Add(so, new List<SerializedProperty>());
                do
                    if (pr.propertyPath.ToLower().Contains(search.ToLower()))
                        prlist[so].Add(pr.Copy());
                while (pr.NextVisible(true));
            }
        }
        if (search.Length <= 1)
            prlist.Clear();

        foreach (var sps in prlist)
            foreach (var sp in sps.Value)
            {
                if (sp != null)
                    EditorGUILayout.PropertyField(sp);
            }

    }
    
    private void DrawObjects()
    {
        if (GUI.Button("Add"))
        {
            string path = GetPath(Selection.activeGameObject.transform);
            if (!instances.Contains(path))
            {                
                instances.Add(path);
            }
        }
        List<string> toremove = new List<string>();
        foreach (var inst in instances)
        {
            GUI.BeginHorizontal();
            UnityEngine.Object o= GameObject.Find(inst);
            if (o != null && GUI.Button(o.name))
            {
                Selection.activeGameObject = GameObject.Find(inst);
            }
            if (GUI.Button("X", GUI.ExpandWidth(false)))
                toremove.Add(inst);                            
            GUI.EndHorizontal();
        }
        foreach(var inst in toremove)
            instances.Remove(inst);

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
    //public List<int> _instances;
    public List<string> instances = new List<string>();
    //{
    //    get
    //    {
    //        if (_instances == null)
    //            _instances = PlayerPrefs.GetString("objlist", "").Split(' ').Select(a => int.Parse(a)).ToList();
    //        return _instances;
    //    }
    //    set { _instances = value; }
    //}
}