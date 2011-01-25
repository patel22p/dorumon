
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static ScoreBoardWindow __ScoreBoardWindow;
    public static ScoreBoardWindow _ScoreBoardWindow { get { if (__ScoreBoardWindow == null) __ScoreBoardWindow = (ScoreBoardWindow)MonoBehaviour.FindObjectOfType(typeof(ScoreBoardWindow)); return __ScoreBoardWindow; } }
}

public class ScoreBoardWindow : WindowBase {
		
	internal bool vScore_table = true;
	internal bool focusScore_table;
	public string[] lScore_table;
	internal int iScore_table = -1;
	public string Score_table { get { if(lScore_table.Length==0) return ""; return lScore_table[iScore_table]; } set { iScore_table = lScore_table.SelectIndex(value); }}
	internal bool vscoreboard_ordeby = true;
	internal bool focusScoreboard_ordeby;
	public string[] lScoreboard_ordeby;
	internal int iScoreboard_ordeby = -1;
	public string Scoreboard_ordeby { get { if(lScoreboard_ordeby.Length==0) return ""; return lScoreboard_ordeby[iScoreboard_ordeby]; } set { iScoreboard_ordeby = lScoreboard_ordeby.SelectIndex(value); }}
	internal bool vRefreshScoreBoard = true;
	internal bool focusRefreshScoreBoard;
	internal bool RefreshScoreBoard=false;
	internal bool vfindUserName = true;
	internal bool focusFindUserName;
	internal bool rFindUserName = false;
	internal string FindUserName = @"";
	private int wndid1;
	private Vector2 sScore_table;
	private Vector2 sScoreboard_ordeby;
	private bool oldMouseOverRefreshScoreBoard;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-293.5f + Screen.width/2,-219f + Screen.height/2,579f,463f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.Label(new Rect(264f, 2f, 70.65334f, 21.96f), @"ScoreBoard");
		GUI.BeginGroup(new Rect(8f, 34f, 563f, 396f), "");
		GUI.Box(new Rect(0, 0, 563f, 396f), "");
		if(vScore_table){
		if(focusScore_table) { focusScore_table = false; GUI.FocusControl("Score_table");}
		GUI.SetNextControlName("Score_table");
		GUI.Box(new Rect(25f, 71f, 405f, 314f), "");
		sScore_table = GUI.BeginScrollView(new Rect(25f, 71f, 405f, 314f), sScore_table, new Rect(0,0, 385f, lScore_table.Length* 15f));
		int oldScore_table = iScore_table;
		iScore_table = GUI.SelectionGrid(new Rect(0,0, 385f, lScore_table.Length* 15f), iScore_table, lScore_table,1,GUI.skin.customStyles[0]);
		if (iScore_table != oldScore_table) Action("Score_table");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(25f, 54f, 405f, 13f), @"    Place                    Name                                   score                   ");
		if(vscoreboard_ordeby){
		if(focusScoreboard_ordeby) { focusScoreboard_ordeby = false; GUI.FocusControl("Scoreboard_ordeby");}
		GUI.SetNextControlName("Scoreboard_ordeby");
		GUI.Box(new Rect(434f, 73f, 111f, 312f), "");
		sScoreboard_ordeby = GUI.BeginScrollView(new Rect(434f, 73f, 111f, 312f), sScoreboard_ordeby, new Rect(0,0, 91f, lScoreboard_ordeby.Length* 44.9599990844727f));
		int oldScoreboard_ordeby = iScoreboard_ordeby;
		iScoreboard_ordeby = GUI.SelectionGrid(new Rect(0,0, 91f, lScoreboard_ordeby.Length* 44.9599990844727f), iScoreboard_ordeby, lScoreboard_ordeby,1,GUI.skin.customStyles[0]);
		if (iScoreboard_ordeby != oldScoreboard_ordeby) Action("Scoreboard_ordeby");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(462f, 57f, 83f, 21.96f), @"ScoreBoards");
		if(vRefreshScoreBoard){
		if(focusRefreshScoreBoard) { focusRefreshScoreBoard = false; GUI.FocusControl("RefreshScoreBoard");}
		GUI.SetNextControlName("RefreshScoreBoard");
		bool oldRefreshScoreBoard = RefreshScoreBoard;
		RefreshScoreBoard = GUI.Button(new Rect(415f, 22f, 79f, 31f), new GUIContent("Refresh",""));
		if (RefreshScoreBoard != oldRefreshScoreBoard && RefreshScoreBoard ) {Action("RefreshScoreBoard");onButtonClick(); }
		onMouseOver = new Rect(415f, 22f, 79f, 31f).Contains(Event.current.mousePosition);
		if (oldMouseOverRefreshScoreBoard != onMouseOver && onMouseOver) onOver();
		oldMouseOverRefreshScoreBoard = onMouseOver;
		}
		if(vfindUserName){
		if(focusFindUserName) { focusFindUserName = false; GUI.FocusControl("FindUserName");}
		GUI.SetNextControlName("FindUserName");
		if(rFindUserName){
		GUI.Label(new Rect(105f, 29f, 306f, 14f), FindUserName.ToString());
		} else
		FindUserName = GUI.TextField(new Rect(105f, 29f, 306f, 14f), FindUserName,100);
		}
		GUI.Label(new Rect(18f, 29f, 80.24667f, 21.96f), @"Search Name");
		GUI.EndGroup();
		if (GUI.Button(new Rect(579f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}