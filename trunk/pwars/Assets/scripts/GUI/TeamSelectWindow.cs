
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static TeamSelectWindow __TeamSelectWindow;
    public static TeamSelectWindow _TeamSelectWindow { get { if (__TeamSelectWindow == null) __TeamSelectWindow = (TeamSelectWindow)MonoBehaviour.FindObjectOfType(typeof(TeamSelectWindow)); return __TeamSelectWindow; } }
}

public class TeamSelectWindow : WindowBase {
		
	internal bool focusTeamsView;
	internal bool enabledTeamsView = true;
	internal bool focusTeams;
	internal string[] Teams = new string[] {"Красная Команда","Синяя Команда",};
	internal int iTeams = 0;
	internal bool focusImages;
	internal int tabImages;
	internal bool focusRed;
	internal string pathRed = "Images/Image2";
	internal bool focusBlue;
	internal string pathBlue = "Images/Image1";
	internal bool focusTeamSelect;
	internal bool TeamSelect=false;
	internal bool focusObserver;
	internal bool Observer=false;
	internal bool focusFraglimit;
	internal bool isReadOnlyFraglimit = true;
	internal int Fraglimit = 0;
	internal bool focusGameType;
	internal int tabGameType;
	private int wndid1;
	private Vector2 sTeams;
	private Rect Red;
	private Rect Blue;
	private bool oldMouseOverTeamSelect;
	private bool oldMouseOverObserver;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Red = new Rect(12f, 2f, 181f, 177f);
		Blue = new Rect(9.573f, 0f, 183.427f, 179f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader._skin;
        
		GUI.Window(wndid1,new Rect(-308.5f + Screen.width/2,-197f + Screen.height/2,596f,316f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.Label(new Rect(0f, 1f, 99.99f, 14f), @"Игровое меню");
		GUI.BeginGroup(new Rect(0f, 19f, 596f, 297f), "");
		GUI.Box(new Rect(0, 0, 596f, 297f), "");
		if(focusTeamsView) { focusTeamsView = false; GUI.FocusControl("TeamsView");}
		GUI.SetNextControlName("TeamsView");
		if(enabledTeamsView){
		GUI.BeginGroup(new Rect(34f, 19f, 317f, 109f), "");
		GUI.Box(new Rect(0, 0, 317f, 109f), "");
		if(focusTeams) { focusTeams = false; GUI.FocusControl("Teams");}
		GUI.SetNextControlName("Teams");
		GUI.Box(new Rect(11f, 20f, 298f, 87f), "");
		sTeams = GUI.BeginScrollView(new Rect(11f, 20f, 298f, 87f), sTeams, new Rect(0,0, 278f, Teams.Length* 28.9599990844727f));
		int oldTeams = iTeams;
		iTeams = GUI.SelectionGrid(new Rect(0,0, 278f, Teams.Length* 28.9599990844727f), iTeams, Teams,1,GUI.skin.customStyles[0]);
		if (iTeams != oldTeams) Action("onTeams",Teams[iTeams]);
		GUI.EndScrollView();
		GUI.EndGroup();
		}
		if(focusImages) { focusImages = false; GUI.FocusControl("Images");}
		GUI.SetNextControlName("Images");
		GUI.BeginGroup(new Rect(358f, 20f, 211f, 195f), "");
		GUI.Box(new Rect(0, 0, 211f, 195f), "");
		if(tabImages==0){
		if(focusRed) { focusRed = false; GUI.FocusControl("Red");}
		GUI.SetNextControlName("Red");
		GUI.DrawTexture(Red,(Texture2D)Resources.Load("Images/Image2"), ScaleMode.ScaleToFit);
		}
		if(tabImages==1){
		if(focusBlue) { focusBlue = false; GUI.FocusControl("Blue");}
		GUI.SetNextControlName("Blue");
		GUI.DrawTexture(Blue,(Texture2D)Resources.Load("Images/Image1"), ScaleMode.ScaleToFit);
		}
		GUI.EndGroup();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(28f, 263f, 542f, 40f), "");
		GUI.Box(new Rect(0, 0, 542f, 40f), "");
		if(focusTeamSelect) { focusTeamSelect = false; GUI.FocusControl("TeamSelect");}
		GUI.SetNextControlName("TeamSelect");
		bool oldTeamSelect = TeamSelect;
		TeamSelect = GUI.Button(new Rect(449f, 9f, 75f, 21.96f), new GUIContent("Войти",""));
		if (TeamSelect != oldTeamSelect && TeamSelect ) {Action("onTeamSelect");onButtonClick(); }
		onMouseOver = new Rect(449f, 9f, 75f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverTeamSelect != onMouseOver && onMouseOver) onOver();
		oldMouseOverTeamSelect = onMouseOver;
		if(focusObserver) { focusObserver = false; GUI.FocusControl("Observer");}
		GUI.SetNextControlName("Observer");
		bool oldObserver = Observer;
		Observer = GUI.Button(new Rect(370f, 8f, 75f, 21.96f), new GUIContent("Зритель",""));
		if (Observer != oldObserver && Observer ) {Action("onObserver");onButtonClick(); }
		onMouseOver = new Rect(370f, 8f, 75f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverObserver != onMouseOver && onMouseOver) onOver();
		oldMouseOverObserver = onMouseOver;
		GUI.Label(new Rect(133f, 13.95f, 73.38f, 21.96f), @"Фраг лимит");
		if(focusFraglimit) { focusFraglimit = false; GUI.FocusControl("Fraglimit");}
		GUI.SetNextControlName("Fraglimit");
		if(isReadOnlyFraglimit){
		GUI.Label(new Rect(213f, 13.95f, 75.333f, 21.96f), Fraglimit.ToString());
		} else
		Fraglimit = int.Parse(GUI.TextField(new Rect(213f, 13.95f, 75.333f, 21.96f), Fraglimit.ToString()));
		GUI.EndGroup();
		if(focusGameType) { focusGameType = false; GUI.FocusControl("GameType");}
		GUI.SetNextControlName("GameType");
		GUI.BeginGroup(new Rect(40f, 155f, 302f, 95f), "");
		GUI.Box(new Rect(0, 0, 302f, 95f), "");
		if(tabGameType==0){
		GUI.Label(new Rect(8f, 8f, 276f, 50f), @"Зомби, выживание.
Убейте максимальное количество зомби.");
		}
		if(tabGameType==1){
		GUI.Label(new Rect(8f, 8f, 276f, 50f), @"Командный бой С зомби.
Выжившая команда выигрывает.");
		}
		if(tabGameType==2){
		GUI.Label(new Rect(8f, 8f, 276f, 50f), @"Бой.
Убейте максимальное количество игроков.");
		}
		if(tabGameType==3){
		GUI.Label(new Rect(8f, 8f, 276f, 50f), @"Командный бой.
Убейте максимальное количество игроков.");
		}
		GUI.EndGroup();
		if (GUI.Button(new Rect(596f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}