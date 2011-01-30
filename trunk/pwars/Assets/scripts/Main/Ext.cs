
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Text;

public static class Ext
{

    public static bool toBool(this int v)
    {
        return v != 0;
    }
    public static int toInt(this bool v)
    {
        return v ? 1 : 0;
    }
    public static string[] Split(this string s,string d)
    {
        return s.Split(new string[] { d }, StringSplitOptions.RemoveEmptyEntries);
    }
    public static string CalculateMD5Hash(string input)
    {
        MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
            sb.Append(hash[i].ToString("X2"));
        return sb.ToString();
    }

    public static int SelectIndex<T>(this IEnumerable<T> strs, T t) where T: class
    {
        int i =0;
        foreach (var a in strs)
        {
            if (a == t)
                return i;
            i++;
        }
        return -1;
    }
    public static IEnumerable<Transform> GetTransforms(this Transform ts)
    {
        yield return ts;
        foreach (Transform t in ts)
        {
            foreach (var t2 in GetTransforms(t))
                yield return t2;
        }
    }
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> coll, int N)
    {
        return coll.Reverse().Take(N).Reverse();
    }

    public static T Random<T>(this IEnumerable<T> source)
    {
        return source.Skip(UnityEngine.Random.Range(0, source.Count())).FirstOrDefault();
    }
    public static T AddOrGet<T>(this GameObject g) where T : Component
    {
        var c = g.GetComponent<T>();
        if (c == null) return g.AddComponent<T>();
        else
            return c;
    }

    public static T Parse<T>(this string s)
    {
        return (T)Enum.Parse(typeof(T), s);
    }
    public static T GetComponentInParrent<T>(this Transform t) where T : Component
    {
        for (int i = 0; ; i++)
        {
            if (t == null || i > 4) return null;
            var c = t.GetComponent<T>();
            if (c != null) return c;
            t = t.parent;
        }

    }
    public static MonoBehaviour GetMonoBehaviorInParrent(this Transform t)
    {
        for (int i = 0; ; i++)
        {
            if (t == null || i > 2) return null;
            var c = t.GetComponent<MonoBehaviour>();
            if (c != null) return c;
            t = t.parent;
        }
    }
    //public static IEnumerable<T> ShuffleIterator<T>(
    //   this IEnumerable<T> source, Random rng)
    //{
    //    T[] buffer = source.ToArray();
    //    for (int n = 0; n < buffer.Length; n++)
    //    {
    //        int k = rng.Next(n, buffer.Length);
    //        yield return buffer[k];

    //        buffer[k] = buffer[n];
    //    }
    //}

}
public class FindAsset : Attribute
{
    public string name;
    public bool overide;
    public FindAsset() { }
    public FindAsset(string from)
    {
        name = from;
    }
}
public class FindTransform : Attribute
{
    public string name;
    public FindTransform() { }
    public FindTransform(string enumName)
    {
        name = enumName;
    }
    public bool scene;
    public FindTransform(string enumName, bool FindInScene)
    {
        scene = FindInScene;
        name = enumName;
    }
}
public class GenerateEnums : Attribute
{
    public bool overide;
    public string name;
    public GenerateEnums(string enumName)
    {        
        name = enumName;
    }
}

