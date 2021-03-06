using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System;

public class TeamSelectGui : Bs
{

    public void OnGUI()
    {
        if (Screen.lockCursor) enabled = false;
        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(200, 200) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.TeamSelect, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), SelectTeam, "Choose A Class");
    }
    
    bool teamSelected;
    Team team;
    void SelectTeam(int id)
    {
        if (teamSelected)
        {
            if (gui.Button("<<Back"))
                teamSelected = false;
            var p = _Game.PlayerPrefab.GetComponent<Player>();
            var skins = (_Game.pv.team == Team.CounterTerrorists ? p.CTerrorSkins : p.TerrorSkins);
            for (int i = 0; i < skins.Length; i++)
            {
                if (gui.Button(skins[i].name) || Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _Game.pv.skin= i;
                    OnTeamSelected();
                }
            }
        }
        else
        {
            if (gui.Button("Terrorists") || Input.GetKeyDown(KeyCode.Alpha1))
            {
                team = Team.Terrorists;
                teamSelected = true;
            }
            if (gui.Button("Counter Terrorists") || Input.GetKeyDown(KeyCode.Alpha2))
            {
                team = Team.CounterTerrorists;
                teamSelected = true;
            }
            if (gui.Button("Spectator") || Input.GetKeyDown(KeyCode.Alpha5))
            {
                _Game.pv.team = Team.Spectators;
                OnTeamSelected();
            }
            if (_Game.levelEditor != null && Network.isServer && gui.Button("Add T Bot"))
            {
                var i = _Game.GetNextFreeSlot();                
                _Game.CallRPC(_Game.AddPlayerView, RPCMode.AllBuffered, i, "Bot" + i);
                _Game.playerViews[i].team = Team.Terrorists;
                _Game.CreateBot(i);
            }
            if (_Game.levelEditor!=null && Network.isServer && gui.Button("Add CT Bot"))
            {
                var i = _Game.GetNextFreeSlot();                
                _Game.CallRPC(_Game.AddPlayerView, RPCMode.AllBuffered, i, "Bot" + i);
                _Game.playerViews[i].team = Team.CounterTerrorists;
                _Game.CreateBot(i);
            }
        }
    }

    private void OnTeamSelected()
    {
        _Game.pv.team = team;
        teamSelected = false;
        this.enabled = false;
        _Game.OnTeamSelected();
    }
}
