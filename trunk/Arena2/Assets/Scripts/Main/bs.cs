using System;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
using doru;
enum NetworkGroup { Player, Zombie }
public enum hostDebug { None, singl, wait, join }

public class bs : Base
{
    public virtual void Awake()
    {        
        InitLoader();
    }
    public bool NotInstance()
    {
        if (!name.Contains("(Clone)")) { Destroy(this.gameObject); return true; }
        return false;
    }

    public TimerA _Timer { get { return _Loader.timer; } }

    private static void InitLoader()
    {
        if (__Loader == null)
            __Loader = (Loader)MonoBehaviour.FindObjectsOfType(typeof(Loader)).FirstOrDefault();
        if (__Loader == null)
            __Loader = ((GameObject)Instantiate(Resources.Load("loader", typeof(GameObject)))).GetComponent<Loader>();
    }

    public void FindTransform(ref GameObject g, string name) 
    {
        if(g==null)
        {
            g = GameObject.Find(name);
            if (g == null)
            {
                g = transform.GetTransforms().FirstOrDefault(a => a.name == name).gameObject;
            }
        }
    }
    public void AddToNetwork()
    {
        if (Network.peerType == NetworkPeerType.Disconnected || Network.peerType == NetworkPeerType.Connecting)
        {
            enabled = false;
            _Game.networkItems.Add(this);
        }
    }
    public static bool DebugKey(KeyCode k)
    {
        return Input.GetKeyDown(k);
    }
    public Vector2 pos2 { get { return new Vector2(pos.x, pos.z); } set { pos = new Vector3(value.x, pos.y, value.y); } }


    public static Player _PlayerOwn { get { return _Game._PlayerOwn; } }
    public static Player _PlayerOther { get { return _Game._PlayerOther; } }

    static MenuGui m_MenuGui;
    public static MenuGui _MenuGui { get { if (m_MenuGui == null) m_MenuGui = (MenuGui)MonoBehaviour.FindObjectOfType(typeof(MenuGui)); return m_MenuGui; } }

    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }

    static Cursor m_Cursor;
    public static Cursor _Cursor { get { if (m_Cursor == null) m_Cursor = (Cursor)MonoBehaviour.FindObjectOfType(typeof(Cursor)); return m_Cursor; } }

    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }

    static Loader __Loader;
    public static Loader _Loader
    {
        get
        {
            InitLoader();
            return __Loader;
        }
    }

    
}