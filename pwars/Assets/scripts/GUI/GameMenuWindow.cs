
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
public enum GameMenuWindowEnum { TeamSelectButton,Options,Disconnect,Close, }
public class GameMenuWindow : WindowBase {
		
	[FindAsset("physx_wars_title")]
	public Texture imgImage2;
	
	internal bool vTeamSelectButton = true;
	
	internal bool focusTeamSelectButton;
	internal bool TeamSelectButton=false;
	
	internal bool voptions = true;
	
	internal bool focusOptions;
	internal bool Options=false;
	
	internal bool vdisconnect = true;
	
	internal bool focusDisconnect;
	internal bool Disconnect=false;
	
	internal bool vfraglimit = true;
	
	internal bool focusFraglimit;
	
	internal bool rFraglimit = true;
	[HideInInspector]
	public int Fraglimit = 0;
	private int wndid1;
	private Rect Image2;
	private bool oldMouseOverTeamSelectButton;
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
    public override void ResetValues()
    {
        
    }
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-245.5f + Screen.width/2,-224f + Screen.height/2,509.968f,328f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(imgImage2!=null)
			GUI.DrawTexture(Image2,imgImage2, ScaleMode.ScaleToFit, imgImage2 is RenderTexture?false:true);
		if(vTeamSelectButton){
		if(focusTeamSelectButton) { focusTeamSelectButton = false; GUI.FocusControl("TeamSelectButton");}
		GUI.SetNextControlName("TeamSelectButton");
		bool oldTeamSelectButton = TeamSelectButton;
		TeamSelectButton = GUI.Button(new Rect(112f, 126f, 313f, 54f), new GUIContent("Join Game",""));
		if (TeamSelectButton != oldTeamSelectButton && TeamSelectButton ) {Action("TeamSelectButton");onButtonClick(); }
		onMouseOver = new Rect(112f, 126f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverTeamSelectButton != onMouseOver && onMouseOver) onOver();
		oldMouseOverTeamSelectButton = onMouseOver;
		}
		if(voptions){
		if(focusOptions) { focusOptions = false; GUI.FocusControl("Options");}
		GUI.SetNextControlName("Options");
		bool oldOptions = Options;
		Options = GUI.Button(new Rect(112f, 184f, 313f, 54f), new GUIContent("Options",""));
		if (Options != oldOptions && Options ) {Action("Options");onButtonClick(); }
		onMouseOver = new Rect(112f, 184f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverOptions != onMouseOver && onMouseOver) onOver();
		oldMouseOverOptions = onMouseOver;
		}
		if(vdisconnect){
		if(focusDisconnect) { focusDisconnect = false; GUI.FocusControl("Disconnect");}
		GUI.SetNextControlName("Disconnect");
		bool oldDisconnect = Disconnect;
		Disconnect = GUI.Button(new Rect(112f, 242f, 313f, 54f), new GUIContent("Disconnect",""));
		if (Disconnect != oldDisconnect && Disconnect ) {Action("Disconnect");onButtonClick(); }
		onMouseOver = new Rect(112f, 242f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverDisconnect != onMouseOver && onMouseOver) onOver();
		oldMouseOverDisconnect = onMouseOver;
		}
		GUI.Label(new Rect(338.635f, 314f, 96f, 21.96f), @"Frag Limit");
		if(vfraglimit){
		if(focusFraglimit) { focusFraglimit = false; GUI.FocusControl("Fraglimit");}
		GUI.SetNextControlName("Fraglimit");
		if(rFraglimit){
		GUI.Label(new Rect(434.635f, 314f, 75.333f, 21.96f), Fraglimit.ToString());
		} else
		Fraglimit = int.Parse(GUI.TextField(new Rect(434.635f, 314f, 75.333f, 21.96f), Fraglimit.ToString(),100));
		}
		if (GUI.Button(new Rect(509.968f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}