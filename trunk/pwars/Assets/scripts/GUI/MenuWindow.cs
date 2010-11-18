
#pragma warning disable 649
#pragma warning disable 168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static MenuWindow __MenuWindow;
    public static MenuWindow _MenuWindow { get { if (__MenuWindow == null) __MenuWindow = (MenuWindow)MonoBehaviour.FindObjectOfType(typeof(MenuWindow)); return __MenuWindow; } }
}

public class MenuWindow : WindowBase {
		
	internal string pathImage2 = "Images/1";
	internal bool focusServers;
	internal bool Servers=false;
	internal bool focusIrcChat;
	internal bool IrcChat=false;
	internal bool focusCreate;
	internal bool Create=false;
	internal bool focusSettings;
	internal bool Settings=false;
	internal bool focusAbout;
	internal bool About=false;
	internal bool focusLogOut;
	internal bool LogOut=false;
	private int wndid1;
	private Rect Image2;
	private bool oldMouseOverServers;
	private bool oldMouseOverIrcChat;
	private bool oldMouseOverCreate;
	private bool oldMouseOverSettings;
	private bool oldMouseOverAbout;
	private bool oldMouseOverLogOut;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image2 = new Rect(0f, 17f, 791f, 677f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = (GUISkin)Resources.Load("Skin/Skin");
        
		GUI.Window(wndid1,new Rect(-403.5f + Screen.width/2,-365f + Screen.height/2,791f,694f), Wnd1,"", GUI.skin.customStyles[5]);

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.DrawTexture(Image2,(Texture2D)Resources.Load("Images/1"), ScaleMode.ScaleToFit);
		if(focusServers) { focusServers = false; GUI.FocusControl("Servers");}
		GUI.SetNextControlName("Servers");
		bool oldServers = Servers;
		Servers = GUI.Button(new Rect(316.5f, 334f, 169.5f, 36f), new GUIContent("Сервера",""));
		if (Servers != oldServers && Servers ) {Action("onServers");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 334f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverServers != onMouseOver && onMouseOver) onOver();
		oldMouseOverServers = onMouseOver;
		if(focusIrcChat) { focusIrcChat = false; GUI.FocusControl("IrcChat");}
		GUI.SetNextControlName("IrcChat");
		bool oldIrcChat = IrcChat;
		IrcChat = GUI.Button(new Rect(316.5f, 374f, 169.5f, 36f), new GUIContent("Ирк Чат",""));
		if (IrcChat != oldIrcChat && IrcChat ) {Action("onIrcChat");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 374f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverIrcChat != onMouseOver && onMouseOver) onOver();
		oldMouseOverIrcChat = onMouseOver;
		if(focusCreate) { focusCreate = false; GUI.FocusControl("Create");}
		GUI.SetNextControlName("Create");
		bool oldCreate = Create;
		Create = GUI.Button(new Rect(316.5f, 294f, 169.5f, 36f), new GUIContent("Создать Игру",""));
		if (Create != oldCreate && Create ) {Action("onCreate");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 294f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverCreate != onMouseOver && onMouseOver) onOver();
		oldMouseOverCreate = onMouseOver;
		if(focusSettings) { focusSettings = false; GUI.FocusControl("Settings");}
		GUI.SetNextControlName("Settings");
		bool oldSettings = Settings;
		Settings = GUI.Button(new Rect(316.5f, 412.667f, 169.5f, 36f), new GUIContent("Настроики",""));
		if (Settings != oldSettings && Settings ) {Action("onSettings");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 412.667f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverSettings != onMouseOver && onMouseOver) onOver();
		oldMouseOverSettings = onMouseOver;
		if(focusAbout) { focusAbout = false; GUI.FocusControl("About");}
		GUI.SetNextControlName("About");
		bool oldAbout = About;
		About = GUI.Button(new Rect(316.5f, 452.667f, 169.5f, 36f), new GUIContent("Авторы",""));
		if (About != oldAbout && About ) {Action("onAbout");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 452.667f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverAbout != onMouseOver && onMouseOver) onOver();
		oldMouseOverAbout = onMouseOver;
		if(focusLogOut) { focusLogOut = false; GUI.FocusControl("LogOut");}
		GUI.SetNextControlName("LogOut");
		bool oldLogOut = LogOut;
		LogOut = GUI.Button(new Rect(316.5f, 492.667f, 169.5f, 36f), new GUIContent("Выйти",""));
		if (LogOut != oldLogOut && LogOut ) {Action("onLogOut");onButtonClick(); }
		onMouseOver = new Rect(316.5f, 492.667f, 169.5f, 36f).Contains(Event.current.mousePosition);
		if (oldMouseOverLogOut != onMouseOver && onMouseOver) onOver();
		oldMouseOverLogOut = onMouseOver;
	}


	void Update () {
	
	}
}