﻿
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
public enum SettingsWindowEnum { GraphicQuality,ScreenSize,FullScreen,Blood,Decals,AtmoSphere,Sao,Shadows,MotionBlur,BloomAndFlares,RenderSettings,Contrast,Reset,ShowKeyboard,Close, }
public class SettingsWindow : WindowBase {
		
	internal int iGraphicQuality{ get { return PlayerPrefs.GetInt(Application.platform +"iGraphicQuality", -1); } set { PlayerPrefs.SetInt(Application.platform +"iGraphicQuality", value); } }
	internal int iScreenSize{ get { return PlayerPrefs.GetInt(Application.platform +"iScreenSize", -1); } set { PlayerPrefs.SetInt(Application.platform +"iScreenSize", value); } }
	internal float Camx{ get { return PlayerPrefs.GetFloat(Application.platform +"Camx", 6f); } set { PlayerPrefs.SetFloat(Application.platform +"Camx", value); } }
	internal float Camy{ get { return PlayerPrefs.GetFloat(Application.platform +"Camy", 4f); } set { PlayerPrefs.SetFloat(Application.platform +"Camy", value); } }
	internal float Fieldof{ get { return PlayerPrefs.GetFloat(Application.platform +"Fieldof", 100f); } set { PlayerPrefs.SetFloat(Application.platform +"Fieldof", value); } }
	internal float CamSmooth{ get { return PlayerPrefs.GetFloat(Application.platform +"CamSmooth", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"CamSmooth", value); } }
	internal float MusicVolume{ get { return PlayerPrefs.GetFloat(Application.platform +"MusicVolume", 0.5f); } set { PlayerPrefs.SetFloat(Application.platform +"MusicVolume", value); } }
	internal float SoundVolume{ get { return PlayerPrefs.GetFloat(Application.platform +"SoundVolume", 0.1f); } set { PlayerPrefs.SetFloat(Application.platform +"SoundVolume", value); } }
	internal float NetworkSendRate{ get { return PlayerPrefs.GetFloat(Application.platform +"NetworkSendRate", 15f); } set { PlayerPrefs.SetFloat(Application.platform +"NetworkSendRate", value); } }
	internal float MouseY{ get { return PlayerPrefs.GetFloat(Application.platform +"MouseY", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"MouseY", value); } }
	internal float MouseX{ get { return PlayerPrefs.GetFloat(Application.platform +"MouseX", 1f); } set { PlayerPrefs.SetFloat(Application.platform +"MouseX", value); } }
	
	internal bool vGraphicQuality = true;
	
	internal bool focusGraphicQuality;
	public string[] lGraphicQuality;
	public string GraphicQuality { get { if(lGraphicQuality.Length==0 || iGraphicQuality == -1) return ""; return lGraphicQuality[iGraphicQuality]; } set { iGraphicQuality = lGraphicQuality.SelectIndex(value); }}
	
	internal bool vScreenSize = true;
	
	internal bool focusScreenSize;
	public string[] lScreenSize;
	public string ScreenSize { get { if(lScreenSize.Length==0 || iScreenSize == -1) return ""; return lScreenSize[iScreenSize]; } set { iScreenSize = lScreenSize.SelectIndex(value); }}
	
	internal bool vFullScreen = true;
	
	internal bool focusFullScreen;
	internal bool FullScreen=false;
	
	internal bool vBlood = true;
	
	internal bool focusBlood;
	internal bool Blood { get { return PlayerPrefs.GetInt("Blood", 1) == 1; } set { PlayerPrefs.SetInt("Blood", value?1:0); } }
	
	internal bool vdecals = true;
	
	internal bool focusDecals;
	internal bool Decals { get { return PlayerPrefs.GetInt("Decals", 1) == 1; } set { PlayerPrefs.SetInt("Decals", value?1:0); } }
	
	internal bool vatmoSphere = true;
	
	internal bool focusAtmoSphere;
	internal bool AtmoSphere { get { return PlayerPrefs.GetInt("AtmoSphere", 1) == 1; } set { PlayerPrefs.SetInt("AtmoSphere", value?1:0); } }
	
