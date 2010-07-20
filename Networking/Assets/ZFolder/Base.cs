using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class Base : MonoBehaviour
{
        
    // Use this for initialization
    protected virtual void OnPlayerConnected(NetworkPlayer player) { }
    protected virtual void OnLevelWasLoaded(int level) { }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) { }
    protected virtual void Start() { }
    protected virtual void FixedUpdate() { }
    protected virtual void Update() { }
    protected virtual void OnTriggerEnter(Collider other) { }
    protected virtual void OnNetworkLoadedLevel() { }
    protected virtual void OnGUI() { }
    protected virtual void OnConnectedToServer() { }
    protected virtual void OnDisconnectedFromServer() { }
    protected virtual void Awake() { }
    protected virtual void OnApplicationQuit(){}
    protected virtual void LateUpdate() { }
    protected virtual void OnMasterServerEvent(MasterServerEvent msEvent) { }
    protected virtual void OnPreRender() { }
    protected virtual void OnWillRenderObject() { }    
    protected virtual void OnRenderObject(int queueIndex) { }
    //protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) { }
    protected virtual void OnParticleCollision(GameObject other) { }
    protected virtual void OnPlayerDisconnected(NetworkPlayer player) { }
    protected virtual void OnCollisionEnter(Collision collisionInfo) { }
    protected virtual void OnNetworkInstantiate(NetworkMessageInfo info) { }
    public virtual void OnSetID() { }
    public bool isMine { get { return networkView.isMine; } }
  
    
    public void Hide() { Show(false); }
    public void Show() { Show(true); }
    public void Show(bool value)
    {
        enabled = value;
        foreach (Renderer r in this.GetComponentsInChildren<Renderer>())
            r.enabled = value;        
    }    
    public void CallRPC(params object[] obs)   
    {        
        if (networkView.isMine)
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

