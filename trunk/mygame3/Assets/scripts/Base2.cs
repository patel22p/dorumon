using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;


public class Base2 : MonoBehaviour
{
    [DebuggerStepThrough]
    public new void print(object ob)
    {
        Loader.write("" + ob);
    }

    public static T Find<T>(string s) where T : Component
    {
        GameObject g = GameObject.Find(s);
        if (g != null) return g.GetComponent<T>();
        return null;
    }
    public static Rect CenterRect(float w, float h)
    {

        Vector2 c = new Vector2(Screen.width, Screen.height);
        Vector2 v = new Vector2(w * c.x, h * c.y);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }

    public static T Find<T>() where T : Component
    {
        return (T)Component.FindObjectOfType(typeof(T));
    }
    protected IPlayer LocalIPlayer { get { return (IPlayer)Find<Cam>().localplayer; } }
    public Player _localPlayer { get { return Find<Player>("LocalPlayer"); } }    
    protected virtual void OnConsole(string s) { }                            
    public virtual void OnSetID() { }

}
