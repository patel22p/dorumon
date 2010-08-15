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


    NetworkPlayer? _OwnerID;
    public NetworkPlayer? OwnerID
    {
        get { return _OwnerID; }
        set { _OwnerID = value; pwnerID2 = value.HasValue ? value.Value.GetHashCode() : -2; }
    }
    public bool isOwner { get { return OwnerID == Network.player; } }
    public bool isOwnerOrServer { get { return (this.isOwner || (Network.isServer && this.OwnerID == null)); } }

    public TimerA _TimerA { get { return TimerA._This; } }
    public Cam _Cam { get { return Find<Cam>(); } }
    bool Offline { get { return !Loader.Online; } }
    public NetworkView myNetworkView
    {
        get
        {
            return GetNetworkView(Network.player);
        }
    }

    public NetworkView GetNetworkView(NetworkPlayer pl)
    {
        foreach (NetworkView b in this.GetComponents<NetworkView>())
            if (b.owner == pl) return b;
        return null;
    }

    public Loader _Loader { get { return Find<Loader>(); } }
    public Spawn _Spawn { get { return Find<Spawn>(); } }
    public static GameObject Root(GameObject g)
    {
        return Root(g.transform).gameObject;
    }
    public static Transform Root(Transform g)
    {
        Transform p = g;
        while (true)
        {
            if (p.parent == null) return p;
            p = p.parent;
        }
    }
    protected virtual void OnTriggerEnter(Collider other) { }
    [RPC]
    void RPCSetOwner(NetworkPlayer owner, NetworkMessageInfo ownerView)
    {
        SetController(owner);
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetID();
        }
    }
    [RPC]
    public void SetController(NetworkPlayer owner)
    {        
        lock ("ser")
            GetComponent<NetworkRigidbody>().selected = owner;
    }
    
    [RPC]
    public void RPCResetOwner()
    {
        CallRPC(true);
        GetComponent<NetworkRigidbody>().selected=null;
        //foreach (NetworkView otherView in this.GetComponents<NetworkView>())
        //    otherView.observed = null;

        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = null;

    }

    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)Group.RPCAssignID;
        nw.observed = GetComponent<NetworkRigidbody>();
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
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
    public void Show(bool value) // bag s timerom
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
     
    
    public static IEnumerable<Transform> getChild(Transform t)
    {        
        for (int i = 0; i < t.childCount; i++)
        {
            yield return t.GetChild(i);
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
    public void CallRPC(bool buffered, params object[] obs)
    {
        if (new System.Diagnostics.StackFrame(2, true).GetMethod() != null)
            networkView.RPC(new System.Diagnostics.StackFrame(1, true).GetMethod().Name, buffered ? RPCMode.OthersBuffered : RPCMode.Others, obs);
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }
}

