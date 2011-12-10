using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class TeamSelectGui : Bs
{
    public void OnGUI()
    {
        {
            GUI.skin = _Loader.skin;
            if (lockCursor) enabled = false;
            var c = new Vector3(Screen.width, Screen.height) / 2f;
            var s = new Vector3(250, 450) / 2f;
            var v1 = c - s;
            var v2 = c + s;
            GUI.Window((int)WindowEnum.TeamSelect, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), SelectTeam,
                       "Choose A Class");
        }
        {
            GUI.skin = _Loader.skin;
            if (lockCursor) enabled = false;

            var c = new Vector3(Screen.width - 500, Screen.height) / 2f;
            var s = new Vector3(200, 200) / 2f;
            var v1 = c - s;
            var v2 = c + s;
            GUI.Window((int)WindowEnum.ConnectionGUI, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), GameGuiWnd, "Game Menu");
        }
    }

    bool teamSelected;
    Team team;
    private Vector2 scrollPos;
    void SelectTeam(int id)
    {
        //todo team select
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
                    _Game.pv.skin = i;
                    OnTeamSelected();
                }
            }
        }
        else
        {

            gui.BeginHorizontal();
            if (_Game.gameType != GameType.Survival && gui.Button("Terrorists") || Input.GetKeyDown(KeyCode.Alpha1))
            {
                team = Team.Terrorists;
                teamSelected = true;
            }
            if (gui.Button("Counter Terrorists") || Input.GetKeyDown(KeyCode.Alpha2))
            {
                team = Team.CounterTerrorists;
                teamSelected = true;
            }
            gui.EndHorizontal();
            if (gui.Button("Spectator") || Input.GetKeyDown(KeyCode.Alpha5))
            {
                _Game.pv.team = Team.Spectators;
                OnTeamSelected();
            }
            scrollPos = gui.BeginScrollView(scrollPos);
            if (_Game.levelEditor != null && Network.isServer)
            {
                if (_Game.gameType == GameType.Survival)
                {
                    DrawBotMenu(Team.CounterTerrorists, PlType.Bot);
                }
                else
                {
                    gui.Label("Counter Terrorists TEAM");
                    DrawBotMenu(Team.CounterTerrorists, PlType.Bot);
                    foreach (var a in _Game.EnableZombies)
                        DrawBotMenu(Team.CounterTerrorists, a);
                    gui.Label("_______________________");
                    gui.Label("Terrorists TEAM");
                    DrawBotMenu(Team.Terrorists, PlType.Bot);
                    foreach (var a in _Game.EnableZombies)
                        DrawBotMenu(Team.Terrorists, a);
                }
            }
            gui.EndScrollView();
        }
    }

    private void DrawBotMenu(Team team, PlType pltype)
    {
        gui.BeginHorizontal();
        gui.Label(pltype + "");
        if (gui.Button("<", gui.ExpandWidth(false)))
            _Game.RemoveBot(pltype, team);
        gui.Label("" + _Game.PlayerViews.Count(a => a.team == team && a.plType == pltype), gui.ExpandWidth(false));
        if (gui.Button(">", gui.ExpandWidth(false)))
            _Game.CreateBot(pltype, team);
        gui.EndHorizontal();
    }

    private void OnTeamSelected()
    {
        _Game.pv.team = team;
        teamSelected = false;
        this.enabled = false;
        _Game.OnTeamSelected();
    }


    void GameGuiWnd(int id)
    {
        if (gui.Button("Close"))
        {
            Application.Quit();
            enabled = false;
            lockCursor = true;
        }
        if (gui.Button("Disconnect"))
            Network.Disconnect();
    }
}