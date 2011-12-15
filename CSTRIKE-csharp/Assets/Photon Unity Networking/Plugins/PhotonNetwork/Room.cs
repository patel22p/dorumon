// ----------------------------------------------------------------------------
// <copyright file="Room.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Represents a room/game on the server and caches the properties of that.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using System;
using System.Collections;
using ExitGames.Client.Photon;

/// <summary>
/// This class is used for room listings mostly. It is able to:
/// - cache "standard" properties (byte keys)
/// - cache custom properties (string keys)
/// </summary>
public class Room
{
    public String name;
    internal Hashtable properties = new Hashtable();

    private byte maxPlayersSetting = 0;
    internal bool IsLocalClientInside { get; set; } // keeps state if the local client is already in the game or still going to join it on gameserver

    /// <summary>
    /// Sets a limit of players to this room. This property is shown in lobby, too.
    /// If the room is full (players count == maxplayers), joining this room will fail.
    /// </summary>
    public byte maxPlayers
    {
        get
        {
            return this.maxPlayersSetting;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
            }

            if (value != this.maxPlayersSetting && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfGame(new Hashtable() { { GameProperties.MaxPlayers, value } }, true, (byte)0);
            }

            this.maxPlayersSetting = value;
        }
    }

    private bool isOpen = true;
    /// <summary>
    /// Defines if the room can be joined.
    /// This does not affect listing in a lobby but joining the room will fail if not open.
    /// If not open, the room is excluded from random matchmaking. 
    /// Due to racing conditions, found matches might become closed before they are joined. 
    /// Simply re-connect to master and find another.
    /// Use property "visible" to not list the room.
    /// </summary>
    public bool open
    {
        get
        {
            return this.isOpen;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
            }

            if (value != this.isOpen && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfGame(new Hashtable() { { GameProperties.IsOpen, value } }, true, (byte)0);
            }

            this.isOpen = value;
        }
    }

    private bool isVisible = true;
    /// <summary>
    /// Defines if the room is listed in its lobby.
    /// Rooms can be created invisible, or changed to invisible.
    /// To change if a room can be joined, use property: open.
    /// </summary>
    public bool visible
    {
        get
        {
            return this.isVisible;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
            }

            if (value != this.isVisible && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfGame(new Hashtable() { { GameProperties.IsVisible, value } }, true, (byte)0);
            }

            this.isVisible = value;
        }
    }

    internal bool mRemoved; // only used internally in lobby, to mark rooms that are no longer listed
    public int playerCount; // only used internally in lobby, to display number of players in room (while you're not in)


    internal Room(String roomName, Hashtable properties)
    {
        this.name = roomName;
        this.CacheProperties(properties);
    }

    internal Room(String roomName, Hashtable properties, bool isVisible, bool isOpen, byte maxPlayers)
    {
        this.name = roomName;
        this.CacheProperties(properties);
        this.isVisible = isVisible;
        this.isOpen = isOpen;
        this.maxPlayersSetting = maxPlayers;
    }

    public override string ToString()
    {
        return string.Format("Room: '{0}' visible: {1} open: {2} max: {3} count: {4}\ncustomProps: {5}", this.name, this.isVisible, this.isOpen, this.maxPlayersSetting, this.playerCount, SupportClass.DictionaryToString(this.properties));
    }

    /// <summary>
    /// Gets all properties of game: Custom AND "well known" ones.
    /// </summary>
    /// <returns></returns>
    internal Hashtable GetAllGameProperties()
    {
        Hashtable allProps = new Hashtable();

        if (this.isOpen)
        {
            allProps[GameProperties.IsOpen] = this.isOpen;
        }

        if (this.isVisible)
        {
            allProps[GameProperties.IsVisible] = this.isVisible;
        }

        if (this.maxPlayersSetting != 0)
        {
            allProps[GameProperties.MaxPlayers] = (byte)this.maxPlayersSetting;
        }

        allProps.Merge(this.properties);
        return allProps;
    }

    internal void CacheProperties(Hashtable properties)
    {
        if (properties == null || properties.Count == 0)
        {
            return;
        }

        // check of this game was removed from the list. in that case, we don't
        // need to read any further properties
        // list updates will remove this game from the game listing
        if (properties.ContainsKey(GameProperties.Removed))
        {
            this.mRemoved = (Boolean)properties[GameProperties.Removed];
            if (this.mRemoved)
            {
                return;
            }
        }

        if (properties.ContainsKey(GameProperties.MaxPlayers))
        {
            this.maxPlayersSetting = (byte)properties[GameProperties.MaxPlayers];
        }

        if (properties.ContainsKey(GameProperties.IsOpen))
        {
            this.isOpen = (Boolean)properties[GameProperties.IsOpen];
        }

        if (properties.ContainsKey(GameProperties.IsVisible))
        {
            this.isVisible = (Boolean)properties[GameProperties.IsVisible];
        }

        if (properties.ContainsKey(GameProperties.PlayerCount))
        {
            this.playerCount = (int)((Byte)properties[GameProperties.PlayerCount]);
        }

        this.properties.MergeStringKeys(properties);
    }
}