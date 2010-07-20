var skin : GUISkin;
var showChat = false;
private var inputField = "";
private var display = true;
private var entries = ArrayList();
private var scrollPosition : Vector2;

private var window = Rect(50, 50, 200, 300);

class ChatEntry
{
	var sender = "";
	var text = "";	
	var mine = true;
}

function CloseChatWindow ()
{
	showChat = false;
	inputField = "";
	entries = new ArrayList();
}

function FocusControl ()
{
	// We can't select it immediately because the control might not have been drawn yet.
	// Thus it is not known to the system!
	yield;
	yield;
	GUI.FocusControl("Chat input field");
}

function OnGUI ()
{
	GUI.skin = skin;
	
	//if (GUILayout.Button(showChat ? "Hide Chat" : "Display Chat"))
	if (GUI.Button(new Rect(Screen.width-100, Screen.height-30, 90, 20), showChat ? "Hide Chat" : "Display Chat"))
	{
		// Focus first element
		if (showChat)
		{	
			CloseChatWindow ();
		}
		else
		{
			showChat = true;
			FocusControl();
		}
	}
	
	if (showChat)
		window = GUI.Window (1, window, GlobalChatWindow, "Chat");
}

function GlobalChatWindow (id : int) {
	
	var closeButtonStyle = GUI.skin.GetStyle("close_button");
	if (GUI.Button(Rect (4, 4, closeButtonStyle.normal.background.width, closeButtonStyle.normal.background.height), "", "close_button"))
	{
		CloseChatWindow();
	}
	
	// Begin a scroll view. All rects are calculated automatically - 
    // it will use up any available screen space and make sure contents flow correctly.
    // This is kept small with the last two parameters to force scrollbars to appear.
	scrollPosition = GUILayout.BeginScrollView (scrollPosition);

	for (var entry : ChatEntry in entries)
	{
		GUILayout.BeginHorizontal();
		if (!entry.mine)
		{
			GUILayout.FlexibleSpace ();
			GUILayout.Label (entry.text, "chat_rightaligned");
		}
		else
		{
			GUILayout.Label (entry.text, "chat_leftaligned");
			GUILayout.FlexibleSpace ();
		}
		
		GUILayout.EndHorizontal();
		GUILayout.Space(3);
		
	}
	// End the scrollview we began above.
    GUILayout.EndScrollView ();
	
	if (Event.current.type == EventType.keyDown && Event.current.character == "\n" && inputField.Length > 0)
	{
		//@TODO: This should be dependent on who actually sent the message
		//var mine = entries.Count % 2 == 0;
		ApplyGlobalChatText(inputField, 1);
		networkView.RPC("ApplyGlobalChatText", RPCMode.Others, inputField, 0);
		inputField = "";
	}
	GUI.SetNextControlName("Chat input field");
	inputField = GUILayout.TextField(inputField);
	
	GUI.DragWindow();
}

@RPC
function ApplyGlobalChatText (str : String, mine : int)
{
	var entry = new ChatEntry();
	entry.sender = "Not implemented";
	entry.text = str;
	if (mine == 1) entry.mine = true;
	else entry.mine = false;

	entries.Add(entry);
	
	if (entries.Count > 50)
		entries.RemoveAt(0);
		
	scrollPosition.y = 1000000;	
}