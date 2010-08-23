using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;


public class Base2 : MonoBehaviour
{    
    public new void print(object ob)
    {
                
        Loader.write("" + ob);
    }
    public static Cam _Cam;
    public static Loader _Loader;
    public static Spawn _Spawn;
    public static IPlayer _localiplayer;
    public static Player _LocalPlayer;
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

            
    protected virtual void OnConsole(string s) { }                            
    public virtual void OnSetID() { }

}
