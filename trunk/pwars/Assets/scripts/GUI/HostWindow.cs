
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
public enum HostWindowEnum { Kick_if_AFK,KickIfErrors,GameMode,Map,StartServer,Player_Hit_Slow,Gunlist,Have_A_Laser,Close, }
public class HostWindow : WindowBase {
		
	internal string Name{ get { return PlayerPrefs.GetString(Application.platform +"Name", @"mygame"); } set { PlayerPrefs.SetString(Application.platform +"Name", value); } }
	internal float Startup_Level{ get { return PlayerPrefs.GetFloat(Application.platform +"Startup_Level", 0f); } set { PlayerPrefs.SetFloat(Application.platform +"Startup_Level", value); } }
	internal float Zombie_Speed{ get { return PlayerPrefs.GetFloat(Application.platform +"Zombie_Speed", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"Zombie_Speed", value); } }
	internal float Zombie_Damage{ get { return PlayerPrefs.GetFloat(Application.platform +"Zombie_Damage", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"Zombie_Damage", value); } }
	internal float Zombie_Life{ get { return PlayerPrefs.GetFloat(Application.platform +"Zombie_Life", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"Zombie_Life", value); } }
	internal float ZombiesAtStart{ get { return PlayerPrefs.GetFloat(Application.platform +"ZombiesAtStart", 5f); } set { PlayerPrefs.SetFloat(Application.platform +"ZombiesAtStart", value); } }
	internal float Money_per_frag{ get { return PlayerPrefs.GetFloat(Application.platform +"Money_per_frag", 0f); } set { PlayerPrefs.SetFloat(Application.platform +"Money_per_frag", value); } }
	internal float Money_Per_Level{ get { return PlayerPrefs.GetFloat(Application.platform +"Money_Per_Level", 0f); } set { PlayerPrefs.SetFloat(Application.platform +"Money_Per_Level", value); } }
	
	internal bool vname = true;
	
	internal bool focusName;
	
	internal bool rName = false;
	
	internal bool vport = true;
	
	internal bool focusPort;
	
	internal bool rPort = false;
	internal int Port = 5300;
	
	internal bool vmaxPlayers = true;
	
	internal bool focusMaxPlayers;
	
	internal bool rMaxPlayers = false;
	internal int MaxPlayers = 6;
	
	internal bool vmaxTime = true;
	
	internal bool focusMaxTime;
	
	internal bool rMaxTime = false;
	internal int MaxTime = 999;
	
	internal bool vKick_if_AFK = true;
	
	internal bool focusKick_if_AFK;
	internal bool Kick_if_AFK=true;
	
	internal bool vkickIfErrors = true;
	
	internal bool focusKickIfErrors;
	internal bool KickIfErrors=false;
	
	internal bool vmaxPing = true;
	
	internal bool focusMaxPing;
	
	internal bool rMaxPing = false;
	internal int MaxPing = 600;
	
	internal bool vfragCanvas = true;
	
	internal bool focusFragCanvas;
	
	internal bool vfragLimitText = true;
	
	internal bool focusFragLimitText;
	
	internal bool rFragLimitText = true;
	internal string FragLimitText = @"Frag Limit";
	
	internal bool vmaxFrags = true;
	
	internal bool focusMaxFrags;
	
	internal bool rMaxFrags = false;
	internal int MaxFrags = 20;
	
	internal bool vGameImage = true;
	
	internal bool focusGameImage;
	public Texture imgGameImage;
	
	internal bool vGameMode = true;
	
	internal bool focusGameMode;
	public string[] lGameMode;
	internal int iGameMode = 0;
	public string GameMode { get { if(lGameMode.Length==0 || iGameMode == -1) return ""; return lGameMode[iGameMode]; } set { iGameMode = lGameMode.SelectIndex(value); }}
	
	internal bool vMap = true;
	
