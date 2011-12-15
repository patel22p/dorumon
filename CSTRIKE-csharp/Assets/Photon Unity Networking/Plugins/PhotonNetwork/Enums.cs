// ----------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting,
    InitializingApplication
}

public enum PeerState
{
    Uninitialized,
    PeerCreated,
    Connecting,
    Connected,
    Queued,
    Authenticated,
    JoinedLobby,
    DisconnectingFromMasterserver,
    ConnectingToGameserver,
    ConnectedToGameserver,
    Joining,
    Joined,
    Leave,
    Leaving,
    Left,
    DisconnectingFromGameserver,
    ConnectingToMasterserver,
    ConnectedComingFromGameserver,
    QueuedComingFromGameserver,
    Disconnect,
    Disconnecting,
    Disconnected
}

public enum JoinType
{
    CreateGame,
    JoinGame,
    JoinRandomGame
}

// Photon properties, internally set by PhotonNetwork (PhotonNetwork builtin properties)

/// <summary>
/// This enum makes up the set of MonoMessages sent by Photon Unity Networking.
/// Implement any of these constant names as method and it will be called
/// in the respective situation.
/// </summary>
/// <example>
/// Implement: 
/// public void OnLeftRoom() { //some work }
/// </example>
public enum PhotonNetworkingMessage
{
    OnConnectedToPhoton,
    OnLeftRoom,
    OnMasterClientSwitched,
    OnPhotonCreateGameFailed,
    OnPhotonJoinGameFailed,
    OnCreatedRoom,
    OnJoinedLobby,
    OnLeftLobby,
    OnDisconnectedFromPhoton,
    OnFailedToConnectToPhoton,
    OnReceivedRoomList,
    OnReceivedRoomListUpdate,
    OnJoinedRoom,
    OnPhotonPlayerConnected,
    OnPhotonPlayerDisconnected,
    OnPhotonRandomJoinFailed,
}