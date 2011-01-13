
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static GameMenuWindow __GameMenuWindow;
    public static GameMenuWindow _GameMenuWindow { get { if (__GameMenuWindow == null) __GameMenuWindow = (GameMenuWindow)MonoBehaviour.FindObjectOfType(typeof(GameMenuWindow)); return __GameMenuWindow; } }
}

public class GameMenuWindow : WindowBase {
		
	[FindAsset("Skin/Images/physx_wars_title.png")]
	public Texture2D ImageImage2;
	public bool focusTeamSelectButton;
	public bool TeamSelectButton=false;
	public bool focusIrcChatButton;
	public bool IrcChatButton=false;
	public bool focusOptions;
	public bool Options=false;
	public bool focusDisconnect;
	public bool Disconnect=false;
	private int wndid1;
	private Rect Image2;
	private bool oldMouseOverTeamSelectButton;
	private bool oldMouseOverIrcChatButton;
	private bool oldMouseOverOptions;
	private bool oldMouseOverDisconnect;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image2 = new Rect(128f, 0f, 250f, 104.348f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-279.5f + Screen.width/2,-226f + Screen.height/2,509.968f,457f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.DrawTexture(Image2,ImageImage2, ScaleMode.ScaleToFit);
		if(focusTeamSelectButton) { focusTeamSelectButton = false; GUI.FocusControl("TeamSelectButton");}
		GUI.SetNextControlName("TeamSelectButton");
		bool oldTeamSelectButton = TeamSelectButton;
		TeamSelectButton = GUI.Button(new Rect(110f, 156f, 313f, 54f), new GUIContent("Select Team",""));
		if (TeamSelectButton != oldTeamSelectButton && TeamSelectButton ) {Action("onTeamSelectButton");onButtonClick(); }
		onMouseOver = new Rect(110f, 156f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverTeamSelectButton != onMouseOver && onMouseOver) onOver();
		oldMouseOverTeamSelectButton = onMouseOver;
		if(focusIrcChatButton) { focusIrcChatButton = false; GUI.FocusControl("IrcChatButton");}
		GUI.SetNextControlName("IrcChatButton");
		bool oldIrcChatButton = IrcChatButton;
		IrcChatButton = GUI.Button(new Rect(110f, 214f, 313f, 54f), new GUIContent("Irc Chat",""));
		if (IrcChatButton != oldIrcChatButton && IrcChatButton ) {Action("onIrcChatButton");onButtonClick(); }
		onMouseOver = new Rect(110f, 214f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverIrcChatButton != onMouseOver && onMouseOver) onOver();
		oldMouseOverIrcChatButton = onMouseOver;
		if(focusOptions) { focusOptions = false; GUI.FocusControl("Options");}
		GUI.SetNextControlName("Options");
		bool oldOptions = Options;
		Options = GUI.Button(new Rect(110f, 272f, 313f, 54f), new GUIContent("Options",""));
		if (Options != oldOptions && Options ) {Action("onOptions");onButtonClick(); }
		onMouseOver = new Rect(110f, 272f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverOptions != onMouseOver && onMouseOver) onOver();
		oldMouseOverOptions = onMouseOver;
		if(focusDisconnect) { focusDisconnect = false; GUI.FocusControl("Disconnect");}
		GUI.SetNextControlName("Disconnect");
		bool oldDisconnect = Disconnect;
		Disconnect = GUI.Button(new Rect(110f, 330f, 313f, 54f), new GUIContent("Exit",""));
		if (Disconnect != oldDisconnect && Disconnect ) {Action("onDisconnect");onButtonClick(); }
		onMouseOver = new Rect(110f, 330f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverDisconnect != onMouseOver && onMouseOver) onOver();
		oldMouseOverDisconnect = onMouseOver;
		if (GUI.Button(new Rect(509.968f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}