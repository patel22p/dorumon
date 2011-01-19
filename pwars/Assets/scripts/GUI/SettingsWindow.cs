
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static SettingsWindow __SettingsWindow;
    public static SettingsWindow _SettingsWindow { get { if (__SettingsWindow == null) __SettingsWindow = (SettingsWindow)MonoBehaviour.FindObjectOfType(typeof(SettingsWindow)); return __SettingsWindow; } }
}

public class SettingsWindow : WindowBase {
		
	private int? _iGraphicQuality;
	internal int iGraphicQuality{ get { if(_iGraphicQuality == null) _iGraphicQuality = PlayerPrefs.GetInt("iGraphicQuality", -1); return _iGraphicQuality.Value; } set { PlayerPrefs.SetInt("iGraphicQuality", value); _iGraphicQuality = value; } }
	private int? _iScreenSize;
	internal int iScreenSize{ get { if(_iScreenSize == null) _iScreenSize = PlayerPrefs.GetInt("iScreenSize", -1); return _iScreenSize.Value; } set { PlayerPrefs.SetInt("iScreenSize", value); _iScreenSize = value; } }
	private float? _Camx;
	internal float Camx{ get { if(_Camx == null) _Camx = PlayerPrefs.GetFloat("Camx", 6f); return _Camx.Value; } set { PlayerPrefs.SetFloat("Camx", value); _Camx = value; } }
	private float? _Camy;
	internal float Camy{ get { if(_Camy == null) _Camy = PlayerPrefs.GetFloat("Camy", 4f); return _Camy.Value; } set { PlayerPrefs.SetFloat("Camy", value); _Camy = value; } }
	private float? _Fieldof;
	internal float Fieldof{ get { if(_Fieldof == null) _Fieldof = PlayerPrefs.GetFloat("Fieldof", 100f); return _Fieldof.Value; } set { PlayerPrefs.SetFloat("Fieldof", value); _Fieldof = value; } }
	private float? _CamSmooth;
	internal float CamSmooth{ get { if(_CamSmooth == null) _CamSmooth = PlayerPrefs.GetFloat("CamSmooth", 1f); return _CamSmooth.Value; } set { PlayerPrefs.SetFloat("CamSmooth", value); _CamSmooth = value; } }
	private float? _MusicVolume;
	internal float MusicVolume{ get { if(_MusicVolume == null) _MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f); return _MusicVolume.Value; } set { PlayerPrefs.SetFloat("MusicVolume", value); _MusicVolume = value; } }
	private float? _SoundVolume;
	internal float SoundVolume{ get { if(_SoundVolume == null) _SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.1f); return _SoundVolume.Value; } set { PlayerPrefs.SetFloat("SoundVolume", value); _SoundVolume = value; } }
	private float? _NetworkSendRate;
	internal float NetworkSendRate{ get { if(_NetworkSendRate == null) _NetworkSendRate = PlayerPrefs.GetFloat("NetworkSendRate", 15f); return _NetworkSendRate.Value; } set { PlayerPrefs.SetFloat("NetworkSendRate", value); _NetworkSendRate = value; } }
	private float? _MouseY;
	internal float MouseY{ get { if(_MouseY == null) _MouseY = PlayerPrefs.GetFloat("MouseY", 1f); return _MouseY.Value; } set { PlayerPrefs.SetFloat("MouseY", value); _MouseY = value; } }
	private float? _MouseX;
	internal float MouseX{ get { if(_MouseX == null) _MouseX = PlayerPrefs.GetFloat("MouseX", 1f); return _MouseX.Value; } set { PlayerPrefs.SetFloat("MouseX", value); _MouseX = value; } }
	internal bool focusGraphicQuality;
	public string[] GraphicQuality = new string[] {"Fastest","Fast","Simple","Good","Beautifull","Fantastic",};
	internal bool focusScreenSize;
	public string[] ScreenSize = new string[] {};
	internal bool focusFullScreen;
	internal bool FullScreen=false;
	internal bool focusBlood;
	internal bool Blood { get { return PlayerPrefs.GetInt("Blood", 1) == 1; } set { PlayerPrefs.SetInt("Blood", value?1:0); } }
	internal bool focusDecals;
	internal bool Decals { get { return PlayerPrefs.GetInt("Decals", 1) == 1; } set { PlayerPrefs.SetInt("Decals", value?1:0); } }
	internal bool focusAtmoSphere;
	internal bool AtmoSphere { get { return PlayerPrefs.GetInt("AtmoSphere", 1) == 1; } set { PlayerPrefs.SetInt("AtmoSphere", value?1:0); } }
	internal bool focusSao;
	internal bool Sao { get { return PlayerPrefs.GetInt("Sao", 1) == 1; } set { PlayerPrefs.SetInt("Sao", value?1:0); } }
	internal bool focusShadows;
	internal bool Shadows { get { return PlayerPrefs.GetInt("Shadows", 1) == 1; } set { PlayerPrefs.SetInt("Shadows", value?1:0); } }
	internal bool focusMotionBlur;
	internal bool MotionBlur { get { return PlayerPrefs.GetInt("MotionBlur", 1) == 1; } set { PlayerPrefs.SetInt("MotionBlur", value?1:0); } }
	internal bool focusBloomAndFlares;
	internal bool BloomAndFlares { get { return PlayerPrefs.GetInt("BloomAndFlares", 1) == 1; } set { PlayerPrefs.SetInt("BloomAndFlares", value?1:0); } }
	internal bool focusRenderSettings;
	public string[] RenderSettings = new string[] {"Vertex Lit","Forward","Deffered",};
	internal int iRenderSettings = -1;
	internal bool focusCamx;
	internal bool focusCamy;
	internal bool focusFieldof;
	internal bool focusCamSmooth;
	internal bool focusMusicVolume;
	internal bool focusSoundVolume;
	internal bool focusReset;
	internal bool Reset=false;
	internal bool focusNetworkSendRate;
	internal bool focusMouseY;
	internal bool focusMouseX;
	internal bool focusShowKeyboard;
	internal bool ShowKeyboard=false;
	private int wndid1;
	private Vector2 sGraphicQuality;
	private Vector2 sScreenSize;
	private bool oldMouseOverFullScreen;
	private bool oldMouseOverBlood;
	private bool oldMouseOverDecals;
	private bool oldMouseOverAtmoSphere;
	private bool oldMouseOverSao;
	private bool oldMouseOverShadows;
	private bool oldMouseOverMotionBlur;
	private bool oldMouseOverBloomAndFlares;
	private Vector2 sRenderSettings;
	private bool oldMouseOverReset;
	private bool oldMouseOverShowKeyboard;
	
    
    
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
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-366.5f + Screen.width/2,-209f + Screen.height/2,729f,418f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(34.5f, 36f, 463.5f, 368.333f), "");
		GUI.Box(new Rect(0, 0, 463.5f, 368.333f), "");
		if(focusGraphicQuality) { focusGraphicQuality = false; GUI.FocusControl("GraphicQuality");}
		GUI.SetNextControlName("GraphicQuality");
		GUI.Box(new Rect(49.5f, 38.833f, 111.013f, 112.167f), "");
		sGraphicQuality = GUI.BeginScrollView(new Rect(49.5f, 38.833f, 111.013f, 112.167f), sGraphicQuality, new Rect(0,0, 91.013f, GraphicQuality.Length* 15.960000038147f));
		int oldGraphicQuality = iGraphicQuality;
		iGraphicQuality = GUI.SelectionGrid(new Rect(0,0, 91.013f, GraphicQuality.Length* 15.960000038147f), iGraphicQuality, GraphicQuality,1,GUI.skin.customStyles[0]);
		if (iGraphicQuality != oldGraphicQuality) Action("onGraphicQuality",GraphicQuality[iGraphicQuality]);
		GUI.EndScrollView();
		if(focusScreenSize) { focusScreenSize = false; GUI.FocusControl("ScreenSize");}
		GUI.SetNextControlName("ScreenSize");
		GUI.Box(new Rect(182.833f, 38.833f, 114.667f, 112.167f), "");
		sScreenSize = GUI.BeginScrollView(new Rect(182.833f, 38.833f, 114.667f, 112.167f), sScreenSize, new Rect(0,0, 94.667f, ScreenSize.Length* 15f));
		int oldScreenSize = iScreenSize;
		iScreenSize = GUI.SelectionGrid(new Rect(0,0, 94.667f, ScreenSize.Length* 15f), iScreenSize, ScreenSize,1,GUI.skin.customStyles[0]);
		if (iScreenSize != oldScreenSize) Action("onScreenSize",ScreenSize[iScreenSize]);
		GUI.EndScrollView();
		GUI.Label(new Rect(184.59f, 24.833f, 137.577f, 21.96f), @"Screen Resolution");
		GUI.Label(new Rect(8f, 8f, 89.5f, 12.833f), @"Graphics");
		GUI.Label(new Rect(38.833f, 24.833f, 130f, 21.96f), @"Graphics Quality");
		if(focusFullScreen) { focusFullScreen = false; GUI.FocusControl("FullScreen");}
		GUI.SetNextControlName("FullScreen");
		bool oldFullScreen = FullScreen;
		FullScreen = GUI.Toggle(new Rect(341.5f, 37f, 105f, 17f),FullScreen, new GUIContent("Full Screen",""));
		if (FullScreen != oldFullScreen ) {Action("onFullScreen");onButtonClick(); }
		onMouseOver = new Rect(341.5f, 37f, 105f, 17f).Contains(Event.current.mousePosition);
		if (oldMouseOverFullScreen != onMouseOver && onMouseOver) onOver();
		oldMouseOverFullScreen = onMouseOver;
		if(focusBlood) { focusBlood = false; GUI.FocusControl("Blood");}
		GUI.SetNextControlName("Blood");
		bool oldBlood = Blood;
		Blood = GUI.Toggle(new Rect(30.5f, 180f, 47.91333f, 15.96f),Blood, new GUIContent("Blood",""));
		if (Blood != oldBlood ) {Action("onBlood");onButtonClick(); }
		onMouseOver = new Rect(30.5f, 180f, 47.91333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverBlood != onMouseOver && onMouseOver) onOver();
		oldMouseOverBlood = onMouseOver;
		if(focusDecals) { focusDecals = false; GUI.FocusControl("Decals");}
		GUI.SetNextControlName("Decals");
		bool oldDecals = Decals;
		Decals = GUI.Toggle(new Rect(30.5f, 199.96f, 49f, 15.96f),Decals, new GUIContent("decals",""));
		if (Decals != oldDecals ) {Action("onDecals");onButtonClick(); }
		onMouseOver = new Rect(30.5f, 199.96f, 49f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverDecals != onMouseOver && onMouseOver) onOver();
		oldMouseOverDecals = onMouseOver;
		if(focusAtmoSphere) { focusAtmoSphere = false; GUI.FocusControl("AtmoSphere");}
		GUI.SetNextControlName("AtmoSphere");
		bool oldAtmoSphere = AtmoSphere;
		AtmoSphere = GUI.Toggle(new Rect(261.63f, 184f, 81.83667f, 15.96f),AtmoSphere, new GUIContent("Atmosphere",""));
		if (AtmoSphere != oldAtmoSphere ) {Action("onAtmoSphere");onButtonClick(); }
		onMouseOver = new Rect(261.63f, 184f, 81.83667f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverAtmoSphere != onMouseOver && onMouseOver) onOver();
		oldMouseOverAtmoSphere = onMouseOver;
		if(focusSao) { focusSao = false; GUI.FocusControl("Sao");}
		GUI.SetNextControlName("Sao");
		bool oldSao = Sao;
		Sao = GUI.Toggle(new Rect(261.63f, 199.96f, 60.53667f, 15.96f),Sao, new GUIContent("ambient",""));
		if (Sao != oldSao ) {Action("onSao");onButtonClick(); }
		onMouseOver = new Rect(261.63f, 199.96f, 60.53667f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverSao != onMouseOver && onMouseOver) onOver();
		oldMouseOverSao = onMouseOver;
		if(focusShadows) { focusShadows = false; GUI.FocusControl("Shadows");}
		GUI.SetNextControlName("Shadows");
		bool oldShadows = Shadows;
		Shadows = GUI.Toggle(new Rect(359.5f, 184f, 64.13333f, 15.96f),Shadows, new GUIContent("Shadows",""));
		if (Shadows != oldShadows ) {Action("onShadows");onButtonClick(); }
		onMouseOver = new Rect(359.5f, 184f, 64.13333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverShadows != onMouseOver && onMouseOver) onOver();
		oldMouseOverShadows = onMouseOver;
		if(focusMotionBlur) { focusMotionBlur = false; GUI.FocusControl("MotionBlur");}
		GUI.SetNextControlName("MotionBlur");
		bool oldMotionBlur = MotionBlur;
		MotionBlur = GUI.Toggle(new Rect(130.5f, 180f, 79.37f, 15.96f),MotionBlur, new GUIContent("motion blur",""));
		if (MotionBlur != oldMotionBlur ) {Action("onMotionBlur");onButtonClick(); }
		onMouseOver = new Rect(130.5f, 180f, 79.37f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverMotionBlur != onMouseOver && onMouseOver) onOver();
		oldMouseOverMotionBlur = onMouseOver;
		if(focusBloomAndFlares) { focusBloomAndFlares = false; GUI.FocusControl("BloomAndFlares");}
		GUI.SetNextControlName("BloomAndFlares");
		bool oldBloomAndFlares = BloomAndFlares;
		BloomAndFlares = GUI.Toggle(new Rect(130.5f, 199.96f, 106.2067f, 15.96f),BloomAndFlares, new GUIContent("bloom and flares",""));
		if (BloomAndFlares != oldBloomAndFlares ) {Action("onBloomAndFlares");onButtonClick(); }
		onMouseOver = new Rect(130.5f, 199.96f, 106.2067f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverBloomAndFlares != onMouseOver && onMouseOver) onOver();
		oldMouseOverBloomAndFlares = onMouseOver;
		if(focusRenderSettings) { focusRenderSettings = false; GUI.FocusControl("RenderSettings");}
		GUI.SetNextControlName("RenderSettings");
		GUI.Box(new Rect(326.5f, 87f, 129f, 64f), "");
		sRenderSettings = GUI.BeginScrollView(new Rect(326.5f, 87f, 129f, 64f), sRenderSettings, new Rect(0,0, 109f, RenderSettings.Length* 15.960000038147f));
		int oldRenderSettings = iRenderSettings;
		iRenderSettings = GUI.SelectionGrid(new Rect(0,0, 109f, RenderSettings.Length* 15.960000038147f), iRenderSettings, RenderSettings,1,GUI.skin.customStyles[0]);
		if (iRenderSettings != oldRenderSettings) Action("onRenderSettings",RenderSettings[iRenderSettings]);
		GUI.EndScrollView();
		GUI.Label(new Rect(330.5f, 72f, 47.76f, 21.96f), @"Render");
		GUI.Label(new Rect(8f, 162f, 44.56333f, 21.96f), @"Effects");
		GUI.BeginGroup(new Rect(8f, 228.92f, 447.5f, 134.667f), "");
		GUI.Box(new Rect(0, 0, 447.5f, 134.667f), "");
		GUI.Label(new Rect(14.667f, 8f, 50.43f, 21.96f), @"Camera");
		GUI.Label(new Rect(43.334f, 35.334f, 170f, 14.666f), @"Camera X");
		if(focusCamx) { focusCamx = false; GUI.FocusControl("Camx");}
		GUI.SetNextControlName("Camx");
		Camx = GUI.HorizontalSlider(new Rect(213.334f, 35.334f, 179.333f, 14f), Camx, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,35.334f,40,15),System.Math.Round(Camx,1).ToString());
		if(focusCamy) { focusCamy = false; GUI.FocusControl("Camy");}
		GUI.SetNextControlName("Camy");
		Camy = GUI.HorizontalSlider(new Rect(213.334f, 62.667f, 179.333f, 14f), Camy, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,62.667f,40,15),System.Math.Round(Camy,1).ToString());
		GUI.Label(new Rect(43.334f, 62.001f, 170f, 14.666f), @"Camera Y");
		GUI.Label(new Rect(43.334f, 103.667f, 106f, 14.666f), @"Field of view");
		if(focusFieldof) { focusFieldof = false; GUI.FocusControl("Fieldof");}
		GUI.SetNextControlName("Fieldof");
		Fieldof = GUI.HorizontalSlider(new Rect(213.334f, 103.667f, 179.333f, 14f), Fieldof, 40f, 100f);
		GUI.Label(new Rect(392.666992553711f,103.667f,40,15),System.Math.Round(Fieldof,1).ToString());
		GUI.Label(new Rect(43.334f, 80.667f, 106f, 14.666f), @"Camera Smooth");
		if(focusCamSmooth) { focusCamSmooth = false; GUI.FocusControl("CamSmooth");}
		GUI.SetNextControlName("CamSmooth");
		CamSmooth = GUI.HorizontalSlider(new Rect(213.334f, 80.667f, 179.333f, 14f), CamSmooth, 0f, 2f);
		GUI.Label(new Rect(392.666992553711f,80.667f,40,15),System.Math.Round(CamSmooth,1).ToString());
		GUI.EndGroup();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(511.5f, 138f, 194.167f, 121f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 121f), "");
		GUI.Label(new Rect(8f, 8f, 51f, 14f), @"Sound");
		GUI.Label(new Rect(64.667f, 22.001f, 126.667f, 14.666f), @"Music volume");
		if(focusMusicVolume) { focusMusicVolume = false; GUI.FocusControl("MusicVolume");}
		GUI.SetNextControlName("MusicVolume");
		MusicVolume = GUI.HorizontalSlider(new Rect(12.001f, 40.667f, 161.333f, 14f), MusicVolume, 0f, 1f);
		GUI.Label(new Rect(173.333992553711f,40.667f,40,15),System.Math.Round(MusicVolume,1).ToString());
		GUI.Label(new Rect(67.5f, 67.668f, 126.667f, 14.666f), @"Sound volume");
		if(focusSoundVolume) { focusSoundVolume = false; GUI.FocusControl("SoundVolume");}
		GUI.SetNextControlName("SoundVolume");
		SoundVolume = GUI.HorizontalSlider(new Rect(14.834f, 86.334f, 158.5f, 14f), SoundVolume, 0f, 1f);
		GUI.Label(new Rect(173.334f,86.334f,40,15),System.Math.Round(SoundVolume,1).ToString());
		GUI.EndGroup();
		GUI.Label(new Rect(8f, 8f, 82f, 16f), @"Settings");
		if(focusReset) { focusReset = false; GUI.FocusControl("Reset");}
		GUI.SetNextControlName("Reset");
		bool oldReset = Reset;
		Reset = GUI.Button(new Rect(94f, 8f, 100f, 24f), new GUIContent("Reset",""));
		if (Reset != oldReset && Reset ) {Action("onReset");onButtonClick(); }
		onMouseOver = new Rect(94f, 8f, 100f, 24f).Contains(Event.current.mousePosition);
		if (oldMouseOverReset != onMouseOver && onMouseOver) onOver();
		oldMouseOverReset = onMouseOver;
		GUI.BeginGroup(new Rect(511.5f, 36f, 194.167f, 98f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 98f), "");
		GUI.Label(new Rect(8f, 8f, 55f, 14f), @"Network");
		GUI.Label(new Rect(12f, 32f, 57.13f, 21.96f), @"Sendrate");
		if(focusNetworkSendRate) { focusNetworkSendRate = false; GUI.FocusControl("NetworkSendRate");}
		GUI.SetNextControlName("NetworkSendRate");
		NetworkSendRate = GUI.HorizontalSlider(new Rect(0f, 50f, 179.333f, 14f), NetworkSendRate, 1f, 50f);
		GUI.Label(new Rect(179.332992553711f,50f,40,15),System.Math.Round(NetworkSendRate,1).ToString());
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(511.5f, 280f, 194.167f, 124.333f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 124.333f), "");
		GUI.Label(new Rect(8f, 7.04f, 72.52f, 15.96f), @"Controls
");
		if(focusMouseY) { focusMouseY = false; GUI.FocusControl("MouseY");}
		GUI.SetNextControlName("MouseY");
		MouseY = GUI.HorizontalSlider(new Rect(8.5f, 95.333f, 159.167f, 12f), MouseY, 0f, 2f);
		GUI.Label(new Rect(167.667007446289f,95.333f,40,15),System.Math.Round(MouseY,1).ToString());
		if(focusMouseX) { focusMouseX = false; GUI.FocusControl("MouseX");}
		GUI.SetNextControlName("MouseX");
		MouseX = GUI.HorizontalSlider(new Rect(8f, 59.373f, 159.667f, 12f), MouseX, 0f, 2f);
		GUI.Label(new Rect(167.667007446289f,59.373f,40,15),System.Math.Round(MouseX,1).ToString());
		GUI.Label(new Rect(8f, 39.413f, 140.52f, 15.96f), @"Mouse Sensivity X
");
		GUI.Label(new Rect(8.5f, 75.373f, 127.52f, 15.96f), @"Mouse Sensivity Y
");
		if(focusShowKeyboard) { focusShowKeyboard = false; GUI.FocusControl("ShowKeyboard");}
		GUI.SetNextControlName("ShowKeyboard");
		bool oldShowKeyboard = ShowKeyboard;
		ShowKeyboard = GUI.Button(new Rect(115.5f, 4f, 70.667f, 31f), new GUIContent("Keyboard",""));
		if (ShowKeyboard != oldShowKeyboard && ShowKeyboard ) {Action("onShowKeyboard");onButtonClick(); }
		onMouseOver = new Rect(115.5f, 4f, 70.667f, 31f).Contains(Event.current.mousePosition);
		if (oldMouseOverShowKeyboard != onMouseOver && onMouseOver) onOver();
		oldMouseOverShowKeyboard = onMouseOver;
		GUI.EndGroup();
		if (GUI.Button(new Rect(729f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("onClose"); }
	}


	void Update () {
	
	}
}