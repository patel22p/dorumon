using UnityEngine;
using System.Collections;
using System;

public class Bs : Base {
    public const int port = 5300;
    public bool IsMine { get { return Network.peerType == NetworkPeerType.Disconnected || networkView.isMine; } }
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

    public float lrotx { get { return lrot.eulerAngles.x; } set { var e = lrot.eulerAngles; e.x = value; lrot = Quaternion.Euler(e); } }
    public float lroty { get { return lrot.eulerAngles.y; } set { var e = lrot.eulerAngles; e.y = value; lrot = Quaternion.Euler(e); } }
    public float lrotz { get { return lrot.eulerAngles.z; } set { var e = lrot.eulerAngles; e.z = value; lrot = Quaternion.Euler(e); } }
    public Quaternion lrot { get { return tr.localRotation; } set { tr.localRotation = value; } }

    //static Transform m_Fx;
    //public static Transform _Fx { get { if (m_Fx == null) m_Fx = (Transform)FindObjectOfType(; return m_Fx; } }
    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static GameGui m_GameGui;
    public static GameGui _GameGui { get { if (m_GameGui == null) m_GameGui = (GameGui)MonoBehaviour.FindObjectOfType(typeof(GameGui)); return m_GameGui; } }
    static Player m_Player;
    public static Player _Player { get { if (m_Player == null) m_Player = (Player)MonoBehaviour.FindObjectOfType(typeof(Player)); return m_Player; } }

    public void Active(bool value)
    {
        foreach (Transform a in this.GetComponentsInChildren<Transform>())
            a.gameObject.active = false;
        this.gameObject.active = false;
    }
    public void SetLayer(int from,int to)
    {
        foreach (Transform a in this.GetComponentsInChildren<Transform>())
            if (a.gameObject.layer == from)
                a.gameObject.layer = to;
        if (gameObject.layer == from)
            this.gameObject.layer = to;
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

    public void CallRPC<T>(Action<T> n, RPCMode m, T p)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p);
        else
            n(p);
    }

    public void CallRPC<T,T2>(Action<T,T2> n, RPCMode m, T p,T2 p2)
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            networkView.RPC(n.Method.Name, m, p, p2);
        else
            n(p,p2);
    }

    protected Vector3 clampAngles(Vector3 a)
    {
        if (a.x > 180) a.x -= 360;
        if (a.y > 180) a.y -= 360;
        if (a.z > 180) a.z -= 360;

        return a;
    }
}
