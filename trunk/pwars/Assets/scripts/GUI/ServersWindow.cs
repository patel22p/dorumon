
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static ServersWindow __ServersWindow;
    public static ServersWindow _ServersWindow { get { if (__ServersWindow == null) __ServersWindow = (ServersWindow)MonoBehaviour.FindObjectOfType(typeof(ServersWindow)); return __ServersWindow; } }
}
public enum ServersWindowEnum { Connect,ServersTable,Refresh,Close, }
public class ServersWindow : WindowBase {
		
	internal string Ipaddress{ get { return PlayerPrefs.GetString(Application.platform +"Ipaddress", @""); } set { PlayerPrefs.SetString(Application.platform +"Ipaddress", value); } }
	internal int Port{ get { return PlayerPrefs.GetInt(Application.platform +"Port", 5300); } set { PlayerPrefs.SetInt(Application.platform +"Port", value); } }
	
	internal bool vipaddress = true;
	
	internal bool focusIpaddress;
	
	internal bool rIpaddress = false;
	
	internal bool vport = true;
	
	internal bool focusPort;
	
	internal bool rPort = false;
	
	internal bool vconnect = true;
	
	internal bool focusConnect;
	internal bool Connect=false;
	
	internal bool vServersTable = true;
	
	internal bool focusServersTable;
	public string[] lServersTable;
	internal int iServersTable = -1;
	public string ServersTable { get { if(lServersTable.Length==0 || iServersTable == -1) return ""; return lServersTable[iServersTable]; } set { iServersTable = lServersTable.SelectIndex(value); }}
	
	internal bool vRefresh = true;
	
	internal bool focusRefresh;
	internal bool Refresh=false;
	
	internal bool vserversTitle = true;
	
	internal bool focusServersTitle;
	
	internal bool rServersTitle = true;
	internal string ServersTitle = @" Creator              Ver    Map                Game_Type                      Users Ping";
	
	internal bool vhostCount = true;
	
	internal bool focusHostCount;
	
	internal bool rHostCount = false;
	internal int HostCount = 0;
	private int wndid1;
	private bool oldMouseOverConnect;
	private Vector2 sServersTable;
	private bool oldMouseOverRefresh;
	
    
    
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
		vipaddress = true;
		vport = true;
		vconnect = true;
		vServersTable = true;
		iServersTable = -1;
		vRefresh = true;
		vserversTitle = true;
		vhostCount = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-373.5f + Screen.width/2,-256f + Screen.height/2,739f,492f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(8f, 27f, 329f, 81f), "");
		GUI.Box(new Rect(0, 0, 329f, 81f), "");
		GUI.Label(new Rect(17f, 30f, 80f, 21.96f), @"ip address");
		if(vipaddress){
		if(focusIpaddress) { focusIpaddress = false; GUI.FocusControl("Ipaddress");}
		GUI.SetNextControlName("Ipaddress");
		if(rIpaddress){
		GUI.Label(new Rect(103.3f, 30f, 123f, 14f), Ipaddress.ToString());
		} else
		try {Ipaddress = GUI.TextField(new Rect(103.3f, 30f, 123f, 14f), Ipaddress,100);}catch{};
		}
		GUI.Label(new Rect(64.38f, 48f, 32.32333f, 21.96f), @"port");
		if(vport){
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(rPort){
		GUI.Label(new Rect(103.3f, 48f, 123f, 14f), Port.ToString());
		} else
		try {Port = int.Parse(GUI.TextField(new Rect(103.3f, 48f, 123f, 14f), Port.ToString(),100));}catch{};
		}
		if(vconnect){
		if(focusConnect) { focusConnect = false; GUI.FocusControl("Connect");}
		GUI.SetNextControlName("Connect");
		bool oldConnect = Connect;
		Connect = GUI.Button(new Rect(244f, 30f, 75f, 32f), new GUIContent("Connect",""));
		if (Connect != oldConnect && Connect ) {Action("Connect");onButtonClick(); }
		onMouseOver = new Rect(244f, 30f, 75f, 32f).Contains(Event.current.mousePosition);
		if (oldMouseOverConnect != onMouseOver && onMouseOver) onOver();
		oldMouseOverConnect = onMouseOver;
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(8f, 112f, 723f, 355f), "");
		GUI.Box(new Rect(0, 0, 723f, 355f), "");
		if(vServersTable){
		if(focusServersTable) { focusServersTable = false; GUI.FocusControl("ServersTable");}
		GUI.SetNextControlName("ServersTable");
		GUI.Box(new Rect(8f, 48f, 707f, 299f), "");
		sServersTable = GUI.BeginScrollView(new Rect(8f, 48f, 707f, 299f), sServersTable, new Rect(0,0, 697f, lServersTable.Length* 15f));
		int oldServersTable = iServersTable;
		iServersTable = GUI.SelectionGrid(new Rect(0,0, 697f, lServersTable.Length* 15f), iServersTable, lServersTable,1,GUI.skin.customStyles[0]);
		if (iServersTable != oldServersTable) Action("ServersTable");
		GUI.EndScrollView();
		}
		GUI.Label(new Rect(0f, 0f, 114.45f, 14f), @"Server List");
		if(vRefresh){
		if(focusRefresh) { focusRefresh = false; GUI.FocusControl("Refresh");}
		GUI.SetNextControlName("Refresh");
		bool oldRefresh = Refresh;
		Refresh = GUI.Button(new Rect(633f, 4.04f, 82f, 21.96f), new GUIContent("Refresh",""));
		if (Refresh != oldRefresh && Refresh ) {Action("Refresh");onButtonClick(); }
		onMouseOver = new Rect(633f, 4.04f, 82f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverRefresh != onMouseOver && onMouseOver) onOver();
		oldMouseOverRefresh = onMouseOver;
		}
		if(vserversTitle){
		if(focusServersTitle) { focusServersTitle = false; GUI.FocusControl("ServersTitle");}
		GUI.SetNextControlName("ServersTitle");
		if(rServersTitle){
		GUI.Label(new Rect(8f, 30f, 707f, 14f), ServersTitle.ToString(), GUI.skin.customStyles[2]);
		} else
		try {ServersTitle = GUI.TextField(new Rect(8f, 30f, 707f, 14f), ServersTitle,100, GUI.skin.customStyles[2]);}catch{};
		}
		GUI.EndGroup();
		GUI.Label(new Rect(8f, 27f, 56.61f, 14f), @"Server");
		if(vhostCount){
		if(focusHostCount) { focusHostCount = false; GUI.FocusControl("HostCount");}
		GUI.SetNextControlName("HostCount");
		if(rHostCount){
		GUI.Label(new Rect(550f, 476f, 39f, 14f), HostCount.ToString(), GUI.skin.customStyles[2]);
		} else
		try {HostCount = int.Parse(GUI.TextField(new Rect(550f, 476f, 39f, 14f), HostCount.ToString(),100, GUI.skin.customStyles[2]));}catch{};
		}
		if (GUI.Button(new Rect(739f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}