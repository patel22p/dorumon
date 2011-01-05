using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using doru;
using System;
using System.IO;


public partial class Base2 : MonoBehaviour
{    
    //public static void print(params object[] ob)
    //{
    //    MethodBase m = new System.Diagnostics.StackFrame(1, true).GetMethod();
    //    StringBuilder sb = new StringBuilder();
    //    sb.Append(m.ReflectedType.Name + "." + m.Name);
    //    foreach (object o in ob)
    //        sb.Append(" " + o + ",");
    //    MonoBehaviour.print(sb.ToString());
    //}
    public static string pr
    {
        get
        {
            MethodBase m = new System.Diagnostics.StackFrame(1, true).GetMethod();
            StringBuilder sb = new StringBuilder();
            sb.Append(m.ReflectedType.Name + "." + m.Name);
            return sb.ToString();
        }
    }
    public static string nick { get { return _Loader.LocalUserV.nick; } set { _Loader.LocalUserV.nick = value; } }
    //public static Cam _Cam; 
    public static T TakeRandom<T>(IList<T> t)
    {
        return t[UnityEngine.Random.Range(0, t.Count-1)];
    }
    static GameWindow __GameWindow;
    public static GameWindow _GameWindow { get { if (__GameWindow == null) __GameWindow = (GameWindow)MonoBehaviour.FindObjectOfType(typeof(GameWindow)); return __GameWindow; } }
    static Irc __Irc;
    public static Irc _Irc { get { if (__Irc == null) __Irc = (Irc)MonoBehaviour.FindObjectOfType(typeof(Irc)); return __Irc; } }
    
    static Game __Game;
    public static Game _Game { get { if (__Game == null) __Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return __Game; } }
    static Cam __Cam;
    public static Cam _Cam { get { if (__Cam == null) __Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return __Cam; } }
    static Loader __Loader;
    public static Loader _Loader { get { if (__Loader == null) __Loader = (Loader)MonoBehaviour.FindObjectOfType(typeof(Loader)); return __Loader; } }
    static Menu __Menu;
    public static Menu _Menu { get { if (__Menu == null) __Menu = (Menu)MonoBehaviour.FindObjectOfType(typeof(Menu)); return __Menu; } }
    static Music __Music;
    public static Music _Music { get { if (__Music == null) __Music = (Music)MonoBehaviour.FindObjectOfType(typeof(Music)); return __Music; } }
    static Console __Console;
    public static Console _Console { get { if (__Console == null) __Console = (Console)MonoBehaviour.FindObjectOfType(typeof(Console)); return __Console; } }
    public long memorystart = 0;    
    
    public static string GenerateTable(string source)
    {
        string table = "";
        MatchCollection m = Regex.Matches(source, @"\w*\s*");
        for (int i = 0; i < m.Count - 1; i++)
            table += "{" + i + ",-" + m[i].Length + "}";
        return table;
    }
    
    public static Player _localPlayer { get { return _Game._localPlayer; } set { _Game._localPlayer = value; } }
    public static MapSetting mapSettings { get { return _Loader.mapSettings; } set { _Loader.mapSettings = value; } }
    public static TimerA _TimerA { get { if (!Application.isPlaying) throw new Exception("access from editor"); return _Loader._TimerA; } }
    public static VK _vk;
    public static bool lockCursor { get { return Screen.lockCursor; } set { Screen.lockCursor = value; } }
    public static Rect CenterRect(float w, float h)
    {
        
        Vector2 c = new Vector2(Screen.width, Screen.height); 
        Vector2 v = new Vector2(w * c.x, h * c.y);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }
    public virtual void OnSetOwner() { }

    public virtual Quaternion rot { get { return transform.rotation; } set { transform.rotation = value; } }
    public virtual Vector3 pos { get { return transform.position; } set { transform.position = value; } }
    public float rotx
    {
        get { return rot.eulerAngles.x; }
        set
        {
            Quaternion r = rot;
            Vector3 v = r.eulerAngles;
            v.x = value;
            r.eulerAngles = v;
            transform.rotation = r;
        }
    }
    public float roty
    {
        get { return rot.eulerAngles.y; }
        set
        {
            Quaternion r = rot;
            Vector3 v = r.eulerAngles;
            v.y = value;
            r.eulerAngles = v;
            transform.rotation = r;
        }
    }
    public float rotz
    {
        get { return rot.eulerAngles.z; }
        set
        {
            Quaternion r = rot;
            Vector3 v = r.eulerAngles;
            v.z = value;
            r.eulerAngles = v;
            transform.rotation = r;
        }
    }
    public float posx
    {
        get { return rot.x; }
        set
        {
            Vector3 p = pos;
            p.x = value;
            pos = p;
        }
    }
    public float posy
    {
        get { return rot.y; }
        set
        {
            Vector3 p = pos;
            p.y = value;
            pos = p;
        }
    }
    public float posz
    {
        get { return rot.z; }
        set
        {
            Vector3 p = pos;
            p.z = value;
            pos = p;
        }
    }
    public virtual void Init()
    {
        inited = true;
    }
    public bool inited;
#if (UNITY_EDITOR)
    public static T FindAsset<T>(string name) where T : Object { return (T)FindAsset(name, typeof(T)); }
    public static string[] files;
    public static IEnumerable<string> GetFiles()
    {
        if(files==null)
            files = Directory.GetFiles("./", "*.*", SearchOption.AllDirectories);
        return files.Select(a => a.Replace("\\", "/").Substring(2));
    }
    public static Object FindAsset(string name, Type t)
    {
        var aset = GetFiles().Where(a => Path.GetFileNameWithoutExtension(a) == name)
            .Select(a => UnityEditor.AssetDatabase.LoadAssetAtPath(a, t))
            .Where(a => a != null).FirstOrDefault();
        if (aset == null) Debug.Log("could not find asset " + name);
        return aset;
    }
#endif

}
