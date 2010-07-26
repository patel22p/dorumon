using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;

public class Player : IPlayer {
    public float flyForce = 300;
    public float maxVelocityChange = 10.0f;
    Cam _cam { get { return Find<Cam>(); } }
    Blood blood { get { return Find<Blood>(); } }
    
    public static Spawn spawn { get { return Find<Spawn>(); } }
    GuiConnection connectionGui { get { return Find<GuiConnection>(); } }
    
    GameObject boxes { get { return GameObject.Find("box"); } }
    
    public float force = 400;
    public float angularvel = 600;
    
    public string Nick;
    public int score;
    protected override void OnLoaded()
    {
        if (networkView.isMine)            
        {            
            RPCSetNick(connectionGui.Nick);
            RPCSetOwner();            
            RPCSpawn();
            _cam.localplayer = this;
        }
    }

    public override void OnSetID()
    {
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
    }
    protected override void Awake()
    {
        
        base.Awake();
    }
    
    protected override void OnUpdate()
    {

        if (isOwner)
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
                RCPSelectGun(1);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                RCPSelectGun(2);
        }
    }
    
    [RPC]
    private void RCPSelectGun(int i)
    {
        CallRPC(i);
        foreach (GunBase gb in gunlist)
            gb.DisableGun();
        gunlist[i-1].EnableGun();      
        
    }

    protected override void OnFixedUpdate()
    {
        
        if (isOwner) LocalMove();
    }

    private void LocalMove()
    {        
        Vector3 moveDirection = Vector3.zero;
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = _cam.transform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();
        this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.deltaTime * angularvel);
        this.rigidbody.AddForce(moveDirection * Time.deltaTime * force);
    }
    
    public static Vector3 SpawnPoint()
    {
        return spawn.transform.GetChild(Random.Range(0, spawn.transform.childCount)).transform.position;
    }

    [RPC]
    void RPCSetNick(string nick)
    {
        CallRPC(nick);
        Nick = nick;
    }
    
    [RPC]
    public override void RPCSetLife(int NwLife)
    {        
        CallRPC(NwLife);
        if (isOwner)
            blood.Hit(Mathf.Abs(NwLife - Life));
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;

    }
    public override void RPCDie()
    {
        if (isOwner)
        {
            _TimerA.AddMethod(2000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if (p.OwnerID == killedyby)
                {
                    if (p.isOwner)
                        localPlayer.networkView.RPC("RPCSetScore", RPCMode.All, localPlayer.score - 1);
                    else
                        p.networkView.RPC("RPCSetScore", RPCMode.All, p.score + 1);
                }
        }
        Show(false);
    }
    [RPC]
    public void  RPCSpawn()
    {
        Show(true);
        CallRPC();        
        RCPSelectGun(1);
        foreach (GunBase gunBase in gunlist)
            gunBase.Reset();        
        Life = 100;
        transform.position = SpawnPoint();
    }
    [RPC]
    public void RPCSetScore(int i)
    {        
        score = i;
    }
    
    
    public static Vector3 Clamp(Vector3 velocityChange,float maxVelocityChange)
    {
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }
    
}
