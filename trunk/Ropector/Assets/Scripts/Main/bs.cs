using System;
using System.Linq;
using UnityEngine;
[AddComponentMenu("Game/Base")]
public class bs : Base // contains static pointers to other objects
{
    internal const string webserver = "http://physxwars.ru/serv/";

    public static string TimeToSTr(float ts)
    {
        var t = TimeSpan.FromSeconds(ts);
        return t.Minutes + ":" + t.Seconds + ":" + t.Milliseconds;
    }
    public Vector2 pos2 { get { return new Vector2(pos.x, pos.y); } set { pos = new Vector3(value.x, value.y, pos.z); } }
    
    static Console _Console;
    public static Console Console { get { if (_Console == null) _Console = (Console)MonoBehaviour.FindObjectOfType(typeof(Console)); return _Console; } }
    
    static Game _Game;
    public static Game Game { get { if (_Game == null) _Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return _Game; } }
    static Cam _Cam;
    public static Cam Cam { get { if (_Cam == null) _Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return _Cam; } }
    static GameGui _GameGui;
    public static GameGui GameGui { get { if (_GameGui == null) _GameGui = (GameGui)MonoBehaviour.FindObjectOfType(typeof(GameGui)); return _GameGui; } }
    static Music _Music;
    public static Music Music { get { if (_Music == null) _Music = (Music)MonoBehaviour.FindObjectOfType(typeof(Music)); return _Music; } }
    static Player _Player;
    public static Player Player { get { if (_Player == null) _Player = (Player)MonoBehaviour.FindObjectOfType(typeof(Player)); return _Player; } }
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
    public virtual void Awake()
    {
        InitLoader();
    }
}