using UnityEditor;
using UnityEngine;

class MassMaterialEditor : EditorWindow
{
    [MenuItem("Tools/z1build")]
    static void build()
    {
        BuildPipeline.BuildPlayer(new string[] { "Assets/z4game.unity" }, "z4game.unity3d", BuildTarget.WebPlayer, BuildOptions.BuildAdditionalStreamedScenes);
    }

    [MenuItem("Tools/zrig")]
    static void zrig()
    {
        foreach (GameObject g in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
            foreach (Rigidbody r in g.GetComponentsInChildren<Rigidbody>())
                r.mass = 10;
    }

    [MenuItem("Tools/zset")]
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
    
    [MenuItem("Tools/Skin Copy")]
    static void Skin()
    {
        Object[] ts = UnityEditor.Selection.GetFiltered(typeof(Object), UnityEditor.SelectionMode.Unfiltered);
        foreach (Object t in ts)
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
    [MenuItem("Tools/ScaleParticle")]
    static void ScaleParticle()
    {
        Object[] ts = UnityEditor.Selection.GetFiltered(typeof(GameObject), UnityEditor.SelectionMode.Unfiltered);
        
        float scale = 2f;        
        foreach (GameObject t in ts)
        {
            Undo.RegisterUndo(t, "scaleparticle");
            t.transform.localScale *= scale;
            foreach(var p in t.GetComponentsInChildren<ParticleEmitter>())
            {
                Undo.RegisterUndo(p, "scaleparticle");
                p.minSize *= scale;
                p.maxSize *= scale;
                p.localVelocity *= scale;
                p.rndVelocity *= scale;
                p.worldVelocity *= scale;
            }
            foreach (var p in t.GetComponentsInChildren<ParticleAnimator>())
            {
                Undo.RegisterUndo(p, "scaleparticle");
                p.force *= scale;
                p.rndForce *= scale;
                p.sizeGrow *= scale;
                p.localRotationAxis *= scale;
            }
            foreach (var p in t.GetComponentsInChildren<ParticleRenderer>())
            {
                Undo.RegisterUndo(p, "scaleparticle");
                p.velocityScale *= scale;
                p.maxParticleSize *= scale;
                p.lengthScale *= scale;
            }
        }
    }





}