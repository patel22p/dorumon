using UnityEngine;
using System.Collections;
using System.Linq;

public class bs2 : Base
{
    public virtual void Awake()
    {
        InitLoader();
    }
    static EGameGUI m_GameGUI;
    public static EGameGUI _GameGUI { get { if (m_GameGUI == null) m_GameGUI = (EGameGUI)MonoBehaviour.FindObjectOfType(typeof(EGameGUI)); return m_GameGUI; } }
    static EGame m_EGame;
    public static EGame _EGame { get { if (m_EGame == null) m_EGame = (EGame)MonoBehaviour.FindObjectOfType(typeof(EGame)); return m_EGame; } }
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
}
