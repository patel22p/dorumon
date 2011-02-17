
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static UserWindow __UserWindow;
    public static UserWindow _UserWindow { get { if (__UserWindow == null) __UserWindow = (UserWindow)MonoBehaviour.FindObjectOfType(typeof(UserWindow)); return __UserWindow; } }
}
public enum UserWindowEnum { SaveUser,Prev,Next,UserScores,RefreshUserInfo,Close, }
public class UserWindow : WindowBase {
		
	
	internal bool vSaveUser = true;
	
	internal bool focusSaveUser;
	internal bool SaveUser=false;
	
	internal bool vUserNick = true;
	
	internal bool focusUserNick;
	
	internal bool rUserNick = true;
	internal string UserNick = @"";
	
	internal bool vFirstName = true;
	
	internal bool focusFirstName;
	
	internal bool rFirstName = false;
	internal string FirstName = @"";
	
	internal bool vAvatarUrl = true;
	
	internal bool focusAvatarUrl;
	
	internal bool rAvatarUrl = false;
	internal string AvatarUrl = @"";
	
	internal bool vDesctiption = true;
	
	internal bool focusDesctiption;
	
	internal bool rDesctiption = false;
	internal string Desctiption = @"";
	
	internal bool vBallRender = true;
	
	internal bool focusBallRender;
	[FindAsset("ballrender")]
	public Texture imgBallRender;
	
	internal bool vPrev = true;
	
	internal bool focusPrev;
	internal bool Prev=false;
	
	internal bool vNext = true;
	
	internal bool focusNext;
	internal bool Next=false;
	
	internal bool vBallImage = true;
	
	internal bool focusBallImage;
	
	internal bool rBallImage = false;
	internal string BallImage = @"";
	
	internal bool vMaterialName = true;
	
	internal bool focusMaterialName;
	
	internal bool rMaterialName = true;
	internal string MaterialName = @"name";
	
	internal bool vUserScores = true;
	
	internal bool focusUserScores;
	public string[] lUserScores;
	internal int iUserScores = -1;
	public string UserScores { get { if(lUserScores.Length==0 || iUserScores == -1) return ""; return lUserScores[iUserScores]; } set { iUserScores = lUserScores.SelectIndex(value); }}
	
	internal bool vtableheader = true;
	
	internal bool focusTableheader;
	
	internal bool rTableheader = false;
	internal string Tableheader = @" Place  Game_Type                        score           Deaths";
	
	internal bool vRefreshUserInfo = true;
	
	internal bool focusRefreshUserInfo;
	internal bool RefreshUserInfo=false;
	
	internal bool vAvatar = true;
	
	internal bool focusAvatar;
	public Texture imgAvatar;
	private int wndid1;
	private bool oldMouseOverSaveUser;
	private Rect BallRender;
	private bool oldMouseOverPrev;
	private bool oldMouseOverNext;
	private Vector2 sUserScores;
	private bool oldMouseOverRefreshUserInfo;
	private Rect Avatar;
	
    
    
