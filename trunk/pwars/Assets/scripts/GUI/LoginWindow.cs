
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static LoginWindow __LoginWindow;
    public static LoginWindow _LoginWindow { get { if (__LoginWindow == null) __LoginWindow = (LoginWindow)MonoBehaviour.FindObjectOfType(typeof(LoginWindow)); return __LoginWindow; } }
}
public enum LoginWindowEnum { LoginAsGuest,Login,Registr, }
public class LoginWindow : WindowBase {
		
	internal string Nick{ get { return PlayerPrefs.GetString(Application.platform +"Nick", @""); } set { PlayerPrefs.SetString(Application.platform +"Nick", value); } }
	internal string LoginNick{ get { return PlayerPrefs.GetString(Application.platform +"LoginNick", @""); } set { PlayerPrefs.SetString(Application.platform +"LoginNick", value); } }
	internal string LoginPassw{ get { return PlayerPrefs.GetString(Application.platform +"LoginPassw", @""); } set { PlayerPrefs.SetString(Application.platform +"LoginPassw", value); } }
	internal string RegNick{ get { return PlayerPrefs.GetString(Application.platform +"RegNick", @""); } set { PlayerPrefs.SetString(Application.platform +"RegNick", value); } }
	internal string RegPassw{ get { return PlayerPrefs.GetString(Application.platform +"RegPassw", @""); } set { PlayerPrefs.SetString(Application.platform +"RegPassw", value); } }
	
	internal int tabTabControl4;
	
	internal bool vNick = true;
	
	internal bool focusNick;
	
	internal bool rNick = false;
	
	internal bool vLoginAsGuest = true;
	
	internal bool focusLoginAsGuest;
	internal bool LoginAsGuest=false;
	
	internal bool vclogin = true;
	
	internal bool focusClogin;
	
	internal bool vLoginNick = true;
	
	internal bool focusLoginNick;
	
	internal bool rLoginNick = false;
	
	internal bool vLogin = true;
	
	internal bool focusLogin;
	internal bool Login=false;
	
	internal bool vLoginPassw = true;
	
	internal bool focusLoginPassw;
	
	internal bool rLoginPassw = false;
	
	internal bool vcreg = true;
	
	internal bool focusCreg;
	
	internal bool vRegNick = true;
	
	internal bool focusRegNick;
	
	internal bool rRegNick = false;
	
	internal bool vRegistr = true;
	
	internal bool focusRegistr;
	internal bool Registr=false;
	
	internal bool vRegPassw = true;
	
	internal bool focusRegPassw;
	
	internal bool rRegPassw = false;
	
	internal bool vbuild = true;
	
	internal bool focusBuild;
	
