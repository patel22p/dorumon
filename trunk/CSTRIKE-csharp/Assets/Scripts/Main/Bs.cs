
using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public delegate void Action<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
public class Bs : Base
{
    public static string version = "w";
    public bool Android { get { return Application.isEditor ? _Loader.Android : Application.platform == RuntimePlatform.Android; } }
    public bool lockCursor
    {
        get { return Android ? false : Screen.lockCursor; }
        set { Screen.lockCursor = value; }
    }
    public virtual void Awake()
    {
    }    
    
    public float temp = 1;
    public float temp1 = 1;        
    //public const int port = 5300;    
    public bool IsMine
    {
        get
        {            
            return PhotonNetwork.connectionState == ConnectionState.Disconnected || photonView.isMine;
        }
    }
    public static Bomb _Bomb;
    public static int NetworkPlayerID { get { return Offline ? 1 : PhotonNetwork.player.ID; } }
    public static bool Offline { get { return PhotonNetwork.room == null; } }
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
    //static LevelEditor m_LevelEditor;
    //public static LevelEditor _LevelEditor { get { if (m_LevelEditor == null) m_LevelEditor = (LevelEditor)MonoBehaviour.FindObjectOfType(typeof(LevelEditor)); return m_LevelEditor; } }



    static TeamSelectGui m_TeamSelectGui;
    public static TeamSelectGui _TeamSelectGui { get { if (m_TeamSelectGui == null) m_TeamSelectGui = (TeamSelectGui)MonoBehaviour.FindObjectOfType(typeof(TeamSelectGui)); return m_TeamSelectGui; } }
    static Hud m_Hud;
    public static Hud _Hud { get { if (m_Hud == null) m_Hud = (Hud)MonoBehaviour.FindObjectOfType(typeof(Hud)); return m_Hud; } }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static Loader m_Loader;
    public static Loader _Loader { get { if (m_Loader == null) m_Loader = (Loader)MonoBehaviour.FindObjectOfType(typeof(Loader)); return m_Loader; } }
    
    static Player m_Player;
    public static Player _Player;

    public Vector3 GetMove()
    {
        return _Game.GetMove();
    }

    public Vector3 GetMouse()
    {
        return _Game.GetMouse();
    }



    public void Active(bool value)
    {
        foreach (Transform a in this.GetComponentsInChildren<Transform>())
            a.gameObject.active = false;
        this.gameObject.active = false;
    }

    
    protected float clampAngle(float a)
    {
        if (a > 180) return a - 360;
        return a;
    }
    public void CallRPC(Action n, PhotonTargets m)
    {        
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m);
        else
            n();
    }
    public void CallRPC(Action n, PhotonPlayer m)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m);
        else
            n();
    }
    public void CallRPC<T>(Action<T> n, PhotonTargets m, T p)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p);
        else
            n(p);
    }
    public void CallRPC<T>(Action<T> n, PhotonPlayer m, T p)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p);
        else
            n(p);
    }
    public void CallRPC<T, T2>(Action<T, T2> n, PhotonTargets m, T p, T2 p2)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected) {
            photonView.RPC(n.Method.Name, m, p, p2);
        }
        else
            n(p, p2);
    }
    public void CallRPC<T, T2>(Action<T, T2> n, PhotonPlayer m, T p, T2 p2)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2);
        else
            n(p, p2);
    }
    public void CallRPC<T, T2, T3>(Action<T, T2, T3> n, PhotonTargets m, T p, T2 p2, T3 p3)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3);
        else
            n(p, p2, p3);
    }
    public void CallRPC<T, T2, T3>(Action<T, T2, T3> n, PhotonPlayer m, T p, T2 p2, T3 p3)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3);
        else
            n(p, p2, p3);
    }
    public void CallRPC<T, T2, T3, T4>(Action<T, T2, T3, T4> n, PhotonTargets m, T p, T2 p2, T3 p3, T4 p4)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3, p4);
        else
            n(p, p2, p3, p4);
    }
    public void CallRPC<T, T2, T3, T4>(Action<T, T2, T3, T4> n, PhotonPlayer m, T p, T2 p2, T3 p3, T4 p4)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3, p4);
        else
            n(p, p2, p3, p4);
    }
    public void CallRPC<T, T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> n, PhotonTargets m, T p, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3, p4, p5);
        else
            n(p, p2, p3, p4, p5);
    }
    public void CallRPC<T, T2, T3, T4, T5, T6>(Action<T, T2, T3, T4, T5, T6> n, PhotonTargets m, T p, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            photonView.RPC(n.Method.Name, m, p, p2, p3, p4, p5, p6);
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
    
    public bool isEditor
    {
        get
        {
            return
            Application.isEditor;
        }
    }
    public static Vector3 ZeroY(Vector3 v)
    {
        v.y = 0;
        return v;
    }

    public static Vector3 ZeroYNorm(Vector3 v)
    {
        v.y = 0;
        return v.normalized;
    }
}
