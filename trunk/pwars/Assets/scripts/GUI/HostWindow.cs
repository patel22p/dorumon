
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
		
	internal string Name{ get { return PlayerPrefs.GetString("Name", @""); } set { PlayerPrefs.SetString("Name", value); } }
	internal bool vname = true;
	internal bool focusName;
	internal bool isReadOnlyName = false;
	internal bool vport = true;
	internal bool focusPort;
	internal bool isReadOnlyPort = false;
	internal int Port = 5300;
	internal bool vmaxPlayers = true;
	internal bool focusMaxPlayers;
	internal bool isReadOnlyMaxPlayers = false;
	internal int MaxPlayers = 6;
	internal bool vfragLimitText = true;
	internal bool focusFragLimitText;
	internal bool isReadOnlyFragLimitText = true;
	internal string FragLimitText = @"Frag Limit";
	internal bool vmaxFrags = true;
	internal bool focusMaxFrags;
	internal bool isReadOnlyMaxFrags = false;
	internal int MaxFrags = 20;
	internal bool vmaxTime = true;
	internal bool focusMaxTime;
	internal bool isReadOnlyMaxTime = false;
	internal int MaxTime = 15;
	internal bool vKick_if_AFK = true;
	internal bool focusKick_if_AFK;
	internal bool Kick_if_AFK=false;
	internal bool vkickIfErrors = true;
	internal bool focusKickIfErrors;
	internal bool KickIfErrors=false;
	internal bool vmaxPing = true;
	internal bool focusMaxPing;
	internal bool isReadOnlyMaxPing = false;
	internal int MaxPing = 0;
	public Texture ImageImage15;
	internal bool vGameMode = true;
	internal bool focusGameMode;
	public string[] GameMode = new string[] {"None",};
	internal int iGameMode = 0;
	internal bool vMap = true;
	internal bool focusMap;
	public string[] Map = new string[] {"none",};
	internal int iMap = 0;
	internal bool vStartServer = true;
	internal bool focusStartServer;
	internal bool StartServer=false;
	internal bool vums = true;
	internal bool focusUms;
	internal bool vStartup_Level = true;
	internal bool focusStartup_Level;
	internal float Startup_Level = 0f;
	internal bool vStartup_Money = true;
	internal bool focusStartup_Money;
	internal float Startup_Money = 0f;
	internal bool vZombie_Speed = true;
	internal bool focusZombie_Speed;
	internal float Zombie_Speed = 1f;
	internal bool vZombieSpeedFactor = true;
	internal bool focusZombieSpeedFactor;
	internal bool isReadOnlyZombieSpeedFactor = false;
	internal string ZombieSpeedFactor = @"ZombieSpeed";
	internal bool vZombieSpeedFactor_Copy = true;
	internal bool focusZombieSpeedFactor_Copy;
	internal bool isReadOnlyZombieSpeedFactor_Copy = false;
	internal string ZombieSpeedFactor_Copy = @"ZombieDamage";
	internal bool vZombie_Damage = true;
	internal bool focusZombie_Damage;
	internal float Zombie_Damage = 1f;
	private int wndid1;
	private bool oldMouseOverKick_if_AFK;
	private bool oldMouseOverKickIfErrors;
	private Rect Image15;
	private Vector2 sGameMode;
	private Vector2 sMap;
	private bool oldMouseOverStartServer;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image15 = new Rect(0f, 0f, 190f, 143f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-306f + Screen.width/2,-273f + Screen.height/2,615f,465.5f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(24.5f, 57f, 556.5f, 103.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 103.667f), "");
		GUI.Label(new Rect(20f, 16f, 131.334f, 14.667f), @"Server Name");
		if(vname){
		if(focusName) { focusName = false; GUI.FocusControl("Name");}
		GUI.SetNextControlName("Name");
		if(isReadOnlyName){
		GUI.Label(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name.ToString());
		} else
		Name = GUI.TextField(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name,20);
		}
		GUI.Label(new Rect(361.333f, 14f, 72f, 19.334f), @"Port");
		if(vport){
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(isReadOnlyPort){
		GUI.Label(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString());
		} else
		Port = int.Parse(GUI.TextField(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString(),4));
		}
		GUI.Label(new Rect(258f, 79.667f, 72f, 19.334f), @"Players");
		if(vmaxPlayers){
		if(focusMaxPlayers) { focusMaxPlayers = false; GUI.FocusControl("MaxPlayers");}
		GUI.SetNextControlName("MaxPlayers");
		if(isReadOnlyMaxPlayers){
		GUI.Label(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString());
		} else
		MaxPlayers = int.Parse(GUI.TextField(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString(),1));
		}
		GUI.Label(new Rect(463.333f, 14f, 97f, 19.334f), @"5300 default");
		GUI.Label(new Rect(378.666f, 79.667f, 97f, 19.334f), @"Max 9");
		if(vfragLimitText){
		if(focusFragLimitText) { focusFragLimitText = false; GUI.FocusControl("FragLimitText");}
		GUI.SetNextControlName("FragLimitText");
		if(isReadOnlyFragLimitText){
		GUI.Label(new Rect(27f, 53.667f, 95f, 14f), FragLimitText.ToString());
		} else
		FragLimitText = GUI.TextField(new Rect(27f, 53.667f, 95f, 14f), FragLimitText);
		}
		if(vmaxFrags){
		if(focusMaxFrags) { focusMaxFrags = false; GUI.FocusControl("MaxFrags");}
		GUI.SetNextControlName("MaxFrags");
		if(isReadOnlyMaxFrags){
		GUI.Label(new Rect(126f, 50.667f, 56f, 17f), MaxFrags.ToString());
		} else
		MaxFrags = int.Parse(GUI.TextField(new Rect(126f, 50.667f, 56f, 17f), MaxFrags.ToString(),2));
		}
		GUI.Label(new Rect(27f, 81.667f, 97f, 14f), @"Time Limit");
		if(vmaxTime){
		if(focusMaxTime) { focusMaxTime = false; GUI.FocusControl("MaxTime");}
		GUI.SetNextControlName("MaxTime");
		if(isReadOnlyMaxTime){
		GUI.Label(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString());
		} else
		MaxTime = int.Parse(GUI.TextField(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString(),2));
		}
		GUI.Label(new Rect(180f, 81.667f, 61.5f, 14f), @"minutes");
		if(vKick_if_AFK){
		if(focusKick_if_AFK) { focusKick_if_AFK = false; GUI.FocusControl("Kick_if_AFK");}
		GUI.SetNextControlName("Kick_if_AFK");
		bool oldKick_if_AFK = Kick_if_AFK;
		Kick_if_AFK = GUI.Toggle(new Rect(466.886f, 63.707f, 75.089f, 15.96f),Kick_if_AFK, new GUIContent("Kick if AFK",""));
		if (Kick_if_AFK != oldKick_if_AFK ) {Action("Kick_if_AFK");onButtonClick(); }
		onMouseOver = new Rect(466.886f, 63.707f, 75.089f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverKick_if_AFK != onMouseOver && onMouseOver) onOver();
		oldMouseOverKick_if_AFK = onMouseOver;
		}
		if(vkickIfErrors){
		if(focusKickIfErrors) { focusKickIfErrors = false; GUI.FocusControl("KickIfErrors");}
		GUI.SetNextControlName("KickIfErrors");
		bool oldKickIfErrors = KickIfErrors;
		KickIfErrors = GUI.Toggle(new Rect(466.886f, 83.041f, 81.61333f, 15.96f),KickIfErrors, new GUIContent("kick If Errors",""));
		if (KickIfErrors != oldKickIfErrors ) {Action("KickIfErrors");onButtonClick(); }
		onMouseOver = new Rect(466.886f, 83.041f, 81.61333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverKickIfErrors != onMouseOver && onMouseOver) onOver();
		oldMouseOverKickIfErrors = onMouseOver;
		}
		GUI.Label(new Rect(408f, 51f, 59.16f, 21.96f), @"Max Ping");
		if(vmaxPing){
		if(focusMaxPing) { focusMaxPing = false; GUI.FocusControl("MaxPing");}
		GUI.SetNextControlName("MaxPing");
		if(isReadOnlyMaxPing){
		GUI.Label(new Rect(475.666f, 50.667f, 72.834f, 13.04f), MaxPing.ToString());
		} else
		MaxPing = int.Parse(GUI.TextField(new Rect(475.666f, 50.667f, 72.834f, 13.04f), MaxPing.ToString()));
		}
		GUI.Box(new Rect(22f, 43f, 521.243f, 1f),"",GUI.skin.customStyles[4]);
		GUI.EndGroup();
		GUI.Label(new Rect(10.833f, 15f, 155.334f, 14.667f), @"Create Server");
		GUI.BeginGroup(new Rect(25.5f, 179f, 556.5f, 181.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 181.667f), "");
		GUI.BeginGroup(new Rect(342f, 25.897f, 192.5f, 147.77f), "");
		GUI.Box(new Rect(0, 0, 192.5f, 147.77f), "");
		if(ImageImage15!=null)
			GUI.DrawTexture(Image15,ImageImage15, ScaleMode.ScaleToFit);
		GUI.EndGroup();
		GUI.Label(new Rect(24.334f, 8f, 84.001f, 14.667f), @"Game Type");
		if(vGameMode){
		if(focusGameMode) { focusGameMode = false; GUI.FocusControl("GameMode");}
		GUI.SetNextControlName("GameMode");
		GUI.Box(new Rect(24.334f, 19.897f, 194.666f, 153.77f), "");
		sGameMode = GUI.BeginScrollView(new Rect(24.334f, 19.897f, 194.666f, 153.77f), sGameMode, new Rect(0,0, 174.666f, GameMode.Length* 26.9599990844727f));
		int oldGameMode = iGameMode;
		iGameMode = GUI.SelectionGrid(new Rect(0,0, 174.666f, GameMode.Length* 26.9599990844727f), iGameMode, GameMode,1,GUI.skin.customStyles[0]);
		if (iGameMode != oldGameMode) Action("GameMode",GameMode[iGameMode]);
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(223f, 8f, 84.001f, 14.667f), @"Map");
		if(vMap){
		if(focusMap) { focusMap = false; GUI.FocusControl("Map");}
		GUI.SetNextControlName("Map");
		GUI.Box(new Rect(223f, 19.897f, 100f, 153.77f), "");
		sMap = GUI.BeginScrollView(new Rect(223f, 19.897f, 100f, 153.77f), sMap, new Rect(0,0, 80f, Map.Length* 26.9599990844727f));
		int oldMap = iMap;
		iMap = GUI.SelectionGrid(new Rect(0,0, 80f, Map.Length* 26.9599990844727f), iMap, Map,1,GUI.skin.customStyles[0]);
		if (iMap != oldMap) Action("Map",Map[iMap]);
		GUI.EndScrollView();
		}
		GUI.Box(new Rect(329f, 9f, 1f, 166.648f),"",GUI.skin.customStyles[4]);
		GUI.EndGroup();
		if(vStartServer){
		if(focusStartServer) { focusStartServer = false; GUI.FocusControl("StartServer");}
		GUI.SetNextControlName("StartServer");
		bool oldStartServer = StartServer;
		StartServer = GUI.Button(new Rect(479.5f, 430.167f, 101.5f, 28.333f), new GUIContent("Create",""));
		if (StartServer != oldStartServer && StartServer ) {Action("StartServer");onButtonClick(); }
		onMouseOver = new Rect(479.5f, 430.167f, 101.5f, 28.333f).Contains(Event.current.mousePosition);
		if (oldMouseOverStartServer != onMouseOver && onMouseOver) onOver();
		oldMouseOverStartServer = onMouseOver;
		}
		if(vums){
		if(focusUms) { focusUms = false; GUI.FocusControl("Ums");}
		GUI.SetNextControlName("Ums");
		GUI.BeginGroup(new Rect(24.5f, 364.667f, 556.5f, 65.5f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 65.5f), "");
		if(vStartup_Level){
		if(focusStartup_Level) { focusStartup_Level = false; GUI.FocusControl("Startup_Level");}
		GUI.SetNextControlName("Startup_Level");
		Startup_Level = GUI.HorizontalSlider(new Rect(137f, 21.333f, 119f, 16f), Startup_Level, 0f, 50f);
		GUI.Label(new Rect(256f,21.333f,40,15),System.Math.Round(Startup_Level,1).ToString());
		}
		GUI.Label(new Rect(8f, 19.333f, 110f, 16f), @"Startup Level");
		GUI.Label(new Rect(8f, 39.333f, 110f, 16f), @"Startup Money");
		if(vStartup_Money){
		if(focusStartup_Money) { focusStartup_Money = false; GUI.FocusControl("Startup_Money");}
		GUI.SetNextControlName("Startup_Money");
		Startup_Money = GUI.HorizontalSlider(new Rect(137f, 37.333f, 119f, 16f), Startup_Money, 0f, 500f);
		GUI.Label(new Rect(256f,37.333f,40,15),System.Math.Round(Startup_Money,1).ToString());
		}
		if(vZombie_Speed){
		if(focusZombie_Speed) { focusZombie_Speed = false; GUI.FocusControl("Zombie_Speed");}
		GUI.SetNextControlName("Zombie_Speed");
		Zombie_Speed = GUI.HorizontalSlider(new Rect(378f, 39.333f, 119f, 16f), Zombie_Speed, 0.2f, 4f);
		GUI.Label(new Rect(497f,39.333f,40,15),System.Math.Round(Zombie_Speed,1).ToString());
		}
		if(vZombieSpeedFactor){
		if(focusZombieSpeedFactor) { focusZombieSpeedFactor = false; GUI.FocusControl("ZombieSpeedFactor");}
		GUI.SetNextControlName("ZombieSpeedFactor");
		if(isReadOnlyZombieSpeedFactor){
		GUI.Label(new Rect(280f, 41.333f, 125f, 14f), ZombieSpeedFactor.ToString());
		} else
		ZombieSpeedFactor = GUI.TextField(new Rect(280f, 41.333f, 125f, 14f), ZombieSpeedFactor);
		}
		if(vZombieSpeedFactor_Copy){
		if(focusZombieSpeedFactor_Copy) { focusZombieSpeedFactor_Copy = false; GUI.FocusControl("ZombieSpeedFactor_Copy");}
		GUI.SetNextControlName("ZombieSpeedFactor_Copy");
		if(isReadOnlyZombieSpeedFactor_Copy){
		GUI.Label(new Rect(280f, 21.333f, 125f, 14f), ZombieSpeedFactor_Copy.ToString());
		} else
		ZombieSpeedFactor_Copy = GUI.TextField(new Rect(280f, 21.333f, 125f, 14f), ZombieSpeedFactor_Copy);
		}
		if(vZombie_Damage){
		if(focusZombie_Damage) { focusZombie_Damage = false; GUI.FocusControl("Zombie_Damage");}
		GUI.SetNextControlName("Zombie_Damage");
		Zombie_Damage = GUI.HorizontalSlider(new Rect(378f, 19.333f, 119f, 16f), Zombie_Damage, 0.2f, 11f);
		GUI.Label(new Rect(497f,19.333f,40,15),System.Math.Round(Zombie_Damage,1).ToString());
		}
		GUI.Label(new Rect(0f, 1.333f, 133f, 14f), @"Use Map Settings");
		GUI.EndGroup();
		}
		GUI.Box(new Rect(11.5f, 31f, 584.112f, 1f),"",GUI.skin.customStyles[4]);
		if (GUI.Button(new Rect(615f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}