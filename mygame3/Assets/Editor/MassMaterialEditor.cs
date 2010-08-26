using System;
using UnityEditor;
using UnityEngine;

class MassMaterialEditor : EditorWindow
{
    [MenuItem("zSet/zrig")]
    static void zrig()
    {
        foreach (GameObject g in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
            foreach (Rigidbody r in g.GetComponentsInChildren<Rigidbody>())
                r.mass = 10;
    }
    [MenuItem("zSet/zset")]
    static void zset()
    {
        Undo.RegisterSceneUndo("zset");
        foreach (Texture t in FindObjectsOfTypeIncludingAssets(typeof(Texture)))
        {
            if (t.name == "flatiron_rendered")
            {

                foreach (GameObject g in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
                {
                    foreach (MeshRenderer r in g.transform.GetComponentsInChildren<MeshRenderer>())
                    {
                        foreach (Material m in r.sharedMaterials)
                        {
                            //m.shader = Shader.Find("Diffuse");
                            //m.shader = Shader.Find("VertexLit");
                            m.shader = Shader.Find("ExternalLightmappingTool/LightmappedDiffuse");
                            
                            //float f = 0.1f;
                            //m.SetColor("_Emission", new Color(f, f, f));
                            m.SetTexture("_LightMap", t);
                            
                        }
                    }
                }

            }
        }

    }






}