	internal bool vsao = true;
	
	internal bool focusSao;
	internal bool Sao { get { return PlayerPrefs.GetInt("Sao", 1) == 1; } set { PlayerPrefs.SetInt("Sao", value?1:0); } }
	
	internal bool vshadows = true;
	
	internal bool focusShadows;
	internal bool Shadows { get { return PlayerPrefs.GetInt("Shadows", 1) == 1; } set { PlayerPrefs.SetInt("Shadows", value?1:0); } }
	
	internal bool vmotionBlur = true;
	
	internal bool focusMotionBlur;
	internal bool MotionBlur { get { return PlayerPrefs.GetInt("MotionBlur", 1) == 1; } set { PlayerPrefs.SetInt("MotionBlur", value?1:0); } }
	
	internal bool vbloomAndFlares = true;
	
	internal bool focusBloomAndFlares;
	internal bool BloomAndFlares { get { return PlayerPrefs.GetInt("BloomAndFlares", 1) == 1; } set { PlayerPrefs.SetInt("BloomAndFlares", value?1:0); } }
	
	internal bool vRenderSettings = true;
	
	internal bool focusRenderSettings;
	public string[] lRenderSettings;
	internal int iRenderSettings = -1;
	public string RenderSettings { get { if(lRenderSettings.Length==0 || iRenderSettings == -1) return ""; return lRenderSettings[iRenderSettings]; } set { iRenderSettings = lRenderSettings.SelectIndex(value); }}
	
	internal bool vcamx = true;
	
	internal bool focusCamx;
	
	internal bool vcamy = true;
	
	internal bool focusCamy;
	
	internal bool vfieldof = true;
	
	internal bool focusFieldof;
	
	internal bool vcamSmooth = true;
	
	internal bool focusCamSmooth;
	
	internal bool vContrast = true;
	
	internal bool focusContrast;
	internal bool Contrast { get { return PlayerPrefs.GetInt("Contrast", 1) == 1; } set { PlayerPrefs.SetInt("Contrast", value?1:0); } }
	
	internal bool vMusicVolume = true;
	
	internal bool focusMusicVolume;
	
	internal bool vSoundVolume = true;
	
	internal bool focusSoundVolume;
	
	internal bool vReset = true;
	
	internal bool focusReset;
	internal bool Reset=false;
	
	internal bool vNetworkSendRate = false;
	
	internal bool focusNetworkSendRate;
	
	internal bool vMouseY = true;
	
	internal bool focusMouseY;
	
	internal bool vMouseX = true;
	
	internal bool focusMouseX;
	
