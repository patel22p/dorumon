var castlePrefab : Transform;
var playerPrefab : Transform;

private var invisiblePlayer : Object;

// Server variables
var maxPlayers = 1;
private var playerCount : int = 0;

// The playerList is used by both server and client, the client sets his offical server approved info here.
var playerList : Array = new Array();

// Local player credentials (his profile)
public var playerName : String = "Testing";
public var playerColor : Color = Color.yellow;

private var isInitialized : boolean = false;
private var gameStarted : boolean = false;

private var playerWindow = Rect (10,60,110,50);

private var serverWindow = Rect (130,60,110,50);

private var colorSelection : String[] = ["Green", "Yellow", "Black", "Grey"];
private var selectedColor : int;
private var playerNumbers : String[] = ["2", "3", "4"];

class CastlePlayer {
	var number : int;
	var color : Color;
	var name : String;
	var player : NetworkPlayer;
	
	function ToString() : String {
		return name + " " + number + " " + color + " " + player;
	}
}

function OnGUI() {
	if (isInitialized) {
		GUI.Label(new Rect(Screen.width-150, 20, 140, 50), "Player name: "+ playerList[0].name + "\nPlayer number: " + playerList[0].number);
	}
	else {
		playerWindow = GUILayout.Window(1, playerWindow, MakePlayerWindow, "Player Info");
		serverWindow = GUILayout.Window(2, serverWindow, MakeServerWindow, "Server Info");
	}
	if (isInitialized && !gameStarted) {
		GUI.contentColor = Color.blue;
		GUI.Label(new Rect(20, Screen.height-60, 250, 20), "Ghost mode, waiting for more players");
	}
}

function MakePlayerWindow(id : int) {
	GUILayout.Label("Name:");
	playerName = GUILayout.TextField(playerName);
	GUILayout.Label("Color:");
	selectedColor = GUILayout.SelectionGrid(selectedColor, colorSelection, 1);
	switch (selectedColor) {
		case 0: playerColor = Color.green; break;
		case 1: playerColor = Color.yellow; break;
		case 2: playerColor = Color.black; break;
		case 3: playerColor = Color.grey; break;
		default: playerColor = Color.blue; break;
	}
}

function MakeServerWindow(id : int) {
	GUILayout.Label("Max Players:");
	var tmp = GUILayout.SelectionGrid(maxPlayers-2, playerNumbers, 3);
	maxPlayers = tmp + 2;
}
function OnNetworkLoadedLevel ()
{
	// Start invisible floating body camera
	invisiblePlayer = Instantiate(playerPrefab, new Vector3(0,2,0), Quaternion.identity);
	Camera.main.SendMessage("SetTarget", invisiblePlayer);
	
	if (Network.isServer && !isInitialized) {
		// Doing server/client now
		Network.maxConnections = maxPlayers-1;
		var playerInfo = new CastlePlayer();
		playerInfo.number = playerCount++;
		playerInfo.name = playerName;
		playerInfo.color = playerColor;
		// Add self to list
		playerList.Add(playerInfo);
		isInitialized = true;
	}
	
	// If I'm a client and I'm already connected to a server, then re-request player entry to game
	if (Network.isClient && Network.connections.Length > 0) {
		OnConnectedToServer();
	}
}

@RPC
function ReceivePlayer (number: int, r: float, g: float, b: float, name: String, player: NetworkPlayer) {
	var somePlayer : CastlePlayer = new CastlePlayer();
	somePlayer.number = number;
	somePlayer.color = new Color(r,g,b);
	somePlayer.name = name;
	somePlayer.player = player;
	playerList.Add(somePlayer);
	Debug.Log("Received info on player: " + somePlayer);
}

// Server uses this to tell the client his network player is approved and ready to join
@RPC
function InitializeClient (player: NetworkPlayer, number: int, r: float, g: float, b: float) {
	Debug.Log("Client initialized with number "+number);
	
	var localPlayer : CastlePlayer = new CastlePlayer();
	localPlayer.player = player;
	localPlayer.name = playerName;
	localPlayer.number = number;
	localPlayer.color = new Color(r,g,b);
	playerList.Add(localPlayer);
	isInitialized = true;
}

