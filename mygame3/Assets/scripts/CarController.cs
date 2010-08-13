using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    public Transform exp;

    protected Vector3 spawnpos;
    protected override void Start()
    {

        spawnpos = transform.position;
        Life = 200;
        base.Start();
        Reset();
        if (Network.peerType == NetworkPeerType.Disconnected) return;
        if (!Network.isServer)
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        
        
    }
    public override Vector3 SpawnPoint()
    {
        return spawnpos;
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
    
    
    double timedown1;
    
    protected override void Update()
    {
        base.Update();

        if (_localPlayer == null) return;
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
            if (transform.Find("door").collider.bounds.Contains(_localPlayer.transform.position)
                && _localPlayer.enabled && Input.GetKeyDown(KeyCode.F) && OwnerID == null)
            {
                _localPlayer.RPCShow(false);
                RPCSetOwner();
                _cam.localplayer = this;
            }
        }
        
        
    }
    void FixedUpdate()
    {        
        UpdateCar();
    }
    private void CarOut()
    {        
        motor = 0;
        steer = 0;
        brake = 0;
        handbrake = 0;
        rigidbody.velocity = Vector3.zero;

        _localPlayer.RPCShow(true);
        _localPlayer.transform.position = transform.position + new Vector3(0, 1.5f, 0); ;
        _cam.localplayer = _localPlayer;
        RPCResetOwner();
    }
    public override void RPCDie()
    {
        
        Destroy(Instantiate(exp, transform.position, Quaternion.identity), 10);
        if (isOwner)
        {
            _cam.localplayer = _localPlayer;
            RPCResetOwner();
            _localPlayer.killedyby = killedyby;
            _localPlayer.RPCSetLife(-2);
            _TimerA.AddMethod(2000, RPCSpawn);
            Screen.lockCursor = false;
        }
        Destroy(Instantiate(brokencar, transform.position, transform.rotation), 20);
        Show(false);
        
    }
    public Transform brokencar;
    public Transform effects;
    [RPC]
    public void RPCSpawn()
    {
        CallRPC(true);
        Show(true);
        
        foreach (Transform item in Base.getChild(effects))
            Destroy(item);
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
 