// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonNetwork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.IO;

using ExitGames.Client.Photon;
using UnityEngine;

/// <summary>
/// The main class to use the PhotonNetwork plugin.
/// This class is static.
/// </summary>
public static class PhotonNetwork
{
    /// <summary>
    /// This Monobehaviour allows Photon to run an Update loop.
    /// </summary>
    public static readonly PhotonHandler photonMono;

    /// <summary>
    /// Our photon class
    /// </summary>
    public static readonly NetworkingPeer networkingPeer;

    /// <summary>
    /// The maximum amount of assigned networkviews PER player (or scene). See the documentation on how to raise this limitation
    /// </summary>
    public static readonly int MAX_VIEW_IDS = 1000; // VIEW & PLAYER LIMIT CAN BE EASILY CHANGED, SEE DOCS

    public static ServerSettings PhotonServerSettings = (ServerSettings)Resources.Load(Path.GetFileNameWithoutExtension(NetworkingPeer.serverSettingsAssetPath), typeof(ServerSettings));


    // "VARIABLES"

    /// <summary>
    /// Are we connected to the photon server (can be IN or OUTSIDE a room)
    /// </summary>
    public static bool connected
    {
        get
        {
            if (offlineMode)
            {
                return true;
            }

            return connectionState == ConnectionState.Connected;
        }
    }

    /// <summary>
    /// Simplified connection state
    /// </summary>
    public static ConnectionState connectionState
    {
        get
        {
            if (offlineMode)
            {
                return ConnectionState.Connected;
            }

            if (networkingPeer == null)
            {
                return ConnectionState.Disconnected;
            }

            switch (networkingPeer.PeerState)
            {
                case ExitGames.Client.Photon.PeerStateValue.Disconnected:
                    return ConnectionState.Disconnected;
                case ExitGames.Client.Photon.PeerStateValue.Connecting:
                    return ConnectionState.Connecting;
                case ExitGames.Client.Photon.PeerStateValue.Connected:
                    return ConnectionState.Connected;
                case ExitGames.Client.Photon.PeerStateValue.Disconnecting:
                    return ConnectionState.Disconnecting;
                case ExitGames.Client.Photon.PeerStateValue.InitializingApplication:
                    return ConnectionState.InitializingApplication;
            }

            return ConnectionState.Disconnected;
        }
    }

    /// <summary>
    /// Detailed connection state
    /// </summary>
    public static PeerState connectionStateDetailed
    {
        get
        {
            if (offlineMode)
            {
                return PeerState.Connected;
            }

            if (networkingPeer == null)
            {
                return PeerState.Disconnected;
            }

            return networkingPeer.State;
        }
    }

    /// <summary>
    /// Get the room we're currently in. null if we aren't in any room
    /// </summary>
    public static Room room
    {
        get
        {
            if (isOfflineMode)
            {
                if (offlineMode_inRoom)
                {
                    return new Room("OfflineRoom", new Hashtable());
                }
                else
                {
                    return null;
                }
            }

            return networkingPeer.mCurrentGame;
        }
    }

    /// <summary>
    /// Network log level.
    /// </summary>
    public static NetworkLogLevel logLevel = NetworkLogLevel.ErrorsOnly;

    /// <summary>
    /// The local PhotonPlayer. Also available when not connected
    /// </summary>
    public static PhotonPlayer player
    {
        get
        {
            if (networkingPeer == null)
            {
                return null; // Surpress ExitApplication errors
            }

            return networkingPeer.mLocalActor;
        }
    }

    /// <summary>
    /// The PhotonPlayer of the master client. The master client is the 'virtual owner' of the room. You can use it if you need authorative decision made by one of the players.
    /// </summary>
    /// <remarks>
    /// The masterClient is null until a room is joined and becomes null again when the room is left.
    /// </remarks>
    public static PhotonPlayer masterClient
    {
        get
        {
            if (networkingPeer == null)
            {
                return null;
            }

            return networkingPeer.mMasterClient;
        }
    }

