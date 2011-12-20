// ----------------------------------------------------------------------------
// <copyright file="PhotonPlayer.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Represents a player, identified by actorID (a.k.a. ActorNumber). 
//   Caches properties of a player.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

/// <summary>
/// Summarizes a "player" within a room, identified (in that room) by actorID.
/// </summary>
/// <remarks>
/// Each player has a actorID, valid for that room. It's -1 until it's assigned by server.
/// </remarks>
public class PhotonPlayer
{
    /// <summary>This player's actorID</summary>
    public int ID
    {
        get { return this.actorID; }
    }

    /// <summary>Identifier of this player in current room.</summary>
    private int actorID = -1;

    /// <summary>Nickname of this player.</summary>
    public string name = "";

    /// <summary>Only one player is controlled by each client. Others are not local.</summary>
    public readonly bool isLocal = false;

    /// <summary>Cache for custom properties of player.</summary>
    internal Hashtable mCustomProperties = new Hashtable();

    public PhotonPlayer(bool isLocal, int actorID, string name)
    {
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.name = name;
    }

    /// <summary>
    /// Caches custom properties for this player.
    /// </summary>
    internal void CacheProperties(Hashtable properties)
    {
        if (properties == null || properties.Count == 0)
        {
            return;
        }

        if (properties.ContainsKey(ActorProperties.PlayerName))
        {
            this.name = (string)properties[ActorProperties.PlayerName];
        }

        this.mCustomProperties.MergeStringKeys(properties);
    }

    /// <summary>
    /// Gives the name.
    /// </summary>
    public override string ToString()
    {
        return this.name;// +" " + SupportClass.HashtableToString(this.mCustomProperties);
    }

    /// <summary>
    /// Makes PhotonPlayer comparable
    /// </summary>
    public override bool Equals(object p)
    {
        PhotonPlayer pp = p as PhotonPlayer;
        return (pp != null && this.GetHashCode() == pp.GetHashCode());
    }

    public override int GetHashCode()
    {
        return this.ID;
    }

    /// <summary>
    /// The player with the lowest actorID is the master and could be used for special tasks. 
    /// </summary>
    public bool isMasterClient
    {
        get { return (PhotonNetwork.networkingPeer.mMasterClient == this); }
    }

    /// <summary>
    /// Used internally, to update this client's playerID when assigned.
    /// </summary>
    internal void ChangeLocalID(int newID)
    {
        if (!this.isLocal)
        {
            Debug.LogError("ERROR You should never change PhotonPlayer IDs!");
            return;
        }

        this.actorID = newID;
    }

    /// <summary>
    /// Try to get a specific player by id. 
    /// </summary>
    /// <param name="ID">ActorID</param>
    /// <returns>The player with matching actorID or null, if the actorID is not in use.</returns>
    public static PhotonPlayer Find(int ID)
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.ID == ID)
                return player;
        }
        return null;
    }


    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public string guid
    {
        get { return ToString(); }
    }
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public string ipAddress
    {
        get { return "NETW_ERROR"; }
    }
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public string externalIP
    {
        get { return "NETW_ERROR"; }
    }
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public int port
    {
        get { return -1; }
    }
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public int externalPort
    {
        get { return -1; }
    }
}