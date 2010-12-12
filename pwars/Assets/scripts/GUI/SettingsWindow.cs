
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
		
	public int iGraphicQuality{ get { return PlayerPrefs.GetInt("iGraphicQuality", -1); } set { PlayerPrefs.SetInt("iGraphicQuality", value); } }
	public int iScreenSize{ get { return PlayerPrefs.GetInt("iScreenSize", -1); } set { PlayerPrefs.SetInt("iScreenSize", value); } }
	public float Camx{ get { return PlayerPrefs.GetFloat("Camx", 6f); } set { PlayerPrefs.SetFloat("Camx", value); } }
	public float Camy{ get { return PlayerPrefs.GetFloat("Camy", 4f); } set { PlayerPrefs.SetFloat("Camy", value); } }
	public float Fieldof{ get { return PlayerPrefs.GetFloat("Fieldof", 100f); } set { PlayerPrefs.SetFloat("Fieldof", value); } }
	public float Cam_near{ get { return PlayerPrefs.GetFloat("Cam_near", 1.8f); } set { PlayerPrefs.SetFloat("Cam_near", value); } }
	public float MusicVolume{ get { return PlayerPrefs.GetFloat("MusicVolume", 0.5f); } set { PlayerPrefs.SetFloat("MusicVolume", value); } }
	public float SoundVolume{ get { return PlayerPrefs.GetFloat("SoundVolume", 0.1f); } set { PlayerPrefs.SetFloat("SoundVolume", value); } }
	public float NetworkSendRate{ get { return PlayerPrefs.GetFloat("NetworkSendRate", 15f); } set { PlayerPrefs.SetFloat("NetworkSendRate", value); } }
	public bool focusGraphicQuality;
	public string[] GraphicQuality = new string[] {"Fastest","Fast","Simple","Good","Beautifull","Fantastic",};
	public bool focusScreenSize;
	public string[] ScreenSize = new string[] {};
	public bool focusFullScreen;
	public bool FullScreen=false;
	public bool focusBlood;
	public bool Blood { get { return PlayerPrefs.GetInt("Blood", 1) == 1; } set { PlayerPrefs.SetInt("Blood", value?1:0); } }
	public bool focusDecals;
	public bool Decals { get { return PlayerPrefs.GetInt("Decals", 1) == 1; } set { PlayerPrefs.SetInt("Decals", value?1:0); } }
	public bool focusAtmoSphere;
	public bool AtmoSphere { get { return PlayerPrefs.GetInt("AtmoSphere", 1) == 1; } set { PlayerPrefs.SetInt("AtmoSphere", value?1:0); } }
	public bool focusSao;
	public bool Sao { get { return PlayerPrefs.GetInt("Sao", 1) == 1; } set { PlayerPrefs.SetInt("Sao", value?1:0); } }
	public bool focusShadows;
	public bool Shadows { get { return PlayerPrefs.GetInt("Shadows", 1) == 1; } set { PlayerPrefs.SetInt("Shadows", value?1:0); } }
	public bool focusMotionBlur;
	public bool MotionBlur { get { return PlayerPrefs.GetInt("MotionBlur", 1) == 1; } set { PlayerPrefs.SetInt("MotionBlur", value?1:0); } }
	public bool focusBloomAndFlares;
	public bool BloomAndFlares { get { return PlayerPrefs.GetInt("BloomAndFlares", 1) == 1; } set { PlayerPrefs.SetInt("BloomAndFlares", value?1:0); } }
	public bool focusRenderSettings;
	public string[] RenderSettings = new string[] {"Самый Быстрый","Forward","Deffered",};
	public int iRenderSettings = -1;
	public bool focusCamx;
	public bool focusCamy;
	public bool focusFieldof;
	public bool focusCam_near;
	public bool focusMusicVolume;
	public bool focusSoundVolume;
	public bool focusReset;
	public bool Reset=false;
	public bool focusNetworkSendRate;
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
		GUI.Label(new Rect(184.59f, 24.833f, 137.577f, 21.96f), @"Разрешение екрана");
		GUI.Label(new Rect(8f, 8f, 89.5f, 12.833f), @"Графика");
		GUI.Label(new Rect(38.833f, 24.833f, 109.16f, 21.96f), @"Качество графики");
		if(focusFullScreen) { focusFullScreen = false; GUI.FocusControl("FullScreen");}
		GUI.SetNextControlName("FullScreen");
		bool oldFullScreen = FullScreen;
		FullScreen = GUI.Toggle(new Rect(341.5f, 37f, 105f, 17f),FullScreen, new GUIContent("Полный Екран",""));
		if (FullScreen != oldFullScreen ) {Action("onFullScreen");onButtonClick(); }
		onMouseOver = new Rect(341.5f, 37f, 105f, 17f).Contains(Event.current.mousePosition);
		if (oldMouseOverFullScreen != onMouseOver && onMouseOver) onOver();
		oldMouseOverFullScreen = onMouseOver;
		if(focusBlood) { focusBlood = false; GUI.FocusControl("Blood");}
		GUI.SetNextControlName("Blood");
		bool oldBlood = Blood;
		Blood = GUI.Toggle(new Rect(30.5f, 180f, 50.44667f, 15.96f),Blood, new GUIContent("Кровь",""));
		if (Blood != oldBlood ) {Action("onBlood");onButtonClick(); }
		onMouseOver = new Rect(30.5f, 180f, 50.44667f, 15.96f).Contains(Event.current.mousePosition);
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
		AtmoSphere = GUI.Toggle(new Rect(261.63f, 184f, 78.33334f, 15.96f),AtmoSphere, new GUIContent("Атмосфера",""));
		if (AtmoSphere != oldAtmoSphere ) {Action("onAtmoSphere");onButtonClick(); }
		onMouseOver = new Rect(261.63f, 184f, 78.33334f, 15.96f).Contains(Event.current.mousePosition);
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
		Shadows = GUI.Toggle(new Rect(359.5f, 184f, 43.46333f, 15.96f),Shadows, new GUIContent("Тени",""));
		if (Shadows != oldShadows ) {Action("onShadows");onButtonClick(); }
		onMouseOver = new Rect(359.5f, 184f, 43.46333f, 15.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverShadows != onMouseOver && onMouseOver) onOver();
		oldMouseOverShadows = onMouseOver;
		if(focusMotionBlur) { focusMotionBlur = false; GUI.FocusControl("MotionBlur");}
		GUI.SetNextControlName("MotionBlur");
		bool oldMotionBlur = MotionBlur;
		MotionBlur = GUI.Toggle(new Rect(130.5f, 180f, 84.20667f, 15.96f),MotionBlur, new GUIContent("моушн блур",""));
		if (MotionBlur != oldMotionBlur ) {Action("onMotionBlur");onButtonClick(); }
		onMouseOver = new Rect(130.5f, 180f, 84.20667f, 15.96f).Contains(Event.current.mousePosition);
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
		GUI.Label(new Rect(330.5f, 72f, 49.81667f, 21.96f), @"Рендер");
		GUI.Label(new Rect(8f, 162f, 49.98f, 21.96f), @"Ефекты");
		GUI.BeginGroup(new Rect(8f, 228.92f, 447.5f, 134.667f), "");
		GUI.Box(new Rect(0, 0, 447.5f, 134.667f), "");
		GUI.Label(new Rect(14.667f, 8f, 50.93f, 21.96f), @"Камера");
		GUI.Label(new Rect(43.334f, 35.334f, 170f, 14.666f), @"Положение камеры Ось X");
		if(focusCamx) { focusCamx = false; GUI.FocusControl("Camx");}
		GUI.SetNextControlName("Camx");
		Camx = GUI.HorizontalSlider(new Rect(213.334f, 35.334f, 179.333f, 14f), Camx, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,35.334f,40,15),System.Math.Round(Camx,1).ToString());
		if(focusCamy) { focusCamy = false; GUI.FocusControl("Camy");}
		GUI.SetNextControlName("Camy");
		Camy = GUI.HorizontalSlider(new Rect(213.334f, 62.667f, 179.333f, 14f), Camy, 0f, 10f);
		GUI.Label(new Rect(392.666992553711f,62.667f,40,15),System.Math.Round(Camy,1).ToString());
		GUI.Label(new Rect(43.334f, 62.001f, 170f, 14.666f), @"Положение камеры Ось Y");
		GUI.Label(new Rect(103.334f, 103.667f, 106f, 14.666f), @"Обзор камеры");
		if(focusFieldof) { focusFieldof = false; GUI.FocusControl("Fieldof");}
		GUI.SetNextControlName("Fieldof");
		Fieldof = GUI.HorizontalSlider(new Rect(213.334f, 103.667f, 179.333f, 14f), Fieldof, 40f, 100f);
		GUI.Label(new Rect(392.666992553711f,103.667f,40,15),System.Math.Round(Fieldof,1).ToString());
		GUI.Label(new Rect(103.334f, 80.667f, 106f, 14.666f), @"Скрывать стены");
		if(focusCam_near) { focusCam_near = false; GUI.FocusControl("Cam_near");}
		GUI.SetNextControlName("Cam_near");
		Cam_near = GUI.HorizontalSlider(new Rect(213.334f, 80.667f, 179.333f, 14f), Cam_near, 0.001f, 10f);
		GUI.Label(new Rect(392.666992553711f,80.667f,40,15),System.Math.Round(Cam_near,1).ToString());
		GUI.EndGroup();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(511.5f, 138f, 194.167f, 169f), "");
		GUI.Box(new Rect(0, 0, 194.167f, 169f), "");
		GUI.Label(new Rect(8f, 8f, 34f, 14f), @"Звук");
		GUI.Label(new Rect(64.667f, 34.001f, 126.667f, 14.666f), @"Громкость музыки");
		if(focusMusicVolume) { focusMusicVolume = false; GUI.FocusControl("MusicVolume");}
		GUI.SetNextControlName("MusicVolume");
		MusicVolume = GUI.HorizontalSlider(new Rect(12.001f, 52.667f, 179.333f, 14f), MusicVolume, 0f, 1f);
		GUI.Label(new Rect(191.333992553711f,52.667f,40,15),System.Math.Round(MusicVolume,1).ToString());
		GUI.Label(new Rect(67.5f, 102.668f, 126.667f, 14.666f), @"Громкость звука");
		if(focusSoundVolume) { focusSoundVolume = false; GUI.FocusControl("SoundVolume");}
		GUI.SetNextControlName("SoundVolume");
		SoundVolume = GUI.HorizontalSlider(new Rect(12.001f, 121.334f, 179.333f, 14f), SoundVolume, 0f, 1f);
		GUI.Label(new Rect(191.333992553711f,121.334f,40,15),System.Math.Round(SoundVolume,1).ToString());
		GUI.EndGroup();
		GUI.Label(new Rect(8f, 8f, 82f, 16f), @"Настройки");
		if(focusReset) { focusReset = false; GUI.FocusControl("Reset");}
		GUI.SetNextControlName("Reset");
		bool oldReset = Reset;
		Reset = GUI.Button(new Rect(94f, 8f, 100f, 24f), new GUIContent("Ресет",""));
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
		if (GUI.Button(new Rect(729f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}