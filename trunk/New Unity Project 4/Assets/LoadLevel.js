var supportedNetworkLevels : String[];
var disconnectedLevel : String;
private var lastLevelPrefix = 0;

function Awake () 
{

    // Network level loading is done in a separate channel.
    DontDestroyOnLoad(this);
    networkView.group = 1;
    Application.LoadLevel(disconnectedLevel);
}

function OnGUI ()
{
	if (Network.peerType != NetworkPeerType.Disconnected)
	{
		GUILayout.BeginArea(Rect(0, Screen.height - 30, Screen.width, 30));
		GUILayout.BeginHorizontal();

		for (var level in supportedNetworkLevels)
		{
			if (GUILayout.Button(level))
			{
				Network.RemoveRPCsInGroup(0); 
				Network.RemoveRPCsInGroup(1);
				networkView.RPC( "LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}

@RPC
function LoadLevel (level : String, levelPrefix : int)
{
	lastLevelPrefix = levelPrefix;

		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);	

		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;

		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);
		yield;
		yield;

		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);

		for (var go in FindObjectsOfType(GameObject))
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
}

function OnDisconnectedFromServer ()
{
	Application.LoadLevel(disconnectedLevel);
}

@script RequireComponent(NetworkView)