using UnityEngine;
using System.Collections;

public class bs2 : bs
{
    static GameGUI m_GameGUI;
    public static GameGUI _GameGUI { get { if (m_GameGUI == null) m_GameGUI = (GameGUI)MonoBehaviour.FindObjectOfType(typeof(GameGUI)); return m_GameGUI; } }	
}
