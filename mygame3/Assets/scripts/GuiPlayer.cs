using UnityEngine;
using System.Collections;


public class GuiPlayer : Base
{
    Rect r;
        void Start()
    {
        if (started || !levelLoaded) return;
        started = true;
        float w = Screen.width;
        float h = Screen.height;
        r = Rect.MinMaxRect(w - w / 4, h - h / 5, Screen.width, Screen.height);
    }
    void OnGUI()
    {
        
        if (player != null)
            r = GUILayout.Window(3, r, DrawWind, "");
    }
    Player player { get { return Find<Player>("LocalPlayer"); } }
    IPlayer iplayer { get { return Find<Cam>().localplayer; } }
    public void DrawWind(int q)
    {
        
        GUILayout.Label("Life: " + iplayer.Life);
        GUILayout.Label("Frags: " + player.score);        
        foreach (GunBase gb in player.gunlist)
            GUILayout.TextField(gb.name + ": " + (int)gb.bullets);
        GUI.DragWindow();
    }
}