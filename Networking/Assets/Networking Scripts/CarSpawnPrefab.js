var playerPrefab : Transform;

function OnNetworkLoadedLevel ()
{
	// Randomize starting location
	var pos : Vector3;
	pos.x = 20*Random.value;
	pos.y = 4;
	pos.z = 20*Random.value;
	Network.Instantiate(playerPrefab, pos, transform.rotation, 0);
}

function OnPlayerDisconnected (player : NetworkPlayer)
{
	Debug.Log("Server destroying player");
	Network.RemoveRPCs(player, 0);
	Network.DestroyPlayerObjects(player);
}
