using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    protected override void Start()
    {        
        if (Network.peerType == NetworkPeerType.Disconnected) return;
        if(!Network.isServer)
            networkView.RPC("RPCNW",RPCMode.All,  Network.AllocateViewID());
        base.Start();
    }
    protected override void OnCollisionEnter(Collision collisionInfo)
    {

        if (collisionInfo.gameObject.name == "LocalPlayer")
        {
            print("car set owner");
            if(Input.GetKey(KeyCode.F))
                SetOwner(Network.player);
        }
    }
    public override void FixedUpdate()
    {
        if (isMine)
        {
            brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
            handbrake = Input.GetButton("Jump") ? 1.0f : 0.0f;
            steer = Input.GetAxis("Horizontal");
            motor = Mathf.Clamp01(Input.GetAxis("Vertical"));
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
    [RPC]
    public void RPCNW(NetworkViewID id)
    {
        print("RPCNW call");
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();                
        nw.group = (int)Group.RPCAssignID;
        nw.observed = null;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;        
    }
}
 