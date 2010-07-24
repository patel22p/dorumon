using UnityEngine;
using System.Collections;

public class Spawn : Base
{
    public Transform _Player;

    
    protected override void Start()
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
            Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);            
    }
    protected override void OnPlayerDisconnected(NetworkPlayer player)
    {
        print("pl disc");                
        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);        
    }
    protected override void OnDisconnectedFromServer()
    {
        Application.LoadLevel(Application.loadedLevel);      
    }
    
    
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }