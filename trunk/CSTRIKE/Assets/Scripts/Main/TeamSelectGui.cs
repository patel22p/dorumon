using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class TeamSelectGui : Bs{

    public void OnGUI()
    {
        if (Screen.lockCursor) return;
        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(200, 100) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.TeamSelect, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), SelectTeam, "Choose A Class");
    }
    void SelectTeam(int id)
    {
        if (gui.Button("Terrorists"))
        {
            _Game.team = Team.Terrorists;
            this.enabled = false;
            _Game.OnTeamSelected();
        }
        if (gui.Button("Counter Terrorists"))
        {
            _Game.team = Team.CounterTerrorists;
            this.enabled = false;
            _Game.OnTeamSelected();            
        }
        if (gui.Button("Spectator"))
        {
            _Game.team = Team.Spectators;
            this.enabled = false;
            _Game.OnTeamSelected();            
        }
    }
}
