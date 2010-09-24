using UnityEngine;
using System.Collections;


public class GuiPlayer : Base
{
    Rect r;
    void Start()
    {
        
        float w = Screen.width;

        r = Rect.MinMaxRect(w - w / 4, 0, Screen.width, 0);
    }
    void OnGUI()
    {

        if (_LocalPlayer != null && _localiplayer != null)
            r = GUILayout.Window(3, r, PlayerWindow, "");
    }


    public void PlayerWindow(int q)
    {
        GUILayout.Label(lc.lf .ToString() + _localiplayer.Life);
        GUILayout.Label(lc.fg .ToString() + _LocalPlayer.frags);
        GUILayout.Label(lc.ntr .ToString() + (int)_localiplayer.nitro);
        if (zombi)
            GUILayout.Label(lc.fz .ToString() + (int)_LocalPlayer.freezedt);
        if (zombi || zombisurive)
        {
            GUILayout.Label(lc.stg .ToString() + _Spawn.stage);
            GUILayout.Label(lc.zl .ToString() + _Spawn.zombies.Count);
        }

        foreach (GunBase gb in _LocalPlayer.guns)
            GUILayout.TextField(gb._Name + ": " + (int)gb.bullets);
        GUI.DragWindow();
    }
}