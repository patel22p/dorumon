using System;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
public class bs : Base
{
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
        enabled = false;
        _Game.networkItems.Add(this);
    }

    public Vector2 pos2 { get { return new Vector2(pos.x, pos.z); } set { pos = new Vector3(value.x, pos.y, value.y); } }


    public static Player _Player { get { return _Game._Player; } }
    public static Player _Player2 { get { return _Game._Player2; } }
    
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }

    static Cursor m_Cursor;
    public static Cursor _Cursor { get { if (m_Cursor == null) m_Cursor = (Cursor)MonoBehaviour.FindObjectOfType(typeof(Cursor)); return m_Cursor; } }

    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }

    public virtual void Awake()
    {
    }
}