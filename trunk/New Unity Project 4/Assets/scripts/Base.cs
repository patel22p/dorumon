using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
        
    // Use this for initialization
    protected virtual void OnPlayerConnected(NetworkPlayer player) { }
    protected virtual void OnLevelWasLoaded(int level) { }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) { }
    protected virtual void OnLoaded() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnLateUpdate() { }

    void Update()
    {
        if (loaded) OnUpdate();
    }
    void LateUpdate()
    {
        if (loaded) OnLateUpdate();
    }

    void FixedUpdate() {
        if (loaded) OnFixedUpdate();
    }
    void Start()
    {
        if (loaded) OnLoaded();
    }
    protected virtual void OnTriggerEnter(Collider other) { }
    static bool loaded;
    void OnNetworkLoadedLevel()
    {
        loaded = true;
        Start();
    }
    protected virtual void OnGUI() { }
    protected virtual void OnConnectedToServer() { }
    protected virtual void OnDisconnectedFromServer() { }
    protected virtual void Awake() { }
    protected virtual void OnApplicationQuit(){}
    
    protected virtual void OnMasterServerEvent(MasterServerEvent msEvent) { }
    protected virtual void OnPreRender() { }
    protected virtual void OnWillRenderObject() { }    
    protected virtual void OnRenderObject(int queueIndex) { }
    //protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) { }
    protected virtual void OnParticleCollision(GameObject other) { }
    protected virtual void OnPlayerDisconnected(NetworkPlayer player) { }
    protected virtual void OnCollisionEnter(Collision collisionInfo) { }
    protected virtual void OnCollisionStay(Collision collisionInfo) { }
    protected virtual void OnTriggerStay(Collider collisionInfo) { }
    protected virtual void OnNetworkInstantiate(NetworkMessageInfo info) { }
    public virtual void OnSetID() { }
    public bool isMine { get { return myNetworkView!=null; } }
    public bool isOwner { get { return OwnerID == Network.player; } }
    //public bool isMineControlled { get { return myNetworkView != null && myNetworkView.observed != null; } }
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
    public NetworkView myNetworkView
    {
        get
        {
            foreach (NetworkView b in this.GetComponents<NetworkView>())
                if (b.isMine) return b;
            return null;
        }
    }

    
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
        CallRPC();
        foreach (NetworkView otherView in this.GetComponents<NetworkView>())
            otherView.observed = null;
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = null;
    }
    
    public void RPCSetOwner()
    {        
        myNetworkView.RPC("RPCSetOwner", RPCMode.All, Network.player);                                
    }

    public void Hide() { Show(false); }
    public void Show() { Show(true); }
    bool hidden;
    public void Show(bool value)
    {
        
        if (rigidbody != null)
        {
            rigidbody.collider.isTrigger = !value;
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
    public void CallRPC(params object[] obs)   
    {        
        if (isOwner)
        {            
            foreach (object o in new System.Diagnostics.StackFrame(2, true).GetMethod().GetCustomAttributes(true))
                if (o is RPC)
                    return;
            networkView.RPC(new System.Diagnostics.StackFrame(1, true).GetMethod().Name, RPCMode.Others, obs);            
        }
    }
    

    public static T Find<T>(string s) where T : Component 
    {
        GameObject g = GameObject.Find(s);
        if (g != null) return g.GetComponent<T>();
        return null;
    }

    public static T Find<T> () where T : Component
    {        
        return (T)Component.FindObjectOfType(typeof(T));
    }
    public NetworkPlayer? OwnerID
    {
        get { return _OwnerID; }
        set { _OwnerID = value; pwnerID2 = value.HasValue ? value.Value.GetHashCode() : -2; }
    }
    NetworkPlayer? _OwnerID;
    public int pwnerID2 = -2;        
    
}

