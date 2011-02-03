
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
public enum TeamSelectWindowEnum { Teams,TeamSelect,Close, }
public class TeamSelectWindow : WindowBase {
		
	
	internal bool vTeamsView = true;
	
	internal bool focusTeamsView;
	
	internal bool vTeams = true;
	
	internal bool focusTeams;
	public string[] lTeams;
	internal int iTeams = 0;
	public string Teams { get { if(lTeams.Length==0 || iTeams == -1) return ""; return lTeams[iTeams]; } set { iTeams = lTeams.SelectIndex(value); }}
	
	internal bool vred = true;
	
	internal bool focusRed;
	public Texture imgRed;
	
	internal bool vTeamSelect = true;
	
	internal bool focusTeamSelect;
	internal bool TeamSelect=false;
	
	internal bool vGameType = true;
	
	internal bool focusGameType;
	
	internal bool vdescription = true;
	
	internal bool focusDescription;
	
	internal bool rDescription = false;
	internal string Description = @"description";
	private int wndid1;
	private Vector2 sTeams;
	private Rect Red;
	private bool oldMouseOverTeamSelect;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Red = new Rect(385f, 39f, 181f, 177f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
        		iTeams = -1;

    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-308.5f + Screen.width/2,-197f + Screen.height/2,596f,316f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		if (AlwaysOnTop) { GUI.BringWindowToFront(id);}		focusWindow = false;
		bool onMouseOver;
		GUI.Label(new Rect(0f, 1f, 99.99f, 14f), @"Team Select");
		GUI.BeginGroup(new Rect(0f, 19f, 596f, 297f), "");
		GUI.Box(new Rect(0, 0, 596f, 297f), "");
		if(vTeamsView){
		if(focusTeamsView) { focusTeamsView = false; GUI.FocusControl("TeamsView");}
		GUI.SetNextControlName("TeamsView");
		GUI.BeginGroup(new Rect(34f, 19f, 317f, 109f), "");
		GUI.Box(new Rect(0, 0, 317f, 109f), "");
		if(vTeams){
		if(focusTeams) { focusTeams = false; GUI.FocusControl("Teams");}
		GUI.SetNextControlName("Teams");
		GUI.Box(new Rect(11f, 20f, 298f, 81f), "");
		sTeams = GUI.BeginScrollView(new Rect(11f, 20f, 298f, 81f), sTeams, new Rect(0,0, 288f, lTeams.Length* 28.9599990844727f));
		int oldTeams = iTeams;
		iTeams = GUI.SelectionGrid(new Rect(0,0, 288f, lTeams.Length* 28.9599990844727f), iTeams, lTeams,1,GUI.skin.customStyles[0]);
		if (iTeams != oldTeams) Action("Teams");
		GUI.EndScrollView();
		}
		GUI.EndGroup();
		}
		if(vred){
		if(focusRed) { focusRed = false; GUI.FocusControl("Red");}
		GUI.SetNextControlName("Red");
		if(imgRed!=null)
			GUI.DrawTexture(Red,imgRed, ScaleMode.ScaleToFit, imgRed is RenderTexture?false:true);
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(28f, 263f, 542f, 40f), "");
		GUI.Box(new Rect(0, 0, 542f, 40f), "");
		if(vTeamSelect){
		if(focusTeamSelect) { focusTeamSelect = false; GUI.FocusControl("TeamSelect");}
		GUI.SetNextControlName("TeamSelect");
		bool oldTeamSelect = TeamSelect;
		TeamSelect = GUI.Button(new Rect(449f, 9f, 75f, 21.96f), new GUIContent("Start",""));
		if (TeamSelect != oldTeamSelect && TeamSelect ) {Action("TeamSelect");onButtonClick(); }
		onMouseOver = new Rect(449f, 9f, 75f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverTeamSelect != onMouseOver && onMouseOver) onOver();
		oldMouseOverTeamSelect = onMouseOver;
		}
		GUI.EndGroup();
		if(vGameType){
		if(focusGameType) { focusGameType = false; GUI.FocusControl("GameType");}
		GUI.SetNextControlName("GameType");
		GUI.BeginGroup(new Rect(40f, 155f, 302f, 95f), "");
		GUI.Box(new Rect(0, 0, 302f, 95f), "");
		if(vdescription){
		if(focusDescription) { focusDescription = false; GUI.FocusControl("Description");}
		GUI.SetNextControlName("Description");
		if(rDescription){
		GUI.Label(new Rect(17f, 8f, 267f, 79f), Description.ToString());
		} else
		try {Description = GUI.TextField(new Rect(17f, 8f, 267f, 79f), Description,100);}catch{};
		}
		GUI.EndGroup();
		}
		if (GUI.Button(new Rect(596f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}