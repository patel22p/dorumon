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
    protected virtual void OnDisconnectedFromServer(NetworkDisconnection info) { }
    protected virtual void OnServerInitialized() { }
    protected virtual void OnPlayerConnected(NetworkPlayer player) { }
    
    protected virtual void OnConnectedToServer() { }
    protected virtual void OnApplicationQuit() { }
    protected virtual void OnConsole(string s) { }
    protected virtual void OnMasterServerEvent(MasterServerEvent msEvent) { }
    //protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) { }
    protected virtual void OnPlayerDisconnected(NetworkPlayer player) { }
    protected virtual void OnCollisionEnter(Collision collisionInfo) { }
    protected virtual void OnCollisionStay(Collision collisionInfo) { }
    protected virtual void OnTriggerStay(Collider collisionInfo) { }
    protected virtual void OnNetworkInstantiate(NetworkMessageInfo info) { }
    public virtual void OnSetID() { }

}
