function Start() {
	MasterServer.dedicatedServer = true;
}

function OnGUI()
{	
	if (Network.isServer)
		GUI.Label(new Rect(20, Screen.height-50, 200, 20), "Running as a dedicated server");
}