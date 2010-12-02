using System.Linq;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using doru;
using System.Xml.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

public class Base : Base2
{
    public int OwnerID = -1;
    //public bool logged; //{ get { return _vk._Status == VK.Status.connected; } }    
    public bool isOwner { get { return OwnerID == Network.player.GetHashCode(); } }
    public bool isOwnerOrServer { get { return (this.isOwner || (Network.isServer && this.OwnerID == -1)); } }    
    public void ShowPopup(string s)
    {
        _PopUpWindow.ShowDontHide(this);
        _PopUpWindow.Text = s;
    }
    protected virtual void Awake()
    {        
        if (_Loader == null)
            Instantiate(GameObject.FindObjectsOfTypeIncludingAssets(typeof(Loader)).First());        
    }
    
    
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
    public static string version { get { return isWebPlayer ? "" : System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
    public const string hosting = "http://physxwars.rh10.ru/";
    public static bool build { get { return _Loader.build; } }
    public static bool skip { get { return _Loader.skip; } }
    public static bool isWebPlayer { get { return Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer; } }
    public static Level _Level { get { return _Loader._Level; } set { _Loader._Level = value; } }
    public static UserView LocalUserV { get { return _Loader.LocalUserV; } set { _Loader.LocalUserV = value; } }
    
    //public static LayerMask collmask { get { return _Loader.collmask; } }
    public static bool DebugKey(KeyCode key)
    {
        if (Input.GetKeyDown(key) && !build) print("Debug Key" + key);
        return Input.GetKeyDown(key) && !build;
    }
    public static string joinString(char j, params object[] o)
    {
        string s = "";
        foreach (object a in o)
            s += a.ToString() + j;
        return s.Trim(j);
    }
    public static IEnumerable<Player> TP(Team t)
    {
        foreach (Player p in players)
                if (p != null && p.team == t) yield return p;
    }
    public static bool IsPointed(LayerMask collmask, float len)
    {
        return RayCast(collmask,len).collider != null;
    }
    public static RaycastHit RayCast(LayerMask msk,float len)
    {
        if (!lockCursor) return default(RaycastHit);
        
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(1);
        RaycastHit h;
        Physics.Raycast(ray, out h, len, msk);
        return h;
    }
    public static IEnumerable<Transform> getChild(Transform t)
    {
        if(t!=null)
        for (int i = 0; i < t.childCount; i++)
            yield return t.GetChild(i);
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }
    public static Player[] players { get { return _Game.players; } }
    public void PlaySound(AudioClip au)
    {
        PlaySound(au, 1);
    }
    public void PlaySound(AudioClip au, float volume)
    {
        transform.root.audio.PlayOneShot(au, volume);
    }
    public void PlayRandSound(AudioClip[] au)
    {
        if (!transform.root.audio.isPlaying)
            transform.root.audio.PlayOneShot((AudioClip)au[UnityEngine.Random.Range(0, au.Length)]);
    }
    public Transform root { get { return this.transform.root; } }
    public virtual void OnPlayerConnected1(NetworkPlayer np)
    {        
    }
    

    public void Hide() { Show(false); }
    public void Show() { Show(true); }

    [RPC]
    public void RPCShow(bool value)
    {
        CallRPC(value);
        Show(value);
    }
    public static void Show(GameObject g, bool value)
    {
        foreach (var rigidbody in g.GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.isKinematic = !value;
            rigidbody.detectCollisions = value;
            rigidbody.useGravity = value;
        }

        foreach (var r in g.GetComponentsInChildren<Renderer>())
            r.enabled = value;
        foreach (var r in g.GetComponentsInChildren<AudioListener>())
            r.enabled = value;
        foreach (var r in g.GetComponentsInChildren<Camera>())
            r.enabled = value;

        foreach (var r in g.GetComponentsInChildren<AudioSource>())
        {
            r.enabled = value;
            if (!value)
                r.Stop();
            else
                if (r.playOnAwake) r.Play();
        }
    }

    public void Show(bool value) 
    {        
        Show(this.gameObject, value);
        foreach (Base r in this.GetComponentsInChildren<Base>())
        {
            r.enabled = value;
            r.onShow(value);
        }
    }
    public virtual void onShow(bool enabled)
    {
    }
    public void CallRPC(params object[] obs)
    {                
        MethodBase rpcmethod = new System.Diagnostics.StackFrame(1, true).GetMethod();
        MethodBase mb;
        for (int i = 2; true; i++)
        {
            mb = new System.Diagnostics.StackFrame(i, true).GetMethod();
            if (mb == null || mb.Name != rpcmethod.Name) break;
        }
        if (mb != null)
            networkView.RPC(rpcmethod.Name, RPCMode.Others, obs);
    }
}
