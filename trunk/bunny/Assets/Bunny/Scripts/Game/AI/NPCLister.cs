using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCLister : MonoBehaviour
{
    public ArrayList CharacterList { get { return characterList; } set { characterList = value; } }
    private ArrayList characterList = new ArrayList();

    public List<Collectable> Berries = new List<Collectable>();

    private static NPCLister _instance;
    public static NPCLister Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = (NPCLister)GameObject.FindObjectOfType(typeof(NPCLister));
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = "GameData";
                    _instance = container.AddComponent(typeof(NPCLister)) as NPCLister;
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }


}