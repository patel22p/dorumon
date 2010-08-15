using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Spawn : Base
{
    public Transform _Player;

    
    [RPC]
    void SetGameTime(int gtime)
    {
        _Loader.gametime = gtime;
        CallRPC(true, gtime);
    }

    void Start()
    {
        
        AudioListener.volume = .1f;
        if (Network.isServer)
        {            
            SetGameTime(_Loader.GameTime);
            Network.incomingPassword = "started";
            MasterServer.UnregisterHost();
        }
        
        
    }
    public Transform effects;
    
    
    public Dictionary<NetworkPlayer, Player> players = new Dictionary<NetworkPlayer, Player>();
    public void OnTeamSelect(bool ata)
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
            Network.InitializeServer(32, 5300);
        Transform t;
        if (_localPlayer == null)
            t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
        else
            t = _localPlayer.transform;
        t.GetComponent<Player>().team = ata ? Team.ata : Team.def;            

    }
              
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        foreach (Box a in GameObject.FindObjectsOfType(typeof(Box)))
        {
            if (a.OwnerID == player)
                a.RPCResetOwner();
            
            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner == player) Destroy(v.viewID);
        }

        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);        
    }
    [RPC]
    private void Destroy(NetworkViewID v)
    {
        CallRPC(false,v);
        NetworkView nw = NetworkView.Find(v);
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);
    }
    
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }