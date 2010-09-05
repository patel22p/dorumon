using UnityEngine;
using System.Collections;

public class StatsBoard : Base {

    public Rect rect;
    int id;
    void Start()
    {
        id = Random.Range(0, int.MaxValue);
        rect = new Rect(0, Screen.height - 200, 0, 0);
    }
    void OnGUI()
    {
        rect = GUILayout.Window(id, rect, Window, "Stats Board" , GUILayout.Height(300), GUILayout.Width(400));
    }
    enum Tab { User, Frieds, TopZombieKill }
    Tab sel;
    void Window(int i)
    {        
        sel = (Tab)GUILayout.Toolbar((int)sel, new string[] { "user", "friends", "Top Zombie Kill" });
        if (sel == Tab.User)
        {
            
            GUILayout.Label(localuser.nick + "'s Status");
            GUILayout.Label(localuser.st.text);
            GUILayout.Label(localuser.texture);

        }
    }
}
