using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using doru;
using System.Xml.Serialization;
using System;
public class Base : Base2, System.IDisposable
{
    public static Version version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
    public static string hosting = "http://physxwars.rh10.ru/";
    public static bool build = true;
    public static bool skip = false;
    public static bool timeLimit = false;
    
    public static bool isWebPlayer = Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer;
    public static Level _Level;
    public static bool Online;
    bool hidden;
    public int OwnerID = -1;
    public static bool logged { get { return _vk._Status == z0Vk.Status.connected; } }
    public static z0Vk.user localuser;
    public static Dictionary<int, z0Vk.user> userviews = new Dictionary<int, z0Vk.user>();
    public static XmlSerializer respxml = new XmlSerializer(typeof(z0Vk.response), new Type[] { typeof(z0Vk.user), typeof(z0Vk.message_info) });
    public bool isOwner { get { return OwnerID == Network.player.GetHashCode(); } }
    public bool isOwnerOrServer { get { return (this.isOwner || (Network.isServer && this.OwnerID == -1)); } }


    protected bool Duplicate()
    {
        if (GameObject.FindObjectsOfType(this.GetType()).Length == 2)
        {
            foreach (Behaviour b in this.GetComponentsInChildren<Behaviour>())
                b.enabled = false;
            Destroy(this.gameObject);
            return true;
        }
        return false;
    }

    public bool DebugKey(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }
    public static string tostring(params object[] o)
    {
        string s = "";
        foreach (object a in o)
            s += a + ",";
        return s.Trim(',');
    }
    public TimerA _TimerA { get { return TimerA._This; } }
    bool Offline { get { return !z0Loader.Online; } }
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
        foreach (Player p in players.Values)
            if (p.team == t) yield return p;
    }
    public IEnumerable<z0Vk.user> TP(Team t)
    {
        foreach (z0Vk.user p in userviews.Values)
            if (p.team == t) yield return p;
    }

    public void PlayRandSound(AudioClip[] au)
    {
        audio.PlayOneShot(au[UnityEngine.Random.Range(0, au.Length - 1)]);
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
    public static Localize lc = new Localize();
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
    void RPCSetOwner(int owner)
    {
        CallRPC(true, owner);
        SetController(owner);
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetOwner();
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
        ((box)this).selected = -1;
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
        RPCSetOwner(Network.player.GetHashCode());
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
    public bool zombi { get { return _hw.gameMode == GameMode.TeamZombieSurvive; } }
    public bool tdm { get { return _hw.gameMode == GameMode.TeamDeathMatch; } }
    public bool dm { get { return _hw.gameMode == GameMode.DeathMatch; } }
    public bool zombisurive { get { return _hw.gameMode == GameMode.ZombieSurive; } }

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
            //foreach (object o in mb.GetCustomAttributes(false))
            //    if (o is RPC) UnityEngine.Debug.Log("Dublicate");
            networkView.RPC(new System.Diagnostics.StackFrame(1, true).GetMethod().Name, buffered ? RPCMode.OthersBuffered : RPCMode.Others, obs);
        }
    }


    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }

    public Dictionary<int, Player> players { get { return _Spawn.players; } }
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

