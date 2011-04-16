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

    public bool DisplayQuestDialog = false;
    private Quest DialogToShow;

    // List of quests
    public List<Quest> QuestList
    {
        get
        {
            if (questList == null)
                questList = new List<Quest>();
            return questList;
        } 
        set
        {
            if (questList == null)
                questList = new List<Quest>();
            questList = value;
        }
    }
    private List<Quest> questList;
    public Texture Background;
    public Rect WindowSize;
    public GUIStyle FontStyle;

    public bool MainQM { get; set; }
    
    // Init
    public void Start()
    {
        // Cheap hack
        QuestManager[] qm = GameObject.FindObjectsOfType(typeof(QuestManager)) as QuestManager[];
        foreach (QuestManager _qm in qm)
        {
            if (_qm.gameObject != this.gameObject)
            {
                if (_qm.QuestList.Count > this.QuestList.Count)
                {
                    this.QuestList = _qm.QuestList;
                    Destroy(_qm.gameObject);
                }
            }
        }
        DontDestroyOnLoad(gameObject);

        WindowSize = new Rect(0, Screen.height - Background.height, Background.width, Background.height);
    }

    public void Update()
    {
        if (DisplayQuestDialog)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) DisplayQuestDialog = false;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 5, 100, 40), "Quests: " + QuestList.Count);
        int i = 0;
        foreach (Quest q in QuestList)
        {
            GUI.Label(new Rect(5, QuestList.Count * 45, 200, 40), QuestList[i].QuestName);
            i++;
        }

        if (DisplayQuestDialog)
        {
            GUI.Label(WindowSize, Background);
            GUI.Label(new Rect(60, Screen.height - Background.height + 60, Background.width - 120, Background.height - 120), DialogToShow.QuestStepDescription[DialogToShow.QuestStep - 1], FontStyle);
        }
    }

    public void NewQuest(Quest q)
    {
        QuestList.Add(q);
        DialogToShow = q;
        DisplayQuestDialog = true;
    }

    public void NextStep(string questName, string description)
    {

        foreach (Quest q in QuestList)
        {
            if (q.QuestName == questName)
            {
                q.AddStep(description);
                DialogToShow = q;
                DisplayQuestDialog = true;
            }
        }
    }

    public void EndQuest(string questName, string description)
    {
        foreach (Quest q in QuestList)
        {
            if (q.QuestName == questName)
            {
                q.AddStep(description);
                DialogToShow = q;
                DisplayQuestDialog = true;
                q.Done = true;
            }
        }
    }

    public void ActivateQ(string questName)
    {
        QuestEvent[] qm = GameObject.FindObjectsOfType(typeof(QuestEvent)) as QuestEvent[];
        foreach (QuestEvent qw in qm)
        {
            if (qw.QuestName == questName)
            {
                if (qw.QuestStep == 0)
                {
                    qw.EventActive = true;
                    break;
                }
            }
        }
    }
}