    /// <summary>
    /// Our local player name
    /// </summary>
    /// <remarks>Setting the name will automatically send it, if connected. Setting null, won't change the name.</remarks>
    public static string playerName
    {
        get
        {
            return networkingPeer.PlayerName;
        }

        set
        {
            networkingPeer.PlayerName = value;
        }
    }

    /// <summary>
    /// The full PhotonPlayer list, including the local player.
    /// </summary>
    public static List<PhotonPlayer> playerList
    {
        get
        {
            if (networkingPeer == null)
            {
                return new List<PhotonPlayer>();
            }

            return networkingPeer.mPlayerList;
        }
    }

    /// <summary>
    /// The other PhotonPlayers, not including our local playe.rov
    /// </summary>
    public static List<PhotonPlayer> otherPlayers
    {
        get
        {
            return networkingPeer.mOtherPlayerList;
        }
    }

    /// <summary>
    /// Offline mode can be set to re-use your multiplayer code in singleplayer game modes. 
    /// When this is on PhotonNetwork will not create any connections and there is near to 
    /// no overhead. Mostly usefull for reusing RPC's and PhotonNetwork.Instantiate
    /// </summary>
    public static bool offlineMode
    {
        get
        {
            return isOfflineMode;
        }

        set
        {
            if (value == isOfflineMode)
            {
                return;
            }

            if (value && connected)
            {
                Debug.LogError("Can't start OFFLINE mode while connected!");
            }
            else
            {
                networkingPeer.Disconnect(); // Cleanup (also calls OnLeftRoom to reset stuff)
                isOfflineMode = value;
                if (isOfflineMode)
                {
                    NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
                    networkingPeer.ChangeLocalID(1);
                    networkingPeer.mMasterClient = player;
                }
            }
        }
    }

    private static bool isOfflineMode = false;

    private static bool offlineMode_inRoom = false;

    /// <summary>
    /// The maximum number of players for a room. Better: Set it in CreateRoom.
    /// If no room is opened, this will return 0.
    /// </summary>
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public static byte maxConnections
    {
        get
        {
            if (room == null)
            {
                return 0;
            }

            return room.maxPlayers;
        }

        set
        {
            room.maxPlayers = value;
        }
    }

    /// <summary>
    /// Enabled per default. When enabled: if a player disconnects, all gameobjects/photonviews from this player will be destroyed automatically.
    /// </summary>
    public static bool autoCleanUpPlayerObjects = true;

