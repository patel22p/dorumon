using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public delegate void Action<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
public class Bs : Base
{

    public const int port = 80;
    public bool IsMine
    {
        get
        {
            if (transform.root.tag == "Enemy") return false;
            return Network.peerType == NetworkPeerType.Disconnected || networkView.isMine;
        }
    }
    
    public static bool Offline { get { return Network.peerType == NetworkPeerType.Disconnected; } }
    public Transform tr { get { return transform; } }
    public Vector3 pos { get { return tr.position; } set { tr.position = value; } }
    public Vector3 position { get { return tr.position; } set { tr.position = value; } }
    public Transform parent { get { return tr.parent; } set { tr.parent = value; } }
    public Vector3 scale { get { return tr.localScale; } set { tr.localScale = value; } }
    public float posx { get { return pos.x; } set { var v = pos; v.x = value; pos = v; } }
    public float posy { get { return pos.y; } set { var v = pos; v.y = value; pos = v; } }
    public float posz { get { return pos.z; } set { var v = pos; v.z = value; pos = v; } }
    public float rotx { get { return rot.eulerAngles.x; } set { var e = rot.eulerAngles; e.x = value; rot = Quaternion.Euler(e); } }
    public float roty { get { return rot.eulerAngles.y; } set { var e = rot.eulerAngles; e.y = value; rot = Quaternion.Euler(e); } }
    public float rotz { get { return rot.eulerAngles.z; } set { var e = rot.eulerAngles; e.z = value; rot = Quaternion.Euler(e); } }
    public Quaternion rot { get { return tr.rotation; } set { tr.rotation = value; } }
    public Vector3 rote { get { return tr.rotation.eulerAngles; } set { tr.rotation = Quaternion.Euler(value); } }

    public float lrotx { get { return lrot.eulerAngles.x; } set { var e = lrot.eulerAngles; e.x = value; lrot = Quaternion.Euler(e); } }
    public float lroty { get { return lrot.eulerAngles.y; } set { var e = lrot.eulerAngles; e.y = value; lrot = Quaternion.Euler(e); } }
    public float lrotz { get { return lrot.eulerAngles.z; } set { var e = lrot.eulerAngles; e.z = value; lrot = Quaternion.Euler(e); } }
    public Quaternion lrot { get { return tr.localRotation; } set { tr.localRotation = value; } }

    //static Transform m_Fx;
    //public static Transform _Fx { get { if (m_Fx == null) m_Fx = (Transform)FindObjectOfType(; return m_Fx; } }
    public static ObsCamera _ObsCamera { get { return _Game.Obs; } }
    static TeamSelectGui m_TeamSelectGui;
    public static TeamSelectGui _TeamSelectGui { get { if (m_TeamSelectGui == null) m_TeamSelectGui = (TeamSelectGui)MonoBehaviour.FindObjectOfType(typeof(TeamSelectGui)); return m_TeamSelectGui; } }
    static Hud m_Hud;
    public static Hud _Hud { get { if (m_Hud == null) m_Hud = (Hud)MonoBehaviour.FindObjectOfType(typeof(Hud)); return m_Hud; } }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static GameGui m_GameGui;
    public static GameGui _GameGui { get { if (m_GameGui == null) m_GameGui = (GameGui)MonoBehaviour.FindObjectOfType(typeof(GameGui)); return m_GameGui; } }
    static Loader m_Loader;
    public static Loader _Loader { get { if (m_Loader == null) m_Loader = (Loader)MonoBehaviour.FindObjectOfType(typeof(Loader)); return m_Loader; } }
    static LoaderGui m_LoaderGui;
    public static LoaderGui _LoaderGui { get { if (m_LoaderGui == null) m_LoaderGui = (LoaderGui)MonoBehaviour.FindObjectOfType(typeof(LoaderGui)); return m_LoaderGui; } }
    static Player m_Player;
    public static Player _Player { get { return _Game._Player; } }
    public static Vector3 GetMove()
    {
        if (!Screen.lockCursor) return Vector3.zero;
        Vector3 v = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) v += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) v += Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) v += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) v += Vector3.right;
        return v.normalized;
    }
    public static Vector3 GetMouse()
    {
        return Screen.lockCursor ? new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) : Vector3.zero;
    }
    

    public void Active(bool value)
    {
        foreach (Transform a in this.GetComponentsInChildren<Transform>())
            a.gameObject.active = false;
        this.gameObject.active = false;
    }

    public virtual void Awake()
    {
        //tr = transform;
    }
    protected float clampAngle(float a)
    {
        if (a > 180) return a - 360;
        return a;
    }
    public void CallRPC(Action n, RPCMode m)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m);
        else
            n();
    }
    public void CallRPC(Action n, NetworkPlayer m)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m);
        else
            n();
    }
    public void CallRPC<T>(Action<T> n, RPCMode m, T p)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p);
        else
            n(p);
    }
    public void CallRPC<T>(Action<T> n, NetworkPlayer m, T p)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p);
        else
            n(p);
    }
    public void CallRPC<T, T2>(Action<T, T2> n, RPCMode m, T p, T2 p2)
    {
        if (Network.peerType != NetworkPeerType.Disconnected) {
            networkView.RPC(n.Method.Name, m, p, p2);
        }
        else
            n(p, p2);
    }
    public void CallRPC<T, T2>(Action<T, T2> n, NetworkPlayer m, T p, T2 p2)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2);
        else
            n(p, p2);
    }
    public void CallRPC<T, T2, T3>(Action<T, T2, T3> n, RPCMode m, T p, T2 p2, T3 p3)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3);
        else
            n(p, p2, p3);
    }
    public void CallRPC<T, T2, T3>(Action<T, T2, T3> n, NetworkPlayer m, T p, T2 p2, T3 p3)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3);
        else
            n(p, p2, p3);
    }
    public void CallRPC<T, T2, T3, T4>(Action<T, T2, T3, T4> n, RPCMode m, T p, T2 p2, T3 p3, T4 p4)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3, p4);
        else
            n(p, p2, p3, p4);
    }
    public void CallRPC<T, T2, T3, T4>(Action<T, T2, T3, T4> n, NetworkPlayer m, T p, T2 p2, T3 p3, T4 p4)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3, p4);
        else
            n(p, p2, p3, p4);
    }
    public void CallRPC<T, T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> n, RPCMode m, T p, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3, p4, p5);
        else
            n(p, p2, p3, p4, p5);
    }
    public void CallRPC<T, T2, T3, T4, T5, T6>(Action<T, T2, T3, T4, T5, T6> n, RPCMode m, T p, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2, p3, p4, p5, p6);
        else
            n(p, p2, p3, p4, p5, p6);
    }

    protected Vector3 clampAngles(Vector3 a)
    {
        if (a.x > 180) a.x -= 360;
        if (a.y > 180) a.y -= 360;
        if (a.z > 180) a.z -= 360;

        return a;
    }
    public static string CreateTable(string source) //create table parse table
    {
        string table = "";
        MatchCollection m = Regex.Matches(source, @"\w*\s*");
        for (int i = 0; i < m.Count - 1; i++)
            table += "{" + i + ",-" + m[i].Length + "}";
        return table;
    }
    public static bool DebugKey(KeyCode keyCode)
    {
        if (!Application.isWebPlayer && Input.GetKeyDown(keyCode))
            return true;
        return false;

    }
    public string RemoveFirstLine(string s)
    {
        return string.Join("\r\n", s.Split("\r\n").Skip(1).ToArray());
    }
    public float Temp = 1;
    public float Temp2 = 1;
    public bool isEditor
    {
        get
        {
            return
            Application.isEditor;
        }
    }

}
