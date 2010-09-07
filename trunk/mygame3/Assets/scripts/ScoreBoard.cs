using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBoard : Base {

    public Rect rect;
    int id;
    void Start()
    {
        id = Random.Range(0, int.MaxValue);
        rect = new Rect(0, Screen.height - 200, 0, 0);
    }
    void OnGUI()
    {
        rect = GUILayout.Window(id, rect, Window, "Stats Board" , GUILayout.Height(300), GUILayout.Width(500));
    }
    enum Tab { TopZombieKill , TopKill}
    Tab sel;
    Vector2 scrollPosition;
    void Window(int wid)
    {
        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
            enabled = false;
        sel = (Tab)GUILayout.Toolbar((int)sel, new string[] { "Top Zombie Kill", "TopKill" });

        const string table = "{0,30}{1,20}{2,10}";
        GUILayout.Label(string.Format(table, "", "Kills", "Deaths"), _Loader._GUIStyle);
        SortedList sl = new SortedList();
        SortedList<float, Vk.user> score = sel == Tab.TopZombieKill ? _vk.highscoresZombie : _vk.highscores;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (score.Count == 32)
            for (int i = score.Values.Count - 1; i >= 0; i--)
            {
                Vk.user user = score.Values[i];
                GUILayout.Label(string.Format(table, user.nick, user.totalkills, user.totaldeaths));
            }
        GUILayout.EndScrollView(); 
        GUI.DragWindow();
    }
}