	void Start () {
		AlwaysOnTop = false;
		wndid1 = 0;
		BallRender = new Rect(0f, 1.333f, 195f, 171.868f);
		Avatar = new Rect(8f, 8f, 82f, 66f);

	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
		vSaveUser = true;
		vUserNick = true;
		vFirstName = true;
		vAvatarUrl = true;
		vDesctiption = true;
		vBallRender = true;
		vPrev = true;
		vNext = true;
		vBallImage = true;
		vMaterialName = true;
		vUserScores = true;
		iUserScores = -1;
		vtableheader = true;
		vRefreshUserInfo = true;
		vAvatar = true;

        base.ResetValues();
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
		GUI.Window(wndid1,new Rect(-328.5f + Screen.width/2,-262f + Screen.height/2,676f,538f), Wnd1,"");
		base.OnGUI();
    }
	void Wnd1(int id){
		if (focusWindow) {GUI.FocusWindow(id);GUI.BringWindowToFront(id);}
		focusWindow = false;
		bool onMouseOver;
		if(vSaveUser){
		if(focusSaveUser) { focusSaveUser = false; GUI.FocusControl("SaveUser");}
		GUI.SetNextControlName("SaveUser");
		bool oldSaveUser = SaveUser;
		SaveUser = GUI.Button(new Rect(567f, 505f, 101f, 25f), new GUIContent(@"Save",""));
		if (SaveUser != oldSaveUser && SaveUser ) {Action("SaveUser");onButtonClick(); }
		onMouseOver = new Rect(567f, 505f, 101f, 25f).Contains(Event.current.mousePosition);
		if (oldMouseOverSaveUser != onMouseOver && onMouseOver) onOver();
		oldMouseOverSaveUser = onMouseOver;
		}
		if(vUserNick){
		if(focusUserNick) { focusUserNick = false; GUI.FocusControl("UserNick");}
		GUI.SetNextControlName("UserNick");
		if(rUserNick){
		GUI.Label(new Rect(110f, 8f, 198f, 13f), UserNick.ToString());
		} else
		try {UserNick = GUI.TextField(new Rect(110f, 8f, 198f, 13f), UserNick,100);}catch{};
		}
		GUI.BeginGroup(new Rect(60f, 100f, 308f, 203f), "");
		GUI.Box(new Rect(0, 0, 308f, 203f), "");
		GUI.Label(new Rect(67f, 23f, 41.69667f, 21.96f), @"Name");
		if(vFirstName){
		if(focusFirstName) { focusFirstName = false; GUI.FocusControl("FirstName");}
		GUI.SetNextControlName("FirstName");
		if(rFirstName){
		GUI.Label(new Rect(115f, 23f, 175f, 14f), FirstName.ToString());
		} else
		try {FirstName = GUI.TextField(new Rect(115f, 23f, 175f, 14f), FirstName,20);}catch{};
		}
		GUI.Label(new Rect(28f, 54.04f, 87f, 21.96f), @"Avatar URL");
		if(vAvatarUrl){
		if(focusAvatarUrl) { focusAvatarUrl = false; GUI.FocusControl("AvatarUrl");}
		GUI.SetNextControlName("AvatarUrl");
		if(rAvatarUrl){
		GUI.Label(new Rect(115f, 54.04f, 175f, 14f), AvatarUrl.ToString());
		} else
		try {AvatarUrl = GUI.TextField(new Rect(115f, 54.04f, 175f, 14f), AvatarUrl,100);}catch{};
		}
		GUI.Label(new Rect(19f, 84f, 92f, 21.96f), @"Description");
		if(vDesctiption){
		if(focusDesctiption) { focusDesctiption = false; GUI.FocusControl("Desctiption");}
		GUI.SetNextControlName("Desctiption");
		if(rDesctiption){
		GUI.Label(new Rect(115f, 84f, 175f, 94f), Desctiption.ToString());
		} else
		try {Desctiption = GUI.TextField(new Rect(115f, 84f, 175f, 94f), Desctiption,100);}catch{};
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(401f, 58f, 246f, 284f), "");
		GUI.Box(new Rect(0, 0, 246f, 284f), "");
		GUI.BeginGroup(new Rect(25f, 46f, 193f, 202f), "");
		GUI.Box(new Rect(0, 0, 193f, 202f), "");
		if(vBallRender){
		if(focusBallRender) { focusBallRender = false; GUI.FocusControl("BallRender");}
		GUI.SetNextControlName("BallRender");
		if(imgBallRender!=null)
			GUI.DrawTexture(BallRender,imgBallRender, ScaleMode.ScaleToFit, imgBallRender is RenderTexture?false:true);
		}
		if(vPrev){
		if(focusPrev) { focusPrev = false; GUI.FocusControl("Prev");}
		GUI.SetNextControlName("Prev");
		bool oldPrev = Prev;
		Prev = GUI.Button(new Rect(-2f, 176.917f, 68f, 25.083f), new GUIContent(@"Prev",""));
		if (Prev != oldPrev && Prev ) {Action("Prev");onButtonClick(); }
		onMouseOver = new Rect(-2f, 176.917f, 68f, 25.083f).Contains(Event.current.mousePosition);
		if (oldMouseOverPrev != onMouseOver && onMouseOver) onOver();
		oldMouseOverPrev = onMouseOver;
		}
		if(vNext){
		if(focusNext) { focusNext = false; GUI.FocusControl("Next");}
		GUI.SetNextControlName("Next");
		bool oldNext = Next;
		Next = GUI.Button(new Rect(125f, 176.917f, 68f, 25.083f), new GUIContent(@"Next",""));
		if (Next != oldNext && Next ) {Action("Next");onButtonClick(); }
		onMouseOver = new Rect(125f, 176.917f, 68f, 25.083f).Contains(Event.current.mousePosition);
		if (oldMouseOverNext != onMouseOver && onMouseOver) onOver();
		oldMouseOverNext = onMouseOver;
		}
		GUI.EndGroup();
		if(vBallImage){
		if(focusBallImage) { focusBallImage = false; GUI.FocusControl("BallImage");}
		GUI.SetNextControlName("BallImage");
		if(rBallImage){
		GUI.Label(new Rect(138f, 252f, 90f, 17f), BallImage.ToString());
		} else
		try {BallImage = GUI.TextField(new Rect(138f, 252f, 90f, 17f), BallImage,100);}catch{};
		}
		GUI.Label(new Rect(16f, 255f, 111.37f, 21.96f), @"Custom Image URL");
		if(vMaterialName){
		if(focusMaterialName) { focusMaterialName = false; GUI.FocusControl("MaterialName");}
		GUI.SetNextControlName("MaterialName");
		if(rMaterialName){
		GUI.Label(new Rect(80f, 28f, 82f, 21.96f), MaterialName.ToString());
		} else
		try {MaterialName = GUI.TextField(new Rect(80f, 28f, 82f, 21.96f), MaterialName,100);}catch{};
		}
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(8f, 357f, 522f, 146f), "");
		GUI.Box(new Rect(0, 0, 522f, 146f), "");
		if(vUserScores){
		if(focusUserScores) { focusUserScores = false; GUI.FocusControl("UserScores");}
		GUI.SetNextControlName("UserScores");
		GUI.Box(new Rect(43.158f, 27f, 447.331f, 105f), "");
		sUserScores = GUI.BeginScrollView(new Rect(43.158f, 27f, 447.331f, 105f), sUserScores, new Rect(0,0, 437.331f, lUserScores.Length* 15f));
		int oldUserScores = iUserScores;
		iUserScores = GUI.SelectionGrid(new Rect(0,0, 437.331f, lUserScores.Length* 15f), iUserScores, lUserScores,1,GUI.skin.customStyles[0]);
		if (iUserScores != oldUserScores) Action("UserScores");
		GUI.EndScrollView();
		}
		if(vtableheader){
		if(focusTableheader) { focusTableheader = false; GUI.FocusControl("Tableheader");}
		GUI.SetNextControlName("Tableheader");
		if(rTableheader){
		GUI.Label(new Rect(41.646f, 3f, 448.843f, 20f), Tableheader.ToString(), GUI.skin.customStyles[2]);
		} else
		try {Tableheader = GUI.TextField(new Rect(41.646f, 3f, 448.843f, 20f), Tableheader,100, GUI.skin.customStyles[2]);}catch{};
		}
		GUI.Box(new Rect(22f, 23f, 470f, 1f),"",GUI.skin.customStyles[4]);//line
		GUI.EndGroup();
		if(vRefreshUserInfo){
		if(focusRefreshUserInfo) { focusRefreshUserInfo = false; GUI.FocusControl("RefreshUserInfo");}
		GUI.SetNextControlName("RefreshUserInfo");
		bool oldRefreshUserInfo = RefreshUserInfo;
		RefreshUserInfo = GUI.Button(new Rect(467f, 505f, 96f, 25f), new GUIContent(@"Refresh",""));
		if (RefreshUserInfo != oldRefreshUserInfo && RefreshUserInfo ) {Action("RefreshUserInfo");onButtonClick(); }
		onMouseOver = new Rect(467f, 505f, 96f, 25f).Contains(Event.current.mousePosition);
		if (oldMouseOverRefreshUserInfo != onMouseOver && onMouseOver) onOver();
		oldMouseOverRefreshUserInfo = onMouseOver;
		}
		GUI.BeginGroup(new Rect(8f, 8f, 98f, 82f), "");
		GUI.Box(new Rect(0, 0, 98f, 82f), "");
		if(vAvatar){
		if(focusAvatar) { focusAvatar = false; GUI.FocusControl("Avatar");}
		GUI.SetNextControlName("Avatar");
		if(imgAvatar!=null)
			GUI.DrawTexture(Avatar,imgAvatar, ScaleMode.ScaleToFit, imgAvatar is RenderTexture?false:true);
		}
		GUI.EndGroup();
		if (GUI.Button(new Rect(676f - 25, 5, 20, 15), "X")) { enabled = false;onButtonClick();Action("Close"); }
	}


	void Update () {
	
	}
}