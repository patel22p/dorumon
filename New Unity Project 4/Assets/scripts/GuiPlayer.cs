using UnityEngine;
using System.Collections;


public class GuiPlayer : Base
{
    
    protected override void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;
        Rect r = Rect.MinMaxRect(w - w / 4, h - h / 5, Screen.width, Screen.height);
        if (player != null)
            GUILayout.Window(0, r, DrawWind, "");
    }
    Player player { get { return Find<Player>("LocalPlayer"); } }
    IPlayer iplayer { get { return Find<Cam>().localplayer; } }
    public void DrawWind(int q)
    {
        
        GUILayout.Label("Life: " + iplayer.Life);
        GUILayout.Label("Frags: " + player.score);

        foreach (GunBase gb in player.gunlist)
            GUILayout.TextField(gb.name + ": " + (int)gb.bullets);
    }
}