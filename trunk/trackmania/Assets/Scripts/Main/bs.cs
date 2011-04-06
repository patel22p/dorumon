using UnityEngine;
using System.Collections;

public class bs : Base {

    public virtual void Awake()
    {

    }
    static Game m_Game;
    public static Game _Game { get { if (m_Game == null) m_Game = (Game)MonoBehaviour.FindObjectOfType(typeof(Game)); return m_Game; } }

    static Player m_Player;
    public static Player _Player { get { if (m_Player == null) m_Player = (Player)MonoBehaviour.FindObjectOfType(typeof(Player)); return m_Player; } }
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
