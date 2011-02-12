
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static GameStatsWindow __GameStatsWindow;
    public static GameStatsWindow _GameStatsWindow { get { if (__GameStatsWindow == null) __GameStatsWindow = (GameStatsWindow)MonoBehaviour.FindObjectOfType(typeof(GameStatsWindow)); return __GameStatsWindow; } }
}
public enum GameStatsWindowEnum { Close, }
public class GameStatsWindow : WindowBase {
		
	
	internal bool vPlayerStatsTitle = true;
	
	internal bool focusPlayerStatsTitle;
	
	internal bool rPlayerStatsTitle = true;
	internal string PlayerStatsTitle = @"       Player_Name           Alive  Team  Score  Frags Deaths  FPS   Ping  Errors";
	
	internal bool vPlayerStats = true;
	
	internal bool focusPlayerStats;
	
	internal bool rPlayerStats = true;
	internal string PlayerStats = @"";
	private int wndid1;
	
    
    
	void Start () {
		AlwaysOnTop = false;
		wndid1 = 0;

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
		vPlayerStatsTitle = true;
		vPlayerStats = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-349.5f + Screen.width/2,-224f + Screen.height/2,665f,371f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.Label(new Rect(0f, 0f, 99.99f, 14f), @"Scores");
		GUI.BeginGroup(new Rect(22f, 18f, 614.5f, 345f), "");
		GUI.Box(new Rect(0, 0, 614.5f, 345f), "");
		if(vPlayerStatsTitle){
		if(focusPlayerStatsTitle) { focusPlayerStatsTitle = false; GUI.FocusControl("PlayerStatsTitle");}
		GUI.SetNextControlName("PlayerStatsTitle");
		if(rPlayerStatsTitle){
		GUI.Label(new Rect(10f, 21f, 606f, 12f), PlayerStatsTitle.ToString(), GUI.skin.customStyles[2]);
		} else
		try {PlayerStatsTitle = GUI.TextField(new Rect(10f, 21f, 606f, 12f), PlayerStatsTitle,100, GUI.skin.customStyles[2]);}catch{};
		}
		GUI.Box(new Rect(59f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(298f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(347f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(395f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(444f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(489f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(10f, 32f, 578.011f, 1f),"",GUI.skin.customStyles[4]);//line
		if(vPlayerStats){
		if(focusPlayerStats) { focusPlayerStats = false; GUI.FocusControl("PlayerStats");}
		GUI.SetNextControlName("PlayerStats");
		if(rPlayerStats){
		GUI.Label(new Rect(10f, 37f, 606f, 300f), PlayerStats.ToString(), GUI.skin.customStyles[2]);
		} else
		try {PlayerStats = GUI.TextField(new Rect(10f, 37f, 606f, 300f), PlayerStats,100, GUI.skin.customStyles[2]);}catch{};
		}
		GUI.Box(new Rect(535.344f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.Box(new Rect(255.25f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);//line
		GUI.EndGroup();
		if (GUI.Button(new Rect(665f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}