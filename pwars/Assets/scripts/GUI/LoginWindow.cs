
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

public class LoginWindow : WindowBase {
		
	internal string Nick{ get { return PlayerPrefs.GetString("Nick", @""); } set { PlayerPrefs.SetString("Nick", value); } }
	internal int tabTabControl4;
	internal bool vEnterAsGuest = true;
	internal bool focusEnterAsGuest;
	internal bool EnterAsGuest=false;
	internal bool vNick = true;
	internal bool focusNick;
	internal bool isReadOnlyNick = false;
	internal bool vLogin = true;
	internal bool focusLogin;
	internal bool Login=false;
	public Texture ImageImage8;
	internal bool vPrev = true;
	internal bool focusPrev;
	internal bool Prev=false;
	internal bool vNext = true;
	internal bool focusNext;
	internal bool Next=false;
	private int wndid1;
	private bool oldMouseOverEnterAsGuest;
	private bool oldMouseOverLogin;
	private Rect Image8;
	private bool oldMouseOverPrev;
	private bool oldMouseOverNext;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image8 = new Rect(-2f, 1.333f, 195f, 171.868f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-302.5f + Screen.width/2,-192f + Screen.height/2,612f,346f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(19f, 21f, 574f, 88f), "");
		GUI.Box(new Rect(0, 0, 574f, 88f), "");
		GUI.Label(new Rect(25f, 22f, 524f, 43f), @"Before I get to the main menu of the game and start playing, you need to login (authentication system).");
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(19f, 130f, 375f, 199f), "");
		GUI.Box(new Rect(0, 0, 375f, 199f), "");
		GUILayout.BeginArea(new Rect(0f, 0, 375, 18));
		tabTabControl4 = GUILayout.Toolbar(tabTabControl4, new string[] { "Login as Guest", }, GUI.skin.customStyles[1], GUILayout.ExpandWidth(false));
		GUILayout.EndArea();
		GUI.BeginGroup(new Rect(0, 18, 375, 181), "");
		GUI.Box(new Rect(0, 0, 375, 181), "");
		if(tabTabControl4==0){
		GUI.Label(new Rect(21f, 21.04f, 327f, 112f), @"Please note: entrance in the ""Visitor "" leaves you many opportunities to play, such as participation in reytenge players and getting degrees.

To login, enter your name (nickname) in the text string ""Name"" and click ""Login"".");
		GUI.Label(new Rect(17f, 137.04f, 47f, 19f), @"Name");
		if(vEnterAsGuest){
		if(focusEnterAsGuest) { focusEnterAsGuest = false; GUI.FocusControl("EnterAsGuest");}
		GUI.SetNextControlName("EnterAsGuest");
		bool oldEnterAsGuest = EnterAsGuest;
		EnterAsGuest = GUI.Button(new Rect(376f, 114.04f, 107f, 19f), new GUIContent("Login",""));
		if (EnterAsGuest != oldEnterAsGuest && EnterAsGuest ) {Action("EnterAsGuest");onButtonClick(); }
		onMouseOver = new Rect(376f, 114.04f, 107f, 19f).Contains(Event.current.mousePosition);
		if (oldMouseOverEnterAsGuest != onMouseOver && onMouseOver) onOver();
		oldMouseOverEnterAsGuest = onMouseOver;
		}
		if(vNick){
		if(focusNick) { focusNick = false; GUI.FocusControl("Nick");}
		GUI.SetNextControlName("Nick");
		if(isReadOnlyNick){
		GUI.Label(new Rect(68f, 137.04f, 189.333f, 19f), Nick.ToString());
		} else
		Nick = GUI.TextField(new Rect(68f, 137.04f, 189.333f, 19f), Nick,10);
		}
		if(vLogin){
		if(focusLogin) { focusLogin = false; GUI.FocusControl("Login");}
		GUI.SetNextControlName("Login");
		bool oldLogin = Login;
		Login = GUI.Button(new Rect(261.333f, 137.04f, 79.667f, 19f), new GUIContent("Login",""));
		if (Login != oldLogin && Login ) {Action("Login");onButtonClick(); }
		onMouseOver = new Rect(261.333f, 137.04f, 79.667f, 19f).Contains(Event.current.mousePosition);
		if (oldMouseOverLogin != onMouseOver && onMouseOver) onOver();
		oldMouseOverLogin = onMouseOver;
		}
		}
		GUI.EndGroup();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(400f, 127f, 193f, 202f), "");
		GUI.Box(new Rect(0, 0, 193f, 202f), "");
		if(ImageImage8!=null)
			GUI.DrawTexture(Image8,ImageImage8, ScaleMode.ScaleToFit);
		if(vPrev){
		if(focusPrev) { focusPrev = false; GUI.FocusControl("Prev");}
		GUI.SetNextControlName("Prev");
		bool oldPrev = Prev;
		Prev = GUI.Button(new Rect(-2f, 176.917f, 68f, 25.083f), new GUIContent("Prev",""));
		if (Prev != oldPrev && Prev ) {Action("Prev");onButtonClick(); }
		onMouseOver = new Rect(-2f, 176.917f, 68f, 25.083f).Contains(Event.current.mousePosition);
		if (oldMouseOverPrev != onMouseOver && onMouseOver) onOver();
		oldMouseOverPrev = onMouseOver;
		}
		if(vNext){
		if(focusNext) { focusNext = false; GUI.FocusControl("Next");}
		GUI.SetNextControlName("Next");
		bool oldNext = Next;
		Next = GUI.Button(new Rect(125f, 176.917f, 68f, 25.083f), new GUIContent("Next",""));
		if (Next != oldNext && Next ) {Action("Next");onButtonClick(); }
		onMouseOver = new Rect(125f, 176.917f, 68f, 25.083f).Contains(Event.current.mousePosition);
		if (oldMouseOverNext != onMouseOver && onMouseOver) onOver();
		oldMouseOverNext = onMouseOver;
		}
		GUI.EndGroup();
	}


	void Update () {
	
	}
}