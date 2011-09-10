using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
public class Tools : Editor{

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	}
    [MenuItem("RTools/SetToon")]
    static void SetTOon()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (Material m in Selection.objects)
        {
            //m.shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            //m.shader = Shader.Find("Diffuse");
            m.shader = Shader.Find("Toon/Basic Outline");
            m.SetTexture("_ToonShade", (Cubemap)AssetDatabase.LoadAssetAtPath(@"Assets\Standard Assets\Toon Shading\Sources\Textures\toony lighting.psd", typeof(Cubemap)));
        }
    }
    [MenuItem("RTools/Clip")]
    static void Clip()
    {
        Undo.RegisterSceneUndo("rtools");
        var a = Selection.transforms[0];
        var b = Selection.transforms[1];
        
        
        var rdif = a.rotation * Quaternion.Inverse(b.rotation);
        b.root.rotation *= rdif;
        var dif = a.position - b.position; // a to b
        b.root.position += dif;        
    }
    [MenuItem("RTools/PrintGlobalPos")]
    static void GlobalPos()
    {
        foreach (var a in Selection.transforms)
            a.position.print();
    }
}
