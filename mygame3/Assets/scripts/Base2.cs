using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;



public class Base2 : MonoBehaviour
{    
    public void printC(object ob)
    {        
        lock("write")
            z0ConsoleWindow.write("" + ob);
    }
    public void log(object ob)
    {
        MonoBehaviour.print(ob);
    }
    public static string Nick { get { return PlayerPrefs.GetString("Nick"); } set { PlayerPrefs.SetString("Nick", value); } }
    public static Cam _Cam;
    public static float autostart;
    public static z0Loader _Loader;
    public static z2Menu _menu;
    public static Music music;
    public static z4Game _Spawn;
    public static z2HostWindow _hw;
    public static IPlayer _localiplayer;
    public static Player _LocalPlayer;
    static bool _lockCursor;
    public static z0ConsoleWindow _cw;
    public static OptionsWindow _options;
    public static z0Vk _vk;
    public static z0VkontakteWindow _Vkontakte;
    
    public static void rpcwrite(string s) { _cw.rpcwrite(s); }
    public static void write(string s) { z0ConsoleWindow.write(s); }
    public static bool lockCursor { get { return _lockCursor; } set { _lockCursor = value; Screen.lockCursor = value; } }

    //public static T Find<T>(string s) where T : Component
    //{
        
    //    GameObject g = GameObject.Find(s);
    //    if (g != null) return g.GetComponent<T>();
    //    return null;
    //}
    public static Rect CenterRect(float w, float h)
    {
        
        Vector2 c = new Vector2(Screen.width, Screen.height);
        Vector2 v = new Vector2(w * c.x, h * c.y);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }

            
    
    public virtual void OnSetOwner() { }

}