    /// <summary>
    /// Defines how many times per second PhotonNetwork should send a package. If you change
    /// this, do not forget to also change 'sendRateOnSerialize'.
    /// </summary>
    /// <remarks>
    /// Less packages are less overhead but more delay.
    /// Setting the sendRate to 50 will create up to 50 packages per second (which is a lot!).
    /// Keep your target platform in mind: mobile networks are slower and less reliable.
    /// </remarks>
    public static int sendRate
    {
        get
        {
            return 1000 / sendInterval;
        }

        set
        {
            sendInterval = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateInterval = sendInterval;
            }

            if (value < sendRateOnSerialize)
            {
                // sendRateOnSerialize needs to be <= sendRate
                sendRateOnSerialize = value;
            }
        }
    }

    /// <summary>
    /// Defines how many times per second OnPhotonSerialize should be called on PhotonViews.
    /// </summary>
    /// <remarks>
    /// Choose this value in relation to 'sendRate'. OnPhotonSerialize will creart the commands to be put into packages.
    /// A lower rate takes up less performance but will cause more lag.
    /// </remarks>
    public static int sendRateOnSerialize
    {
        get
        {
            return 1000 / sendIntervalOnSerialize;
        }

        set
        {
            if (value > sendRate)
            {
                Debug.LogError("Error, can not set the OnSerialize SendRate more often then the overall SendRate");
                value = sendRate;
            }

            sendIntervalOnSerialize = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateIntervalOnSerialize = sendIntervalOnSerialize;
            }
        }
    }

    private static int sendInterval = 50;

    private static int sendIntervalOnSerialize = 100;

    /// <summary>
    /// Can be used to pause dispatch of incoming evtents (RPCs, Instantiates and anything else incoming).
    /// This can be useful if you first want to load a level, then go on receiving data of PhotonViews and RPCs.
    /// The client will go on receiving and sending acknowledgements for incoming packages and your RPCs/Events.
    /// This adds "lag" and can cause issues when the pause is longer, as all incoming messages are just queued.
    /// </summary>
    public static bool isMessageQueueRunning
    {
        get
        {
            return m_isMessageQueueRunning;
        }

        set
        {
            if (value == m_isMessageQueueRunning) return;
            PhotonNetwork.networkingPeer.IsSendingOnlyAcks = !value;
            m_isMessageQueueRunning = value;
            if (!value)
            {
                PhotonHandler.StartThread(); // Background loading thread: keeps connection alive
            }
        }
    }

    private static bool m_isMessageQueueRunning = true;

    /// <summary>
    /// Used once per dispatch to limit unreliable commands per channel (so after a pause, many channels can still cause a lot of unreliable commands)
    /// </summary>
    public static int unreliableCommandsLimit = 20;

    /// <summary>
    /// Photon network time, synched with the server
    /// </summary>
    /// <remarks>
    /// v1.3:
    /// This time reflects milliseconds since start of the server, cut down to 4 bytes.
    /// It will overflow every 49 days from a high value to 0. We do not (yet) compensate this overflow.
    /// Master- and Game-Server will have different time values.
    /// </remarks>
    public static double time
    {
        get
        {
            if (offlineMode)
            {
                return Time.time;
            }
            else
            {
                return (uint)networkingPeer.ServerTimeInMilliSeconds / 1000.0f;
            }
        }
    }

    /// <summary>
    /// Are we the master client?
    /// </summary>
    public static bool isMasterClient
    {
        get
        {
            if (offlineMode)
            {
                return true;
            }
            else
            {
                return networkingPeer.mMasterClient == networkingPeer.mLocalActor;
            }
        }
    }

    /// <summary>
    /// True if we are in a game (client) and NOT the game's masterclient
    /// </summary>
    public static bool isNonMasterClientInGame
    {
        get
        {
            return !isMasterClient && room != null;
        }
    }

    static PhotonNetwork()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying || !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        // This can happen when you recompile a script IN play made
        // This helps to surpress some errors, but will not fix breaking
        bool doubleInstall = false;
        GameObject pGO = GameObject.Find("PhotonMono");
        doubleInstall = pGO != null;
        if (doubleInstall)
        {
            GameObject.Destroy(pGO);
            Debug.LogWarning("The Unity recompile forced a restart of UnityPhoton!");
        }

