using UnityEngine;
using System.Collections;


public class GuiPlayer : Base
{
    Rect r;
    void Start()
    {
        
        float w = Screen.width;
        float h = Screen.height;
        r = Rect.MinMaxRect(w - w / 4, h - h / 5, Screen.width, Screen.height);
    }
    void OnGUI()
    {

        if (_LocalPlayer != null && _localiplayer != null)
            r = GUILayout.Window(3, r, PlayerWindow, "");
    }


    public void PlayerWindow(int q)
    {
        if (zombi)
        {
            GUILayout.Label("Stage: " + _Spawn.stage);
            GUILayout.Label("Zombies Left: " + _Spawn.zombies.Count);
        }

        GUILayout.Label("Nitro: " + (int)_localiplayer.nitro);
        GUILayout.Label("Life: " + _localiplayer.Life);
        GUILayout.Label("Frags: " + _LocalPlayer.frags);
        GUILayout.Label("Frozen: " + _LocalPlayer.frozentime);
        foreach (GunBase gb in _LocalPlayer.guns)
            GUILayout.TextField(gb.name + ": " + (int)gb.bullets);
        GUI.DragWindow();
    }
}