using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

using doru;
public class Base : Base2 , IDisposable
{

    
    bool hidden;



    public int OwnerID = -1;
    
    public bool isOwner { get { return OwnerID == Network.player.GetHashCode(); } }
    public bool isOwnerOrServer { get { return (this.isOwner || (Network.isServer && this.OwnerID == -1)); } }
    

    public TimerA _TimerA { get { return TimerA._This; } }
    
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
    
    public static RaycastHit ScreenRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Physics.Raycast(ray, out h, float.MaxValue, collmask);
        return h;
    }

    public static GameObject Root(GameObject g)
    {
        return Root(g.transform).gameObject;
    }
    static internal int collmask = 1 << 8 | 1 << 9 | 1 << 12 | 1 << 13;
    public static Transform Root(Transform g)
    {
        Transform p = g;
        while (true)
        {
            if (p.parent == null) return p;
            p = p.parent;
        }
    }
    
    [RPC]
    void RPCSetOwner(int owner, NetworkMessageInfo ownerView)
    {
        SetController(owner);
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetID();
        }
    }
    [RPC]
    public void SetController(int owner)
    {        
        lock ("ser")
            ((box)this).selected = owner;
    }
    
    //public void Enable(GameObject t , bool b)
    //{
    //    foreach (Behaviour a in t.GetComponentsInChildren<Behaviour>())
    //        a.enabled = b;
    //}

    [RPC]
    public void RPCResetOwner()
    {
        CallRPC(true);
        ((box)this).selected=-1;
        //foreach (NetworkView otherView in this.GetComponents<NetworkView>())
        //    otherView.observed = null;

        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = -1;

    }

    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {
        
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)Group.RPCAssignID;
        nw.observed = this;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }
    public void RPCSetOwner()
    {
        myNetworkView.RPC("RPCSetOwner", RPCMode.AllBuffered, Network.player.GetHashCode());
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
    public bool zombi { get { return _Loader.gameMode == Loader.GameMode.TeamZombieSurvive; } }
    public bool tdm { get { return _Loader.gameMode == Loader.GameMode.TeamDeathMatch; } }
    public bool dm { get { return _Loader.gameMode == Loader.GameMode.DeathMatch; } }
    static bool _lockCursor;
    public static bool lockCursor { get { return _lockCursor; } set { _lockCursor = value; Screen.lockCursor = value; } }
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
        MethodBase mb = new System.Diagnostics.StackFrame(2, true).GetMethod();

        if (mb != null)
        {
            foreach (object o in mb.GetCustomAttributes(false))
                if (o is RPC) UnityEngine.Debug.Log("Dublicate");
            networkView.RPC(new System.Diagnostics.StackFrame(1, true).GetMethod().Name, buffered ? RPCMode.OthersBuffered : RPCMode.Others, obs);
        }
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }

    public Dictionary<int,Player> players { get { return _Spawn.players; } }
    public void Destroy()
    {        
        foreach (Base b in this.GetComponentsInChildren<Base>())
            b.Dispose();
        Destroy(this.gameObject);        
        
    }
    public void Destroy(int time)
    {        
        _TimerA.AddMethod(time, Destroy);
    }


    public virtual void Dispose()
    {
        
    }
}