#endif
        Application.runInBackground = true;

        // Set up a MonoBheaviour to run Photon, and hide it.        
        GameObject photonGO = new GameObject();
        photonMono = (PhotonHandler)photonGO.AddComponent<PhotonHandler>();
        photonGO.name = "PhotonMono";
        photonGO.hideFlags = UnityEngine.HideFlags.HideInHierarchy;

        // Set up the NetworkingPeer.
        // TODO: Add parameter for custom(ers) client version (being v1.0 now)
        networkingPeer = new NetworkingPeer(photonMono, "Guest " + UnityEngine.Random.Range(1, 9999), "v1.0", ExitGames.Client.Photon.ConnectionProtocol.Udp);
        networkingPeer.LimitOfUnreliableCommands = 20;

        // Local player
        CustomTypes.Register();
    }

    // FUNCTIONS

    /// <summary>
    /// Connect to the configured Photon server:
    /// Reads NetworkingPeer.serverSettingsAssetPath and connects to cloud or your own server.
    /// Uses: Connect(string serverAddress, int port, string uniqueGameID)
    /// </summary>
    public static void ConnectUsingSettings()
    {
        if (PhotonServerSettings == null)
        {
            Debug.LogError("Loading the settings file failed. Check path: " + NetworkingPeer.serverSettingsAssetPath);
            return;
        }

        Connect(PhotonServerSettings.ServerAddress, PhotonServerSettings.ServerPort, PhotonServerSettings.AppID);
    }

    /// <summary>
    /// Connect to the photon server
    /// </summary>
    /// <param name="serverAddress"></param>
    /// <param name="port"></param>
    /// <param name="uniqueGameID"></param>
    public static void Connect(string serverAddress, int port, string uniqueGameID)
    {
        if (port <= 0)
        {
            Debug.LogError("Aborted Connect: invalid port: " + port);
            return;
        }

        if (serverAddress.Length <= 2)
        {
            Debug.LogError("Aborted Connect: invalid serverAddress: " + serverAddress);
            return;
        }

        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("Connect() only works when disconnected. Current state: " + networkingPeer.PeerState);
            return;
        }

        if (offlineMode)
        {
            offlineMode = false; // Cleanup offline mode
            Debug.LogWarning("Shut down offline mode due to a connect attempt");
        }

        if (!isMessageQueueRunning)
        {
            isMessageQueueRunning = true;
            Debug.LogWarning("Forced enabling of isMessageQueueRunning because of a Connect()");
        }

        serverAddress = serverAddress + ":" + port;

        //Debug.Log("Connecting to: " + serverAddress + " app: " + uniqueGameID);
        networkingPeer.Connect(serverAddress, uniqueGameID, 0);
    }

    /// <summary>
    /// Disconnect from the photon server (also quits your multiplayer game, if any)
    /// </summary>
    public static void Disconnect()
    {
        if (networkingPeer == null)
        {
            return; // Surpress error when quitting game
        }

        networkingPeer.Disconnect();
    }

    /// <summary>
    /// Set up security protocols. See documentation for details
    /// </summary>
    public static void InitializeSecurity()
    {
        if (offlineMode)
        {
            return;
        }

        if (!connected)
        {
            Debug.LogError("Cannot InitializeSecurity when not connected!");
            return;
        }

        networkingPeer.EstablishEncryption();
    }

    /// <summary>
    /// Create a room with given title. This will fail if the room title is already in use.
    /// </summary>
    /// <param name="roomName">Unique name of the room to create.</param>
    public static void CreateRoom(string roomName)
    {
        if (room != null)
        {
            Debug.LogError("CreateRoom aborted: You are already in a room!");
        }
        else if (roomName == string.Empty)
        {
            Debug.LogError("CreateRoom aborted: You must specifiy a room name!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
            }
            else
            {
                networkingPeer.OpCreateGame(roomName, true, true, 0, null);
            }
        }
    }

    /// <summary>
    /// Creates a room with given name but fails if this room is existing already.
    /// </summary>
    /// <remarks>
    /// If you don't want to create a unique room-name, the master server will create one if roomName is null.
    /// This only works on the master server. The CreateRoom response will contain the room name to use on the
    /// game server. 
    /// The game server will respond with an error if roomName is null or empty.
    /// </remarks>
    /// <param name="roomName">Unique name of the room to create.</param>
    /// <param name="isVisible">Shows (or hides) this room from the lobby's listing of rooms.</param>
    /// <param name="isOpen">Allows (or disallows) others to join this room.</param>
    /// <param name="maxPlayers">Max number of players that can join the room.</param>
    public static void CreateRoom(string roomName, bool isVisible, bool isOpen, byte maxPlayers)
    {
        if (room != null)
        {
            Debug.LogError("CreateRoom aborted: You are already in a room!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
            }
            else
            {
                networkingPeer.OpCreateGame(roomName, isVisible, isOpen, (byte)maxPlayers, null);
            }
        }
    }

    /// <summary>
    /// Join room with given title. If no such room exists the room will be created.
    /// </summary>
    /// <param name="roomName">Unique name of the room to create.</param>
    public static void JoinRoom(string roomName)
    {
        if (room != null)
        {
            Debug.LogError("JoinRoom aborted: You are already in a room!");
        }
        else if (roomName == string.Empty)
        {
            Debug.LogError("JoinRoom aborted: You must specifiy a room name!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
            }
            else
            {
                networkingPeer.OpJoin(roomName);
            }
        }
    }

    /// <summary>
    /// Joins any available room but will fail if none is currently available.
    /// </summary>
    /// <remarks>
    /// If this fails, you can still create a room (and make this available for the next who uses JoinRandomRoom.
    /// </remarks>
    public static void JoinRandomRoom()
    {
        if (room != null)
        {
            Debug.LogError("JoinRandomRoom aborted: You are already in a room!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
            }
            else
            {
                networkingPeer.OpJoinRandom(null);
            }
        }
    }

    /// <summary>
    /// Leave the current game room
    /// </summary>
    public static void LeaveRoom()
    {
        if (room == null)
        {
            UnityEngine.Debug.LogError("PhotonNetwork: Error, you cannot leave a room if you're not in a room!");
            return;
        }

        if (offlineMode)
        {
            offlineMode_inRoom = false;
            NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom);
        }
        else
        {
            networkingPeer.OpLeave();
        }
    }

    /// <summary>
    /// Get the game list. This list is automatically updated in near-real time. The list is only updated when connected to photon, and when not inside any room.
    /// </summary>
    /// <remarks>Will copy mGameList to new array.</remarks>
    /// <returns>An array of rooms.</returns>
    public static Room[] GetRoomList()
    {
        if (offlineMode)
        {
            return new Room[0];
        }

        if (networkingPeer == null)
        {
            return new Room[0]; // Surpress erorrs when quitting game
        }

        Room[] array = new Room[networkingPeer.mGameList.Count];
        networkingPeer.mGameList.Values.CopyTo(array, 0);
        return array;
    }


    /// <summary>
    /// Request a new viewID for the local player
    /// </summary>
    /// <returns></returns>
    public static PhotonViewID AllocateViewID()
    {
        // int playerID = player.ID;
        int newID = 0;
        while (networkingPeer.allocatedIDs.ContainsKey(newID))
        {
            newID++;
        }

        if (newID > MAX_VIEW_IDS)
        {
            Debug.LogError("ERROR: Too many view IDs used!");
            newID = 0;
        }

        int ID = newID;
        PhotonViewID viewID = new PhotonViewID(ID, player);
        networkingPeer.allocatedIDs.Add(newID, viewID);
        return viewID;
    }

    /// <summary>
    /// Unregister a view ID
    /// </summary>
    /// <param name="viewID">PhotonViewID instance</param>
    public static void UnAllocateViewID(PhotonViewID viewID)
    {
        UnAllocateViewID(viewID.ID);
    }

    /// <summary>
    /// Unregister a view ID
    /// </summary>
    /// <param name="ID">ID of a PhotonView</param>
    public static void UnAllocateViewID(int ID)
    {
        networkingPeer.allocatedIDs.Remove(ID);
    }

    /// <summary>
    /// Instantiate a prefab over the network. This prefab needs to be located in the root of a "Resources" folder.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="group"></param>
    /// <returns>The new instance.</returns>
    public static GameObject Instantiate(Object prefab, Vector3 position, Quaternion rotation, int group)
    {
        return Instantiate(prefab, position, rotation, group, null);
    }

    /// <summary>
    /// Instantiate a prefab over the network. This prefab needs to be located in the root of a "Resources" folder.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="group"></param>
    /// <param name="data">optional instantiation data. This will be saved to it's PhotonView</param>
    /// <returns>The new instance.</returns>
    public static GameObject Instantiate(Object prefab, Vector3 position, Quaternion rotation, int group, object[] data)
    {
        if (!VerifyCanUseNetwork())
        {
            return null;
        }

        GameObject prefabGo = (GameObject)Resources.Load(prefab.name, typeof(GameObject));
        if (prefabGo == null)
        {
            Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + prefab.name + "]. Please verify you have this gameobject in a Resources folder (and not in a subfolder)");
            return null;
        }

        Component[] views = (Component[])prefabGo.GetComponentsInChildren<PhotonView>(true);
        PhotonViewID[] viewIDs = new PhotonViewID[views.Length];
        for (int i = 0; i < viewIDs.Length; i++)
        {
            viewIDs[i] = AllocateViewID();
        }

        // Send to others, create info
        Hashtable instantiateEvent = networkingPeer.SendInstantiate(prefab.name, position, rotation, group, viewIDs, data);

        // Instantiate the GO locally (but the same way as if it was done via event). This will also cache the instantiationId
        return networkingPeer.DoInstantiate(instantiateEvent, networkingPeer.mLocalActor, prefabGo);
    }

    /// <summary>
    /// The current roundtrip time to the photon server
    /// </summary>
    /// <returns>Roundtrip time (to server and back).</returns>
    public static int GetPing()
    {
        return networkingPeer.RoundTripTime;
    }

    /// <summary>
    /// Can be used to immediately send the RPCs and Instantiates just made, 
    /// so they are on their way to the other players.
    /// </summary>
    /// <remarks>
    /// This could be useful if you do a RPC to load a level and then load it yourself.
    /// While loading, no RPCs are sent to others, so this would delay the "load" RPC.
    /// You can send the RPC to "others", use this method, disable the message queue 
    /// (by isMessageQueueRunning) and then load.
    /// </remarks>
    public static void SendOutgoingCommands()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        while (networkingPeer.SendOutgoingCommands())
        {
        }
    }

    /// <summary>
    /// Request a client to disconnect (KICK). Only the master client can do this.
    /// </summary>
    /// <param name="kickPlayer">The PhotonPlayer to kick.</param>
    public static void CloseConnection(PhotonPlayer kickPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (!player.isMasterClient)
        {
            Debug.LogError("CloseConnection: Only the masterclient can kick another player.");
        }

        if (kickPlayer == null)
        {
            Debug.LogError("CloseConnection: No such player connected!");
        }
        else
        {
            int[] rec = new int[1];
            rec[0] = kickPlayer.ID;
            networkingPeer.OpRaiseEvent(PhotonNetworkMessages.CloseConnection, null, true, 0, rec);
        }
    }

    /// <summary>
    /// Destroy a PhotonView with given ID. This will remove all Buffered RPCs and destroy the GameObject this view is attached to.
    /// </summary>
    /// <param name="viewID"></param>
    public static void Destroy(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        Destroy(view);
    }

    /// <summary>
    /// Destroy given PhotonView with given ID. This will remove all Buffered RPCs and destroy the GameObject this view is attached to.
    /// </summary>
    /// <param name="view"></param>
    public static void Destroy(PhotonView view)
    {
        if (view != null && view.owner != null)
        {
            networkingPeer.DestroyPhotonView(view);
        }
        else
        {
            Debug.LogError("Destroy: Could not destroy view ID [" + view + "]. Does not exist, or is not ours!");
        }
    }

    /// <summary>
    /// Destroys given GameObject. Requires it to have photonview(s) attached.
    /// </summary>
    /// <param name="go"></param>
    public static void Destroy(GameObject go)
    {
        networkingPeer.RemoveInstantiatedGO(go);
    }


    /// <summary>
    /// Destroy all GameObjects/PhotonViews of this player.
    /// </summary>
    /// <param name="player"></param>
    public static void DestroyPlayerObjects(PhotonPlayer player)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.DestroyPlayerObjects(player);
    }

    /// <summary>
    /// Destroy ALL instantiated GameObjects
    /// </summary>
    public static void RemoveAllInstantiatedObjects()
    {
        networkingPeer.RemoveAllInstantiatedObjects();
    }

    /// <summary>
    /// Destroy ALL instantiated GameObjects by given player
    /// </summary>
    /// <param name="player"></param>
    public static void RemoveAllInstantiatedObjects(PhotonPlayer player)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveAllInstantiatedObjectsByPlayer(player);
    }

    /// <summary>
    /// Send an RPC on given PhotonView. Do not call this directly; You should use PhotonView.RPC instead!
    /// </summary>
    /// <param name="view"></param>
    /// <param name="methodName"></param>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    internal static void RPC(PhotonView view, string methodName, PhotonTargets target, params object[] parameters)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (room == null)
        {
            Debug.LogWarning("Cannot send RPCs in Lobby! RPC dropped.");
            return;
        }

        if (networkingPeer != null)
        {
            networkingPeer.RPC(view, methodName, target, parameters);
        }
        else
        {
            Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
        }
    }

    /// <summary>
    /// Send an RPC on given PhotonView. Do not call this directly; You should use PhotonView.RPC instead!
    /// </summary>
    /// <param name="view"></param>
    /// <param name="methodName"></param>
    /// <param name="targetPlayer"></param>
    /// <param name="parameters"></param>
    internal static void RPC(PhotonView view, string methodName, PhotonPlayer targetPlayer, params object[] parameters)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (room == null)
        {
            Debug.LogWarning("Cannot send RPCs in Lobby, only processed locally");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Error; Sending RPC to player null! Aborted \"" + methodName + "\"");
        }

        if (networkingPeer != null)
        {
            networkingPeer.RPC(view, methodName, targetPlayer, parameters);
        }
        else
        {
            Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
        }
    }

    /// <summary>
    /// Remove ALL buffered RPCs of this player
    /// </summary>
    public static void RemoveRPCs()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveRPCsOfLocalPlayer();
    }

    /// <summary>
    /// Remove all buffered RPCs on given PhotonView (if they are owned by this player).
    /// </summary>
    /// <param name="view"></param>
    public static void RemoveRPCs(PhotonView view)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveRPCs(view);
    }

    /// <summary>
    /// Remove all buffered RPCs with given group
    /// </summary>
    /// <param name="group"></param>
    public static void RemoveRPCsInGroup(int group)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveRPCsInGroup(group);
    }

    /// <summary>
    /// Enable/disable receiving on given group (applied to PhotonViews)
    /// </summary>
    /// <param name="group"></param>
    /// <param name="enabled"></param>
    public static void SetReceivingEnabled(int group, bool enabled)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetReceivingEnabled(group, enabled);
    }

    /// <summary>
    /// Enable/disable sending on given group (applied to PhotonViews)
    /// </summary>
    /// <param name="group"></param>
    /// <param name="enabled"></param>
    public static void SetSendingEnabled(int group, bool enabled)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetSendingEnabled(group, enabled);
    }

    /// <summary>
    /// Adds a level prefix to all PhotonViews. If any other client uses a differnt prefix, their messages will be dropped.
    /// They will also drop your messages! Be aware that PUN never resets this value, you'll have to do so yourself.
    /// </summary>
    /// <param name="prefix"></param>
    public static void SetLevelPrefix(int prefix)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetLevelPrefix(prefix);
    }

    /// <summary>
    /// Helper function which is called inside this class to erify if certain functions can be used (e.g. RPC when not connected)
    /// </summary>
    /// <returns></returns>
    private static bool VerifyCanUseNetwork()
    {
        if (networkingPeer != null && (offlineMode || connected))
        {
            return true;
        }

        Debug.LogError("Cannot send messages when not connected; Either connect to Photon OR use offline mode!");
        return false;
    }
}