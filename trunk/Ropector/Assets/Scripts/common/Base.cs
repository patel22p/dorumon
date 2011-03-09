using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Object = UnityEngine.Object;
[AddComponentMenu("Base")]
public class Base : MonoBehaviour
{
    public Vector3 pos { get { return transform.position; } set { transform.position = value; } }
    public Quaternion rot { get { return transform.rotation; } set { transform.rotation = value; } }
    public virtual void Awake()
    {
    }
    public static string[] files;
#if (UNITY_EDITOR && UNITY_STANDALONE_WIN)
    public static IEnumerable<string> GetFiles()
    {
        if (files == null)
            files = Directory.GetFiles("./", "*.*", SearchOption.AllDirectories);
        return files.Select(a => a.Replace("\\", "/").Substring(2));
    }
    public static T FindAsset<T>(string name) where T : Object { return (T)FindAsset(name, typeof(T)); }
    public static Object FindAsset(string name, Type t)
    {
        var aset = GetFiles().Where(a => Path.GetFileNameWithoutExtension(a) == name)
            .Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, t))
            .Where(a => a != null).FirstOrDefault();
        if (aset == null) Debug.Log("could not find asset " + name);
        return aset;
    }
#endif
    public virtual void InitValues()
    {
    }

    public virtual void Init()
    {

    }
}