// Client uses this to announce himself and his desired credentials (like color) to the server
@RPC
function RequestPlayer (name: String, r: float, g: float, b: float, info: NetworkMessageInfo) {
	var newColor = new Color(r,g,b);
	// Check if color is already used
	var available : boolean = true;
	for (var p : CastlePlayer in playerList) {
		if (newColor == p.color)
			available = false;
	}
	// Create new player in list
	var playerInfo = new CastlePlayer();
	playerInfo.player = info.sender;

	// Initilize the rest of the variables
	playerInfo.number = playerCount++;
	if (available)
		playerInfo.color = newColor;
	else {
		Debug.Log("Requested color already in use, giving player default color");
		playerInfo.color = Color.green;
	}
	playerInfo.name = name;

	playerList.Add(playerInfo);

	// Send init message to client, confirming he is in the game and his parameters are initialized
	networkView.RPC("InitializeClient", info.sender, info.sender, playerInfo.number, playerInfo.color.r, playerInfo.color.g, playerInfo.color.b);
	
	// If all players have arrived, start the game
	if (Network.isServer && playerCount == maxPlayers)
	{
		// Send player list to clients
		for (var receiver : CastlePlayer in playerList) {
			for (var playerInfo : CastlePlayer in playerList) {
				if (receiver.player != playerInfo.player && receiver.player.ToString() != "0") {
					Debug.Log("Sending info on player " + playerInfo.player + " to player " + receiver.player);
					networkView.RPC("ReceivePlayer", receiver.player, playerInfo.number, playerInfo.color.r, playerInfo.color.g, playerInfo.color.b, playerInfo.name, playerInfo.player);
				}
			}
		}
		Debug.Log ("All " + playerCount + " player(s) connected, starting game.");
		networkView.RPC("OnGameReady", RPCMode.All);
	}
}

// Run everywhere when all players have connected, also runs on server as he is also a client
@RPC
function OnGameReady ()
{
	Debug.Log ("Starting game with player number " + playerList[0].number);

	var viewID : NetworkViewID = Network.AllocateViewID();
	switch (playerList[0].number)
	{
		case 0:
			networkView.RPC("SpawnCastle", RPCMode.AllBuffered, viewID, new Vector3(-40,1,-40), playerList[0].number);
			Camera.main.transform.position = new Vector3(-45,5,-45);
			break;
		case 1:
			networkView.RPC("SpawnCastle", RPCMode.AllBuffered, viewID, new Vector3(-40,1, 40), playerList[0].number);
			Camera.main.transform.position = new Vector3(-45,5,45);
			break;
		case 2: 
			networkView.RPC("SpawnCastle", RPCMode.AllBuffered, viewID, new Vector3(40,1, -40), playerList[0].number);
			Camera.main.transform.position = new Vector3(45,5,-45);
			break;
		case 3: 
			networkView.RPC("SpawnCastle", RPCMode.AllBuffered, viewID, new Vector3( 40,1, 40), playerList[0].number);
			Camera.main.transform.position = new Vector3(45,5,45);
			break;
		default: 
			Debug.Log ("Invalid player number given during game start: " + playerList[0].number);
	}
	gameStarted = true;
	
	// Destroy invisible player
	Destroy(invisiblePlayer.gameObject);
}

function GetColor(playerNumber: int) : Color {
	for (var p : CastlePlayer in playerList) {
		if (p.number == playerNumber)
			return p.color;
	}
	Debug.Log("Couldn't find color for player " + playerNumber);
	return Color.red;
}

@RPC
function SpawnCastle(viewID: NetworkViewID, position: Vector3, number: int) {
	var castle: Object = Instantiate(castlePrefab, position, transform.rotation);

	castle.GetComponent(NetworkView).viewID = viewID;

	if (!castle.GetComponent(NetworkView).isMine) {
		castle.name += "Remote";
		Debug.Log("Looking up color for player " +number);
		castle.renderer.material.color = GetColor(number);
	}
	else {
		castle.renderer.material.color = playerList[0].color;
		Debug.Log("Instantiated own castle");
	}
}

// When connected to server, request to get local player initialized in the game
function OnConnectedToServer() {
	networkView.RPC("RequestPlayer", RPCMode.Server, playerName, playerColor.r, playerColor.g, playerColor.b);
}

// Process each player when he connects to the on the server. Add to player list.
function OnPlayerConnected (player : NetworkPlayer)
{
	Debug.Log ("Server: player " + player.ToString() + " connected");
	if (gameStarted) {
		Debug.Log("Rejecting new player as the game is already started");
		Network.CloseConnection(player, true);
	}
}

function OnPlayerDisconnected (player : NetworkPlayer)
{
	Network.RemoveRPCs(player, 0);
	Network.DestroyPlayerObjects(player);
}
