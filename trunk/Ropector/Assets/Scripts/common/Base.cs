using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Object = UnityEngine.Object;
using System.Collections;
using System.Text.RegularExpressions;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
public partial class Base : MonoBehaviour
{
    public bool selected { get { return UnityEditor.Selection.activeGameObject == this.gameObject; } }
    public static string[] scenes
    {
        get
        {
            return new string[] { 
            "Assets/scenes/1.unity",                        
        };
        }
    }
    public static void IgnoreAll(string name, params string[] layers)
    {
        for (int i = 1; i < 31; i++)
            if (!layers.Contains(LayerMask.LayerToName(i)))
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer(name), i, true);
    }
   
    public void SetLayer(int l)
    {
        foreach (var t in this.transform.GetTransforms())
            t.gameObject.layer = l;
    }
    public void SetActive(bool value)
    {
        foreach (var t in this.transform.GetTransforms())
            t.gameObject.active = value;
    }
    public Vector3 pos { get { return transform.position; } set { transform.position = value; } }
    public float x { get { return pos.x; } set { var v = pos; v.x = value; pos = v; } }
    public float y { get { return pos.y; } set { var v = pos; v.y = value; pos = v; } }
    public float z { get { return pos.z; } set { var v = pos; v.z = value; pos = v; } }
    public Vector3 lpos { get { return transform.localPosition; } set { transform.localPosition = value; } }
    public Quaternion rot { get { return transform.rotation; } set { transform.rotation = value; } }
    public static string CreateTable(string source) //create table parse table
    {
        string table = "";
        MatchCollection m = Regex.Matches(source, @"\w*\s*");
        for (int i = 0; i < m.Count - 1; i++)
            table += "{" + i + ",-" + m[i].Length + "}";
        return table;
    }
    public static string[] files;

#if (UNITY_EDITOR && UNITY_STANDALONE_WIN)
    public static bool debug
    {
        get
        {
            return EditorPrefs.GetBool("Debug");            
        }
        set { EditorPrefs.SetBool("Debug", value); }
    }
    
#else
    public static bool debug;
#endif
    

#if (UNITY_EDITOR && UNITY_STANDALONE_WIN)
    
    public static IEnumerable<string> GetFiles()
    {
        if (files == null)
            files = Directory.GetFiles("./", "*.*", SearchOption.AllDirectories);
        return files.Select(a => a.Replace("\\", "/").Substring(2));
    }
    //public static T FindAsset<T>(string name) where T : Object { return (T)FindAsset(name, typeof(T)); }
    public static Object FindAsset(string name, Type t)
    {
        var aset = GetFiles().Where(a => Path.GetFileNameWithoutExtension(a) == name)
            .Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, t))
            .Where(a => a != null).FirstOrDefault();
        if (aset == null) throw new Exception("could not find asset " + name);
        return aset;
    }
#endif
    
    public virtual void Init()
    {
    }
    public virtual void OnEditorGui()
    {

    }
    public virtual void OnSceneGUI()
    {

    }
    public static void Combine(GameObject g)
    {
        var generateTriangleStrips = true;
        Component[] filters = g.GetComponentsInChildren(typeof(MeshFilter));
        Matrix4x4 myTransform = g.transform.worldToLocalMatrix;
        Hashtable materialToMesh = new Hashtable();

        for (int i = 0; i < filters.Length; i++)
        {			
            MeshFilter filter = (MeshFilter)filters[i];
            Renderer curRenderer = filters[i].renderer;
            MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance();
            instance.mesh = filter.sharedMesh;
            if (curRenderer != null && curRenderer.enabled && instance.mesh != null)
            {
                instance.transform = myTransform * filter.transform.localToWorldMatrix;

                Material[] materials = curRenderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);

                    ArrayList objects = (ArrayList)materialToMesh[materials[m]];
                    if (objects != null)
                    {
                        objects.Add(instance);
                    }
                    else
                    {
                        objects = new ArrayList();
                        objects.Add(instance);
                        materialToMesh.Add(materials[m], objects);
                    }
                }

                curRenderer.enabled = false;
            }
        }

        foreach (DictionaryEntry de in materialToMesh)
        {
            ArrayList elements = (ArrayList)de.Value;
            MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

            // We have a maximum of one material, so just attach the mesh to our own game object
            if (materialToMesh.Count == 1)
            {
                // Make sure we have a mesh filter & renderer
                if (g.GetComponent(typeof(MeshFilter)) == null)
                    g.gameObject.AddComponent(typeof(MeshFilter));
                if (!g.GetComponent("MeshRenderer"))
                    g.gameObject.AddComponent("MeshRenderer");

                MeshFilter filter = (MeshFilter)g.GetComponent(typeof(MeshFilter));
                filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
                g.renderer.material = (Material)de.Key;
                g.renderer.enabled = true;
            }
            // We have multiple materials to take care of, build one mesh / gameobject for each material
            // and parent it to this object
            else
            {
                GameObject go = new GameObject("Combined mesh");
                go.transform.parent = g.transform;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent(typeof(MeshFilter));
                go.AddComponent("MeshRenderer");
                go.renderer.material = (Material)de.Key;
                MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
                filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
            }
        }
    }
    public static string TimeToString(TimeSpan t)
    {
        var s = t.ToString();
        if (s.IndexOf(".") != -1)
            return s.Substring(0, s.IndexOf("."));
        else
            return s;
    }   
}