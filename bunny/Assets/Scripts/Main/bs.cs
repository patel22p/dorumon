using UnityEngine;
using System.Collections;

public class bs : Base {

    
    static MyNodes m_MyNodes;
    public static MyNodes _MyNodes { get { if (m_MyNodes == null) m_MyNodes = (MyNodes)MonoBehaviour.FindObjectOfType(typeof(MyNodes)); return m_MyNodes; } }

    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }
    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }
    static Player m_Player;
    public static Player _Player { get { if (m_Player == null) m_Player = (Player)MonoBehaviour.FindObjectOfType(typeof(Player)); return m_Player; } }

    
	void Start () {
	
	}
	
	void Update () {
	
	}
}
