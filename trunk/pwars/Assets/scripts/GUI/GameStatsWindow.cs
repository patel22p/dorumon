
#pragma warning disable 649
#pragma warning disable 168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static GameStatsWindow __GameStatsWindow;
    public static GameStatsWindow _GameStatsWindow { get { if (__GameStatsWindow == null) __GameStatsWindow = (GameStatsWindow)MonoBehaviour.FindObjectOfType(typeof(GameStatsWindow)); return __GameStatsWindow; } }
}

public class GameStatsWindow : WindowBase {
		
	internal bool focusPlayerStatsTitle;
	internal bool isReadOnlyPlayerStatsTitle = true;
	internal string PlayerStatsTitle = @"       Имя_Игрока               Команда   Очки   Фраги Смерти  Фпс   Пинг";
	internal bool focusPlayerStats;
	internal bool isReadOnlyPlayerStats = true;
	internal string PlayerStats = @"";
	private int wndid1;
	
    
    
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
		GUI.skin = (GUISkin)Resources.Load("Skin/Skin");
        
		GUI.Window(wndid1,new Rect(-310.5f + Screen.width/2,-220f + Screen.height/2,611f,371f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.Label(new Rect(0f, 0f, 99.99f, 14f), @"Таблиьа очков");
		GUI.BeginGroup(new Rect(22f, 18f, 564f, 345f), "");
		GUI.Box(new Rect(0, 0, 564f, 345f), "");
		if(focusPlayerStatsTitle) { focusPlayerStatsTitle = false; GUI.FocusControl("PlayerStatsTitle");}
		GUI.SetNextControlName("PlayerStatsTitle");
		if(isReadOnlyPlayerStatsTitle){
		GUI.Label(new Rect(10f, 21f, 546f, 12f), PlayerStatsTitle.ToString(), GUI.skin.customStyles[2]);
		} else
		PlayerStatsTitle = GUI.TextField(new Rect(10f, 21f, 546f, 12f), PlayerStatsTitle, GUI.skin.customStyles[2]);
		GUI.Box(new Rect(59f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(298f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(347f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(395f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(444f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(489f, 16f, 1f, 310.015f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(10f, 32f, 525.344f, 1f),"",GUI.skin.customStyles[4]);
		if(focusPlayerStats) { focusPlayerStats = false; GUI.FocusControl("PlayerStats");}
		GUI.SetNextControlName("PlayerStats");
		if(isReadOnlyPlayerStats){
		GUI.Label(new Rect(10f, 37f, 546f, 300f), PlayerStats.ToString(), GUI.skin.customStyles[2]);
		} else
		PlayerStats = GUI.TextField(new Rect(10f, 37f, 546f, 300f), PlayerStats, GUI.skin.customStyles[2]);
		GUI.EndGroup();
		if (GUI.Button(new Rect(611f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}