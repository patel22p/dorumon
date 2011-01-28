
#pragma warning disable 0169, 0414,649,168
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
		
	
	internal bool vmsgs = true;
	
	internal bool focusMsgs;
	
	internal bool rMsgs = true;
	[HideInInspector]
	public string Msgs = @"";
	
	internal bool vinput = true;
	
	internal bool focusInput;
	
	internal bool rInput = false;
	[HideInInspector]
	public string Input = @"";
	
	internal bool vUsers = true;
	
	internal bool focusUsers;
	public string[] lUsers;
	[HideInInspector]
	public int iUsers = -1;
	public string Users { get { if(lUsers.Length==0) return ""; return lUsers[iUsers]; } set { iUsers = lUsers.SelectIndex(value); }}
	
	internal bool vircSend = true;
	
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
    public void ResetValues()
    {
        	iUsers = -1;

    }
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-347.5f + Screen.width/2,-320f + Screen.height/2,679f,608.476f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(vmsgs){
		if(focusMsgs) { focusMsgs = false; GUI.FocusControl("Msgs");}
		GUI.SetNextControlName("Msgs");
		if(rMsgs){
		GUI.Label(new Rect(30f, 50f, 503f, 488f), Msgs.ToString());
		} else
		Msgs = GUI.TextField(new Rect(30f, 50f, 503f, 488f), Msgs,100);
		}
		if(vinput){
		if(focusInput) { focusInput = false; GUI.FocusControl("Input");}
		GUI.SetNextControlName("Input");
		if(rInput){
		GUI.Label(new Rect(30f, 551f, 503f, 21f), Input.ToString());
		} else
		Input = GUI.TextField(new Rect(30f, 551f, 503f, 21f), Input,100);
		}
		if(vUsers){
		if(focusUsers) { focusUsers = false; GUI.FocusControl("Users");}
		GUI.SetNextControlName("Users");
		GUI.Box(new Rect(546f, 50f, 114f, 488f), "");
		sUsers = GUI.BeginScrollView(new Rect(546f, 50f, 114f, 488f), sUsers, new Rect(0,0, 94f, lUsers.Length* 15f));
		int oldUsers = iUsers;
		iUsers = GUI.SelectionGrid(new Rect(0,0, 94f, lUsers.Length* 15f), iUsers, lUsers,1,GUI.skin.customStyles[0]);
		if (iUsers != oldUsers) Action("Users");
		GUI.EndScrollView();
		}
		if(vircSend){
		if(focusIrcSend) { focusIrcSend = false; GUI.FocusControl("IrcSend");}
		GUI.SetNextControlName("IrcSend");
		bool oldIrcSend = IrcSend;
		IrcSend = GUI.Button(new Rect(546f, 551f, 114f, 21.96f), new GUIContent("send",""));
		if (IrcSend != oldIrcSend && IrcSend ) {Action("IrcSend");onButtonClick(); }
		onMouseOver = new Rect(546f, 551f, 114f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverIrcSend != onMouseOver && onMouseOver) onOver();
		oldMouseOverIrcSend = onMouseOver;
		}
		GUI.Label(new Rect(8f, 8f, 94f, 21.96f), @"Irc Chat");
		GUI.Label(new Rect(151f, 36f, 353f, 14f), @"Сервер:irc.quakenet.org:6667 Комната:#PhysxWars");
		if (GUI.Button(new Rect(679f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}