
#if UNITY_EDITOR && UNITY_STANDALONE_WIN

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
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;
[assembly: AssemblyVersion("1.0.*")]
public partial class RTools : InspectorSearch
{
    string file { get { return EditorPrefs.GetString("bf"); } set { EditorPrefs.SetString("bf", value); } } 
    string cspath = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\scripts\GUI\";
    public bool bake;
    public override void Awake()
    {
        base.Awake();        
    }    
    protected override void OnGUI()
    {
        if (GUILayout.Button("Build"))
        {
            Build();
            return;
        }
        BuildButtons();        
        GUI.BeginHorizontal();
        bake = GUI.Toggle(bake, "Bake"); 
        if (GUI.Button("SetupLevel"))
        {
            
            var l = GameObject.Find("level");
            var p = l.transform.parent;
            DestroyImmediate(l);
            string path = EditorApplication.currentScene.Split('.')[0] + "/";
            path = path.Substring("Assets/".Length);
            Debug.Log("setup level: " + path);
            l = (GameObject)Editor.Instantiate(GetAssets<GameObject>(path, "*.FBX").FirstOrDefault());
            l.transform.parent = p;            
            l.name = "level";
            Selection.activeGameObject = l;
            SetupLevel();
            Inits(cspath);
            _TimerA.AddMethod(delegate
            {
                if (bake)
                {
                    var old = RenderSettings.ambientLight;
                    RenderSettings.ambientLight = Color.white * .2f;
                    var en = new Queue<LightShadows>();
                    
                    foreach (Light a in GameObject.FindObjectsOfType(typeof(Light)))
                    {
                        en.Enqueue(a.shadows);
                        a.shadows = LightShadows.Soft;
                    }
                    Lightmapping.BakeAsync();
                    foreach (Light a in GameObject.FindObjectsOfType(typeof(Light)))
                        a.shadows = en.Dequeue();
                    RenderSettings.ambientLight = old;

                }
            });
        }


        if (GUI.Button("Init"))
        {
            Undo.RegisterSceneUndo("SceneInit");
            SetupMaterials();
            

            if (Selection.activeGameObject != null)
                Inits(cspath);
        }

        GUI.EndHorizontal();

        BuildGUI();
        base.OnGUI();
    }
    private void BuildButtons()
    {

        GUI.BeginHorizontal();
        if (GUILayout.Button("Server Editor"))
        {
            _Loader.mapSettings.host = true;
            EditorUtility.SetDirty(_Loader);
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
            EditorUtility.SetDirty(_Loader);
            EditorApplication.isPlaying = true;
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
            var c =new GameObject("cam", typeof(Camera)).GetComponent<Camera>();
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
                if (scr.networkView != null && scr.networkView.observed == null)
                    scr.networkView.stateSynchronization = NetworkStateSynchronization.Off;
                try
                {
                    scr.Init();
                }
                catch (Exception e) { Debug.LogError(e); }
            }
        }

        _TimerA.AddMethod(delegate()
        {
            foreach (var au in Selection.activeGameObject.GetComponentsInChildren<AudioSource>())
                au.minDistance = 10;
        });
                 
    }
    protected override void SetupLevel()
    {
        List<GameObject> destroy = new List<GameObject>();
        
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            GameObject g = t.gameObject;
            var p = Selection.activeGameObject.transform.parent.Find(t.name);
            if (p != null)
            {
                Debug.Log("found parent " + g.name);
                //p.transform.position = g.transform.position;
                destroy.Add(g);
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
                var c =  f.gameObject.GetComponent<Collider>();
                if(c!=null)
                    DestroyImmediate(c);
            }            
        }
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            GameObject g = t.gameObject;
            string[] param = g.name.Split(',');
        
            if (param[0] == ("coll"))
            {
                foreach (var a in g.GetComponentsInChildren<Collider>())
                    a.gameObject.AddOrGet<Box>();
            }

            if (param[0].ToLower() == "zombiespawn")
            {
                g.renderer.enabled = false;
                g.collider.isTrigger = true;
                g.tag = "SpawnZombie";
            }
            if (g.name == "path")
            {
                g.active = false;
                DestroyImmediate(g.GetComponent<MeshCollider>());                
                Debug.Log("founded path");
                destroy.Add(g);
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
                GameObject g = atr.scene ? GameObject.Find(atr.name).gameObject : scr.transform.GetTransforms().FirstOrDefault(a=>a.name == name).gameObject;
                if (pf.FieldType == typeof(GameObject))
                    pf.SetValue(scr, g);
                else
                    pf.SetValue(scr, g.GetComponent(pf.FieldType));
            }
            catch { Debug.Log("cound not find path " + scr.name + "+" + name); }
        }
    }
    private static void InitLoadPath(Base2 scr, FieldInfo pf)
    {
        FindAsset ap = (FindAsset)pf.GetCustomAttributes(true).FirstOrDefault(a => a is FindAsset);        
        if (ap != null)
        {
            string name = (ap.name == null) ? pf.Name : ap.name;
            object value = pf.GetValue(scr);
            if (value is Array)
            {
                var o = Base2.GetFiles().Where(a => a.Contains(name))
                                    .Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, pf.FieldType))
                                    .Where(a => a != null).Cast<AudioClip>().ToArray();
                if (o.Length == 0)
                    Debug.Log("could not find audioSources " + name);

                pf.SetValue(scr, o);
            }
            else
                if ((value == null || value.Equals(null)) || ap.overide)
                {                                        
                    pf.SetValue(scr, Base2.FindAsset(name, pf.FieldType));
                }
        }
    }
    private void BuildGUI()
    {
        GUI.Space(10);
        if (Application.isPlaying && Base2._Game!=null)
        {
            foreach (Player p in Base2._Game.players)
                if (p != null)
                    if (GUI.Button(p.name + ":" + p.OwnerID))
                        Selection.activeObject = p;
        }        
       
        //if (_Loader != null)
        //{
        //    _Loader.build = GUI.Toggle(_Loader.build, "build");
        //    _Loader.disablePathFinding = GUI.Toggle(_Loader.disablePathFinding, "disable path finding");
        //    _Loader.dontcheckwin = GUI.Toggle(_Loader.dontcheckwin, "dont check win");
        //}
    }
    private static void CreateEnum(string cspath, Base2 g, FieldInfo f)
    {
        
        GenerateEnums ge = (GenerateEnums)f.GetCustomAttributes(true).FirstOrDefault(a => a is GenerateEnums);
        if (ge != null)
        {
            string cs = "";
            Debug.Log("Found!" + ge.name);
            cs += "public enum " + ge.name + ":int{none = -1,";
            var ie = (IEnumerable)f.GetValue(g);
            foreach (object o in ie)
            {
                if (o != null)
                    cs += o + ",";
            }
            cs = cs.Trim(new[] { ',' });
            cs += "}";
            Debug.Log("geneerated:" + cs);
            File.WriteAllText(cspath + ge.name + ".cs", cs);
        }
    }
    
    private void Build()
    {
        PlayerSettings.productName = "Physics Wars Build " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        file = "Builds/" + DateTime.Now.ToFileTime() + "/";
        Directory.CreateDirectory(file);
        BuildPipeline.BuildPlayer(new[] { EditorApplication.currentScene }, (file = file + "Game.Exe"), BuildTarget.StandaloneWindows, BuildOptions.Development);
    }
    protected override void Update()
    {        
        
        base.Update();
    }
    Loader loader ;
    Loader _Loader
    {
        get
        {
            if (loader == null)
                loader = Base2.FindAsset<Loader>("loader"); //(Loader)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Loader)).First();
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
}

#endif