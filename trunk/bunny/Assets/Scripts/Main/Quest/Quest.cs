using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Quest
{
    public bool Done = false;
    public string QuestName = "QuestName";
    public int QuestStep = 0;
    public List<string> QuestStepDescription
    { 
        get 
        {
            if (questStepDescription == null)
            {
                questStepDescription = new List<string>();
            }
            return questStepDescription;
        }
        set
        {
            if (questStepDescription == null)
            {
                questStepDescription = new List<string>();
            }
            questStepDescription = value;
        }
    }
    private List<string> questStepDescription;

    public Quest(string name, string Description)
    {
        questStepDescription = new List<string>();
        QuestName = name;
        questStepDescription.Add(Description);
        QuestStep++;
        ActivateNextStep(QuestName);
    }

    public void AddStep(string Description)
    {
        QuestStep++;
        questStepDescription.Add(Description);
        ActivateNextStep(QuestName);
    }

    public void ActivateNextStep(string questName)
    {
        QuestEvent[] qm = GameObject.FindObjectsOfType(typeof(QuestEvent)) as QuestEvent[];
        foreach (QuestEvent qw in qm)
        {
            if (qw.QuestName == questName)
            {
                if (qw.QuestStep == QuestStep)
                {
                    qw.EventActive = true;
                    break;
                }
            }
        }
    }
}
