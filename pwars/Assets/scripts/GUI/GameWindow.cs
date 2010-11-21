
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static GameWindow __GameWindow;
    public static GameWindow _GameWindow { get { if (__GameWindow == null) __GameWindow = (GameWindow)MonoBehaviour.FindObjectOfType(typeof(GameWindow)); return __GameWindow; } }
}

public class GameWindow : WindowBase {
		
	internal bool focusLife;
	internal float Life = 100;
	internal bool focusEnergy;
	internal float Energy = 100;
	internal bool focusZombiScore;
	internal bool enabledZombiScore = true;
	internal bool focusZombies;
	internal bool isReadOnlyZombies = true;
	internal int Zombies = 0;
	internal bool focusStage;
	internal bool isReadOnlyStage = true;
	internal int Stage = 0;
	internal bool focusFrags;
	internal bool isReadOnlyFrags = false;
	internal int Frags = 0;
	internal bool focusGunImages;
	internal int tabGunImages;
	internal string pathImage8 = "Images/minigun";
	internal string pathImage9 = "Images/rocketLauncher";
	internal string pathImage10 = "Images/energygun";
	internal bool focusPatrony;
	internal bool isReadOnlyPatrony = true;
	internal string Patrony = @"TextBox";
	internal bool focusMenu;
	internal bool Menu=false;
	internal bool focusScoreBoard;
	internal bool ScoreBoard=false;
	internal bool focusShowMap;
	internal bool ShowMap=false;
	internal bool focusMessages;
	internal bool isReadOnlyMessages = true;
	internal string Messages = @"";
	internal bool focusMsg;
	internal bool isReadOnlyMsg = true;
	internal string Msg = @"";
	internal bool focusKillmessages;
	internal bool isReadOnlyKillmessages = true;
	internal string Killmessages = @"";
	internal bool focusTimeLeft;
	internal bool isReadOnlyTimeLeft = false;
	internal string TimeLeft = @"";
	internal bool focusTeamScore;
	internal bool isReadOnlyTeamScore = false;
	internal string TeamScore = @"";
	private int wndid1;
	private int wndid7;
	private Rect Image8;
	private Rect Image9;
	private Rect Image10;
	private bool oldMouseOverMenu;
	private bool oldMouseOverScoreBoard;
	private bool oldMouseOverShowMap;
	private int wndid11;
	private int wndid12;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		wndid7 = UnityEngine.Random.Range(0, 1000);
		Image8 = new Rect(0f, 0f, 128f, 87f);
		Image9 = new Rect(12.333f, 0f, 99.014f, 81.333f);
		Image10 = new Rect(0f, 0f, 128f, 80f);
		wndid11 = UnityEngine.Random.Range(0, 1000);
		wndid12 = UnityEngine.Random.Range(0, 1000);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader._skin;
        
