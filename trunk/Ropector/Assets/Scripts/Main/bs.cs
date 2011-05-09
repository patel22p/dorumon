using System;
using System.Linq;
using UnityEngine;
using doru;
using System.Collections.Generic;
[AddComponentMenu("Game/Base")]
public class bs : Base 
{
    public bool editor { get { return Application.loadedLevel == (int)Scene.mapEditor; } }
    //public override string ToString()
    //{
    //    if (networkView != null)
    //        return "id:" + networkView.owner.GetHashCode() + ", nv:" + networkView.viewID.GetHashCode();
    //    else
    //        return base.ToString();
    //}
    
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

    public void LocalConnect()
    {
        var ips = new List<string>();
        var ip = Network.player.ipAddress;
        ip = ip.Substring(0, ip.LastIndexOf('.')) + ".";
        Debug.Log(ip);
        for (int i = 0; i < 255; i++)
            ips.Add(ip + i);
        Network.Connect(ips.ToArray(), 5300);
    }
    static EGameGUI m_GameGUI;
    public static EGameGUI _EGameGUI { get { if (m_GameGUI == null) m_GameGUI = (EGameGUI)MonoBehaviour.FindObjectOfType(typeof(EGameGUI)); return m_GameGUI; } }
    static EGame m_EGame;
    public static EGame _EGame { get { if (m_EGame == null) m_EGame = (EGame)MonoBehaviour.FindObjectOfType(typeof(EGame)); return m_EGame; } }

    public Vector2 pos2 { get { return new Vector2(transform.position.x, transform.position.y); } set { transform.position = new Vector3(value.x, value.y, transform.position.z); } }
    //public static TimerA _Timer { get { return _Loader.timer; } }
    static MenuGui m_MyGui;
    public static MenuGui _MenuGui { get { if (m_MyGui == null) m_MyGui = (MenuGui)MonoBehaviour.FindObjectOfType(typeof(MenuGui)); return m_MyGui; } }
    static Popup m_Popup;
    public static Popup _Popup { get { if (m_Popup == null) m_Popup = (Popup)MonoBehaviour.FindObjectOfType(typeof(Popup)); return m_Popup; } }
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
    private static void InitLoader()
    {
        if (__Loader == null)
            __Loader = (Loader)MonoBehaviour.FindObjectsOfType(typeof(Loader)).FirstOrDefault();
        if (__Loader == null)
            __Loader = ((GameObject)Instantiate(Resources.Load("loader", typeof(GameObject)))).GetComponent<Loader>();
    }   
    public static string TimeToSTr(float ts)
    {
        var t = TimeSpan.FromSeconds(ts);
        return t.Minutes + ":" + t.Seconds + ":" + t.Milliseconds;
    }

}