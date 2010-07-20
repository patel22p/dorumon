using UnityEngine;
using System.Collections;

public class Spawn : Base
{
    public Transform _Player;

    
    protected override void Start()
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
        {            
            Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
            Network.RemoveRPCs(Network.player, (int)Group.Player);
            
        }
    }

    protected override void OnPlayerDisconnected(NetworkPlayer player)
    {

        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner == player) Destroy(v.viewID);
        //Network.DestroyPlayerObjects(player);
        //Network.RemoveRPCs(player);        
    }
    

    [RPC]
    private void Destroy(NetworkViewID v)
    {
        CallRPC(v);
        NetworkView nw = NetworkView.Find(v);
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);

    }    
    
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }