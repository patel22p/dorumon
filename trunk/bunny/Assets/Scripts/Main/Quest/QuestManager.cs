using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance
    {
        get
        {
            if (!_instance)
            {
                // Search for questmanager
                _instance = GameObject.FindObjectOfType(typeof(QuestManager)) as QuestManager;
                if (!_instance)
                {
                    // Does not exists, create one.
                    GameObject _container = new GameObject();
                    _instance = (QuestManager)_container.AddComponent(typeof(QuestManager));
                }
            }
            return _instance;
        }
    }
    private static QuestManager _instance;
}