	internal bool focusMap;
	public string[] lMap;
	internal int iMap = 0;
	public string Map { get { if(lMap.Length==0 || iMap == -1) return ""; return lMap[iMap]; } set { iMap = lMap.SelectIndex(value); }}
	
	internal bool vStartServer = true;
	
	internal bool focusStartServer;
	internal bool StartServer=false;
	
	internal int tabTabControl14;
	
	internal bool vdescription = true;
	
	internal bool focusDescription;
	
	internal bool rDescription = true;
	internal string Description = @"";
	
	internal bool vPlayer_Hit_Slow = true;
	
	internal bool focusPlayer_Hit_Slow;
	internal bool Player_Hit_Slow=false;
	
	internal bool vcommon = true;
	
	internal bool focusCommon;
	
	internal bool vStartMoney = true;
	
	internal bool focusStartMoney;
	internal float StartMoney = 0f;
	
	internal bool vMoney_per_playerKill = true;
	
	internal bool focusMoney_per_playerKill;
	internal float Money_per_playerKill = 0f;
	
	internal bool vgunlist = true;
	
	internal bool focusGunlist;
	public string[] lGunlist;
	internal int iGunlist = -1;
	public string Gunlist { get { if(lGunlist.Length==0 || iGunlist == -1) return ""; return lGunlist[iGunlist]; } set { iGunlist = lGunlist.SelectIndex(value); }}
	
	internal bool vgunBullets = true;
	
	internal bool focusGunBullets;
	
	internal bool rGunBullets = false;
	internal int GunBullets = 0;
	
	internal bool vdamageFactor = true;
	
	internal bool focusDamageFactor;
	internal float DamageFactor = 1f;
	
	internal bool vHave_A_Laser = true;
	
	internal bool focusHave_A_Laser;
	internal bool Have_A_Laser=false;
	
	internal bool vums = true;
	
	internal bool focusUms;
	
	internal bool vStartup_Level = true;
	
	internal bool focusStartup_Level;
	
	internal bool vZombie_Speed = true;
	
	internal bool focusZombie_Speed;
	
	internal bool vZombie_Damage = true;
	
	internal bool focusZombie_Damage;
	
	internal bool vZombie_Life = true;
	
	internal bool focusZombie_Life;
	
	internal bool vzombiesAtStart = true;
	
	internal bool focusZombiesAtStart;
	
	internal bool vMoney_per_frag = true;
	
	internal bool focusMoney_per_frag;
	
	internal bool vMoney_Per_Level = true;
	
	internal bool focusMoney_Per_Level;
	private int wndid1;
	private bool oldMouseOverKick_if_AFK;
	private bool oldMouseOverKickIfErrors;
	private Rect GameImage;
	private Vector2 sGameMode;
	private Vector2 sMap;
	private bool oldMouseOverStartServer;
	private bool oldMouseOverPlayer_Hit_Slow;
	private Vector2 sGunlist;
	private bool oldMouseOverHave_A_Laser;
	
    
    
