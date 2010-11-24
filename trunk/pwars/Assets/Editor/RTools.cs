using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using GUI = UnityEngine.GUILayout;
using System.IO;
[ExecuteInEditMode]
public class RTools : EditorWindow
{

    [MenuItem("RTools/RTools")]
    static void rtoolsclick()
    {
        if (_ewnd == null) _ewnd = EditorWindow.GetWindow<RTools>();
    }

    static float t1;
    void OnGUI()
    {

        if (!EditorApplication.currentScene.Contains("Game.unity")) return;
        if (GUI.Button("Loader"))
            Selection.activeObject = FindObjectsOfTypeIncludingAssets(typeof(Loader))[0];
        if (GUI.Button("Player"))
            Selection.activeObject = FindObjectsOfTypeIncludingAssets(typeof(Player))[0];
        if (GUI.Button("Zombie"))
            Selection.activeObject = FindObjectsOfTypeIncludingAssets(typeof(Zombie))[0];
        if (GUI.Button("Game"))
            Selection.activeObject = FindObjectOfType(typeof(Game));

        if (Application.isPlaying && Application.loadedLevelName.Contains("Game"))
        {
            foreach (Player p in Base2._Game.players)
                if (p != null)
                    if (GUI.Button(p.name+":"+p.OwnerID))
                        Selection.activeObject = p;            

        }


        GUI.BeginHorizontal();
        foreach (string n in Enum.GetNames(typeof(GameMode)))
            if (GUI.Button(n))
            {
                _loader.mapSettings.gameMode = (GameMode)Enum.Parse(typeof(GameMode), n);
            }
        GUI.EndHorizontal();
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
            _loader.mapSettings.host = true;
            EditorApplication.isPlaying = true;
        }
        if (GUILayout.Button("Client Editor"))
        {
            _loader.mapSettings.host = false;
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

        if (Application.isPlaying)
            InterPrenter();
    }

    

