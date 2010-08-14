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
    void OnGUI()
    {
        if (!teamselected)
            GUI.Window(4, CenterRect(.6f, .4f), TeamSelectWindow, "Team Select");
    }
    bool teamselected;
    public Dictionary<NetworkPlayer, Player> players = new Dictionary<NetworkPlayer, Player>();
    public void TeamSelectWindow(int a)
    {
        if (teamselected) return;
        GUI.BringWindowToFront(a);
        bool ata, def;
        if ((ata = GUILayout.Button("Atackers")) || (def = GUILayout.Button("Defenders")))
        {
            if (Network.peerType == NetworkPeerType.Disconnected)
                Network.InitializeServer(32, 5300);                
            Transform t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
            t.GetComponent<Player>().team = ata ? Team.ata : Team.def;
            teamselected = true;            
        }
        
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