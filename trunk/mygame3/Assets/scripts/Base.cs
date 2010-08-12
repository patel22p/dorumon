using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using doru;
public class Base : Base2
{
    
    public int pwnerID2 = -2;
    bool hidden;
    public bool started;
    public static bool levelLoaded;
    NetworkPlayer? _OwnerID;
    public NetworkPlayer? OwnerID
    {
        get { return _OwnerID; }
        set { _OwnerID = value; pwnerID2 = value.HasValue ? value.Value.GetHashCode() : -2; }
    }
    public bool isOwner { get { return OwnerID == Network.player; } }
    public TimerA _TimerA { get { return TimerA._This; } }
    public Cam _Cam { get { return Find<Cam>(); } }
    bool Offline { get { return !Loader.Online; } }
    public NetworkView myNetworkView
    {
        get
        {
            foreach (NetworkView b in this.GetComponents<NetworkView>())
                if (b.isMine) return b;
            return null;
        }
    }
    public bool isControlled
    {
        get
        {
            foreach (NetworkView b in this.GetComponents<NetworkView>())
                if (b.observed != null)
                    return true;
            return false;
        }
    }
    public Loader _Loader { get { return Find<Loader>(); } }
    public Spawn spawn { get { return Find<Spawn>(); } }
    
    
    
    public static GameObject Root(GameObject g)
    {
        Transform p = g.transform;
        while (true)
        {
            if (p.parent == null) return p.gameObject;
            p = p.parent;
        }
    }
    
    
    protected virtual void OnTriggerEnter(Collider other) { }
    
    [RPC]
    void RPCSetOwner(NetworkPlayer owner, NetworkMessageInfo ownerView)
    {        
        foreach (NetworkView otherView in this.GetComponents<NetworkView>())
            otherView.observed = null;
        ownerView.networkView.observed = this.rigidbody; 

        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetID();
        }       
    }
    [RPC]
    public void RPCResetOwner()
    {
        CallRPC(true);
        foreach (NetworkView otherView in this.GetComponents<NetworkView>())
            otherView.observed = null;
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = null;
    }
    public void RPCSetOwner()
    {        
        myNetworkView.RPC("RPCSetOwner", RPCMode.AllBuffered, Network.player);                                
    }
    public void Hide() { Show(false); }
    public void Show() { Show(true); }
    [RPC]
    public void RPCShow(bool value)
    {
        CallRPC(true, value);
        Show(value);
    }
    public void Show(bool value)
    {        
        if (rigidbody != null)
        {            
            rigidbody.detectCollisions = value;
            rigidbody.useGravity = value;
            rigidbody.velocity = rigidbody.angularVelocity = Vector3.zero;            
        }
        if (value)
        {
            if (hidden) { transform.localPosition += new Vector3(99999, 0, 0); hidden = false; }
        }
        else
        {
            if (!hidden) { transform.localPosition -= new Vector3(99999, 0, 0); hidden = true; }
            
        }        
        foreach (Base r in this.GetComponentsInChildren<Base>())
            r.enabled = value;
    }    
    public IEnumerable<Transform> getChild(Transform t)
    {
        yield return t;
        for (int i = 0; i < t.childCount; i++)
        {
            foreach (Transform a in getChild(t.GetChild(i)))
                yield return a;
        }
    }
    private void Active(bool value, Transform t)
    {
        for (int i = 0; i < t.transform.childCount; i++)
        {
            t.transform.GetChild(i).gameObject.active = value;
            Active(value, t.transform.GetChild(i));
        }
    }
    public void CallRPC(bool buffered,params object[] obs)
    {

        if (new System.Diagnostics.StackFrame(2, true).GetMethod() != null)
        {
            foreach (object o in new System.Diagnostics.StackFrame(2, true).GetMethod().GetCustomAttributes(true))
                if (o is RPC)
                    return;
            networkView.RPC(new System.Diagnostics.StackFrame(1, true).GetMethod().Name, buffered ? RPCMode.OthersBuffered : RPCMode.Others, obs);
        }        
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }    
}

