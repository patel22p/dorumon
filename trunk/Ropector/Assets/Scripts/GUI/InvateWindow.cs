
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base:MonoBehaviour
{    
    static InvateWindow __InvateWindow;
    public static InvateWindow _InvateWindow { get { if (__InvateWindow == null) __InvateWindow = (InvateWindow)MonoBehaviour.FindObjectOfType(typeof(InvateWindow)); return __InvateWindow; } }
}
public enum InvateWindowEnum { Invite,Skip,Close, }
public class InvateWindow : WindowBase {
		
	
	internal bool vInvite = true;
	
	internal bool focusInvite;
	internal bool Invite=false;
	
	internal bool vskip = true;
	
	internal bool focusSkip;
	internal bool Skip=false;
	private int wndid1;
	private bool oldMouseOverInvite;
	private bool oldMouseOverSkip;
	
    
    
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
		vInvite = true;
		vskip = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		base.OnGUI();
        
		GUI.Window(wndid1,new Rect(-197.5f + Screen.width/2,-164f + Screen.height/2,414f,238f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(vInvite){
		if(focusInvite) { focusInvite = false; GUI.FocusControl("Invite");}
		GUI.SetNextControlName("Invite");
		bool oldInvite = Invite;
		Invite = GUI.Button(new Rect(109f, 91f, 204f, 27f), new GUIContent(@"Invite Friends to game",""));
		if (Invite != oldInvite && Invite ) {Action(InvateWindowEnum.Invite);onButtonClick(); }
		onMouseOver = new Rect(109f, 91f, 204f, 27f).Contains(Event.current.mousePosition);
		if (oldMouseOverInvite != onMouseOver && onMouseOver) onOver();
		oldMouseOverInvite = onMouseOver;
		}
		if(vskip){
		if(focusSkip) { focusSkip = false; GUI.FocusControl("Skip");}
		GUI.SetNextControlName("Skip");
		bool oldSkip = Skip;
		Skip = GUI.Button(new Rect(109f, 122f, 204f, 27f), new GUIContent(@"Skip",""));
		if (Skip != oldSkip && Skip ) {Action(InvateWindowEnum.Skip);onButtonClick(); }
		onMouseOver = new Rect(109f, 122f, 204f, 27f).Contains(Event.current.mousePosition);
		if (oldMouseOverSkip != onMouseOver && onMouseOver) onOver();
		oldMouseOverSkip = onMouseOver;
		}
		if (GUI.Button(new Rect(414f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action(InvateWindowEnum.Close); }
	}


	void Update () {
	
	}
}