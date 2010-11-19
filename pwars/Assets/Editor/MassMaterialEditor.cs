using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
class MassMaterialEditor : EditorWindow
{
    [MenuItem("RTools/z1build")]
    static void build()
    {
        BuildPipeline.BuildPlayer(new string[] { "Assets/z4game.unity" }, "z4game.unity3d", BuildTarget.WebPlayer, BuildOptions.BuildAdditionalStreamedScenes);
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
        GameObject first=null;
        var vls = new Dictionary<Type,List<object>>();
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
            foreach(var p in t.GetComponentsInChildren<ParticleEmitter>())
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