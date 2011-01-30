
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static PopUpWindow __PopUpWindow;
    public static PopUpWindow _PopUpWindow { get { if (__PopUpWindow == null) __PopUpWindow = (PopUpWindow)MonoBehaviour.FindObjectOfType(typeof(PopUpWindow)); return __PopUpWindow; } }
}
public enum PopUpWindowEnum { Ok, }
public class PopUpWindow : WindowBase {
		
	
	internal bool vtext = true;
	
	internal bool focusText;
	
	internal bool rText = true;
	[HideInInspector]
	public string Text = @"";
	
	internal bool vok = true;
	
	internal bool focusOk;
	internal bool Ok=false;
	private int wndid1;
	private bool oldMouseOverOk;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
        
    }
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-223.5f + Screen.width/2,-192f + Screen.height/2,409.9f,277f), Wnd1,"", GUI.skin.customStyles[6]);

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(vtext){
		if(focusText) { focusText = false; GUI.FocusControl("Text");}
		GUI.SetNextControlName("Text");
		if(rText){
		GUI.Label(new Rect(23f, 25f, 360f, 189f), Text.ToString());
		} else
		Text = GUI.TextField(new Rect(23f, 25f, 360f, 189f), Text,100);
		}
		if(vok){
		if(focusOk) { focusOk = false; GUI.FocusControl("Ok");}
		GUI.SetNextControlName("Ok");
		bool oldOk = Ok;
		Ok = GUI.Button(new Rect(125f, 230f, 142f, 27f), new GUIContent("OK",""));
		if (Ok != oldOk && Ok ) {Action("Ok");onButtonClick(); }
		onMouseOver = new Rect(125f, 230f, 142f, 27f).Contains(Event.current.mousePosition);
		if (oldMouseOverOk != onMouseOver && onMouseOver) onOver();
		oldMouseOverOk = onMouseOver;
		}
	}


	void Update () {
	
	}
}