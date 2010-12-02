
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static StaticsWindow __StaticsWindow;
    public static StaticsWindow _StaticsWindow { get { if (__StaticsWindow == null) __StaticsWindow = (StaticsWindow)MonoBehaviour.FindObjectOfType(typeof(StaticsWindow)); return __StaticsWindow; } }
}

public class StaticsWindow : WindowBase {
		
	public int tabTabControl2;
	public bool focusYourRank;
	[LoadPath("Skin/Images/1_ефрейтор.jpg")]
	public Texture2D ImageYourRank;
	public bool focusNextRank;
	[LoadPath("Skin/Images/2_мл сержант.jpg")]
	public Texture2D ImageNextRank;
	public bool focusUserRank;
	public float UserRank = 100;
	public bool Button11=false;
	[LoadPath("Skin/Images/unity.jpg")]
	public Texture2D Image;
	public bool focusPlayer;
	public bool isReadOnlyPlayer = true;
	public string Player = @"";
	public bool focusFriendsPanel;
	public Action onStackPanelDraw13;
	public bool focusChat;
	public bool isReadOnlyChat = true;
	public string Chat = @"";
	public bool focusMsg;
	public bool isReadOnlyMsg = false;
	public string Msg = @"";
	public bool focusUsers;
	public string[] Users = new string[] {};
	public int iUsers = -1;
	public bool Button14=false;
	public bool focusTop;
	public string[] Top = new string[] {};
	public int iTop = -1;
	public bool focusSortTops;
	public string[] SortTops = new string[] {"Убил Игроков","Убил Зомби",};
	public int iSortTops = -1;
	private int wndid1;
	private Rect YourRank;
	private Rect NextRank;
	private bool oldMouseOverButton11;
	private Vector2 sUsers;
	private bool oldMouseOverButton14;
	private Vector2 sTop;
	private Vector2 sSortTops;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		YourRank = new Rect(12.32f, 45f, 115.68f, 103f);
		NextRank = new Rect(8f, 49f, 120f, 98f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-309.5f + Screen.width/2,-228f + Screen.height/2,617f,389f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(42f, 30f, 534f, 341f), "");
		GUI.Box(new Rect(0, 0, 534f, 341f), "");
		GUILayout.BeginArea(new Rect(0f, 0, 534, 18));
		tabTabControl2 = GUILayout.Toolbar(tabTabControl2, new string[] { "Ранг","Друзья","Чат","Рейтинг", }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));
		GUILayout.EndArea();
		GUI.BeginGroup(new Rect(0, 18, 534, 323), "");
		GUI.Box(new Rect(0, 0, 534, 323), "");
		if(tabTabControl2==0){
		GUI.BeginGroup(new Rect(8f, 31f, 136f, 189f), "");
		GUI.Box(new Rect(0, 0, 136f, 189f), "");
		GUI.Label(new Rect(12.32f, 18f, 111.72f, 21.96f), @"Ваш текуший ранк");
		if(focusYourRank) { focusYourRank = false; GUI.FocusControl("YourRank");}
		GUI.SetNextControlName("YourRank");
		GUI.DrawTexture(YourRank,ImageYourRank, ScaleMode.ScaleToFit);
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(380f, 31f, 136f, 189f), "");
		GUI.Box(new Rect(0, 0, 136f, 189f), "");
		GUI.Label(new Rect(8f, 18f, 119.2633f, 21.96f), @"Ваш следуйши ранк");
		if(focusNextRank) { focusNextRank = false; GUI.FocusControl("NextRank");}
		GUI.SetNextControlName("NextRank");
		GUI.DrawTexture(NextRank,ImageNextRank, ScaleMode.ScaleToFit);
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(148f, 31f, 228f, 189f), "");
		GUI.Box(new Rect(0, 0, 228f, 189f), "");
		if(focusUserRank) { focusUserRank = false; GUI.FocusControl("UserRank");}
		GUI.SetNextControlName("UserRank");
		GUI.HorizontalScrollbar(new Rect(18f, 33f, 202f, 17f), 0, Mathf.Min(Mathf.Max(0, UserRank),1600), 0, 1600);
		GUI.Label(new Rect(68.5f,33f,100,15),UserRank+"/"+1600 );
		GUI.Label(new Rect(53f, 15f, 75.55f, 21.96f), @"Очки опыта");
		GUI.Label(new Rect(18f, 73f, 189f, 91f), @"Очки опыта выдаются за убийства других игроков и спиритов, за получение различных наград.
");
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(8f, 230f, 508f, 75f), "");
		GUI.Box(new Rect(0, 0, 508f, 75f), "");
		bool oldButton11 = Button11;
		Button11 = GUI.Button(new Rect(28f, 8f, 64f, 59f), new GUIContent(Image,"+100 очков. Медаль за установку плеера unity."));
		if (Button11 != oldButton11 && Button11 ) {Action("onButton11");onButtonClick(); }
		onMouseOver = new Rect(28f, 8f, 64f, 59f).Contains(Event.current.mousePosition);
		if (oldMouseOverButton11 != onMouseOver && onMouseOver) onOver();
		oldMouseOverButton11 = onMouseOver;
		GUI.Label(new Rect(207f, 0f, 56.61f, 14f), @"Награды");
		GUI.EndGroup();
		if(focusPlayer) { focusPlayer = false; GUI.FocusControl("Player");}
		GUI.SetNextControlName("Player");
		if(isReadOnlyPlayer){
		GUI.Label(new Rect(36f, 8f, 89f, 21.96f), Player.ToString(), GUI.skin.customStyles[3]);
		} else
		Player = GUI.TextField(new Rect(36f, 8f, 89f, 21.96f), Player, GUI.skin.customStyles[3]);
		}
		if(tabTabControl2==1){
		if(focusFriendsPanel) { focusFriendsPanel = false; GUI.FocusControl("FriendsPanel");}
		GUI.SetNextControlName("FriendsPanel");
		GUI.Box(new Rect(18f, 24f, 485f, 281f), "");
		GUILayout.BeginArea(new Rect(18f, 24f, 485f, 281f));
		if(onStackPanelDraw13 != null) onStackPanelDraw13();
		GUILayout.EndArea();
		}
		if(tabTabControl2==2){
		if(focusChat) { focusChat = false; GUI.FocusControl("Chat");}
		GUI.SetNextControlName("Chat");
		if(isReadOnlyChat){
		GUI.Label(new Rect(8f, 13.667f, 421f, 264f), Chat.ToString(), GUI.skin.customStyles[2]);
		} else
		Chat = GUI.TextField(new Rect(8f, 13.667f, 421f, 264f), Chat, GUI.skin.customStyles[2]);
		if(focusMsg) { focusMsg = false; GUI.FocusControl("Msg");}
		GUI.SetNextControlName("Msg");
		if(isReadOnlyMsg){
		GUI.Label(new Rect(8f, 283.667f, 421f, 18f), Msg.ToString());
		} else
		Msg = GUI.TextField(new Rect(8f, 283.667f, 421f, 18f), Msg);
		if(focusUsers) { focusUsers = false; GUI.FocusControl("Users");}
		GUI.SetNextControlName("Users");
		GUI.Box(new Rect(438.333f, 13.667f, 72f, 264f), "");
		sUsers = GUI.BeginScrollView(new Rect(438.333f, 13.667f, 72f, 264f), sUsers, new Rect(0,0, 52f, Users.Length* 15f));
		int oldUsers = iUsers;
		iUsers = GUI.SelectionGrid(new Rect(0,0, 52f, Users.Length* 15f), iUsers, Users,1,GUI.skin.customStyles[0]);
		if (iUsers != oldUsers) Action("onUsers",Users[iUsers]);
		GUI.EndScrollView();
		bool oldButton14 = Button14;
		Button14 = GUI.Button(new Rect(438.333f, 283.667f, 75f, 18f), new GUIContent("Send",""));
		if (Button14 != oldButton14 && Button14 ) {Action("onButton14");onButtonClick(); }
		onMouseOver = new Rect(438.333f, 283.667f, 75f, 18f).Contains(Event.current.mousePosition);
		if (oldMouseOverButton14 != onMouseOver && onMouseOver) onOver();
		oldMouseOverButton14 = onMouseOver;
		}
		if(tabTabControl2==3){
		if(focusTop) { focusTop = false; GUI.FocusControl("Top");}
		GUI.SetNextControlName("Top");
		GUI.Box(new Rect(22f, 72f, 474f, 233f), "");
		sTop = GUI.BeginScrollView(new Rect(22f, 72f, 474f, 233f), sTop, new Rect(0,0, 454f, Top.Length* 15f));
		int oldTop = iTop;
		iTop = GUI.SelectionGrid(new Rect(0,0, 454f, Top.Length* 15f), iTop, Top,1,GUI.skin.customStyles[0]);
		if (iTop != oldTop) Action("onTop",Top[iTop]);
		GUI.EndScrollView();
		GUI.Label(new Rect(41f, 24f, 107.22f, 14f), @"Упорядочить по");
		if(focusSortTops) { focusSortTops = false; GUI.FocusControl("SortTops");}
		GUI.SetNextControlName("SortTops");
		GUI.Box(new Rect(151f, 22f, 117f, 31f), "");
		sSortTops = GUI.BeginScrollView(new Rect(151f, 22f, 117f, 31f), sSortTops, new Rect(0,0, 97f, SortTops.Length* 12f));
		int oldSortTops = iSortTops;
		iSortTops = GUI.SelectionGrid(new Rect(0,0, 97f, SortTops.Length* 12f), iSortTops, SortTops,1,GUI.skin.customStyles[0]);
		if (iSortTops != oldSortTops) Action("onSortTops",SortTops[iSortTops]);
		GUI.EndScrollView();
		}
		GUI.EndGroup();
		GUI.EndGroup();
		if (GUI.Button(new Rect(617f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}