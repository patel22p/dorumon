
#pragma warning disable 649
#pragma warning disable 168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static IrcChatWindow __IrcChatWindow;
    public static IrcChatWindow _IrcChatWindow { get { if (__IrcChatWindow == null) __IrcChatWindow = (IrcChatWindow)MonoBehaviour.FindObjectOfType(typeof(IrcChatWindow)); return __IrcChatWindow; } }
}

public class IrcChatWindow : WindowBase {
		
	internal bool focusMsgs;
	internal bool isReadOnlyMsgs = true;
	internal string Msgs = @"";
	internal bool focusInput;
	internal bool isReadOnlyInput = false;
	internal string Input = @"";
	internal bool focusUsers;
	internal string[] Users = new string[] {};
	internal int iUsers = -1;
	internal bool focusIrcSend;
	internal bool IrcSend=false;
	private int wndid1;
	private Vector2 sUsers;
	private bool oldMouseOverIrcSend;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = (GUISkin)Resources.Load("Skin/Skin");
        
		GUI.Window(wndid1,new Rect(-347.5f + Screen.width/2,-320f + Screen.height/2,679f,608.476f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(focusMsgs) { focusMsgs = false; GUI.FocusControl("Msgs");}
		GUI.SetNextControlName("Msgs");
		if(isReadOnlyMsgs){
		GUI.Label(new Rect(30f, 50f, 503f, 488f), Msgs.ToString());
		} else
		Msgs = GUI.TextField(new Rect(30f, 50f, 503f, 488f), Msgs);
		if(focusInput) { focusInput = false; GUI.FocusControl("Input");}
		GUI.SetNextControlName("Input");
		if(isReadOnlyInput){
		GUI.Label(new Rect(30f, 551f, 503f, 21f), Input.ToString());
		} else
		Input = GUI.TextField(new Rect(30f, 551f, 503f, 21f), Input);
		if(focusUsers) { focusUsers = false; GUI.FocusControl("Users");}
		GUI.SetNextControlName("Users");
		GUI.Box(new Rect(546f, 50f, 114f, 488f), "");
		sUsers = GUI.BeginScrollView(new Rect(546f, 50f, 114f, 488f), sUsers, new Rect(0,0, 94f, Users.Length* 15f));
		int oldUsers = iUsers;
		iUsers = GUI.SelectionGrid(new Rect(0,0, 94f, Users.Length* 15f), iUsers, Users,1,GUI.skin.customStyles[0]);
		if (iUsers != oldUsers) Action("onUsers",Users[iUsers]);
		GUI.EndScrollView();
		if(focusIrcSend) { focusIrcSend = false; GUI.FocusControl("IrcSend");}
		GUI.SetNextControlName("IrcSend");
		bool oldIrcSend = IrcSend;
		IrcSend = GUI.Button(new Rect(546f, 551f, 114f, 21.96f), new GUIContent("send",""));
		if (IrcSend != oldIrcSend && IrcSend ) {Action("onIrcSend");onButtonClick(); }
		onMouseOver = new Rect(546f, 551f, 114f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverIrcSend != onMouseOver && onMouseOver) onOver();
		oldMouseOverIrcSend = onMouseOver;
		GUI.Label(new Rect(8f, 8f, 94f, 21.96f), @"Irc Chat");
		GUI.Label(new Rect(151f, 36f, 353f, 14f), @"Сервер:irc.quakenet.org:6667 Комната:#PhysxWars");
		if (GUI.Button(new Rect(679f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}