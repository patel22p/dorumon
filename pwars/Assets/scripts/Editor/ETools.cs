
#if UNITY_EDITOR && UNITY_STANDALONE_WIN

using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using gui = UnityEditor.EditorGUILayout;
using System.IO;
using System.Collections;
using AstarClasses;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;
[assembly: AssemblyVersion("1.0.*")]
public partial class ETools : InspectorSearch
{
    string[] scenes
    {
        get
        {
            return new string[] { 
            p + "/Menu.unity",
            p + "/test.unity",
            p + "/Pitt.unity",
        };
        }
    }
    string cspath = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\scripts\GUI\";
    public bool bake;
    public bool web, buildall;
    public override void Awake()
    {
        base.Awake();
    }
    public bool stopZombies, disableSounds, disablePathFinding,client, debug;
    protected override void OnGUI()
    {
        GUI.BeginHorizontal();
        web = GUI.Toggle(web, "web", GUI.ExpandWidth(false));
        var old = buildall;
        buildall = GUI.Toggle(buildall, "all", GUI.ExpandWidth(false));
        if (buildall && buildall != old)
            disablePathFinding = debug = disableSounds = stopZombies = false;

        if (GUILayout.Button("Build"))
        {
            EditorUtility.SetDirty(_Loader);
            Build();
            return;
        }
        if (GUI.Button("Refresh"))
        {
            if (!isPlaying)
            {
                var c = EditorApplication.currentScene;
                EditorApplication.SaveScene(c);
                for (int i = 0; i < 4; i++)
                {
                    EditorApplication.OpenScene("Assets/scenes/Menu.unity");
                    EditorApplication.OpenScene("Assets/scenes/Pitt.unity");
                }
                EditorApplication.OpenScene(c);
                return;
            }
        }
        GUI.EndHorizontal();
        BuildButtons();

        
        GUI.BeginHorizontal();
        if (GUI.Button("loader"))
            EditorUtility.SetDirty(loader);
        debug = GUI.Toggle(debug, "Debug");
        _Loader.build = !debug;
        disablePathFinding = GUI.Toggle(disablePathFinding, "DPath");
        _Loader.disablePathFinding = disablePathFinding;
        disableSounds = GUI.Toggle(disableSounds, "Dsounds");
        _Loader.disableSounds = disableSounds;
        stopZombies = GUI.Toggle(stopZombies, "DZombies");
        _Loader.stopZombies = stopZombies;
        client = GUI.Toggle(client, "client");
        _Loader.host = !client;

        GUI.EndHorizontal();
        GUI.BeginHorizontal();
        bake = GUI.Toggle(bake, "Bake");

        lfactor = EditorGUILayout.FloatField(lfactor, GUI.Width(20));
        dfactor = EditorGUILayout.FloatField(dfactor, GUI.Width(20));

        if (GUI.Button("SetupLevel"))
        {
            LevelSetup();
        }
        if (GUI.Button("InitV"))
            foreach (GameObject a in GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)))
            {

                if (AssetDatabase.IsMainAsset(a))
                {
                    bool mod = false;
                    var ar = a.transform.GetTransforms().ToArray();
                    foreach (var b in ar)
                    {
                        var bs = b.GetComponent<bs>();
                        if (bs != null)
                        {
                            bs.InitValues();
                            mod = true;
                        }
                    }
                    if (mod)
                        EditorUtility.SetDirty(a);
                }
                else if (EditorApplication.isPlaying)
                    a.SendMessage("InitValues", SendMessageOptions.DontRequireReceiver);
            }

        if (GUI.Button("Init"))
        {
            Undo.RegisterSceneUndo("SceneInit");
            SetupMaterials();
            if (Selection.activeGameObject != null)
                Inits(cspath);
        }
        GUI.EndHorizontal();
        //BuildGUI();
        //GUI.Space(10);
        //if (Application.isPlaying && Base2._Game != null)
        //{
        //    foreach (Player p in Base2._Game.players)
        //        if (p != null)
        //            if (GUI.Button(p.name + ":" + p.OwnerID))
        //                Selection.activeObject = p;
        //}
        base.OnGUI();
    }
    private void LevelSetup()
    {
        var Level = GameObject.Find("Level");
        var oldl = Level.transform.Find("level");
        if (oldl != null)
            DestroyImmediate(oldl.gameObject);
        string path = EditorApplication.currentScene.Split('.')[0] + "/";
        path = path.Substring("Assets/".Length);
        Debug.Log("setup level: " + path);
        var nl = (GameObject)EditorUtility.InstantiatePrefab(GetAssets<GameObject>(path, "*.FBX").FirstOrDefault());
        nl.transform.parent = Level.transform;
        nl.name = "level";
        Selection.activeGameObject = nl;
        SetupLevel();
        Inits(cspath);
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
    protected override void Update()
    {
        _Loader.SerializedObject.ApplyModifiedProperties();
        base.Update();
    }
    private void BuildButtons()
    {
        GUI.BeginHorizontal();
        if (GUILayout.Button("Client App"))
        {
            ResetCam();
            _TimerA.AddMethod(delegate
            {
                System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + path, "client");
            });
        }
        if (GUILayout.Button("Server App"))
        {
            ResetCam();
            _TimerA.AddMethod(delegate
            {
                System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + path, "server");
            });
        }
        GUI.EndHorizontal();        
    }
    static Color NormalizeColor(Color c, float procent, float a)
    {
        //var n = 1f / Math.Max(Math.Max(c.r, c.b), c.g);
        var c2 = c;//new Color(c.r * n, c.b * n, c.g * n) * procent;
        c2.a = a;
        return c2;
    }
    private static void SetupMaterials()
    {
        if (Selection.activeObject is Cubemap)
        {
            Cubemap cb = (Cubemap)Selection.activeObject;
            var pos = SceneView.lastActiveSceneView.camera.transform.position;
            var c = new GameObject("cam", typeof(Camera)).GetComponent<Camera>();
            c.transform.position = pos;
            c.RenderToCubemap(cb);
            DestroyImmediate(c);
            Debug.Log("rendered to cubemap");
        }
        foreach (var o in Selection.objects)
        {
            if (o is Texture2D)
            {
                TextureImporter ti = ((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)));
                int max = 256;
                if (!ti.lightmap)
                {
                    TextureImporterSettings tis = new TextureImporterSettings();
                    ti.ReadTextureSettings(tis);
                    tis.maxTextureSize = max;
                    ti.SetTextureSettings(tis);

                    AssetDatabase.ImportAsset(ti.assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
        foreach (Material m in Selection.objects.Where(a => a is Material))
        {
            Debug.Log("Material " + m.name);
            var n = m.shader.name;
            bool isSpec = (n == "Specular" || n == "Parallax Specular");
            if (n == "Diffuse" || isSpec)
            {
                m.color = NormalizeColor(m.color, .8f, .2f);
                if (isSpec)
                    m.SetColor("_SpecColor", NormalizeColor(m.color, .3f, .2f));
            }
        }
    }
    private void Inits(string cspath)
    {
        foreach (var go in Selection.gameObjects)
        {
            foreach (Animation anim in go.GetComponentsInChildren<Animation>().Cast<Animation>().ToArray())
                if (anim.clip == null) DestroyImmediate(anim);
            foreach (var scr in go.GetComponentsInChildren<Base2>())
            {
                foreach (var pf in scr.GetType().GetFields())
                {
                    InitLoadPath(scr, pf);
                    CreateEnum(cspath, scr, pf);
                    PathFind(scr, pf);
                }
                //try
                {
                    scr.Init();
                }
                //catch (Exception e) { Debug.LogError(e); }
            }
        }

        _TimerA.AddMethod(delegate()
        {
            foreach (var au in Selection.activeGameObject.GetComponentsInChildren<AudioSource>())
                au.minDistance = 10;
        });

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
    protected override void SetupLevel()
    {
        List<GameObject> destroy = new List<GameObject>();
        foreach (var d in GetTransforms2(new DTR { transform = Selection.activeGameObject.transform }))
        {
            var g = d.transform.gameObject;
            var path = d.path.TrimStart(new char[] { '/' });
            var p = Selection.activeGameObject.transform.parent.Find(path);
            if (p != null)
            {
                p.transform.position = g.transform.position;
                p.transform.rotation = g.transform.rotation;
                Clear(g, true);
            }
        }
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            if (t.name.ToLower() == "path")
            {
                var a = (AstarPath)GameObject.FindObjectOfType(typeof(AstarPath));
                a.navmesh = t.GetComponent<MeshFilter>().sharedMesh;
                a.navmeshRotation = (Quaternion.AngleAxis(270, new Vector3(1, 0, 0))).eulerAngles;
                a.meshGrid.offset = t.position;
                a.meshGrid.offset.y += .2f;
                a.boundsMargin = 1;
                a.meshGrid.scale = 1;
                t.renderer.enabled = false;
                if (t.collider != null)
                    DestroyImmediate(t.collider);
            }
        }
        foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = LayerMask.NameToLayer("Level");
            t.gameObject.isStatic = true;
            if (t.gameObject.animation != null && t.gameObject.animation.clip == null)
                DestroyImmediate(t.gameObject.animation);
        }

        foreach (Animation t in Selection.activeGameObject.GetComponentsInChildren<Animation>())
        {
            foreach (var f in t.transform.GetComponentsInChildren<Transform>())
            {
                f.gameObject.isStatic = false;
                f.gameObject.layer = LayerMask.NameToLayer("Default");
                var c = f.gameObject.GetComponent<Collider>();
                if (c != null)
                    DestroyImmediate(c);
            }
        }

        foreach (var a in destroy)
            DestroyImmediate(a);
        _TimerA.AddMethod(delegate
        {
            foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
                if (t.name.StartsWith("hide"))
                {
                    DestroyImmediate(t.gameObject);
                }
        });
        base.SetupLevel();
    }
    private static void PathFind(Base2 scr, FieldInfo pf)
    {
        FindTransform atr = (FindTransform)pf.GetCustomAttributes(true).FirstOrDefault(a => a is FindTransform);
        if (atr != null)
        {
            string name = (atr.name == null) ? pf.Name : atr.name;
            try
            {
                GameObject g = atr.scene ? GameObject.Find(name).gameObject : scr.transform.GetTransforms().FirstOrDefault(a => a.name == name).gameObject;
                if (g == null) throw new Exception();
                if (pf.FieldType == typeof(GameObject))
                    pf.SetValue(scr, g);
                else
                    pf.SetValue(scr, g.GetComponent(pf.FieldType));
            }
            catch { Debug.Log("cound not find path " + scr.name + "+" + name); }
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
    private static void InitLoadPath(Base2 scr, FieldInfo pf)
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
                    var q = Base2.GetFiles().Where(a => a.Contains(name)).Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, type)).Where(a => a != null);
                    if (q.Count() == 0)
                        Debug.Log("could not find folder " + name);

                    pf.SetValue(scr, Cast(q, type));
                }
                else
                {
                    Debug.Log("FindAsset " + name);
                    pf.SetValue(scr, Base2.FindAsset(name, pf.FieldType));
                }
            }
        }
    }
    private static void CreateEnum(string cspath, Base2 g, FieldInfo f)
    {
        GenerateEnums ge = (GenerateEnums)f.GetCustomAttributes(true).FirstOrDefault(a => a is GenerateEnums);
        if (ge != null)
        {
            string cs = "";
            var fpa = cspath + ge.name + ".cs";
            if (!File.Exists(fpa) || ge.overide)
            {
                cs += "public enum " + ge.name + ":int{none = -1,";
                var ie = (IEnumerable)f.GetValue(g);
                foreach (Object o in ie)
                {
                    if (o != null)
                        cs += o.name + ",";
                }
                cs = cs.Trim(new[] { ',' });
                cs += "}";
                Debug.Log("geneerated:" + cs);
                File.WriteAllText(fpa, cs);
            }
        }
    }
    private void Build()
    {
        Debug.Log("build");
        var fn = "Game.Exe";
        PlayerSettings.productName = "Physics Wars Build " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        var dt = DateTime.Now.ToFileTime();
        //path = "Builds/";
        path = "Builds/" + dt + "/";
        Directory.CreateDirectory(path);
        if (web)
        {
            File.WriteAllText(path + "WebClient.bat", "start file://contentmine-14/builds/" + dt + "/Game.Exe/Game.unity3d#client");
            File.WriteAllText(path + "WebServer.bat", "start file://contentmine-14/builds/" + dt + "/Game.Exe/Game.unity3d#server");
        }
        else
        {
            File.WriteAllText(path + "Client.bat", "start Game.Exe client");
            File.WriteAllText(path + "Server.bat", "start Game.Exe server");
        }

        BuildPipeline.BuildPlayer(buildall ? scenes : new[] { EditorApplication.currentScene }, path + fn, web ? BuildTarget.WebPlayer : BuildTarget.StandaloneWindows, BuildOptions.Development | BuildOptions.WebPlayerOfflineDeployment);
        if (web) BuildPipeline.BuildPlayer(new[] { "" }, "", BuildTarget.StandaloneWindows, BuildOptions.Development);
    }
    public static Loader loader;
    public static Loader _Loader
    {
        get
        {
            if (loader == null)
                loader = ((Loader)GameObject.FindObjectOfType(typeof(Loader))) ?? Base2.FindAsset<Loader>("loader"); //(Loader)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Loader)).First();
            return loader;
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
    static string p { get { return Path.GetDirectoryName(EditorApplication.currentScene); } }
    string path { get { return EditorPrefs.GetString("bf"); } set { EditorPrefs.SetString("bf", value); } }
    float lfactor { get { return EditorPrefs.GetFloat("lightmap" + EditorApplication.currentScene, .2f); } set { EditorPrefs.SetFloat("lightmap" + EditorApplication.currentScene, value); } }
    float dfactor { get { return EditorPrefs.GetFloat("lightmapDT" + EditorApplication.currentScene, .1f); } set { EditorPrefs.SetFloat("lightmapDT" + EditorApplication.currentScene, value); } }
}
#endif