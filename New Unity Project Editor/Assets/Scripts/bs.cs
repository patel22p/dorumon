using System;
using System.Linq;
using UnityEngine;
using doru;
using System.Collections.Generic;
[AddComponentMenu("Game/Base")]
public class bs : Base 
{
    public override string ToString()
    {
        return "id:" + networkView.owner.GetHashCode() + ", nv:" + networkView.viewID.GetHashCode();
    }
    public virtual void Awake()
    {
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
   
    public Vector2 pos2 { get { return new Vector2(transform.position.x, transform.position.y); } set { transform.position = new Vector3(value.x, value.y, transform.position.z); } }
    //public static TimerA _Timer { get { return _Loader.timer; } }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }

    public virtual void AlwaysUpdate() { }
    
    public static string TimeToSTr(float ts)
    {
        var t = TimeSpan.FromSeconds(ts);
        return t.Minutes + ":" + t.Seconds + ":" + t.Milliseconds;
    }

}