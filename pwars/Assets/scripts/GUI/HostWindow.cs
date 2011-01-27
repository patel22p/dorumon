
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
		
	public string Name{ get { return PlayerPrefs.GetString("Name", @""); } set { PlayerPrefs.SetString("Name", value); } }
	public float Startup_Level{ get { return PlayerPrefs.GetFloat("Startup_Level", 0f); } set { PlayerPrefs.SetFloat("Startup_Level", value); } }
	public float Startup_Money{ get { return PlayerPrefs.GetFloat("Startup_Money", 0f); } set { PlayerPrefs.SetFloat("Startup_Money", value); } }
	public float Zombie_Speed{ get { return PlayerPrefs.GetFloat("Zombie_Speed", 1f); } set { PlayerPrefs.SetFloat("Zombie_Speed", value); } }
	public float Zombie_Damage{ get { return PlayerPrefs.GetFloat("Zombie_Damage", 1f); } set { PlayerPrefs.SetFloat("Zombie_Damage", value); } }
	public float Zombie_Life{ get { return PlayerPrefs.GetFloat("Zombie_Life", 1f); } set { PlayerPrefs.SetFloat("Zombie_Life", value); } }
	public float ZombiesAtStart{ get { return PlayerPrefs.GetFloat("ZombiesAtStart", 5f); } set { PlayerPrefs.SetFloat("ZombiesAtStart", value); } }
	public float Money_per_frag{ get { return PlayerPrefs.GetFloat("Money_per_frag", 0f); } set { PlayerPrefs.SetFloat("Money_per_frag", value); } }
	[HideInInspector]
	public bool vname = true;
	[HideInInspector]
	public bool focusName;
	[HideInInspector]
	public bool rName = false;
	[HideInInspector]
	public bool vport = true;
	[HideInInspector]
	public bool focusPort;
	[HideInInspector]
	public bool rPort = false;
	[HideInInspector]
	public int Port = 5300;
	[HideInInspector]
	public bool vmaxPlayers = true;
	[HideInInspector]
	public bool focusMaxPlayers;
	[HideInInspector]
	public bool rMaxPlayers = false;
	[HideInInspector]
	public int MaxPlayers = 6;
	[HideInInspector]
	public bool vmaxTime = true;
	[HideInInspector]
	public bool focusMaxTime;
	[HideInInspector]
	public bool rMaxTime = false;
	[HideInInspector]
	public int MaxTime = 15;
	[HideInInspector]
	public bool vKick_if_AFK = true;
	[HideInInspector]
	public bool focusKick_if_AFK;
	internal bool Kick_if_AFK=false;
	[HideInInspector]
	public bool vkickIfErrors = true;
	[HideInInspector]
	public bool focusKickIfErrors;
	internal bool KickIfErrors=false;
	[HideInInspector]
	public bool vmaxPing = true;
	[HideInInspector]
	public bool focusMaxPing;
	[HideInInspector]
	public bool rMaxPing = false;
	[HideInInspector]
	public int MaxPing = 0;
	[HideInInspector]
	public bool vfragCanvas = true;
	[HideInInspector]
	public bool focusFragCanvas;
	[HideInInspector]
	public bool vfragLimitText = true;
	[HideInInspector]
	public bool focusFragLimitText;
	[HideInInspector]
	public bool rFragLimitText = true;
	[HideInInspector]
	public string FragLimitText = @"Frag Limit";
	[HideInInspector]
	public bool vmaxFrags = true;
	[HideInInspector]
	public bool focusMaxFrags;
	[HideInInspector]
	public bool rMaxFrags = false;
	[HideInInspector]
	public int MaxFrags = 20;
	[HideInInspector]
	public bool vGameImage = true;
	[HideInInspector]
	public bool focusGameImage;
	public Texture imgGameImage;
	[HideInInspector]
	public bool vGameMode = true;
	[HideInInspector]
	public bool focusGameMode;
	public string[] lGameMode;
	[HideInInspector]
	public int iGameMode = 0;
	public string GameMode { get { if(lGameMode.Length==0) return ""; return lGameMode[iGameMode]; } set { iGameMode = lGameMode.SelectIndex(value); }}
	[HideInInspector]
	public bool vMap = true;
	[HideInInspector]
	public bool focusMap;
	public string[] lMap;
	[HideInInspector]
	public int iMap = 0;
	public string Map { get { if(lMap.Length==0) return ""; return lMap[iMap]; } set { iMap = lMap.SelectIndex(value); }}
	[HideInInspector]
	public bool vStartServer = true;
	[HideInInspector]
	public bool focusStartServer;
	internal bool StartServer=false;
	[HideInInspector]
	public bool vums = true;
	[HideInInspector]
	public bool focusUms;
	[HideInInspector]
	public bool vStartup_Level = true;
	[HideInInspector]
	public bool focusStartup_Level;
	[HideInInspector]
	public bool vStartup_Money = true;
	[HideInInspector]
	public bool focusStartup_Money;
	[HideInInspector]
	public bool vZombie_Speed = true;
	[HideInInspector]
	public bool focusZombie_Speed;
	[HideInInspector]
	public bool vZombie_Damage = true;
	[HideInInspector]
	public bool focusZombie_Damage;
	[HideInInspector]
	public bool vZombie_Life = true;
	[HideInInspector]
	public bool focusZombie_Life;
	[HideInInspector]
	public bool vzombiesAtStart = true;
	[HideInInspector]
	public bool focusZombiesAtStart;
	[HideInInspector]
	public bool vMoney_per_frag = true;
	[HideInInspector]
	public bool focusMoney_per_frag;
	private int wndid1;
	private bool oldMouseOverKick_if_AFK;
	private bool oldMouseOverKickIfErrors;
	private Rect GameImage;
	private Vector2 sGameMode;
	private Vector2 sMap;
	private bool oldMouseOverStartServer;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		GameImage = new Rect(0f, 0f, 190f, 143f);

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
		GUI.BeginGroup(new Rect(25.5f, 36f, 556.5f, 118f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 118f), "");
		GUI.Label(new Rect(20f, 16f, 131.334f, 14.667f), @"Server Name");
		if(vname){
		if(focusName) { focusName = false; GUI.FocusControl("Name");}
		GUI.SetNextControlName("Name");
		if(rName){
		GUI.Label(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name.ToString());
		} else
		Name = GUI.TextField(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name,20);
		}
		GUI.Label(new Rect(361.333f, 14f, 72f, 19.334f), @"Port");
		if(vport){
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(rPort){
		GUI.Label(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString());
		} else
		Port = int.Parse(GUI.TextField(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString(),4));
		}
		GUI.Label(new Rect(258f, 79.667f, 72f, 19.334f), @"Players");
		if(vmaxPlayers){
		if(focusMaxPlayers) { focusMaxPlayers = false; GUI.FocusControl("MaxPlayers");}
		GUI.SetNextControlName("MaxPlayers");
		if(rMaxPlayers){
		GUI.Label(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString());
		} else
		MaxPlayers = int.Parse(GUI.TextField(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString(),1));
		}
		GUI.Label(new Rect(27f, 81.667f, 97f, 14f), @"Time Limit");
		if(vmaxTime){
		if(focusMaxTime) { focusMaxTime = false; GUI.FocusControl("MaxTime");}
		GUI.SetNextControlName("MaxTime");
		if(rMaxTime){
		GUI.Label(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString());
		} else
		MaxTime = int.Parse(GUI.TextField(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString(),2));
		}
		if(vKick_if_AFK){
		if(focusKick_if_AFK) { focusKick_if_AFK = false; GUI.FocusControl("Kick_if_AFK");}
		GUI.SetNextControlName("Kick_if_AFK");
		bool oldKick_if_AFK = Kick_if_AFK;
		Kick_if_AFK = GUI.Toggle(new Rect(426.886f, 67.707f, 121.614f, 15.96f),Kick_if_AFK, new GUIContent("Kick if AFK",""));
		if (Kick_if_AFK != oldKick_if_AFK ) {Action("Kick_if_AFK");onButtonClick(); }
		onMouseOver = new Rect(426.886f, 67.707f, 121.614f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverKick_if_AFK != onMouseOver && onMouseOver) onOver();
		oldMouseOverKick_if_AFK = onMouseOver;
		}
		if(vkickIfErrors){
		if(focusKickIfErrors) { focusKickIfErrors = false; GUI.FocusControl("KickIfErrors");}
		GUI.SetNextControlName("KickIfErrors");
		bool oldKickIfErrors = KickIfErrors;
		KickIfErrors = GUI.Toggle(new Rect(426.886f, 87.041f, 121.614f, 15.96f),KickIfErrors, new GUIContent("kick If Errors",""));
		if (KickIfErrors != oldKickIfErrors ) {Action("KickIfErrors");onButtonClick(); }
		onMouseOver = new Rect(426.886f, 87.041f, 121.614f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverKickIfErrors != onMouseOver && onMouseOver) onOver();
		oldMouseOverKickIfErrors = onMouseOver;
		}
		GUI.Label(new Rect(408f, 51f, 59.16f, 21.96f), @"Max Ping");
		if(vmaxPing){
		if(focusMaxPing) { focusMaxPing = false; GUI.FocusControl("MaxPing");}
		GUI.SetNextControlName("MaxPing");
		if(rMaxPing){
		GUI.Label(new Rect(475.666f, 50.667f, 72.834f, 13.04f), MaxPing.ToString());
		} else
		MaxPing = int.Parse(GUI.TextField(new Rect(475.666f, 50.667f, 72.834f, 13.04f), MaxPing.ToString(),100));
		}
		GUI.Box(new Rect(22f, 43f, 521.243f, 1f),"",GUI.skin.customStyles[4]);//line
		if(vfragCanvas){
		if(focusFragCanvas) { focusFragCanvas = false; GUI.FocusControl("FragCanvas");}
		GUI.SetNextControlName("FragCanvas");
		GUI.BeginGroup(new Rect(20f, 51f, 173f, 26.667f), "");
		GUI.Box(new Rect(0, 0, 173f, 26.667f), "");
		if(vfragLimitText){
		if(focusFragLimitText) { focusFragLimitText = false; GUI.FocusControl("FragLimitText");}
		GUI.SetNextControlName("FragLimitText");
		if(rFragLimitText){
		GUI.Label(new Rect(6f, 8.667f, 95f, 14f), FragLimitText.ToString());
		} else
		FragLimitText = GUI.TextField(new Rect(6f, 8.667f, 95f, 14f), FragLimitText,100);
		}
		if(vmaxFrags){
		if(focusMaxFrags) { focusMaxFrags = false; GUI.FocusControl("MaxFrags");}
		GUI.SetNextControlName("MaxFrags");
		if(rMaxFrags){
		GUI.Label(new Rect(105f, 5.667f, 56f, 17f), MaxFrags.ToString());
		} else
		MaxFrags = int.Parse(GUI.TextField(new Rect(105f, 5.667f, 56f, 17f), MaxFrags.ToString(),2));
		}
		GUI.EndGroup();
		}
		GUI.EndGroup();
		GUI.Label(new Rect(10.833f, 15f, 155.334f, 14.667f), @"Create Server");
		GUI.BeginGroup(new Rect(25.5f, 158f, 556.5f, 181.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 181.667f), "");
		GUI.BeginGroup(new Rect(348f, 13.897f, 192.5f, 147.77f), "");
		GUI.Box(new Rect(0, 0, 192.5f, 147.77f), "");
		if(vGameImage){
		if(focusGameImage) { focusGameImage = false; GUI.FocusControl("GameImage");}
		GUI.SetNextControlName("GameImage");
		if(imgGameImage!=null)
			GUI.DrawTexture(GameImage,imgGameImage, ScaleMode.ScaleToFit);
		}
		GUI.EndGroup();
		GUI.Label(new Rect(24.334f, 8f, 84.001f, 14.667f), @"Game Type");
		if(vGameMode){
		if(focusGameMode) { focusGameMode = false; GUI.FocusControl("GameMode");}
		GUI.SetNextControlName("GameMode");
		GUI.Box(new Rect(24.334f, 19.897f, 194.666f, 153.77f), "");
		sGameMode = GUI.BeginScrollView(new Rect(24.334f, 19.897f, 194.666f, 153.77f), sGameMode, new Rect(0,0, 174.666f, lGameMode.Length* 26.9599990844727f));
		int oldGameMode = iGameMode;
		iGameMode = GUI.SelectionGrid(new Rect(0,0, 174.666f, lGameMode.Length* 26.9599990844727f), iGameMode, lGameMode,1,GUI.skin.customStyles[0]);
		if (iGameMode != oldGameMode) Action("GameMode");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(223f, 8f, 84.001f, 14.667f), @"Map");
		if(vMap){
		if(focusMap) { focusMap = false; GUI.FocusControl("Map");}
		GUI.SetNextControlName("Map");
		GUI.Box(new Rect(223f, 19.897f, 100f, 153.77f), "");
		sMap = GUI.BeginScrollView(new Rect(223f, 19.897f, 100f, 153.77f), sMap, new Rect(0,0, 80f, lMap.Length* 26.9599990844727f));
		int oldMap = iMap;
		iMap = GUI.SelectionGrid(new Rect(0,0, 80f, lMap.Length* 26.9599990844727f), iMap, lMap,1,GUI.skin.customStyles[0]);
		if (iMap != oldMap) Action("Map");
		GUI.EndScrollView();
		}
		GUI.Box(new Rect(329f, 9f, 1f, 166.648f),"",GUI.skin.customStyles[4]);//line
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
		GUI.BeginGroup(new Rect(25.5f, 343.667f, 556.5f, 82.5f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 82.5f), "");
		if(vStartup_Level){
		if(focusStartup_Level) { focusStartup_Level = false; GUI.FocusControl("Startup_Level");}
		GUI.SetNextControlName("Startup_Level");
		Startup_Level = GUI.HorizontalSlider(new Rect(8f, 24f, 119f, 16f), Startup_Level, 0f, 50f);
		GUI.Label(new Rect(127f,24f,40,15),System.Math.Round(Startup_Level,1).ToString());
		}
		GUI.Label(new Rect(8f, 8f, 110f, 16f), @"Startup Level");
		GUI.Label(new Rect(8f, 44f, 110f, 16f), @"Startup Money");
		if(vStartup_Money){
		if(focusStartup_Money) { focusStartup_Money = false; GUI.FocusControl("Startup_Money");}
		GUI.SetNextControlName("Startup_Money");
		Startup_Money = GUI.HorizontalSlider(new Rect(8f, 60f, 119f, 16f), Startup_Money, 0f, 500f);
		GUI.Label(new Rect(127f,60f,40,15),System.Math.Round(Startup_Money,1).ToString());
		}
		if(vZombie_Speed){
		if(focusZombie_Speed) { focusZombie_Speed = false; GUI.FocusControl("Zombie_Speed");}
		GUI.SetNextControlName("Zombie_Speed");
		Zombie_Speed = GUI.HorizontalSlider(new Rect(143f, 59.5f, 119f, 18.167f), Zombie_Speed, 0.2f, 4f);
		GUI.Label(new Rect(262f,59.5f,40,15),System.Math.Round(Zombie_Speed,1).ToString());
		}
		GUI.Label(new Rect(143f, 45.5f, 125f, 14f), @"ZombieSpeed");
		GUI.Label(new Rect(143f, 8f, 125f, 14f), @"ZombieDamage");
		if(vZombie_Damage){
		if(focusZombie_Damage) { focusZombie_Damage = false; GUI.FocusControl("Zombie_Damage");}
		GUI.SetNextControlName("Zombie_Damage");
		Zombie_Damage = GUI.HorizontalSlider(new Rect(143f, 26f, 119f, 16f), Zombie_Damage, 0.2f, 11f);
		GUI.Label(new Rect(262f,26f,40,15),System.Math.Round(Zombie_Damage,1).ToString());
		}
		GUI.Label(new Rect(283f, 10f, 80f, 14f), @"ZombieLife");
		if(vZombie_Life){
		if(focusZombie_Life) { focusZombie_Life = false; GUI.FocusControl("Zombie_Life");}
		GUI.SetNextControlName("Zombie_Life");
		Zombie_Life = GUI.HorizontalSlider(new Rect(283f, 21.833f, 119f, 18.167f), Zombie_Life, 0.2f, 4f);
		GUI.Label(new Rect(402f,21.833f,40,15),System.Math.Round(Zombie_Life,1).ToString());
		}
		if(vzombiesAtStart){
		if(focusZombiesAtStart) { focusZombiesAtStart = false; GUI.FocusControl("ZombiesAtStart");}
		GUI.SetNextControlName("ZombiesAtStart");
		ZombiesAtStart = GUI.HorizontalSlider(new Rect(283f, 59.5f, 119f, 18.167f), ZombiesAtStart, 5f, 100f);
		GUI.Label(new Rect(402f,59.5f,40,15),System.Math.Round(ZombiesAtStart,1).ToString());
		}
		if(vMoney_per_frag){
		if(focusMoney_per_frag) { focusMoney_per_frag = false; GUI.FocusControl("Money_per_frag");}
		GUI.SetNextControlName("Money_per_frag");
		Money_per_frag = GUI.HorizontalSlider(new Rect(406f, 26f, 119f, 16f), Money_per_frag, 0f, 10f);
		GUI.Label(new Rect(525f,26f,40,15),System.Math.Round(Money_per_frag,1).ToString());
		}
		GUI.Label(new Rect(406f, 10f, 142.5f, 16f), @"Money per frag");
		GUI.EndGroup();
		}
		GUI.Box(new Rect(11.5f, 31f, 584.112f, 1f),"",GUI.skin.customStyles[4]);//line
		if (GUI.Button(new Rect(615f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}