
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

public class AboutWindow : WindowBase {
		
	internal string pathImage2 = "Images/physx_wars_title";
	private int wndid1;
	private Rect Image2;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image2 = new Rect(12f, 11f, 690f, 288f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    
    void OnGUI()
    {		
		GUI.skin = _Loader._skin;
        
		GUI.Window(wndid1,new Rect(-372.5f + Screen.width/2,-291f + Screen.height/2,715f,529f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.DrawTexture(Image2,(Texture2D)Resources.Load("Images/physx_wars_title"), ScaleMode.ScaleToFit);
		GUI.Label(new Rect(111f, 290f, 480f, 176f), @"Copyright PhysxWars Team


:Programming:
Igor Levochkin


:Design:
Dead Firsache
Igor Levochkin

:Music producers:
CentaSpike
Никита Сафонов", GUI.skin.customStyles[7]);
		if (GUI.Button(new Rect(715f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}