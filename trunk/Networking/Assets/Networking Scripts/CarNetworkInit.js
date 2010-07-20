function OnNetworkInstantiate (msg : NetworkMessageInfo) {
	// This is our own player
	if (networkView.isMine)
	{
		Camera.main.SendMessage("SetTarget", transform);
		GetComponent("NetworkRigidbody").enabled = false;
	}
	// This is just some remote controlled player, don't execute direct
	// user input on this
	else
	{
		name += "Remote";
		GetComponent(Car).SetEnableUserInput(false);
		GetComponent("NetworkRigidbody").enabled = true;
	}
}