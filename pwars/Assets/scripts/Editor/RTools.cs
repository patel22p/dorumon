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
    protected override void OnGUI()
    {
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
        }
        if (GUI.Button("Init"))
        {
            Undo.RegisterSceneUndo("SceneInit");
            if (Selection.activeGameObject != null)
                Inits(cspath);            
        }
        base.OnGUI();
        BuildGUI();
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
    IEnumerable<T> GetAssets<T>(string path ,string pattern) where T : Object
    {
        foreach (string f2 in Directory.GetFiles("Assets/" + path, pattern, SearchOption.AllDirectories))
        {
            string  f = f2.Replace(@"\", "/").Replace("//", "/");
            var a = (T)AssetDatabase.LoadAssetAtPath(f, typeof(T));            
            if (a != null)
                yield return a;
        }
    }
    
    private void SetupLevel()
    {
        List<GameObject> destroy = new List<GameObject>();
        foreach (Transform t in Selection.activeGameObject.GetComponentInChildren<Transform>())
        {
            if (t.gameObject.animation == null || t.gameObject.animation.clip == null)
                DestroyImmediate(t.gameObject.animation);
            t.gameObject.isStatic = true;
        }
        
        var items = GetAssets<GameObject>("/Items/","*.Prefab");                
        Debug.Log("+items count" + items.Count());
        foreach (Transform t in Selection.activeGameObject.transform)
        {
            GameObject g = t.gameObject;
            if (t.name.StartsWith("fragmentation"))
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
            if (t.name.Contains("glass"))
            {
                foreach (var r in t.GetComponentsInChildren<Renderer>())
                {
                    r.castShadows = false;
                    r.name += ",glass";
                }
            }
            if (t.name.StartsWith("coll"))
            {
                g.AddOrGet<Box>().Init();
            }
            foreach (var itemPrefab in items)
            {
                if (g.name.ToLower().StartsWith(itemPrefab.name.ToLower()) && g.GetComponent<MonoBehaviour>() == null)
                {
                    GameObject item = ((GameObject)Instantiate(itemPrefab));
                    item.transform.position = t.position;
                    try
                    {
                        item.transform.rotation = Quaternion.LookRotation(MapItem.ParseRotation(item.name.Split(',')[1]));
                    }
                    catch (Exception ){  }
                    item.transform.parent = t.parent;
                    t.parent = item.transform;
                    item.name = g.name;
                    if(!item.name.StartsWith("lamp"))
                        destroy.Add(t.gameObject);
                    
                }
            }
            if (t.name.ToLower().StartsWith("zombiespawn"))
            {
                t.tag = "SpawnZombie";
                DestroyImmediate(t.renderer);
                DestroyImmediate(t.collider);
            }
            if (t.name.ToLower().StartsWith("playerspawn"))
            {
                t.tag = "SpawnNone";
                DestroyImmediate(t.renderer);
                DestroyImmediate(t.collider);
            }

            g.layer = LayerMask.NameToLayer("Level");
            if (g.name == "path")
            {
                g.renderer.enabled = false;
                DestroyImmediate(g.collider);
            }
            
            foreach (string s in Enum.GetNames(typeof(MapItemType)))
            {
                if (t.name.StartsWith(s) && g.GetComponent<MapItem>() == null)
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
        PathFind ap = (PathFind)pf.GetCustomAttributes(true).FirstOrDefault(a => a is PathFind);
        if (ap != null)
        {
            Debug.Log(pf.Name);
            if (ap.scene)
            {
                if (pf.FieldType == typeof(GameObject))
                    pf.SetValue(scr, GameObject.Find(ap.name).gameObject);
                else
                    pf.SetValue(scr, GameObject.Find(ap.name).GetComponent(pf.FieldType));
            }
            else
            {
                if (pf.FieldType == typeof(GameObject))
                    pf.SetValue(scr, scr.transform.Find(ap.name).gameObject);
                else
                    pf.SetValue(scr, scr.transform.Find(ap.name).GetComponent(pf.FieldType));
            }
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
}

