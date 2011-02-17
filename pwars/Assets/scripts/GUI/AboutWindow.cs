
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static AboutWindow __AboutWindow;
    public static AboutWindow _AboutWindow { get { if (__AboutWindow == null) __AboutWindow = (AboutWindow)MonoBehaviour.FindObjectOfType(typeof(AboutWindow)); return __AboutWindow; } }
}
public enum AboutWindowEnum { Forum,Close, }
public class AboutWindow : WindowBase {
		
	[FindAsset("physx_wars_title")]
	public Texture imgImage2;
	
	internal bool vforum = true;
	
	internal bool focusForum;
	internal bool Forum=false;
	private int wndid1;
	private Rect Image2;
	private bool oldMouseOverForum;
	
    
    
	void Start () {
		AlwaysOnTop = false;
		wndid1 = 0;
		Image2 = new Rect(12f, 11f, 690f, 288f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
		vforum = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-355.5f + Screen.width/2,-275f + Screen.height/2,715f,529f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(imgImage2!=null)
			GUI.DrawTexture(Image2,imgImage2, ScaleMode.ScaleToFit, imgImage2 is RenderTexture?false:true);
		GUI.Label(new Rect(111f, 290f, 480f, 176f), @"Copyright PhysxWars Team


:Programming:
Igor Levochkin


:Graphic Design:
Jarmo Juurikka

:Music producer:
CentaSpike", GUI.skin.customStyles[7]);
		if(vforum){
		if(focusForum) { focusForum = false; GUI.FocusControl("Forum");}
		GUI.SetNextControlName("Forum");
		bool oldForum = Forum;
		Forum = GUI.Button(new Rect(491f, 482f, 216f, 39f), new GUIContent(@"Report bug/Discussion thread",""));
		if (Forum != oldForum && Forum ) {Action("Forum");onButtonClick(); }
		onMouseOver = new Rect(491f, 482f, 216f, 39f).Contains(Event.current.mousePosition);
		if (oldMouseOverForum != onMouseOver && onMouseOver) onOver();
		oldMouseOverForum = onMouseOver;
		}
		if (GUI.Button(new Rect(715f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}