    void InterPrenter()
    {
        //GUILayout.TextArea(cs.outputText, cs.outputText.Length);
        GUILayout.BeginScrollView(new Vector2(0, 2000));
        cs.outputText = GUILayout.TextArea(cs.outputText);
        GUILayout.EndScrollView();
        if (Event.current.keyCode == KeyCode.Return && cs.inputText != "")
        {
            //cs.OnExecuteInput();
            cs.ExecuteCode(cs.inputText);
            cs.inputText = "";

        }
        cs.inputText = GUILayout.TextField(cs.inputText);
    }
    void Update()
    {
        if ((t1 -= 1) < 0)
        {
            t1 = 50;
            ewnd.Repaint();
        }
    }
    string file;
    private void Build()
    {
        file = "Builds/" + DateTime.Now.ToFileTime() + "/";
        Directory.CreateDirectory(file);
        BuildPipeline.BuildPlayer(new[] { "Assets/scenes/Game.unity" }, (file = file + "Game.Exe"), BuildTarget.StandaloneWindows,BuildOptions.Development | BuildOptions.StripDebugSymbols);
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
    static CSharpInterpreter _cs;
    static CSharpInterpreter cs
    {
        get
        {
            if (_cs == null) _cs = (CSharpInterpreter)FindObjectOfType(typeof(CSharpInterpreter));
            return _cs;
        }
    }
    private static Loader _loader
    {
        get
        {

            GameObject g = (GameObject)AssetDatabase.LoadAssetAtPath(@"Assets/Resources/Prefabs/loader.prefab", typeof(GameObject));
            Loader l = g.GetComponent<Loader>();
            return l;
        }
    }



    [MenuItem("RTools/zrig")]
    static void zrig()
    {
        foreach (GameObject g in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
            foreach (Rigidbody r in g.GetComponentsInChildren<Rigidbody>())
                r.mass = 10;
    }

    [MenuItem("RTools/zset")]
    static void zset()
    {
        Undo.RegisterSceneUndo("zset");
        //     foreach (Texture t in FindObjectsOfTypeIncludingAssets(typeof(Texture)))
        {
            //  if (t.name == "flatiron_rendered")
            {

                foreach (GameObject g in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
                {
                    foreach (MeshRenderer r in g.transform.GetComponentsInChildren<MeshRenderer>())
                    {
                        foreach (Material m in r.sharedMaterials)
                        {
                            //m.shader = Shader.Find("Diffuse");
                            //m.shader = Shader.Find("VertexLit");
                            m.shader = Shader.Find("Diffuse");

                            //float f = 0.1f;
                            //m.SetColor("_Emission", new Color(f, f, f));
                            //m.SetTexture("_LightMap", t);

                        }
                    }
                }

            }
        }

    }

    [MenuItem("RTools/Skin Copy")]
    static void Skin()
    {
        UnityEngine.Object[] ts = UnityEditor.Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Unfiltered);
        foreach (UnityEngine.Object t in ts)
        {
            if (t is GUISkin)
            {
                GUISkin skin = (GUISkin)t;
                skin.customStyles[14] = skin.horizontalScrollbarLeftButton;
                skin.customStyles[15] = skin.horizontalScrollbarLeftButton;
                skin.customStyles[16] = skin.horizontalScrollbarRightButton;
                skin.customStyles[17] = skin.horizontalScrollbarRightButton;
                //skin.customStyles[11] = skin.horizontalScrollbarThumb;
            }
        }
    }
    static void print(params object[] p)
    {
        string s = "Editor:";
        foreach (var d in p) s += d;
        MonoBehaviour.print(s);
    }
    [MenuItem("RTools/Duplicate Changes")]
    static void DuplicateChanges()
    {
        Undo.RegisterSceneUndo("zset");
        var ts = UnityEditor.Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Unfiltered).Cast<GameObject>();
        GameObject first = null;
        var vls = new Dictionary<Type, List<object>>();
        Undo.RegisterSnapshot();
        GameObject g = ts.First();
        if (first == null)
            first = g;
        var components = first.GetComponents<Component>();

        foreach (var c in components)
            foreach (var f in props(c))
            {
                if (!vls.ContainsKey(c.GetType())) vls.Add(c.GetType(), new List<object>());
                vls[c.GetType()].Add(f.GetValue(c, null));
            }
        print(vls.Count);
        foreach (GameObject t in ts.Skip(1))
        {
            foreach (var c in t.GetComponents<Component>())
            {
                int i = 0;
                if (vls.ContainsKey(c.GetType()))
                    foreach (var f in props(c))
                    {
                        f.SetValue(c, vls[c.GetType()][i], null);
                        i++;
                    }
            }
        }
    }
    static IEnumerable<PropertyInfo> props(Component c)
    {

        foreach (var member in c.GetType().GetMembers())
            if (member is PropertyInfo)
            {
                var f = ((PropertyInfo)member);
                if (our(f))
                    yield return f;
            }
    }
    static bool our(PropertyInfo f)
    {
        Type o = f.PropertyType;

        return f.CanWrite && f.CanRead &&
            (o == typeof(bool) || o == typeof(string) || o == typeof(int) || o == typeof(float) || o == typeof(Vector2) || o == typeof(Vector3) || o == typeof(double) || o == typeof(Quaternion));
    }
    [MenuItem("RTools/ScaleParticle")]
    static void ScaleParticle()
    {
        UnityEngine.Object[] ts = UnityEditor.Selection.GetFiltered(typeof(GameObject), UnityEditor.SelectionMode.Unfiltered);
        Undo.RegisterSceneUndo("zset");
        float scale = 2f;
        foreach (GameObject t in ts)
        {
            t.transform.localScale *= scale;
            foreach (var p in t.GetComponentsInChildren<ParticleEmitter>())
            {
                p.minSize *= scale;
                p.maxSize *= scale;
                p.localVelocity *= scale;
                p.rndVelocity *= scale;
                p.worldVelocity *= scale;
            }
            foreach (var p in t.GetComponentsInChildren<ParticleAnimator>())
            {
                p.force *= scale;
                p.rndForce *= scale;
                p.sizeGrow *= scale;
                p.localRotationAxis *= scale;
            }
            foreach (var p in t.GetComponentsInChildren<ParticleRenderer>())
            {
                p.velocityScale *= scale;
                p.maxParticleSize *= scale;
                p.lengthScale *= scale;
            }
        }
    }





}