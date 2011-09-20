using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class GameGui : Bs
{

    public override void Awake()
    {
        enabled = false;
        base.Awake();
    }
    public void OnGUI()
    {
        if (Screen.lockCursor) enabled = false;

        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(200, 100) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.ConnectionGUI, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), SelectTeam, "Game Menu");
    }
    void SelectTeam(int id)
    {
        if (gui.Button("Close"))
        {
            enabled = false;
            Screen.lockCursor = true;
        }
        if (gui.Button("Disconnect"))
            Network.Disconnect();
    }
}
