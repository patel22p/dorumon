//DontDestroyOnLoad(this);
var remoteIP = "127.0.0.1";
var remotePort = 25001;
var listenPort = 25000;
var useNAT = false;

function Awake() 
{
	if (FindObjectOfType(ConnectGuiMasterServer))
		this.enabled = false;
}

function OnGUI ()
{
	GUILayout.Space(10);

	GUILayout.BeginHorizontal();
	GUILayout.Space(10);
	if (Network.peerType == NetworkPeerType.Disconnected)
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button ("Connect"))
		{
			Network.useNat = useNAT;
			Network.Connect(remoteIP, remotePort);
		}
		if (GUILayout.Button ("Start Server"))
		{
			Network.useNat = useNAT;
			Network.InitializeServer(32, listenPort);
			// Notify our objects that the level and the network is ready
			for (var go in FindObjectsOfType(GameObject))
				go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);		
		}
		GUILayout.EndVertical();
		remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));
		remotePort = parseInt(GUILayout.TextField(remotePort.ToString()));
	}
	else
	{
		if (GUILayout.Button ("Disconnect"))
		{
			Network.Disconnect(200);
		}
	}
	
	GUILayout.FlexibleSpace();
	GUILayout.EndHorizontal();
}

function OnConnectedToServer() {
	// Notify our objects that the level and the network is ready
	for (var go in FindObjectsOfType(GameObject))
		go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);		
}

function OnDisconnectedFromServer () {
	if (this.enabled != false)
		Application.LoadLevel(Application.loadedLevel);
	else
		FindObjectOfType(NetworkLevelLoad).OnDisconnectedFromServer();
}