	internal bool rBuild = true;
	internal string Build = @"Build";
	private int wndid1;
	private bool oldMouseOverLoginAsGuest;
	private bool oldMouseOverLogin;
	private bool oldMouseOverRegistr;
	
    
    
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
		vNick = true;
		vLoginAsGuest = true;
		vclogin = true;
		vLoginNick = true;
		vLogin = true;
		vLoginPassw = true;
		vcreg = true;
		vRegNick = true;
		vRegistr = true;
		vRegPassw = true;
		vbuild = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-302.5f + Screen.width/2,-192f + Screen.height/2,612f,371f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(19f, 21f, 574f, 88f), "");
		GUI.Box(new Rect(0, 0, 574f, 88f), "");
		GUI.Label(new Rect(25f, 22f, 524f, 43f), @"Before I get to the main menu of the game and start playing, you need to login (authentication system).");
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(19f, 130f, 574f, 199f), "");
		GUI.Box(new Rect(0, 0, 574f, 199f), "");
		GUILayout.BeginArea(new Rect(0f, 0, 574f, 18));
		tabTabControl4 = GUILayout.Toolbar(tabTabControl4, new string[] { "Login as Guest","Login","Register", }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));
		GUILayout.EndArea();
		GUI.BeginGroup(new Rect(0, 18, 574f, 181f), "");
		GUI.Box(new Rect(0, 0, 574f, 181f), "");
		if(tabTabControl4==0){
		GUI.Label(new Rect(21f, 21.04f, 498f, 112f), @"Please note: entrance in the ""Visitor "" leaves you many opportunities to play, such as participation in reytenge players and getting degrees.

To login, enter your name (nickname) in the text string ""Name"" and click ""Login"".");
		GUI.Label(new Rect(17f, 137.04f, 47f, 19f), @"Name");
		if(vNick){
		if(focusNick) { focusNick = false; GUI.FocusControl("Nick");}
		GUI.SetNextControlName("Nick");
		if(rNick){
		GUI.Label(new Rect(68f, 137.04f, 189.333f, 19f), Nick.ToString());
		} else
		try {Nick = GUI.TextField(new Rect(68f, 137.04f, 189.333f, 19f), Nick,10);}catch{};
		}
		if(vLoginAsGuest){
		if(focusLoginAsGuest) { focusLoginAsGuest = false; GUI.FocusControl("LoginAsGuest");}
		GUI.SetNextControlName("LoginAsGuest");
		bool oldLoginAsGuest = LoginAsGuest;
		LoginAsGuest = GUI.Button(new Rect(261.333f, 137.04f, 79.667f, 19f), new GUIContent("Login",""));
		if (LoginAsGuest != oldLoginAsGuest && LoginAsGuest ) {Action("LoginAsGuest");onButtonClick(); }
		onMouseOver = new Rect(261.333f, 137.04f, 79.667f, 19f).Contains(Event.current.mousePosition);
		if (oldMouseOverLoginAsGuest != onMouseOver && onMouseOver) onOver();
		oldMouseOverLoginAsGuest = onMouseOver;
		}
		}
		if(tabTabControl4==1){
		if(vclogin){
		if(focusClogin) { focusClogin = false; GUI.FocusControl("Clogin");}
		GUI.SetNextControlName("Clogin");
		GUI.BeginGroup(new Rect(0f, 0f, 564f, 171f), "");
		GUI.Box(new Rect(0, 0, 564f, 171f), "");
		GUI.Label(new Rect(142f, 52.04f, 47f, 19f), @"Name");
		if(vLoginNick){
		if(focusLoginNick) { focusLoginNick = false; GUI.FocusControl("LoginNick");}
		GUI.SetNextControlName("LoginNick");
		if(rLoginNick){
		GUI.Label(new Rect(193f, 52.04f, 189.333f, 19f), LoginNick.ToString());
		} else
		try {LoginNick = GUI.TextField(new Rect(193f, 52.04f, 189.333f, 19f), LoginNick,10);}catch{};
		}
		if(vLogin){
		if(focusLogin) { focusLogin = false; GUI.FocusControl("Login");}
		GUI.SetNextControlName("Login");
		bool oldLogin = Login;
		Login = GUI.Button(new Rect(302.666f, 98.04f, 79.667f, 19f), new GUIContent("Login",""));
		if (Login != oldLogin && Login ) {Action("Login");onButtonClick(); }
		onMouseOver = new Rect(302.666f, 98.04f, 79.667f, 19f).Contains(Event.current.mousePosition);
		if (oldMouseOverLogin != onMouseOver && onMouseOver) onOver();
		oldMouseOverLogin = onMouseOver;
		}
		GUI.Label(new Rect(121f, 75.04f, 68f, 19f), @"Password");
		if(vLoginPassw){
		if(focusLoginPassw) { focusLoginPassw = false; GUI.FocusControl("LoginPassw");}
		GUI.SetNextControlName("LoginPassw");
		if(rLoginPassw){
		GUI.Label(new Rect(193f, 75.04f, 189.333f, 19f), LoginPassw.ToString());
		} else
		try {LoginPassw = GUI.PasswordField(new Rect(193f, 75.04f, 189.333f, 19f), LoginPassw,'*',10);}catch{};
		}
		GUI.EndGroup();
		}
		}
		if(tabTabControl4==2){
		if(vcreg){
		if(focusCreg) { focusCreg = false; GUI.FocusControl("Creg");}
		GUI.SetNextControlName("Creg");
		GUI.BeginGroup(new Rect(0f, 0f, 564f, 171f), "");
		GUI.Box(new Rect(0, 0, 564f, 171f), "");
		GUI.Label(new Rect(142f, 52.04f, 47f, 19f), @"Name");
		if(vRegNick){
		if(focusRegNick) { focusRegNick = false; GUI.FocusControl("RegNick");}
		GUI.SetNextControlName("RegNick");
		if(rRegNick){
		GUI.Label(new Rect(193f, 52.04f, 189.333f, 19f), RegNick.ToString());
		} else
		try {RegNick = GUI.TextField(new Rect(193f, 52.04f, 189.333f, 19f), RegNick,10);}catch{};
		}
		if(vRegistr){
		if(focusRegistr) { focusRegistr = false; GUI.FocusControl("Registr");}
		GUI.SetNextControlName("Registr");
		bool oldRegistr = Registr;
		Registr = GUI.Button(new Rect(302.666f, 122.04f, 79.667f, 19f), new GUIContent("Registr",""));
		if (Registr != oldRegistr && Registr ) {Action("Registr");onButtonClick(); }
		onMouseOver = new Rect(302.666f, 122.04f, 79.667f, 19f).Contains(Event.current.mousePosition);
		if (oldMouseOverRegistr != onMouseOver && onMouseOver) onOver();
		oldMouseOverRegistr = onMouseOver;
		}
		GUI.Label(new Rect(121f, 75.04f, 68f, 19f), @"Password");
		if(vRegPassw){
		if(focusRegPassw) { focusRegPassw = false; GUI.FocusControl("RegPassw");}
		GUI.SetNextControlName("RegPassw");
		if(rRegPassw){
		GUI.Label(new Rect(193f, 75.04f, 189.333f, 19f), RegPassw.ToString());
		} else
		try {RegPassw = GUI.PasswordField(new Rect(193f, 75.04f, 189.333f, 19f), RegPassw,'*',10);}catch{};
		}
		GUI.EndGroup();
		}
		}
		GUI.EndGroup();
		GUI.EndGroup();
		if(vbuild){
		if(focusBuild) { focusBuild = false; GUI.FocusControl("Build");}
		GUI.SetNextControlName("Build");
		if(rBuild){
		GUI.Label(new Rect(19f, 4f, 574f, 17f), Build.ToString(), GUI.skin.customStyles[2]);
		} else
		try {Build = GUI.TextField(new Rect(19f, 4f, 574f, 17f), Build,100, GUI.skin.customStyles[2]);}catch{};
		}
	}


	void Update () {
	
	}
}