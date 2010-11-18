using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using doru;

public static class Ext { public static T print<T>(this T ob) { MonoBehaviour.print(":"+ob); return ob; } }

public partial class Base2 : MonoBehaviour
{    
    public static void print(params object[] ob)
    {
        MethodBase m = new System.Diagnostics.StackFrame(1, true).GetMethod();
        StringBuilder sb = new StringBuilder();
        sb.Append(m.ReflectedType.Name + "." + m.Name);
        foreach (object o in ob)
            sb.Append(" " + o + ",");
        MonoBehaviour.print(sb.ToString());
    }

    public string nick { get { return _Loader.LocalUserV.nick; } set { _Loader.LocalUserV.nick = value; } }
    //public static Cam _Cam; 
    public T TakeRandom<T>(IList<T> t)
    {
        return t[UnityEngine.Random.Range(0, t.Count-1)];
    }

    static Irc __Irc;
    public static Irc _Irc { get { if (__Irc == null) __Irc = (Irc)MonoBehaviour.FindObjectOfType(typeof(Irc)); return __Irc; } }

    static GuiBlood __GuiBlood;
    public static GuiBlood _GuiBlood { get { if (__GuiBlood == null) __GuiBlood = (GuiBlood)MonoBehaviour.FindObjectOfType(typeof(GuiBlood)); return __GuiBlood;  }  }
    static Game __Game;
    public static Game _Game { get { if (__Game == null) __Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return __Game; } }
    static Cam __Cam;
    public static Cam _Cam { get { if (__Cam == null) __Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return __Cam; } }
    static Loader __Loader;
    public static Loader _Loader { get { if (__Loader == null) __Loader = (Loader)MonoBehaviour.FindObjectOfType(typeof(Loader)); return __Loader; } }
    static Menu __Menu;
    public static Menu _Menu { get { if (__Menu == null) __Menu = (Menu)MonoBehaviour.FindObjectOfType(typeof(Menu)); return __Menu; } }
    static Music __Music;
    public static Music _Music { get { if (__Music == null) __Music = (Music)MonoBehaviour.FindObjectOfType(typeof(Music)); return __Music; } }
    static Console __Console;
    public static Console _Console { get { if (__Console == null) __Console = (Console)MonoBehaviour.FindObjectOfType(typeof(Console)); return __Console; } }

    //public static Cam _Cam;
    //public static Loader _Loader;
    //public static Menu _Menu;
    //public static Music _Music;
    //public static Game _Game;
    //public static Console _Console;
    public string GenerateTable(string source)
    {
        string table = "";
        MatchCollection m = Regex.Matches(source, @"\w*\s*");
        for (int i = 0; i < m.Count - 1; i++)
            table += "{" + i + ",-" + m[i].Length + "}";
        return table;
    }

    static public IPlayer _localiplayer { get { return _Game._localiplayer; } set { _Game._localiplayer = value; } }
    static public Player _localPlayer { get { return _Game._localPlayer; } set { _Game._localPlayer = value; } }
    static public MapSetting mapSettings { get { return _Loader.mapSettings; } set { _Loader.mapSettings = value; } }
    public TimerA _TimerA { get { return _Loader._TimerA; } } 
    public static VK _vk;

    //static bool _lockCursor;
    //public static bool lockCursor { get { return _lockCursor; } set { _lockCursor = value; Screen.lockCursor = value; } }    
    public static bool lockCursor { get { return Screen.lockCursor; } set { Screen.lockCursor = value; } }
    public static Rect CenterRect(float w, float h)
    {
        
        Vector2 c = new Vector2(Screen.width, Screen.height); 
        Vector2 v = new Vector2(w * c.x, h * c.y);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }
    public virtual void OnSetOwner() { }

    

}
[Serializable]
public class MapSetting
{
    public List<GameMode> supportedModes = new List<GameMode>();
    public string mapName ="none";
    public string title = "none";
    public GameMode gameMode;
    public int fragLimit = 20;
    public string[] ipaddress;
    public int port=5300;
    public bool host = true;
    public int maxPlayers = 4;
    public float timeLimit=15;
    public bool TeamZombiSurvive { get { return gameMode == GameMode.TeamZombieSurvive; } }
    public bool TDM { get { return gameMode == GameMode.TeamDeathMatch; } }
    public bool DM { get { return gameMode == GameMode.DeathMatch; } }
    public bool ZombiSurvive { get { return gameMode == GameMode.ZombieSurive; } }
    public bool Team { get { return TeamZombiSurvive || TDM; } }
    public bool zombi { get { return ZombiSurvive || TeamZombiSurvive; } }
}



