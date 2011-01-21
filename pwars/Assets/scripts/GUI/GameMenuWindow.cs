
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
		
	[FindAsset("physx_wars_title")]
	public Texture ImageImage2;
	internal bool vTeamSelectButton = true;
	internal bool focusTeamSelectButton;
	internal bool TeamSelectButton=false;
	internal bool voptions = true;
	internal bool focusOptions;
	internal bool Options=false;
	internal bool vdisconnect = true;
	internal bool focusDisconnect;
	internal bool Disconnect=false;
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
    
    void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-279.5f + Screen.width/2,-226f + Screen.height/2,509.968f,457f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(ImageImage2!=null)
			GUI.DrawTexture(Image2,ImageImage2, ScaleMode.ScaleToFit);
		if(vTeamSelectButton){
		if(focusTeamSelectButton) { focusTeamSelectButton = false; GUI.FocusControl("TeamSelectButton");}
		GUI.SetNextControlName("TeamSelectButton");
		bool oldTeamSelectButton = TeamSelectButton;
		TeamSelectButton = GUI.Button(new Rect(110f, 156f, 313f, 54f), new GUIContent("Select Team",""));
		if (TeamSelectButton != oldTeamSelectButton && TeamSelectButton ) {Action("TeamSelectButton");onButtonClick(); }
		onMouseOver = new Rect(110f, 156f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverTeamSelectButton != onMouseOver && onMouseOver) onOver();
		oldMouseOverTeamSelectButton = onMouseOver;
		}
		if(voptions){
		if(focusOptions) { focusOptions = false; GUI.FocusControl("Options");}
		GUI.SetNextControlName("Options");
		bool oldOptions = Options;
		Options = GUI.Button(new Rect(110f, 272f, 313f, 54f), new GUIContent("Options",""));
		if (Options != oldOptions && Options ) {Action("Options");onButtonClick(); }
		onMouseOver = new Rect(110f, 272f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverOptions != onMouseOver && onMouseOver) onOver();
		oldMouseOverOptions = onMouseOver;
		}
		if(vdisconnect){
		if(focusDisconnect) { focusDisconnect = false; GUI.FocusControl("Disconnect");}
		GUI.SetNextControlName("Disconnect");
		bool oldDisconnect = Disconnect;
		Disconnect = GUI.Button(new Rect(110f, 330f, 313f, 54f), new GUIContent("Exit",""));
		if (Disconnect != oldDisconnect && Disconnect ) {Action("Disconnect");onButtonClick(); }
		onMouseOver = new Rect(110f, 330f, 313f, 54f).Contains(Event.current.mousePosition);
		if (oldMouseOverDisconnect != onMouseOver && onMouseOver) onOver();
		oldMouseOverDisconnect = onMouseOver;
		}
		if (GUI.Button(new Rect(509.968f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}