	void Start () {
		AlwaysOnTop = false;
		wndid1 = 0;
		GameImage = new Rect(0f, 0f, 190f, 143f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
		vname = true;
		vport = true;
		vmaxPlayers = true;
		vmaxTime = true;
		vKick_if_AFK = true;
		vkickIfErrors = true;
		vmaxPing = true;
		vfragCanvas = true;
		vfragLimitText = true;
		vmaxFrags = true;
		vGameImage = true;
		vGameMode = true;
		iGameMode = -1;
		vMap = true;
		iMap = -1;
		vStartServer = true;
		vdescription = true;
		vPlayer_Hit_Slow = true;
		vcommon = true;
		vStartMoney = true;
		vMoney_per_playerKill = true;
		vgunlist = true;
		iGunlist = -1;
		vgunBullets = true;
		vdamageFactor = true;
		vHave_A_Laser = true;
		vums = true;
		vStartup_Level = true;
		vZombie_Speed = true;
		vZombie_Damage = true;
		vZombie_Life = true;
		vzombiesAtStart = true;
		vMoney_per_frag = true;
		vMoney_Per_Level = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-312f + Screen.width/2,-244.5f + Screen.height/2,615f,490.5f), Wnd1,"");
		base.OnGUI();
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
		try {Name = GUI.TextField(new Rect(144.667f, 12.667f, 185.333f, 20.667f), Name,20);}catch{};
		}
		GUI.Label(new Rect(361.333f, 14f, 72f, 19.334f), @"Port");
		if(vport){
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(rPort){
		GUI.Label(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString());
		} else
		try {Port = int.Parse(GUI.TextField(new Rect(400.667f, 10f, 62.666f, 20.667f), Port.ToString(),4));}catch{};
		}
		GUI.Label(new Rect(258f, 79.667f, 72f, 19.334f), @"Players");
		if(vmaxPlayers){
		if(focusMaxPlayers) { focusMaxPlayers = false; GUI.FocusControl("MaxPlayers");}
		GUI.SetNextControlName("MaxPlayers");
		if(rMaxPlayers){
		GUI.Label(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString());
		} else
		try {MaxPlayers = int.Parse(GUI.TextField(new Rect(316f, 75.667f, 62.666f, 20.667f), MaxPlayers.ToString(),1));}catch{};
		}
		GUI.Label(new Rect(27f, 81.667f, 97f, 14f), @"Time Limit");
		if(vmaxTime){
		if(focusMaxTime) { focusMaxTime = false; GUI.FocusControl("MaxTime");}
		GUI.SetNextControlName("MaxTime");
		if(rMaxTime){
		GUI.Label(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString());
		} else
		try {MaxTime = int.Parse(GUI.TextField(new Rect(126f, 78.667f, 56f, 17f), MaxTime.ToString(),2));}catch{};
		}
		if(vKick_if_AFK){
		if(focusKick_if_AFK) { focusKick_if_AFK = false; GUI.FocusControl("Kick_if_AFK");}
		GUI.SetNextControlName("Kick_if_AFK");
		bool oldKick_if_AFK = Kick_if_AFK;
		Kick_if_AFK = GUI.Toggle(new Rect(426.886f, 67.707f, 121.614f, 15.96f),Kick_if_AFK, new GUIContent(@"Kick if AFK",""));
		if (Kick_if_AFK != oldKick_if_AFK ) {Action("Kick_if_AFK");onButtonClick(); }
		onMouseOver = new Rect(426.886f, 67.707f, 121.614f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverKick_if_AFK != onMouseOver && onMouseOver) onOver();
		oldMouseOverKick_if_AFK = onMouseOver;
		}
		if(vkickIfErrors){
		if(focusKickIfErrors) { focusKickIfErrors = false; GUI.FocusControl("KickIfErrors");}
		GUI.SetNextControlName("KickIfErrors");
		bool oldKickIfErrors = KickIfErrors;
		KickIfErrors = GUI.Toggle(new Rect(426.886f, 87.041f, 121.614f, 15.96f),KickIfErrors, new GUIContent(@"kick If Errors",""));
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
		try {MaxPing = int.Parse(GUI.TextField(new Rect(475.666f, 50.667f, 72.834f, 13.04f), MaxPing.ToString(),100));}catch{};
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
		try {FragLimitText = GUI.TextField(new Rect(6f, 8.667f, 95f, 14f), FragLimitText,100);}catch{};
		}
		if(vmaxFrags){
		if(focusMaxFrags) { focusMaxFrags = false; GUI.FocusControl("MaxFrags");}
		GUI.SetNextControlName("MaxFrags");
		if(rMaxFrags){
		GUI.Label(new Rect(105f, 5.667f, 56f, 17f), MaxFrags.ToString());
		} else
		try {MaxFrags = int.Parse(GUI.TextField(new Rect(105f, 5.667f, 56f, 17f), MaxFrags.ToString(),2));}catch{};
		}
		GUI.EndGroup();
		}
		GUI.EndGroup();
		GUI.Label(new Rect(10.833f, 15f, 155.334f, 14.667f), @"Create Server");
		GUI.BeginGroup(new Rect(25.5f, 158f, 556.5f, 169.667f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 169.667f), "");
		GUI.BeginGroup(new Rect(348f, 13.897f, 192.5f, 147.77f), "");
		GUI.Box(new Rect(0, 0, 192.5f, 147.77f), "");
		if(vGameImage){
		if(focusGameImage) { focusGameImage = false; GUI.FocusControl("GameImage");}
		GUI.SetNextControlName("GameImage");
		if(imgGameImage!=null)
			GUI.DrawTexture(GameImage,imgGameImage, ScaleMode.ScaleToFit, imgGameImage is RenderTexture?false:true);
		}
		GUI.EndGroup();
		GUI.Label(new Rect(24.334f, 8f, 84.001f, 14.667f), @"Game Type");
		if(vGameMode){
		if(focusGameMode) { focusGameMode = false; GUI.FocusControl("GameMode");}
		GUI.SetNextControlName("GameMode");
		GUI.Box(new Rect(24.334f, 19.897f, 194.666f, 141.77f), "");
		sGameMode = GUI.BeginScrollView(new Rect(24.334f, 19.897f, 194.666f, 141.77f), sGameMode, new Rect(0,0, 184.666f, lGameMode.Length* 26.9599990844727f));
		int oldGameMode = iGameMode;
		iGameMode = GUI.SelectionGrid(new Rect(0,0, 184.666f, lGameMode.Length* 26.9599990844727f), iGameMode, lGameMode,1,GUI.skin.customStyles[0]);
		if (iGameMode != oldGameMode) Action("GameMode");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(223f, 8f, 84.001f, 14.667f), @"Map");
		if(vMap){
		if(focusMap) { focusMap = false; GUI.FocusControl("Map");}
		GUI.SetNextControlName("Map");
		GUI.Box(new Rect(223f, 19.897f, 100f, 141.77f), "");
		sMap = GUI.BeginScrollView(new Rect(223f, 19.897f, 100f, 141.77f), sMap, new Rect(0,0, 90f, lMap.Length* 26.9599990844727f));
		int oldMap = iMap;
		iMap = GUI.SelectionGrid(new Rect(0,0, 90f, lMap.Length* 26.9599990844727f), iMap, lMap,1,GUI.skin.customStyles[0]);
		if (iMap != oldMap) Action("Map");
		GUI.EndScrollView();
		}
		GUI.EndGroup();
		if(vStartServer){
		if(focusStartServer) { focusStartServer = false; GUI.FocusControl("StartServer");}
		GUI.SetNextControlName("StartServer");
		bool oldStartServer = StartServer;
		StartServer = GUI.Button(new Rect(480.5f, 455.834f, 101.5f, 28.333f), new GUIContent(@"Start",""));
		if (StartServer != oldStartServer && StartServer ) {Action("StartServer");onButtonClick(); }
		onMouseOver = new Rect(480.5f, 455.834f, 101.5f, 28.333f).Contains(Event.current.mousePosition);
		if (oldMouseOverStartServer != onMouseOver && onMouseOver) onOver();
		oldMouseOverStartServer = onMouseOver;
		}
		GUI.BeginGroup(new Rect(25.5f, 331.667f, 556.5f, 120.167f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 120.167f), "");
		GUILayout.BeginArea(new Rect(0f, 0, 556.5f, 18));
		tabTabControl14 = GUILayout.Toolbar(tabTabControl14, new string[] { "Description","Common Settings","Zombie Custom", }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));
		GUILayout.EndArea();
		GUI.BeginGroup(new Rect(0, 18, 556.5f, 102.167f), "");
		GUI.Box(new Rect(0, 0, 556.5f, 102.167f), "");
		if(tabTabControl14==0){
		if(vdescription){
		if(focusDescription) { focusDescription = false; GUI.FocusControl("Description");}
		GUI.SetNextControlName("Description");
		if(rDescription){
		GUI.Label(new Rect(8f, 6f, 538.5f, 53.333f), Description.ToString(), GUI.skin.customStyles[2]);
		} else
		try {Description = GUI.TextField(new Rect(8f, 6f, 538.5f, 53.333f), Description,100, GUI.skin.customStyles[2]);}catch{};
		}
		if(vPlayer_Hit_Slow){
		if(focusPlayer_Hit_Slow) { focusPlayer_Hit_Slow = false; GUI.FocusControl("Player_Hit_Slow");}
		GUI.SetNextControlName("Player_Hit_Slow");
		bool oldPlayer_Hit_Slow = Player_Hit_Slow;
		Player_Hit_Slow = GUI.Toggle(new Rect(393f, 59.333f, 145.5f, 28.834f),Player_Hit_Slow, new GUIContent(@"Player Slow onDmg
(hard/easy)",""));
		if (Player_Hit_Slow != oldPlayer_Hit_Slow ) {Action("Player_Hit_Slow");onButtonClick(); }
		onMouseOver = new Rect(393f, 59.333f, 145.5f, 28.834f).Contains(Event.current.mousePosition);
		if (oldMouseOverPlayer_Hit_Slow != onMouseOver && onMouseOver) onOver();
		oldMouseOverPlayer_Hit_Slow = onMouseOver;
		}
		}
		if(tabTabControl14==1){
		if(vcommon){
		if(focusCommon) { focusCommon = false; GUI.FocusControl("Common");}
		GUI.SetNextControlName("Common");
		GUI.BeginGroup(new Rect(0f, 0f, 546.5f, 92.167f), "");
		GUI.Box(new Rect(0, 0, 546.5f, 92.167f), "");
		GUI.Label(new Rect(8f, 8f, 76f, 14f), @"StartMoney");
		if(vStartMoney){
		if(focusStartMoney) { focusStartMoney = false; GUI.FocusControl("StartMoney");}
		GUI.SetNextControlName("StartMoney");
		StartMoney = GUI.HorizontalSlider(new Rect(8f, 22f, 119f, 16f), StartMoney, 0f, 500f);
		GUI.Label(new Rect(127f,22f,40,15),System.Math.Round(StartMoney,1).ToString());
		}
		if(vMoney_per_playerKill){
		if(focusMoney_per_playerKill) { focusMoney_per_playerKill = false; GUI.FocusControl("Money_per_playerKill");}
		GUI.SetNextControlName("Money_per_playerKill");
		Money_per_playerKill = GUI.HorizontalSlider(new Rect(152f, 60f, 119f, 16f), Money_per_playerKill, 0f, 100f);
		GUI.Label(new Rect(271f,60f,40,15),System.Math.Round(Money_per_playerKill,1).ToString());
		}
		GUI.Label(new Rect(152f, 44f, 168.5f, 16f), @"Money per player kill");
		if(vgunlist){
		if(focusGunlist) { focusGunlist = false; GUI.FocusControl("Gunlist");}
		GUI.SetNextControlName("Gunlist");
		GUI.Box(new Rect(442f, 8f, 96.5f, 76.167f), "");
		sGunlist = GUI.BeginScrollView(new Rect(442f, 8f, 96.5f, 76.167f), sGunlist, new Rect(0,0, 86.5f, lGunlist.Length* 15f));
		int oldGunlist = iGunlist;
		iGunlist = GUI.SelectionGrid(new Rect(0,0, 86.5f, lGunlist.Length* 15f), iGunlist, lGunlist,1,GUI.skin.customStyles[0]);
		if (iGunlist != oldGunlist) Action("Gunlist");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(355f, 8f, 83f, 0f), @"Gun bullets");
		if(vgunBullets){
		if(focusGunBullets) { focusGunBullets = false; GUI.FocusControl("GunBullets");}
		GUI.SetNextControlName("GunBullets");
		if(rGunBullets){
		GUI.Label(new Rect(355f, 26f, 72f, 14f), GunBullets.ToString());
		} else
		try {GunBullets = int.Parse(GUI.TextField(new Rect(355f, 26f, 72f, 14f), GunBullets.ToString(),100));}catch{};
		}
		GUI.Label(new Rect(8f, 44f, 103f, 14f), @"Damage Factor");
		if(vdamageFactor){
		if(focusDamageFactor) { focusDamageFactor = false; GUI.FocusControl("DamageFactor");}
		GUI.SetNextControlName("DamageFactor");
		DamageFactor = GUI.HorizontalSlider(new Rect(8f, 58f, 119f, 16f), DamageFactor, 0f, 3f);
		GUI.Label(new Rect(127f,58f,40,15),System.Math.Round(DamageFactor,1).ToString());
		}
		if(vHave_A_Laser){
		if(focusHave_A_Laser) { focusHave_A_Laser = false; GUI.FocusControl("Have_A_Laser");}
		GUI.SetNextControlName("Have_A_Laser");
		bool oldHave_A_Laser = Have_A_Laser;
		Have_A_Laser = GUI.Toggle(new Rect(321.5f, 64f, 105.5f, 13f),Have_A_Laser, new GUIContent(@"Have A Laser",""));
		if (Have_A_Laser != oldHave_A_Laser ) {Action("Have_A_Laser");onButtonClick(); }
		onMouseOver = new Rect(321.5f, 64f, 105.5f, 13f).Contains(Event.current.mousePosition);
		if (oldMouseOverHave_A_Laser != onMouseOver && onMouseOver) onOver();
		oldMouseOverHave_A_Laser = onMouseOver;
		}
		GUI.EndGroup();
		}
		}
		if(tabTabControl14==2){
		if(vums){
		if(focusUms) { focusUms = false; GUI.FocusControl("Ums");}
		GUI.SetNextControlName("Ums");
		GUI.BeginGroup(new Rect(0f, 0f, 546.5f, 92.167f), "");
		GUI.Box(new Rect(0, 0, 546.5f, 92.167f), "");
		if(vStartup_Level){
		if(focusStartup_Level) { focusStartup_Level = false; GUI.FocusControl("Startup_Level");}
		GUI.SetNextControlName("Startup_Level");
		Startup_Level = GUI.HorizontalSlider(new Rect(8f, 24f, 119f, 16f), Startup_Level, 0f, 50f);
		GUI.Label(new Rect(127f,24f,40,15),System.Math.Round(Startup_Level,1).ToString());
		}
		GUI.Label(new Rect(8f, 8f, 110f, 16f), @"Stage");
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
		GUI.Label(new Rect(286f, 47.333f, 118f, 14f), @"Zombies At Start");
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
		if(vMoney_Per_Level){
		if(focusMoney_Per_Level) { focusMoney_Per_Level = false; GUI.FocusControl("Money_Per_Level");}
		GUI.SetNextControlName("Money_Per_Level");
		Money_Per_Level = GUI.HorizontalSlider(new Rect(8f, 61.333f, 119f, 16f), Money_Per_Level, 0f, 50f);
		GUI.Label(new Rect(127f,61.333f,40,15),System.Math.Round(Money_Per_Level,1).ToString());
		}
		GUI.Label(new Rect(8f, 45.333f, 131f, 16f), @"Money * Stage");
		GUI.EndGroup();
		}
		}
		GUI.EndGroup();
		GUI.EndGroup();
		GUI.Box(new Rect(11.5f, 31f, 584.112f, 1f),"",GUI.skin.customStyles[4]);//line
		if (GUI.Button(new Rect(615f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}