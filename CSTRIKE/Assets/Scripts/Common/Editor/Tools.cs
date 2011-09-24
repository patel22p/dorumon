using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
public class Tools : Editor{


    
    [MenuItem("RTools/SetToon")]
    public static void SetTOon()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (Material m in Selection.objects)
        {
            //m.shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            m.shader = Shader.Find("Diffuse");
            m.shader = Shader.Find("Toon/Basic Outline");
            m.SetTexture("_ToonShade", (Cubemap)AssetDatabase.LoadAssetAtPath(@"Assets\Standard Assets\Toon Shading\Sources\Textures\toony lighting.psd", typeof(Cubemap)));
            m.SetFloat("_Outline", .002f);
            m.SetColor("_Color", Color.white);
        }
    }
    
    [MenuItem("RTools/Parent")]
    public static void CreateParent()
    {
        Undo.RegisterSceneUndo("rtools");
        var t = Selection.activeTransform;
        var t2 = new GameObject("Parent").transform;
        t2.position = t.position;
        t2.rotation = t.rotation;
        t2.parent = t.parent;
        t.parent = t2;
        t2.name = t.name;
    }

    [MenuItem("RTools/CopyColor")]
    public static void CopyColor()
    {
        Undo.RegisterSceneUndo("rtools");
        var c = Selection.activeGameObject.guiTexture.color;
        
        foreach (var a in Selection.gameObjects)
        {
            if(a.guiTexture!=null)
            a.guiTexture.color = c;
            if (a.guiText != null)
            {
                var mat = (Material)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Material)).FirstOrDefault(b => b.name == "txt");
                mat.color = c;
                a.guiText.material = mat;
            }
        }
    }
    [MenuItem("RTools/Clip")]
    public static void Clip()
    {
        Undo.RegisterSceneUndo("rtools");
        var a = Selection.transforms[0];
        var b = Selection.transforms[1];
        
        
        var rdif = a.rotation * Quaternion.Inverse(b.rotation);
        b.root.rotation *= rdif;
        var dif = a.position - b.position; // a to b
        b.root.position += dif;        
    }
    
}
