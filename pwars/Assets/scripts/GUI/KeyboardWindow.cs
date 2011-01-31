
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static KeyboardWindow __KeyboardWindow;
    public static KeyboardWindow _KeyboardWindow { get { if (__KeyboardWindow == null) __KeyboardWindow = (KeyboardWindow)MonoBehaviour.FindObjectOfType(typeof(KeyboardWindow)); return __KeyboardWindow; } }
}
public enum KeyboardWindowEnum { Close, }
public class KeyboardWindow : WindowBase {
		
	[FindAsset("keyboard")]
	public Texture imgImage3;
	private int wndid1;
	private Rect Image3;
	
    
    
	void Start () {
		wndid1 = UnityEngine.Random.Range(0, 1000);
		Image3 = new Rect(22f, 30.802f, 732.67f, 418.03f);

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
        
		GUI.Window(wndid1,new Rect(-395.5f + Screen.width/2,-240f + Screen.height/2,775f,470f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(imgImage3!=null)
			GUI.DrawTexture(Image3,imgImage3, ScaleMode.ScaleToFit, imgImage3 is RenderTexture?false:true);
		if (GUI.Button(new Rect(775f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}