using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    protected override void Start()
    {
        base.Start();
        if (Network.peerType == NetworkPeerType.Disconnected) return;        
        if(!Network.isServer)
            networkView.RPC("RPCNW",RPCMode.All,  Network.AllocateViewID());
        
    }
    [RPC]
    public void RPCNW(NetworkViewID id)
    {        
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)Group.RPCAssignID;
        nw.observed = null;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }

    Player localPlayer  { get { return Find<Player>("LocalPlayer"); } }
    Cam _cam { get { return Find<Cam>(); } }
    
    protected override void  OnTriggerStay(Collider collisionInfo)
    {
        
        if (collisionInfo.gameObject.name == "LocalPlayer" && !isControlled)
        {            
            if (Input.GetKeyDown(KeyCode.F))
            {
                localPlayer.Show(false);
                RPCSetOwner();
                _cam.localplayer = this;
            }
        }
    }
    
    public override void FixedUpdate()
    {
        if (isMineControlled)
        {            
            brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
            handbrake = Input.GetButton("Jump") ? 1f : 0.0f;
            steer = Input.GetAxis("Horizontal");
            motor = Mathf.Clamp01(Input.GetAxis("Vertical"));
            if (Input.GetKeyDown(KeyCode.F))
            {
                localPlayer.Show(true);                
                localPlayer.transform.position = transform.Find("door").position;
                _cam.localplayer = localPlayer;
                RPCResetOwner();
            }
            if (Input.GetButton("Jump")) rigidbody.velocity *= .99f;
        }
        else
        {
            motor = 0;
            steer = 0;
            brake = 0;
            handbrake = 0;
        }

        base.FixedUpdate();
    }
    
}
 