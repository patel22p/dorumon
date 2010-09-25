using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ScoreBoard : WindowBase {

    void Start()
    {
        title = lc.sb.ToString();
        size = new Vector2(500,300);
    }
    
    enum Tab { TopZombieKill , TopKill}
    Tab sel;
    Vector2 scrollPosition;
    protected override void Window(int wid)
    {
        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
            enabled = false;
        sel = (Tab)GUILayout.Toolbar((int)sel, new string[] { lc.tpz .ToString(), lc.tk .ToString() });

        const string table = "{0,30}{1,20}{2,10}";
        GUILayout.Label(string.Format(table, "", lc.kills, lc.deaths));

        SortedList<float, z0Vk.user> score = sel == Tab.TopZombieKill ? _vk.highscoresZombie : _vk.highscores;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (score.Count == 32)
            for (int i = score.Values.Count - 1; i >= 0; i--)
            {
                z0Vk.user user = score.Values[i];
                GUILayout.Label(string.Format(table, user.nick, user.totalkills, user.totaldeaths));
            }
        GUILayout.EndScrollView(); 
        GUI.DragWindow();
    }
}
