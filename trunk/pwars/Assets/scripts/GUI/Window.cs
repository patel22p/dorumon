
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static Window __Window;
    public static Window _Window { get { if (__Window == null) __Window = (Window)MonoBehaviour.FindObjectOfType(typeof(Window)); return __Window; } }
}

public class Window : WindowBase {
		
	private int wndid1;
	
    
    
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
		GUI.skin = _Loader._skin;
        
		GUI.Window(wndid1,new Rect(-226.5f + Screen.width/2,-243f + Screen.height/2,414f,371f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if (GUI.Button(new Rect(414f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}