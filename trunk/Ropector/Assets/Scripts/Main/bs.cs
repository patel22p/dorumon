using System;
using System.Linq;
using UnityEngine;
using doru;
[AddComponentMenu("Game/Base")]
public class bs : Base 
{
    public override string ToString()
    {
        return "id:" + networkView.owner.GetHashCode() + ", nv:" + networkView.viewID.GetHashCode();
    }
    private static void InitLoader()
    {
        if (__Loader == null)
            __Loader = (Loader)MonoBehaviour.FindObjectsOfType(typeof(Loader)).FirstOrDefault();
        if (__Loader == null)
            __Loader = ((GameObject)Instantiate(Resources.Load("loader", typeof(GameObject)))).GetComponent<Loader>();
    }
    public virtual void Awake()
    {
        InitLoader();
    }
    public void AddToNetwork()
    {
        if (Network.peerType == NetworkPeerType.Disconnected || Network.peerType == NetworkPeerType.Connecting)
        {
            enabled = false;
            _Game.networkItems.Add(this);
        }
    }

    
    public Vector2 pos2 { get { return new Vector2(transform.position.x, transform.position.y); } set { transform.position = new Vector3(value.x, value.y, transform.position.z); } }
    //public static TimerA _Timer { get { return _Loader.timer; } }
    static MyGui m_MyGui;
    public static MyGui _MyGui { get { if (m_MyGui == null) m_MyGui = (MyGui)MonoBehaviour.FindObjectOfType(typeof(MyGui)); return m_MyGui; } }
    static Menu m_Menu;
    public static Menu _Menu { get { if (m_Menu == null) m_Menu = (Menu)MonoBehaviour.FindObjectOfType(typeof(Menu)); return m_Menu; } }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }
    static GameGui m_GameGui;
    public static GameGui _GameGui { get { if (m_GameGui == null) m_GameGui = (GameGui)MonoBehaviour.FindObjectOfType(typeof(GameGui)); return m_GameGui; } }
    static Music m_Music;
    public static Music _Music { get { if (m_Music == null) m_Music = (Music)MonoBehaviour.FindObjectOfType(typeof(Music)); return m_Music; } }

    public static Player _Player { get { return _Game._Player; } }
    public virtual void AlwaysUpdate() { }
    static Loader __Loader;
    public static Loader _Loader
    {
        get
        {
            InitLoader();
            return __Loader;
        }
    }
    
    public static string TimeToSTr(float ts)
    {
        var t = TimeSpan.FromSeconds(ts);
        return t.Minutes + ":" + t.Seconds + ":" + t.Milliseconds;
    }

}