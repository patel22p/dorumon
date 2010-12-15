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
[ExecuteInEditMode]
public partial class RTools : InspectorSearch
{
    string file;
    string cspath = @"C:\Users\igolevoc\Documents\PhysxWars\Assets\scripts\GUI\";
    public GameObject selectedGameObject;
    public bool bake;
    protected override void Awake()
    {
        base.Awake();
        
        
    }
    protected override void OnGUI()
    {
        
        GUI.BeginHorizontal();
        bake = GUI.Toggle(bake, "Bake");
        if (GUI.Button("SetupLevel"))
        {
            
            DestroyImmediate(GameObject.Find("level"));
            string path = EditorApplication.currentScene.Split('.')[0] + "/";
            path = path.Substring("Assets/".Length);
            Debug.Log(path);
            Selection.activeObject = Editor.Instantiate(GetAssets<GameObject>(path, "*.FBX").FirstOrDefault());
            Selection.activeObject.name = "level";
            SetupLevel();
            Inits(cspath);
            if (bake)
            {
                var old = RenderSettings.ambientLight;
                RenderSettings.ambientLight = Color.white * .3f;
                Lightmapping.BakeAsync();                
                RenderSettings.ambientLight = old;
            }
        }
        

        if (GUI.Button("Init"))
        {
            Undo.RegisterSceneUndo("SceneInit");
            if (Selection.activeGameObject != null)
                Inits(cspath);            
        }
            
        GUI.EndHorizontal();
        base.OnGUI();
        BuildGUI();
    }
    private void Inits(string cspath)
    {
        _TimerA.AddMethod(delegate()
        {
            foreach (var go in Selection.gameObjects)
            {
                foreach (var scr in go.GetComponentsInChildren<Base2>())
                {
                    scr.Init();
                    foreach (var pf in scr.GetType().GetFields())
                    {
                        InitLoadPath(scr, pf);
                        CreateEnum(cspath, scr, pf);
                        PathFind(scr, pf);
                    }
                    if (scr.networkView != null && scr.networkView.observed == null)
                        scr.networkView.stateSynchronization = NetworkStateSynchronization.Off;
                }
            }
        });

        _TimerA.AddMethod(delegate()
        {
            foreach (var au in Selection.activeGameObject.GetComponentsInChildren<AudioSource>())
                au.minDistance = 10;
        });
        
    }
    private void SetupLevel()
    {
        List<GameObject> destroy = new List<GameObject>();
        
        foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.animation == null || t.gameObject.animation.clip == null)
                DestroyImmediate(t.gameObject.animation);
            t.gameObject.isStatic = true;
        }

        foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
            t.gameObject.layer = LayerMask.NameToLayer("Level");
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            GameObject g = t.gameObject;
            string[] param = g.name.Split(',');
            if (param[0] == ("fragmentation"))
            {
                foreach (Transform cur in t)
                {
                    if (!cur.name.Contains("_"))
                    {
                        if (cur.GetComponent<Fragment>() == null)
                            AddFragment(cur, t, true);
                    }
                }
            }
            if (t.name.Contains("glass") || t.name.Contains("dontcast"))
            {
                foreach (var t2 in t.GetComponentsInChildren<Transform>())
                {
                    if (t2.GetComponent<Renderer>() != null)
                        t2.renderer.castShadows = false;
                    if (t.name.Contains("glass")) t2.name += ",glass";
                }
            }
            if (param[0] == ("coll"))
            {
                g.AddOrGet<Box>().Init();
            }
            var items = GetAssets<GameObject>("/Items/", "*.Prefab");
            foreach (var itemPrefab in items)
            {
                if (param[0].ToLower() == itemPrefab.name.ToLower() && g.GetComponent<MonoBehaviour>() == null)
                {
                    GameObject item = ((GameObject)Instantiate(itemPrefab));
                    if (ParseRotation(g.name) != Vector3.zero)
                        item.transform.rotation = Quaternion.LookRotation(ParseRotation(g.name));
                    item.transform.position = t.position;
                    item.transform.parent = t.parent;
                    t.parent = item.transform;
                    item.name = g.name;
                    if (!item.name.StartsWith("lamp"))
                        destroy.Add(t.gameObject);

                }
            }
            if (g.name == "path")
            {
                Debug.Log("founded path");
                destroy.Add(g);
            }

            foreach (string s in Enum.GetNames(typeof(MapItemType)))
            {
                if (param[0].ToLower() == "i" + s.ToLower() && g.GetComponent<MapItem>() == null)
                {
                    g.AddComponent<MapItem>().Init();
                }
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
    }
    private void AddFragment(Transform cur, Transform root, bool first)
    {
        GameObject g = cur.gameObject;
        g.isStatic = true;
        Fragment f = g.AddComponent<Fragment>();
        f.first = first;
        ((MeshCollider)cur.collider).convex = true;
        if (!first)
        {
            g.layer = LayerMask.NameToLayer("HitLevelOnly");
            g.active = false;
        }
        int i = 1;
        for (; ; i++)
        {
            string nwpath = cur.name + "_frag_" + string.Format("{0:D2}", i);
            Transform nw = root.Find(nwpath);            
            if (nw == null) break;
            f.child.Add(nw);
            nw.parent = cur;
            AddFragment(nw, root, false);
        }
    }
    private static void PathFind(Base2 scr, FieldInfo pf)
    {
        PathFind atr = (PathFind)pf.GetCustomAttributes(true).FirstOrDefault(a => a is PathFind);
        if (atr != null)
        {
            GameObject g = atr.scene ? GameObject.Find(atr.name) : scr.transform.Find(atr.name).gameObject;
            if (g == null) Debug.Log("cound not find path " + atr.name);
            else if (pf.FieldType == typeof(GameObject))
                pf.SetValue(scr, g);
            else
                pf.SetValue(scr, g.GetComponent(pf.FieldType));
        }
    }
    private static void SetupTextures()
    {
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
    }
    private static void InitLoadPath(Base2 scr, FieldInfo pf)
    {

        LoadPath ap = (LoadPath)pf.GetCustomAttributes(true).FirstOrDefault(a => a is LoadPath);
        if (ap != null)
        {
            object value = pf.GetValue(scr);
            if (value is Array)
            {
                object o = FindObjectsOfTypeIncludingAssets(pf.FieldType).Where(a => AssetDatabase.GetAssetPath(a).Contains(ap.name)).Cast<AudioClip>().ToArray();                
                pf.SetValue(scr, o);
            }
            else
            if ((value == null || value.Equals(null)))
            {
                object o = FindObjectsOfTypeIncludingAssets(pf.FieldType).FirstOrDefault(a => a.name == ap.name);
                pf.SetValue(scr, o);
            }
        }
    }
    private void BuildGUI()
    {
        CopyComponent();

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
        if (_Loader != null)
        {
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
            foreach (object o in ie)
                cs += o+ ",";
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
    protected override void Update()
    {        
        
        base.Update();
    }
    private static Loader _Loader
    {
        get
        {
            Loader l = (Loader)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Loader)).FirstOrDefault();
            return l;
        }
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
    public static Vector3 ParseRotation(string name)
    {
        Match m;
        if ((m = Regex.Match(name, ",(-?(?:y|x|z))(?:$|,)")).Success)
        {
            string s = m.Groups[1].Value;
            Vector3 v = new Vector3();
            switch (s)
            {
                case "x":
                    v = (new Vector3(-1, 0, 0));
                    break;
                case "-x":
                    v = (new Vector3(1, 0, 0));
                    break;
                case "-z":
                    v = (new Vector3(0, -1, 0));
                    break;
                case "z":
                    v = (new Vector3(0, 1, 0));
                    break;
                case "y":
                    v = (new Vector3(0, 0, -1));
                    break;
                case "-y":
                    v = (new Vector3(0, 0, 1));
                    break;
            };
            return v;
        }
        return new Vector3();
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
    private void CopyComponent()
    {
        if (GUI.Button("Select"))
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
}

