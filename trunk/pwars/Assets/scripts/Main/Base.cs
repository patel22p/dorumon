using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class bs : Base2
{
    public static NetworkPlayer? sendto;
    internal const string webserver = "http://physxwars.ru/serv/";
    //internal const string webserver = "http://192.168.30.113/serv/";
    internal const int maxConId= 100;
    internal int OwnerID = -1;
    internal bool isOwner { get { return OwnerID == Network.player.GetHashCode(); } }
    bool Defenabled = true;
    protected virtual void OnServerInitialized() { Enable(); }
    protected virtual void OnConnectedToServer() { Enable(); }
    protected virtual void Enable() { if (networkView != null && Defenabled) enabled = true; }
    public void ShowPopup(string s)
    {
        Debug.Log("PopUp: " + s);
        if (_PopUpWindow.enabled)
            _PopUpWindow.Text += "\r\n" + s;
        else
            _PopUpWindow.Text = s;
        _PopUpWindow.ShowDontHide(_Loader);                
    }
    public virtual void Awake()
    {
        if (name == "pistol") Debug.Log("Awake" + name);         
        if (networkView != null && enabled)
        {
            //name += "+" + Regex.Match(networkView.viewID.ToString(), @"\d+").Value;
            if (Network.peerType == NetworkPeerType.Disconnected)
            {                
                enabled = false;
            }
        }
    }
    public override void InitValues()
    {        
        Defenabled = enabled;
        base.InitValues();
    }
    public virtual void OnLevelLoading(){}
    public virtual void OnPlayerConnectedBase(NetworkPlayer np) { }
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
    public void PlaySound(AudioClip au)
    {
        PlaySound(au, 1);
    }
    public void PlaySound(AudioClip au, float volume)
    {
        transform.GetComponentInParrent<AudioSource>().PlayOneShot(au, volume);
    }
    public void PlayRandSound(AudioClip[] au) { PlayRandSound(au, 1); }
    public void PlayRandSound(AudioClip[] au,float volume)
    {        
        if (!transform.root.audio.isPlaying)
            transform.GetComponentInParrent<AudioSource>().audio.PlayOneShot((AudioClip)au[UnityEngine.Random.Range(0, au.Length)], volume);
    }
    public void LocalHide() { Show(false); }
//    public int GetPing(HostData hd,Action callback)
//    {
//        var dt = DateTime.Now;
//        IPEndPoint edp = null;
//        UdpClient c = new UdpClient();
//        c.Connect(hd.ip[0], 5300);
//        var bts = Hex(@"01 00 00 00 00 09
//1B 43 54 00 FF FF 00 FE-FE FE FE FD FD FD FD 12
//34 56 78");
//        c.Send(bts, bts.Length);
//        c.Receive(ref edp);
//        _TimerA.AddMethod(callback);
//    }
//    public static byte[] Hex(string s)
//    {
//        MatchCollection ms = Regex.Matches(s, "[0-9a-fA-F]{2,2}");
//        byte[] _bytes = new byte[ms.Count];
//        for (int i = 0; i < ms.Count; i++)
//            _bytes[i] = byte.Parse(ms[i].Value, System.Globalization.NumberStyles.HexNumber);
//        return _bytes;
//    }
    public void LocalShow() { Show(true); }
    public void RPCShow(bool v) { CallRPC("Show",v); }
    [RPC]
    public void Show(bool value)
    {
        Show(this.gameObject, value);
        foreach (bs r in this.GetComponentsInChildren<bs>())
        {
            r.enabled = value;
            r.onShow(value);
        }
    }
    public static void Show(GameObject g, bool value)
    {
        foreach (var rigidbody in g.GetComponentsInChildren<Rigidbody>())
        {
            //rigidbody.isKinematic = !value;
            rigidbody.detectCollisions = value;
            rigidbody.useGravity = value;                        
        }
        foreach (var t in g.GetComponentsInChildren<Transform>())
            t.gameObject.layer = value ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("Ignore Raycast");
    
        foreach (var r in g.GetComponentsInChildren<Renderer>())
            r.enabled = value;
        foreach (var r in g.GetComponentsInChildren<AudioListener>())
            r.enabled = value;
        foreach (var r in g.GetComponentsInChildren<Camera>())
            r.enabled = value;

        //foreach (var r in g.GetComponentsInChildren<AudioSource>())
        //{
        //    r.enabled = value;
        //    if (!value)
        //        r.Stop();
        //    else
        //        if (r.playOnAwake) r.Play();
        //}
        
    }
    public virtual void onShow(bool e)
    {
    }
    public void CallRPC(string name, params object[] obs)
    {        
        if (new StackTrace().FrameCount > 30) throw new StackOverflowException();
        _Loader.rpcCount++;
        if (sendto == null)
        {
            networkView.RPC(name, RPCMode.Others, obs);
            try
            {
                GetType().GetMethod(name).Invoke(this, obs); // public
            }
            catch (TargetParameterCountException) { this.GetType().GetMethod(name).Invoke(this, obs.Concat(new object[] { new NetworkMessageInfo() }).ToArray()); }
        }
        else
            networkView.RPC(name, sendto.Value, obs);
        
    }
    public Transform root { get { return this.transform.root; } }
    public static string GetDescr(GameMode g)
    {
        switch (g)
        {
            case GameMode.ZombieSurvival:
            case GameMode.CustomZombieSurvival:
                return "Zombie survival. \r\nkill the maximum number of zombies.";
            case GameMode.DeathMatch:
                return "DeathMatch. \r\nkill the maximum number of players.";
            case GameMode.TeamDeathMatch:
                return "Team battle. \r\n kill the maximum number of players.";
            default:
                return "";
        }
    }
    public static bool build { get { return _Loader.build; } }
    public static bool debug { get { return !_Loader.build; } }
    public static Player[] players { get { return _Game.players; } }
    public static bool isWebPlayer { get { return Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer; } }
    public static Level _Level { get { return _Loader._Level; } set { _Loader._Level = value; } }
    public static bool DebugKey(KeyCode key)
    {
        
        if (_Loader.completeBuild)
            return false;
        var a = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(key);
        if (a)
            print("Debug Key" + key);
        return a;

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
        return RayCast(collmask, len).collider != null;
    }
    public static RaycastHit RayCast(LayerMask msk, float len)
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
        if (t != null)
            for (int i = 0; i < t.childCount; i++)
                yield return t.GetChild(i);
    }
    public static float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }
    public static void PlayTextAnimation(TextMesh text, string s)
    {
        text.text = s;
        text.animation.Play();
    }
    public bool Antigrav { get { return Physics.gravity != _Game.gravity; } }
}
