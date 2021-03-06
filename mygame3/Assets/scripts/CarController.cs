﻿using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    public Transform exp;
    public Transform nitropref;
    Renderer[] nitroprerrenderrs;
    protected override void Start()
    {
        nitroprerrenderrs = nitropref.GetComponentsInChildren<Renderer>();
        base.Start();
        rigidbody.angularDrag = 1;
        StartCar();
        Life = life;
        Reset();

    }

    double timedown1;
    protected override void Update()
    {
        audio.enabled = OwnerID != -1;
        base.Update();
        if (_LocalPlayer == null) return;
        rigidbody.drag = OwnerID == -1 ? 1 : 0;
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

            if (Input.GetKeyDown(KeyCode.LeftShift)) RPCNitro(true);
            if (Input.GetKeyUp(KeyCode.LeftShift)) RPCNitro(false);
            if (Input.GetKeyDown(KeyCode.F))
            {
                _LocalPlayer.RPCShow(true);
                _LocalPlayer.RPCSelectGun(_LocalPlayer.selectedgun);
                _LocalPlayer.transform.position = transform.position + new Vector3(0, 1.5f, 0); ;
                _localiplayer = _LocalPlayer;
                RPCCarOut(Network.player.GetHashCode());
                RPCResetOwner();
            }
            //if (Input.GetButton("Jump")) rigidbody.velocity *= .99f;
        }
        else
        {
            if (transform.Find("door").collider.bounds.Contains(_LocalPlayer.transform.position)
                && _LocalPlayer.enabled && Input.GetKeyDown(KeyCode.F) && OwnerID == -1)
            {
                _LocalPlayer.RPCShow(false);
                RPCSetOwner();
                _localiplayer = this;
                RPCCarIn(Network.player.GetHashCode());
            }
        }
    }
    [RPC]
    private void RPCNitro(bool p)
    {
        CallRPC(false, p);
        nitroenabled = p;
    }
    [RPC]
    private void RPCCarIn(int np)
    {
        CallRPC(false, np);
        _Spawn.players[np].car = this;
        audio.PlayOneShot((AudioClip)Resources.Load("Car_Start_02"));        
    }
    bool nitroenabled;
    protected virtual void FixedUpdate()
    {
        

        if (nitroenabled)
        {
            if (nitro < 0 && isOwner)
                RPCNitro(false);

            foreach (Renderer r in nitroprerrenderrs)
                r.enabled = true;
            this.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, 50000));
            nitro -= .2f;
        }
        else
            foreach (Renderer r in nitroprerrenderrs)
                r.enabled = false;
                
        if (OwnerID != -1)
            FixedUpdateCar();
    }
    [RPC]
    private void RPCCarOut(int np)
    {
        CallRPC(false, np);
        rigidbody.velocity = Vector3.zero;
        _Spawn.players[np].car = null;
    }

    [RPC]
    public override void RPCHealth()
    {
        CallRPC(false);
        if (Life < life)
            Life += 30;
        guns[0].bullets = guns[1].bullets += 50;
    }

    [RPC]
    public override void RPCDie(int killedby)
    {
        CallRPC(false, killedby);
        Destroy(Instantiate(exp, transform.position, Quaternion.identity), 10);
        if (isOwnerOrServer)
            _TimerA.AddMethod(10000, RPCSpawn);
        if (isOwner)
        {
            RPCCarOut(Network.player.GetHashCode());
            _localiplayer = _LocalPlayer;
            RPCResetOwner();            
            _LocalPlayer.RPCSetLife(-200, killedby); 
            lockCursor = false;
        }

        Destroy(Instantiate(brokencar, transform.position, transform.rotation), 20);
        Show(false);

    }
    public Transform brokencar;
    public Transform effects;
    [RPC]
    public void RPCSpawn()
    {
        CallRPC(false);
        Show(true);
        foreach (Transform item in Base.getChild(effects))
            Destroy(item);
        transform.position = spawnpos;
        transform.rotation = Quaternion.identity;
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
