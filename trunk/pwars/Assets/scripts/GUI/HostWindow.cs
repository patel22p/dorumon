
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static HostWindow __HostWindow;
    public static HostWindow _HostWindow { get { if (__HostWindow == null) __HostWindow = (HostWindow)MonoBehaviour.FindObjectOfType(typeof(HostWindow)); return __HostWindow; } }
}

public class HostWindow : WindowBase {
		
	private string _Name;
	internal string Name{ get { if(_Name == null) _Name = PlayerPrefs.GetString("Name", @""); return _Name; } set { PlayerPrefs.SetString("Name", value); _Name = value; } }
	internal bool focusName;
	internal bool isReadOnlyName = false;
	internal bool focusPort;
	internal bool isReadOnlyPort = false;
	internal int Port = 5300;
	internal bool focusMaxPlayers;
	internal bool isReadOnlyMaxPlayers = false;
	internal int MaxPlayers = 6;
	internal bool focusMaxFrags;
	internal bool isReadOnlyMaxFrags = false;
	internal int MaxFrags = 20;
	internal bool focusMaxTime;
	internal bool isReadOnlyMaxTime = false;
	internal int MaxTime = 15;
	internal bool focusGameMode;
	public string[] GameMode = new string[] {"����� ���������","����� ��������� ����","���������","��������� ����",};
	internal int iGameMode = 0;
	internal bool focusMap;
	public string[] Map = new string[] {"none",};
	internal int iMap = 0;
	internal int tabTabControl15;
	[FindAsset("Image3")]
	public Texture2D ImageImage16;
	internal bool focusStartServer;
	internal bool StartServer=false;
	private int wndid1;
	private Vector2 sGameMode;
	private Vector2 sMap;
	private Rect Image16;
	private bool oldMouseOverStartServer;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image16 = new Rect(0f, 0f, 190f, 143f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-305f + Screen.width/2,-216f + Screen.height/2,615f,422.5f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(24.5f, 57f, 556.5f, 103.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 103.667f), "");
		GUI.Label(new Rect(20f, 16f, 131.334f, 14.667f), @"Server Name");
		if(focusName) { focusName = false; GUI.FocusControl("Name");}
		GUI.SetNextControlName("Name");
		if(isReadOnlyName){
		GUI.Label(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name.ToString());
		} else
		Name = GUI.TextField(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name,20);
		GUI.Label(new Rect(361.333f, 14f, 72f, 19.334f), @"Port");
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(isReadOnlyPort){
		GUI.Label(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString());
		} else
		Port = int.Parse(GUI.TextField(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString(),4));
		GUI.Label(new Rect(241.5f, 76.333f, 72f, 19.334f), @"Players");
		if(focusMaxPlayers) { focusMaxPlayers = false; GUI.FocusControl("MaxPlayers");}
		GUI.SetNextControlName("MaxPlayers");
		if(isReadOnlyMaxPlayers){
		GUI.Label(new Rect(299.5f, 72.333f, 62.666f, 20.667f), MaxPlayers.ToString());
		} else
		MaxPlayers = int.Parse(GUI.TextField(new Rect(299.5f, 72.333f, 62.666f, 20.667f), MaxPlayers.ToString(),1));
		GUI.Label(new Rect(463.333f, 14f, 97f, 19.334f), @"5300 default");
		GUI.Label(new Rect(362.166f, 76.333f, 97f, 19.334f), @"Max 9");
		GUI.Label(new Rect(27f, 53.667f, 95f, 14f), @"Frag Limit");
		if(focusMaxFrags) { focusMaxFrags = false; GUI.FocusControl("MaxFrags");}
		GUI.SetNextControlName("MaxFrags");
		if(isReadOnlyMaxFrags){
		GUI.Label(new Rect(126f, 50.667f, 56f, 17f), MaxFrags.ToString());
		} else
		MaxFrags = int.Parse(GUI.TextField(new Rect(126f, 50.667f, 56f, 17f), MaxFrags.ToString(),2));
		GUI.Label(new Rect(16f, 79.667f, 105f, 14f), @"Time Limit");
		if(focusMaxTime) { focusMaxTime = false; GUI.FocusControl("MaxTime");}
		GUI.SetNextControlName("MaxTime");
		if(isReadOnlyMaxTime){
		GUI.Label(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString());
		} else
		MaxTime = int.Parse(GUI.TextField(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString(),2));
		GUI.Label(new Rect(180f, 81.667f, 61.5f, 14f), @"minutes");
		GUI.EndGroup();
		GUI.Label(new Rect(10.833f, 15f, 155.334f, 14.667f), @"Create Server");
		GUI.BeginGroup(new Rect(25.5f, 179f, 556.5f, 181.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 181.667f), "");
		GUI.Label(new Rect(47f, 0f, 84.001f, 14.667f), @"Game Type");
		if(focusGameMode) { focusGameMode = false; GUI.FocusControl("GameMode");}
		GUI.SetNextControlName("GameMode");
		GUI.Box(new Rect(24.334f, 19.897f, 194.666f, 153.77f), "");
		sGameMode = GUI.BeginScrollView(new Rect(24.334f, 19.897f, 194.666f, 153.77f), sGameMode, new Rect(0,0, 174.666f, GameMode.Length* 26.9599990844727f));
		int oldGameMode = iGameMode;
		iGameMode = GUI.SelectionGrid(new Rect(0,0, 174.666f, GameMode.Length* 26.9599990844727f), iGameMode, GameMode,1,GUI.skin.customStyles[0]);
		if (iGameMode != oldGameMode) Action("onGameMode",GameMode[iGameMode]);
		GUI.EndScrollView();
		GUI.Label(new Rect(213.667f, 0f, 84.001f, 14.667f), @"Map");
		if(focusMap) { focusMap = false; GUI.FocusControl("Map");}
		GUI.SetNextControlName("Map");
		GUI.Box(new Rect(223f, 19.897f, 100f, 153.77f), "");
		sMap = GUI.BeginScrollView(new Rect(223f, 19.897f, 100f, 153.77f), sMap, new Rect(0,0, 80f, Map.Length* 26.9599990844727f));
		int oldMap = iMap;
		iMap = GUI.SelectionGrid(new Rect(0,0, 80f, Map.Length* 26.9599990844727f), iMap, Map,1,GUI.skin.customStyles[0]);
		if (iMap != oldMap) Action("onMap",Map[iMap]);
		GUI.EndScrollView();
		GUI.BeginGroup(new Rect(346f, 19.897f, 202.5f, 153.77f), "");
		GUI.Box(new Rect(0, 0, 202.5f, 153.77f), "");
		if(tabTabControl15==0){
		GUI.DrawTexture(Image16,ImageImage16, ScaleMode.ScaleToFit);
		}
		GUI.EndGroup();
		GUI.EndGroup();
		if(focusStartServer) { focusStartServer = false; GUI.FocusControl("StartServer");}
		GUI.SetNextControlName("StartServer");
		bool oldStartServer = StartServer;
		StartServer = GUI.Button(new Rect(480.5f, 376.667f, 101.5f, 28.333f), new GUIContent("Create",""));
		if (StartServer != oldStartServer && StartServer ) {Action("onStartServer");onButtonClick(); }
		onMouseOver = new Rect(480.5f, 376.667f, 101.5f, 28.333f).Contains(Event.current.mousePosition);
		if (oldMouseOverStartServer != onMouseOver && onMouseOver) onOver();
		oldMouseOverStartServer = onMouseOver;
		if (GUI.Button(new Rect(615f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("onClose"); }
	}


	void Update () {
	
	}
}