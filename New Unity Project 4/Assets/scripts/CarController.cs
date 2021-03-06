﻿using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    Vector3 startpos;
    protected override void OnLoaded()
    {
        startpos = transform.position;
        Life = 300;
        base.OnLoaded();
        if (Network.peerType == NetworkPeerType.Disconnected) return;        
        if(!Network.isServer)
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        Reset();
    }    
     
    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {
        print("new nw");
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)Group.RPCAssignID;
        nw.observed = null;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }

    
    Cam _cam { get { return Find<Cam>(); } }
    
    protected override void  OnTriggerStay(Collider collisionInfo)
    {
        
        if (collisionInfo.gameObject.name == "LocalPlayer" && !isControlled && localPlayer.enabled)
        {            
            if (Input.GetKeyDown(KeyCode.F))
            {                
                localPlayer.Show(false);
                RPCSetOwner();
                _cam.localplayer = this;
            }
        }
    }
    
    protected override void OnFixedUpdate()
    {
        if (isOwner)
        {            
            brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
            handbrake = Input.GetButton("Jump") ? 1f : 0.0f;
            steer = Input.GetAxis("Horizontal");
            motor = Mathf.Clamp01(Input.GetAxis("Vertical"));
            if (Input.GetKeyDown(KeyCode.F))
            {
                CarOut();
            }
            if (Input.GetButton("Jump")) rigidbody.velocity *= .99f;
        }
        else
        {

        }

        base.OnFixedUpdate();
    }

    private void CarOut()
    {
        motor = 0;
        steer = 0;
        brake = 0;
        handbrake = 0;
        rigidbody.velocity = Vector3.zero;

        localPlayer.Show(true);
        localPlayer.transform.position = transform.position + new Vector3(0, 1.5f, 0); ;
        _cam.localplayer = localPlayer;
        RPCResetOwner();
    }
    public override void RPCDie()
    {
        if (isOwner)
        {
            CarOut();            
            _TimerA.AddMethod(2000, RPCSpawn);
        }
        Show(false);
    }
    [RPC]
    public void RPCSpawn()
    {
        
        CallRPC(true);        
        Show(true);
        transform.position = startpos;
        Life = life;
        Reset();        
    }

    private void Reset()
    {
        foreach (GunBase gunBase in gunlist)
            gunBase.Reset();
    }
    const int life = 300;
    
}
 