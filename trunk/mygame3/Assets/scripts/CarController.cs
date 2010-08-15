using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    public Transform exp;

    
    protected override void Start()
    {
        base.Start();
        rigidbody.angularDrag = 5;
        StartCar();
        
        Life = 200;        
        Reset();
        
    }


    
    Cam _cam { get { return Find<Cam>(); } }
    double timedown1;
    protected override void Update()
    {
        base.Update();
        if (_localPlayer == null) return;
        if (isOwner)
        {
            if (Mathf.Abs(clamp(transform.rotation.eulerAngles.z)) > 70 || Mathf.Abs(clamp(transform.rotation.eulerAngles.x)) > 70)
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
            //if (Input.GetButton("Jump")) rigidbody.velocity *= .99f;
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
    protected virtual void FixedUpdate()
    {
        FixedUpdateCar();                
    }
    private void CarOut()
    {        
        motor = 0;
        steer = 0;
        brake = 0;
        handbrake = 0;
        rigidbody.velocity = Vector3.zero;
        _localPlayer.RPCShow(true);
        _localPlayer.RCPSelectGun(1);
        _localPlayer.transform.position = transform.position + new Vector3(0, 1.5f, 0); ;
        _cam.localplayer = _localPlayer;
        RPCResetOwner();
    }
    public override void RPCDie()
    {
        
        Destroy(Instantiate(exp, transform.position, Quaternion.identity), 10);
        if (isOwnerOrServer)
            _TimerA.AddMethod(2000, RPCSpawn);
        if (isOwner)
        {
            _cam.localplayer = _localPlayer;
            RPCResetOwner();
            _localPlayer.killedyby = killedyby;
            _localPlayer.RPCSetLife(-2);            
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
        foreach (GunBase gunBase in guns)
            gunBase.Reset();
    }
    const int life = 300;
    
}
 