using UnityEngine;
using System.Collections;

public class Spawn : Base
{
    public Transform _Player;


    protected override void OnLoaded()
    {
        AudioListener.volume = .1f;
        if (Network.peerType != NetworkPeerType.Disconnected)
            Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);            
        
    }
    void Update()
    {
        
        if (!Loader.Online && Network.peerType == NetworkPeerType.Disconnected && Input.GetMouseButtonDown(1))
        {            
            Network.InitializeServer(32, 5300);
            OnLoaded();
        }
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
        CallRPC(false,v);
        NetworkView nw = NetworkView.Find(v);
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);
    }
    protected override void OnDisconnectedFromServer()
    {
        Base.loaded = false;
    }
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }