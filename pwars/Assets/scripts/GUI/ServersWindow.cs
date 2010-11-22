
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

public class ServersWindow : WindowBase {
		
	public string Ipaddress{ get { return PlayerPrefs.GetString("Ipaddress", @""); } set { PlayerPrefs.SetString("Ipaddress", value); } }
	public int Port{ get { return PlayerPrefs.GetInt("Port", 5300); } set { PlayerPrefs.SetInt("Port", value); } }
	public bool focusIpaddress;
	public bool isReadOnlyIpaddress = false;
	public bool focusPort;
	public bool isReadOnlyPort = false;
	public bool focusConnect;
	public bool Connect=false;
	public bool focusServersTable;
	public string[] ServersTable = new string[] {};
	public int iServersTable = -1;
	public bool focusRefresh;
	public bool Refresh=false;
	public bool focusServersTitle;
	public bool isReadOnlyServersTitle = true;
	public string ServersTitle = @"  Название_сервера         Карта           Тип_Игры          Игроков        Пинг";
	private int wndid1;
	private bool oldMouseOverConnect;
	private Vector2 sServersTable;
	private bool oldMouseOverRefresh;
	
    
    
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
        
		GUI.Window(wndid1,new Rect(-326.5f + Screen.width/2,-287f + Screen.height/2,618f,492f), Wnd1,"");

    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		GUI.BeginGroup(new Rect(8f, 27f, 329f, 81f), "");
		GUI.Box(new Rect(0, 0, 329f, 81f), "");
		GUI.Label(new Rect(21f, 30f, 66.89667f, 21.96f), @"ip аддресс");
		if(focusIpaddress) { focusIpaddress = false; GUI.FocusControl("Ipaddress");}
		GUI.SetNextControlName("Ipaddress");
		if(isReadOnlyIpaddress){
		GUI.Label(new Rect(103.3f, 30f, 123f, 14f), Ipaddress.ToString());
		} else
		Ipaddress = GUI.TextField(new Rect(103.3f, 30f, 123f, 14f), Ipaddress);
		GUI.Label(new Rect(64.38f, 48f, 32.32333f, 21.96f), @"port");
		if(focusPort) { focusPort = false; GUI.FocusControl("Port");}
		GUI.SetNextControlName("Port");
		if(isReadOnlyPort){
		GUI.Label(new Rect(103.3f, 48f, 123f, 14f), Port.ToString());
		} else
		Port = int.Parse(GUI.TextField(new Rect(103.3f, 48f, 123f, 14f), Port.ToString()));
		if(focusConnect) { focusConnect = false; GUI.FocusControl("Connect");}
		GUI.SetNextControlName("Connect");
		bool oldConnect = Connect;
		Connect = GUI.Button(new Rect(244f, 30f, 75f, 32f), new GUIContent("Соеденить",""));
		if (Connect != oldConnect && Connect ) {Action("onConnect");onButtonClick(); }
		onMouseOver = new Rect(244f, 30f, 75f, 32f).Contains(Event.current.mousePosition);
		if (oldMouseOverConnect != onMouseOver && onMouseOver) onOver();
		oldMouseOverConnect = onMouseOver;
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(8f, 112f, 602f, 355f), "");
		GUI.Box(new Rect(0, 0, 602f, 355f), "");
		if(focusServersTable) { focusServersTable = false; GUI.FocusControl("ServersTable");}
		GUI.SetNextControlName("ServersTable");
		GUI.Box(new Rect(8f, 48f, 586f, 299f), "");
		sServersTable = GUI.BeginScrollView(new Rect(8f, 48f, 586f, 299f), sServersTable, new Rect(0,0, 566f, ServersTable.Length* 15f));
		int oldServersTable = iServersTable;
		iServersTable = GUI.SelectionGrid(new Rect(0,0, 566f, ServersTable.Length* 15f), iServersTable, ServersTable,1,GUI.skin.customStyles[0]);
		if (iServersTable != oldServersTable) Action("onServersTable",ServersTable[iServersTable]);
		GUI.EndScrollView();
		GUI.Label(new Rect(0f, 0f, 114.45f, 14f), @"Список Серверов");
		if(focusRefresh) { focusRefresh = false; GUI.FocusControl("Refresh");}
		GUI.SetNextControlName("Refresh");
		bool oldRefresh = Refresh;
		Refresh = GUI.Button(new Rect(512f, 4.04f, 82f, 21.96f), new GUIContent("Refresh",""));
		if (Refresh != oldRefresh && Refresh ) {Action("onRefresh");onButtonClick(); }
		onMouseOver = new Rect(512f, 4.04f, 82f, 21.96f).Contains(Event.current.mousePosition);
		if (oldMouseOverRefresh != onMouseOver && onMouseOver) onOver();
		oldMouseOverRefresh = onMouseOver;
		if(focusServersTitle) { focusServersTitle = false; GUI.FocusControl("ServersTitle");}
		GUI.SetNextControlName("ServersTitle");
		if(isReadOnlyServersTitle){
		GUI.Label(new Rect(8f, 30f, 586f, 14f), ServersTitle.ToString(), GUI.skin.customStyles[2]);
		} else
		ServersTitle = GUI.TextField(new Rect(8f, 30f, 586f, 14f), ServersTitle, GUI.skin.customStyles[2]);
		GUI.Box(new Rect(173f, 30f, 1f, 317f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(282f, 30f, 1f, 318.907f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(404f, 30f, 1f, 318.511f),"",GUI.skin.customStyles[4]);
		GUI.Box(new Rect(523f, 30f, 1f, 318.191f),"",GUI.skin.customStyles[4]);
		GUI.EndGroup();
		GUI.Label(new Rect(8f, 27f, 56.61f, 14f), @"Сервера");
		if (GUI.Button(new Rect(618f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();ActionAll("onClose"); }
	}


	void Update () {
	
	}
}