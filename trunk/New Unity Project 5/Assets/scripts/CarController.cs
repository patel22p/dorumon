using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    public Transform exp;

    
    protected override void OnLoaded()
    {
        spawnpos = transform.position;
        Life = 200;
        base.OnLoaded();
        if (Network.peerType == NetworkPeerType.Disconnected) return;        
        if(!Network.isServer)
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        Reset();
        
    }    
     
    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {        
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
                localPlayer.RPCShow(false);
                RPCSetOwner();
                _cam.localplayer = this;
            }
        }
    }
    double timedown1;
    public float clamp(float a)
    {
        if (a > 180) return a - 360f;
        return a;
    }
    protected override void OnFixedUpdate()
    {                        
        if (isOwner)
        {
            if (Mathf.Abs(clamp(transform.rotation.eulerAngles.z)) > 90 || Mathf.Abs(clamp(transform.rotation.eulerAngles.x)) > 90)
                timedown1 += _TimerA._SecodsElapsed;
            else
                timedown1 = 0;
            if (timedown1 > 3)
            {
                transform.rotation = Quaternion.identity;
            }

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
        Object dt = (Object)Instantiate(exp, transform.position, Quaternion.identity);
        Destroy(dt, 10);
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
        transform.position = spawnpos;
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
 