		GUI.Window(wndid1,new Rect(-319f + Screen.width,0f,303f,152f), Wnd1,"", GUI.skin.customStyles[3]);
		GUI.Window(wndid7,new Rect(0f,0f,171f,152f), Wnd7,"", GUI.skin.customStyles[3]);
		GUI.Window(wndid11,new Rect(0f,-349f + Screen.height,454f,249f), Wnd11,"", GUI.skin.customStyles[3]);
		GUI.Window(wndid12,new Rect(-122.833f + Screen.width/2,0f,173.333f,89.333f), Wnd12,"", GUI.skin.customStyles[3]);

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(focusLife) { focusLife = false; GUI.FocusControl("Life");}
		GUI.SetNextControlName("Life");
		GUI.HorizontalScrollbar(new Rect(83f, 8f, 212f, 18f), 0, Mathf.Min(Mathf.Max(0, Life),100), 0, 100, GUI.skin.customStyles[10]);
		GUI.Label(new Rect(136f,8f,100,15),Life+"/"+100 );
		if(focusEnergy) { focusEnergy = false; GUI.FocusControl("Energy");}
		GUI.SetNextControlName("Energy");
		GUI.HorizontalScrollbar(new Rect(83f, 30f, 212f, 18f), 0, Mathf.Min(Mathf.Max(0, Energy),100), 0, 100, GUI.skin.customStyles[12]);
		GUI.Label(new Rect(136f,30f,100,15),Energy+"/"+100 );
		GUI.Label(new Rect(24f, 8f, 45.69667f, 21.96f), @"Жизнь");
		GUI.Label(new Rect(24f, 30f, 53.93f, 21.96f), @"Енергия");
		if(focusZombiScore) { focusZombiScore = false; GUI.FocusControl("ZombiScore");}
		GUI.SetNextControlName("ZombiScore");
		if(enabledZombiScore){
		GUI.BeginGroup(new Rect(99.5f, 88f, 195.5f, 56f), "");
		GUI.Box(new Rect(0, 0, 195.5f, 56f), "");
		GUI.Label(new Rect(83f, 8f, 44.73f, 21.96f), @"зомби", GUI.skin.customStyles[5]);
		if(focusZombies) { focusZombies = false; GUI.FocusControl("Zombies");}
		GUI.SetNextControlName("Zombies");
		if(isReadOnlyZombies){
		GUI.Label(new Rect(134f, 8f, 38f, 21.96f), Zombies.ToString(), GUI.skin.customStyles[5]);
		} else
		Zombies = int.Parse(GUI.TextField(new Rect(134f, 8f, 38f, 21.96f), Zombies.ToString(), GUI.skin.customStyles[5]));
		GUI.Label(new Rect(75f, 36f, 55.49667f, 21.96f), @"уровень", GUI.skin.customStyles[5]);
		if(focusStage) { focusStage = false; GUI.FocusControl("Stage");}
		GUI.SetNextControlName("Stage");
		if(isReadOnlyStage){
		GUI.Label(new Rect(134f, 36f, 38f, 21.96f), Stage.ToString(), GUI.skin.customStyles[5]);
		} else
		Stage = int.Parse(GUI.TextField(new Rect(134f, 36f, 38f, 21.96f), Stage.ToString(), GUI.skin.customStyles[5]));
		GUI.EndGroup();
		}
		GUI.Label(new Rect(99.5f, 58.666f, 43.44667f, 21.96f), @"Фраги", GUI.skin.customStyles[5]);
		if(focusFrags) { focusFrags = false; GUI.FocusControl("Frags");}
		GUI.SetNextControlName("Frags");
		if(isReadOnlyFrags){
		GUI.Label(new Rect(125f, 53.334f, 105.332f, 30.666f), Frags.ToString(), GUI.skin.customStyles[3]);
		} else
		Frags = int.Parse(GUI.TextField(new Rect(125f, 53.334f, 105.332f, 30.666f), Frags.ToString(), GUI.skin.customStyles[3]));
	}
	void Wnd7(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(focusGunImages) { focusGunImages = false; GUI.FocusControl("GunImages");}
		GUI.SetNextControlName("GunImages");
		GUI.BeginGroup(new Rect(8f, 8f, 135f, 97.333f), "");
		GUI.Box(new Rect(0, 0, 135f, 97.333f), "");
		if(tabGunImages==0){
		GUI.DrawTexture(Image8,(Texture2D)Resources.Load("Images/minigun"), ScaleMode.ScaleToFit);
		}
		if(tabGunImages==1){
		GUI.DrawTexture(Image9,(Texture2D)Resources.Load("Images/rocketLauncher"), ScaleMode.ScaleToFit);
		}
		if(tabGunImages==2){
		GUI.DrawTexture(Image10,(Texture2D)Resources.Load("Images/energygun"), ScaleMode.ScaleToFit);
		}
		GUI.EndGroup();
		if(focusPatrony) { focusPatrony = false; GUI.FocusControl("Patrony");}
		GUI.SetNextControlName("Patrony");
		if(isReadOnlyPatrony){
		GUI.Label(new Rect(3.39f, 109.333f, 139.61f, 21.96f), Patrony.ToString(), GUI.skin.customStyles[5]);
		} else
		Patrony = GUI.TextField(new Rect(3.39f, 109.333f, 139.61f, 21.96f), Patrony, GUI.skin.customStyles[5]);
		if(focusMenu) { focusMenu = false; GUI.FocusControl("Menu");}
		GUI.SetNextControlName("Menu");
		bool oldMenu = Menu;
		Menu = GUI.Button(new Rect(633f, 672f, 75f, 21.96f), new GUIContent("Меню(esc)",""));
		if (Menu != oldMenu && Menu ) {Action("onMenu");onButtonClick(); }
		onMouseOver = new Rect(633f, 672f, 75f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverMenu != onMouseOver && onMouseOver) onOver();
		oldMouseOverMenu = onMouseOver;
		if(focusScoreBoard) { focusScoreBoard = false; GUI.FocusControl("ScoreBoard");}
		GUI.SetNextControlName("ScoreBoard");
		bool oldScoreBoard = ScoreBoard;
		ScoreBoard = GUI.Button(new Rect(712f, 672f, 75f, 21.96f), new GUIContent("Очки(tab)",""));
		if (ScoreBoard != oldScoreBoard && ScoreBoard ) {Action("onScoreBoard");onButtonClick(); }
		onMouseOver = new Rect(712f, 672f, 75f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverScoreBoard != onMouseOver && onMouseOver) onOver();
		oldMouseOverScoreBoard = onMouseOver;
		if(focusShowMap) { focusShowMap = false; GUI.FocusControl("ShowMap");}
		GUI.SetNextControlName("ShowMap");
		bool oldShowMap = ShowMap;
		ShowMap = GUI.Button(new Rect(501.333f, 672f, 127.667f, 21.96f), new GUIContent("Показать карту(m)",""));
		if (ShowMap != oldShowMap && ShowMap ) {Action("onShowMap");onButtonClick(); }
		onMouseOver = new Rect(501.333f, 672f, 127.667f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverShowMap != onMouseOver && onMouseOver) onOver();
		oldMouseOverShowMap = onMouseOver;
	}
	void Wnd11(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(focusMessages) { focusMessages = false; GUI.FocusControl("Messages");}
		GUI.SetNextControlName("Messages");
		if(isReadOnlyMessages){
		GUI.Label(new Rect(7.223f, 34f, 423.777f, 87f), Messages.ToString(), GUI.skin.customStyles[3]);
		} else
		Messages = GUI.TextField(new Rect(7.223f, 34f, 423.777f, 87f), Messages, GUI.skin.customStyles[3]);
		if(focusMsg) { focusMsg = false; GUI.FocusControl("Msg");}
		GUI.SetNextControlName("Msg");
		if(isReadOnlyMsg){
		GUI.Label(new Rect(7.223f, 8f, 423.777f, 20f), Msg.ToString(), GUI.skin.customStyles[3]);
		} else
		Msg = GUI.TextField(new Rect(7.223f, 8f, 423.777f, 20f), Msg, GUI.skin.customStyles[3]);
		if(focusKillmessages) { focusKillmessages = false; GUI.FocusControl("Killmessages");}
		GUI.SetNextControlName("Killmessages");
		if(isReadOnlyKillmessages){
		GUI.Label(new Rect(0f, 125f, 423.777f, 87f), Killmessages.ToString(), GUI.skin.customStyles[3]);
		} else
		Killmessages = GUI.TextField(new Rect(0f, 125f, 423.777f, 87f), Killmessages, GUI.skin.customStyles[3]);
	}
	void Wnd12(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(focusTimeLeft) { focusTimeLeft = false; GUI.FocusControl("TimeLeft");}
		GUI.SetNextControlName("TimeLeft");
		if(isReadOnlyTimeLeft){
		GUI.Label(new Rect(30.667f, 50.667f, 105.332f, 30.666f), TimeLeft.ToString(), GUI.skin.customStyles[3]);
		} else
		TimeLeft = GUI.TextField(new Rect(30.667f, 50.667f, 105.332f, 30.666f), TimeLeft, GUI.skin.customStyles[3]);
		if(focusTeamScore) { focusTeamScore = false; GUI.FocusControl("TeamScore");}
		GUI.SetNextControlName("TeamScore");
		if(isReadOnlyTeamScore){
		GUI.Label(new Rect(30.667f, 8f, 105.332f, 30.666f), TeamScore.ToString(), GUI.skin.customStyles[3]);
		} else
		TeamScore = GUI.TextField(new Rect(30.667f, 8f, 105.332f, 30.666f), TeamScore, GUI.skin.customStyles[3]);
	}


	void Update () {
	
	}
}