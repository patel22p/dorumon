using UnityEngine;
using System.Collections;

public class Spawn : Base
{
    public Transform _Player;


    protected override void OnLoaded()
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);            
    }
    protected override void OnPlayerDisconnected(NetworkPlayer player)
    {
        foreach (CarController a in GameObject.FindObjectsOfType(typeof(CarController)))
            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner == player) Destroy(v.viewID);

        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);        
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

    protected override void OnDisconnectedFromServer()
    {
        Application.LoadLevel(Application.loadedLevel);      
    }
    
    
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }