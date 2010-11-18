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
        
    public Version version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
    public const string hosting = "http://physxwars.rh10.ru/";
    static public bool build { get { return  _Loader.build; } }
    static public bool skip { get { return _Loader.skip; } }
    static public bool isWebPlayer { get { return Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer; } }
    public Level _Level { get { return _Loader._Level; } set { _Loader._Level = value; } }
    bool hidden;
    public int OwnerID = -1;
    //public bool logged; //{ get { return _vk._Status == VK.Status.connected; } }    
    static public UserView LocalUserV { get { return _Loader.LocalUserV; } set { _Loader.LocalUserV = value; } }
    static public UserView[] userViews { get { return _Loader.userViews; } }    
    public bool isOwner { get { return OwnerID == Network.player.GetHashCode(); } }
    public bool isOwnerOrServer { get { return (this.isOwner || (Network.isServer && this.OwnerID == -1)); } }    
    public const int collmask = 1 << 8 | 1 << 9 | 1 << 12 | 1 << 13;
    public void ShowPopup(string s)
    {
        _PopUpWindow.Show(this);
        _PopUpWindow.Text = s;
    }
    protected virtual void Awake()
    {
        if (_Loader == null)
            Instantiate(Resources.Load("Prefabs/loader"));

        OwnerID = -1;
    }
    public bool DebugKey(KeyCode key)
    {
        return Input.GetKeyDown(key) && !build;
    }
    public string joinString(char j,params object[] o)
    { 
        string s = "";
        foreach (object a in o)
            s += a.ToString() + j;
        return s.Trim(j); 
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
    public IEnumerable<Player> TP2(Team t)
    {
        foreach (Player p in players)
            if (p!=null && p.team == t) yield return p;
    }
    public IEnumerable<UserView> TP(Team t)
    {
        foreach (UserView p in userViews)
            if (p != null)
                if (p.team == t) yield return p;
    }
    public void PlaySound(string path)
    {
        PlaySound(path, 1);
    }
    public void PlaySound(string path,float volume)
    {        
        AudioClip au = (AudioClip)Resources.Load(path);
        if (au == null) print("could not load" + path);
        else
            transform.root.audio.PlayOneShot(au,volume);
    }
    public void PlayRandSound(string s)
    {
         PlayRandSound(Resources.LoadAll(s));
    }
    public GameObject Load(string s)
    {
        GameObject g = (GameObject)Resources.Load(s);
        if (g == null) print("Could Not Load GameObject");
        return g;
    }
    void PlayRandSound(UnityEngine.Object[] au)
    {
        if (!transform.root.audio.isPlaying)
            transform.root.audio.PlayOneShot((AudioClip)au[UnityEngine.Random.Range(0, au.Length)]);
    }
    public static RaycastHit ScreenRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Physics.Raycast(ray, out h, float.MaxValue, collmask);
        return h;
    }
    public static GameObject Root(MonoBehaviour g)
    {
        return Root(g.transform).gameObject;
    }
    public static GameObject Root(GameObject g)
    {
        return Root(g.transform).gameObject;
    }
    public static Transform Root(Transform g)
    {
        return g.root;        
    }
    public virtual void OnPlayerConnected1(NetworkPlayer np)
    {        
        
    }
    public void Hide() { Show(false); }
    public void Show() { Show(true); }
    public void Show(bool value) 
    {
        if (this != null && rigidbody != null)
        {
            rigidbody.isKinematic = !value;
            rigidbody.detectCollisions = value;
            rigidbody.useGravity = value;
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
    object[] substr(object[] ob,int sub)
    {
        object[] obs = new object[ob.Length - sub];
        ob.CopyTo(obs, sub);
        return obs;
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }
    public Player[] players { get { return _Game.players; } }
    
}
