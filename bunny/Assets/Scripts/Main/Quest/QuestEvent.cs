using UnityEngine;
using System.Collections;

public enum QuestEventType { start, trigger, end,target, collectable }
public class QuestEvent : MonoBehaviour
{
    public bool EventDone = false; // The event step, if done. It will be ignored.
    public bool EventActive = false; // Is this event active
    private bool initialized = false; // initializing procedure, dunno what should be done here.. hmm?
    public string QuestName = "QuestName";
    public int QuestStep = 0;
    public string QuestDescription = "";
    public QuestEventType type = QuestEventType.trigger;
    public string PlayerTag = "Player";
    public GameObject player { get; set; }
    public string WhenDoneActiveOtherQuest = "";

    public bool Init()
    {
        player = GameObject.FindWithTag("Player");
        if (!player)
        {
            Debug.LogError("Could not find player, tagged correctly?");
            DestroyImmediate(gameObject);
        }
        switch (type)
        {
            case QuestEventType.trigger:
                if (!collider.isTrigger) collider.isTrigger = true;
                break;
        }
        return true;
    }

    public void Update()
    {
        if(!initialized) initialized = Init();

        if (EventActive && type == QuestEventType.collectable)
            if (!collider.isTrigger) collider.isTrigger = true;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (EventActive)
        {
            if (col.gameObject == player)
            {
                switch (type)
                {
                    case QuestEventType.start:
                        QuestManager.Instance.NewQuest(new Quest(QuestName, QuestDescription));
                        EventActive = false;
                        Destroy(this.gameObject);
                        break;
                    case QuestEventType.trigger:
                        QuestManager.Instance.NextStep(QuestName, QuestDescription);
                        EventActive = false;
                        Destroy(this.gameObject);
                        break;
                    case QuestEventType.end:
                        QuestManager.Instance.EndQuest(QuestName, QuestDescription);
                        EventActive = false;
                        if (WhenDoneActiveOtherQuest != "")
                        {
                            QuestManager.Instance.ActivateQ(WhenDoneActiveOtherQuest);
                        }
                        Destroy(this.gameObject);
                        break;
                    case QuestEventType.collectable:
                        QuestManager.Instance.NextStep(QuestName, QuestDescription);
                        EventActive = false;
                        Destroy(this.gameObject);
                        break;
                }
            }
        }
        else
        {
            // Obviously we are a trigger.
        }
    }

    public void OnDestroy()
    {
        if (EventActive)
        {
            if (type == QuestEventType.target)
            {
                QuestManager.Instance.NextStep(QuestName, QuestDescription);
                EventActive = false;
            }
        }
    }
}