	internal bool vShowKeyboard = true;
	
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
	private bool oldMouseOverContrast;
	private bool oldMouseOverReset;
	private bool oldMouseOverShowKeyboard;
	
    
    
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
		vGraphicQuality = true;
		iGraphicQuality = -1;
		vScreenSize = true;
		iScreenSize = -1;
		vFullScreen = true;
		vBlood = true;
		vdecals = true;
		vatmoSphere = true;
		vsao = true;
		vshadows = true;
		vmotionBlur = true;
		vbloomAndFlares = true;
		vRenderSettings = true;
		iRenderSettings = -1;
		vcamx = true;
		vcamy = true;
		vfieldof = true;
		vcamSmooth = true;
		vContrast = true;
		vMusicVolume = true;
		vSoundVolume = true;
		vReset = true;
		vNetworkSendRate = false;
		vMouseY = true;
		vMouseX = true;
		vShowKeyboard = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-366.5f + Screen.width/2,-209f + Screen.height/2,729f,418f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(34.5f, 36f, 463.5f, 368.333f), "");
		GUI.Box(new Rect(0, 0, 463.5f, 368.333f), "");
		if(vGraphicQuality){
		if(focusGraphicQuality) { focusGraphicQuality = false; GUI.FocusControl("GraphicQuality");}
		GUI.SetNextControlName("GraphicQuality");
		GUI.Box(new Rect(49.5f, 38.833f, 111.013f, 112.167f), "");
		sGraphicQuality = GUI.BeginScrollView(new Rect(49.5f, 38.833f, 111.013f, 112.167f), sGraphicQuality, new Rect(0,0, 101.013f, lGraphicQuality.Length* 15.960000038147f));
		int oldGraphicQuality = iGraphicQuality;
		iGraphicQuality = GUI.SelectionGrid(new Rect(0,0, 101.013f, lGraphicQuality.Length* 15.960000038147f), iGraphicQuality, lGraphicQuality,1,GUI.skin.customStyles[0]);
		if (iGraphicQuality != oldGraphicQuality) Action("GraphicQuality");
		GUI.EndScrollView();
		}
		if(vScreenSize){
		if(focusScreenSize) { focusScreenSize = false; GUI.FocusControl("ScreenSize");}
		GUI.SetNextControlName("ScreenSize");
		GUI.Box(new Rect(182.833f, 38.833f, 114.667f, 112.167f), "");
		sScreenSize = GUI.BeginScrollView(new Rect(182.833f, 38.833f, 114.667f, 112.167f), sScreenSize, new Rect(0,0, 104.667f, lScreenSize.Length* 15f));
		int oldScreenSize = iScreenSize;
		iScreenSize = GUI.SelectionGrid(new Rect(0,0, 104.667f, lScreenSize.Length* 15f), iScreenSize, lScreenSize,1,GUI.skin.customStyles[0]);
		if (iScreenSize != oldScreenSize) Action("ScreenSize");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(184.59f, 24.833f, 137.577f, 21.96f), @"Screen Resolution");
		GUI.Label(new Rect(8f, 8f, 89.5f, 12.833f), @"Graphics");
		GUI.Label(new Rect(38.833f, 24.833f, 130f, 21.96f), @"Graphics Quality");
		if(vFullScreen){
		if(focusFullScreen) { focusFullScreen = false; GUI.FocusControl("FullScreen");}
		GUI.SetNextControlName("FullScreen");
		bool oldFullScreen = FullScreen;
		FullScreen = GUI.Toggle(new Rect(341.5f, 37f, 105f, 17f),FullScreen, new GUIContent(@"Full Screen",""));
		if (FullScreen != oldFullScreen ) {Action("FullScreen");onButtonClick(); }
		onMouseOver = new Rect(341.5f, 37f, 105f, 17f).Contains(Event.current.mousePosition);
		if (oldMouseOverFullScreen != onMouseOver && onMouseOver) onOver();
		oldMouseOverFullScreen = onMouseOver;
		}
		if(vBlood){
		if(focusBlood) { focusBlood = false; GUI.FocusControl("Blood");}
		GUI.SetNextControlName("Blood");
		bool oldBlood = Blood;
		Blood = GUI.Toggle(new Rect(30.5f, 180f, 47.91333f, 15.96f),Blood, new GUIContent(@"Blood",""));
		if (Blood != oldBlood ) {Action("Blood");onButtonClick(); }
		onMouseOver = new Rect(30.5f, 180f, 47.91333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverBlood != onMouseOver && onMouseOver) onOver();
		oldMouseOverBlood = onMouseOver;
		}
		if(vdecals){
		if(focusDecals) { focusDecals = false; GUI.FocusControl("Decals");}
		GUI.SetNextControlName("Decals");
		bool oldDecals = Decals;
		Decals = GUI.Toggle(new Rect(30.5f, 199.96f, 49f, 15.96f),Decals, new GUIContent(@"decals",""));
		if (Decals != oldDecals ) {Action("Decals");onButtonClick(); }
		onMouseOver = new Rect(30.5f, 199.96f, 49f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverDecals != onMouseOver && onMouseOver) onOver();
		oldMouseOverDecals = onMouseOver;
		}
		if(vatmoSphere){
		if(focusAtmoSphere) { focusAtmoSphere = false; GUI.FocusControl("AtmoSphere");}
		GUI.SetNextControlName("AtmoSphere");
		bool oldAtmoSphere = AtmoSphere;
		AtmoSphere = GUI.Toggle(new Rect(261.63f, 184f, 81.83667f, 15.96f),AtmoSphere, new GUIContent(@"Atmosphere",""));
		if (AtmoSphere != oldAtmoSphere ) {Action("AtmoSphere");onButtonClick(); }
		onMouseOver = new Rect(261.63f, 184f, 81.83667f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverAtmoSphere != onMouseOver && onMouseOver) onOver();
		oldMouseOverAtmoSphere = onMouseOver;
		}
		if(vsao){
		if(focusSao) { focusSao = false; GUI.FocusControl("Sao");}
		GUI.SetNextControlName("Sao");
		bool oldSao = Sao;
		Sao = GUI.Toggle(new Rect(261.63f, 199.96f, 60.53667f, 15.96f),Sao, new GUIContent(@"ambient",""));
		if (Sao != oldSao ) {Action("Sao");onButtonClick(); }
		onMouseOver = new Rect(261.63f, 199.96f, 60.53667f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverSao != onMouseOver && onMouseOver) onOver();
		oldMouseOverSao = onMouseOver;
		}
		if(vshadows){
		if(focusShadows) { focusShadows = false; GUI.FocusControl("Shadows");}
		GUI.SetNextControlName("Shadows");
		bool oldShadows = Shadows;
		Shadows = GUI.Toggle(new Rect(359.5f, 184f, 64.13333f, 15.96f),Shadows, new GUIContent(@"Shadows",""));
		if (Shadows != oldShadows ) {Action("Shadows");onButtonClick(); }
		onMouseOver = new Rect(359.5f, 184f, 64.13333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverShadows != onMouseOver && onMouseOver) onOver();
		oldMouseOverShadows = onMouseOver;
		}
		if(vmotionBlur){
		if(focusMotionBlur) { focusMotionBlur = false; GUI.FocusControl("MotionBlur");}
		GUI.SetNextControlName("MotionBlur");
		bool oldMotionBlur = MotionBlur;
		MotionBlur = GUI.Toggle(new Rect(130.5f, 180f, 79.37f, 15.96f),MotionBlur, new GUIContent(@"motion blur",""));
		if (MotionBlur != oldMotionBlur ) {Action("MotionBlur");onButtonClick(); }
		onMouseOver = new Rect(130.5f, 180f, 79.37f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverMotionBlur != onMouseOver && onMouseOver) onOver();
		oldMouseOverMotionBlur = onMouseOver;
		}
		if(vbloomAndFlares){
		if(focusBloomAndFlares) { focusBloomAndFlares = false; GUI.FocusControl("BloomAndFlares");}
		GUI.SetNextControlName("BloomAndFlares");
		bool oldBloomAndFlares = BloomAndFlares;
		BloomAndFlares = GUI.Toggle(new Rect(130.5f, 199.96f, 106.2067f, 15.96f),BloomAndFlares, new GUIContent(@"bloom and flares",""));
		if (BloomAndFlares != oldBloomAndFlares ) {Action("BloomAndFlares");onButtonClick(); }
		onMouseOver = new Rect(130.5f, 199.96f, 106.2067f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverBloomAndFlares != onMouseOver && onMouseOver) onOver();
		oldMouseOverBloomAndFlares = onMouseOver;
		}
		if(vRenderSettings){
		if(focusRenderSettings) { focusRenderSettings = false; GUI.FocusControl("RenderSettings");}
		GUI.SetNextControlName("RenderSettings");
		GUI.Box(new Rect(326.5f, 87f, 129f, 64f), "");
		sRenderSettings = GUI.BeginScrollView(new Rect(326.5f, 87f, 129f, 64f), sRenderSettings, new Rect(0,0, 119f, lRenderSettings.Length* 15.960000038147f));
		int oldRenderSettings = iRenderSettings;
		iRenderSettings = GUI.SelectionGrid(new Rect(0,0, 119f, lRenderSettings.Length* 15.960000038147f), iRenderSettings, lRenderSettings,1,GUI.skin.customStyles[0]);
		if (iRenderSettings != oldRenderSettings) Action("RenderSettings");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(330.5f, 72f, 47.76f, 21.96f), @"Render");
		GUI.Label(new Rect(8f, 162f, 44.56333f, 21.96f), @"Effects");
		GUI.BeginGroup(new Rect(8f, 228.92f, 447.5f, 134.667f), "");
		GUI.Box(new Rect(0, 0, 447.5f, 134.667f), "");
		GUI.Label(new Rect(14.667f, 8f, 50.43f, 21.96f), @"Camera");
		GUI.Label(new Rect(43.334f, 35.334f, 170f, 14.666f), @"Camera X");
		if(vcamx){
		if(focusCamx) { focusCamx = false; GUI.FocusControl("Camx");}
		GUI.SetNextControlName("Camx");
		Camx = GUI.HorizontalSlider(new Rect(213.334f, 35.334f, 179.333f, 14f), Camx, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,35.334f,40,15),System.Math.Round(Camx,1).ToString());
		}
		if(vcamy){
		if(focusCamy) { focusCamy = false; GUI.FocusControl("Camy");}
		GUI.SetNextControlName("Camy");
		Camy = GUI.HorizontalSlider(new Rect(213.334f, 62.667f, 179.333f, 14f), Camy, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,62.667f,40,15),System.Math.Round(Camy,1).ToString());
		}
		GUI.Label(new Rect(43.334f, 62.001f, 170f, 14.666f), @"Camera Y");
		GUI.Label(new Rect(43.334f, 103.667f, 106f, 14.666f), @"Field of view");
		if(vfieldof){
		if(focusFieldof) { focusFieldof = false; GUI.FocusControl("Fieldof");}
		GUI.SetNextControlName("Fieldof");
		Fieldof = GUI.HorizontalSlider(new Rect(213.334f, 103.667f, 179.333f, 14f), Fieldof, 40f, 100f);
		GUI.Label(new Rect(392.666992553711f,103.667f,40,15),System.Math.Round(Fieldof,1).ToString());
		}
		GUI.Label(new Rect(43.334f, 80.667f, 106f, 14.666f), @"Camera Smooth");
		if(vcamSmooth){
		if(focusCamSmooth) { focusCamSmooth = false; GUI.FocusControl("CamSmooth");}
		GUI.SetNextControlName("CamSmooth");
		CamSmooth = GUI.HorizontalSlider(new Rect(213.334f, 80.667f, 179.333f, 14f), CamSmooth, 0f, 2f);
		GUI.Label(new Rect(392.666992553711f,80.667f,40,15),System.Math.Round(CamSmooth,1).ToString());
		}
		GUI.EndGroup();
		if(vContrast){
		if(focusContrast) { focusContrast = false; GUI.FocusControl("Contrast");}
		GUI.SetNextControlName("Contrast");
		bool oldContrast = Contrast;
		Contrast = GUI.Toggle(new Rect(359.5f, 199.96f, 61.75667f, 15.96f),Contrast, new GUIContent(@"Contrast",""));
		if (Contrast != oldContrast ) {Action("Contrast");onButtonClick(); }
		onMouseOver = new Rect(359.5f, 199.96f, 61.75667f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverContrast != onMouseOver && onMouseOver) onOver();
		oldMouseOverContrast = onMouseOver;
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(511.5f, 138f, 194.167f, 121f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 121f), "");
		GUI.Label(new Rect(8f, 8f, 51f, 14f), @"Sound");
		GUI.Label(new Rect(64.667f, 22.001f, 126.667f, 14.666f), @"Music volume");
		if(vMusicVolume){
		if(focusMusicVolume) { focusMusicVolume = false; GUI.FocusControl("MusicVolume");}
		GUI.SetNextControlName("MusicVolume");
		MusicVolume = GUI.HorizontalSlider(new Rect(12.001f, 40.667f, 161.333f, 14f), MusicVolume, 0f, 1f);
		GUI.Label(new Rect(173.333992553711f,40.667f,40,15),System.Math.Round(MusicVolume,1).ToString());
		}
		GUI.Label(new Rect(67.5f, 67.668f, 126.667f, 14.666f), @"Sound volume");
		if(vSoundVolume){
		if(focusSoundVolume) { focusSoundVolume = false; GUI.FocusControl("SoundVolume");}
		GUI.SetNextControlName("SoundVolume");
		SoundVolume = GUI.HorizontalSlider(new Rect(14.834f, 86.334f, 158.5f, 14f), SoundVolume, 0f, 1f);
		GUI.Label(new Rect(173.334f,86.334f,40,15),System.Math.Round(SoundVolume,1).ToString());
		}
		GUI.EndGroup();
		GUI.Label(new Rect(8f, 8f, 82f, 16f), @"Settings");
		if(vReset){
		if(focusReset) { focusReset = false; GUI.FocusControl("Reset");}
		GUI.SetNextControlName("Reset");
		bool oldReset = Reset;
		Reset = GUI.Button(new Rect(94f, 8f, 100f, 24f), new GUIContent(@"Reset",""));
		if (Reset != oldReset && Reset ) {Action("Reset");onButtonClick(); }
		onMouseOver = new Rect(94f, 8f, 100f, 24f).Contains(Event.current.mousePosition);
		if (oldMouseOverReset != onMouseOver && onMouseOver) onOver();
		oldMouseOverReset = onMouseOver;
		}
		GUI.BeginGroup(new Rect(511.5f, 36f, 194.167f, 98f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 98f), "");
		GUI.Label(new Rect(8f, 8f, 55f, 14f), @"Network");
		GUI.Label(new Rect(12f, 32f, 57.13f, 21.96f), @"Sendrate");
		if(vNetworkSendRate){
		if(focusNetworkSendRate) { focusNetworkSendRate = false; GUI.FocusControl("NetworkSendRate");}
		GUI.SetNextControlName("NetworkSendRate");
		NetworkSendRate = GUI.HorizontalSlider(new Rect(0f, 50f, 179.333f, 14f), NetworkSendRate, 1f, 50f);
		GUI.Label(new Rect(179.332992553711f,50f,40,15),System.Math.Round(NetworkSendRate,1).ToString());
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(511.5f, 280f, 194.167f, 124.333f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 124.333f), "");
		GUI.Label(new Rect(8f, 7.04f, 72.52f, 15.96f), @"Controls
");
		if(vMouseY){
		if(focusMouseY) { focusMouseY = false; GUI.FocusControl("MouseY");}
		GUI.SetNextControlName("MouseY");
		MouseY = GUI.HorizontalSlider(new Rect(8.5f, 95.333f, 159.167f, 12f), MouseY, 0f, 2f);
		GUI.Label(new Rect(167.667007446289f,95.333f,40,15),System.Math.Round(MouseY,1).ToString());
		}
		if(vMouseX){
		if(focusMouseX) { focusMouseX = false; GUI.FocusControl("MouseX");}
		GUI.SetNextControlName("MouseX");
		MouseX = GUI.HorizontalSlider(new Rect(8f, 59.373f, 159.667f, 12f), MouseX, 0f, 2f);
		GUI.Label(new Rect(167.667007446289f,59.373f,40,15),System.Math.Round(MouseX,1).ToString());
		}
		GUI.Label(new Rect(8f, 39.413f, 140.52f, 15.96f), @"Mouse Sensivity X
");
		GUI.Label(new Rect(8.5f, 75.373f, 127.52f, 15.96f), @"Mouse Sensivity Y
");
		if(vShowKeyboard){
		if(focusShowKeyboard) { focusShowKeyboard = false; GUI.FocusControl("ShowKeyboard");}
		GUI.SetNextControlName("ShowKeyboard");
		bool oldShowKeyboard = ShowKeyboard;
		ShowKeyboard = GUI.Button(new Rect(115.5f, 4f, 70.667f, 31f), new GUIContent(@"Keyboard",""));
		if (ShowKeyboard != oldShowKeyboard && ShowKeyboard ) {Action("ShowKeyboard");onButtonClick(); }
		onMouseOver = new Rect(115.5f, 4f, 70.667f, 31f).Contains(Event.current.mousePosition);
		if (oldMouseOverShowKeyboard != onMouseOver && onMouseOver) onOver();
		oldMouseOverShowKeyboard = onMouseOver;
		}
		GUI.EndGroup();
		if (GUI.Button(new Rect(729f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}