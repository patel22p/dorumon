using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;

public enum Team : int { ata, def }
public class Player : IPlayer
{
    internal new Team team;
    public float flyForce = 300;
    public Transform bloodexp;
    public float force = 400;
    public int frags;
    public float angularvel = 600;
    public float maxVelocityChange = 10.0f;
    public string Nick;    
    GuiBlood blood { get { return GuiBlood._This; } }

    protected override void Start()
    {
        if (networkView.isMine)
        {            
            _localiplayer =_LocalPlayer = this;
            RPCSetNick(GuiConnection.Nick);
            RPCSetOwner();
            RPCSpawn();
            RPCSetTeam((int)team);
        }
    }
    [RPC]
    public void RPCSpawn()
    {
        CallRPC(true);
        Show(true);
        RCPSelectGun(1);
        foreach (GunBase gunBase in guns)
            gunBase.Reset();
        Life = 100;
        transform.position = SpawnPoint();
    }
    int guni;

    public int fps;
    public int ping;
    [RPC]
    void RPCPingFps(int ping, int fps)
    {
        CallRPC(true, ping, fps);
        this.ping = ping;
        this.fps = fps;

    }
    public Player serverPl
    {
        get
        {
            return Network.isServer ? _LocalPlayer : _Spawn.players[Network.connections[0]];
        }
    }
    
    protected override void Update()
    {        
        
        //this.transform.Find("Sphere").rotation = Quaternion.Euler(this.rigidbody.angularVelocity);
        

        if (_TimerA.TimeElapsed(1000) && isOwner && _Spawn.players.Count > 0)
            RPCPingFps(Network.GetLastPing(serverPl.OwnerID.Value), _Loader.fps);
        if (isOwner && Screen.lockCursor)
        {


            NextGun(Input.GetAxis("Mouse ScrollWheel"));
            

            if (Input.GetKeyDown(KeyCode.Alpha1))
                RCPSelectGun(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                RCPSelectGun(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                RCPSelectGun(2);
            if (Input.GetKey(KeyCode.Space))
            {
                rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        base.Update();
    }

    public void NextGun(float a)
    {        
        if (a != 0)
        {
            transform.Find("Guns").GetComponent<AudioSource>().Play();
            if (a > 0)
                guni++;
            if (a < 0)
                guni--;
            if (guni > guns.Length - 1) guni = 0;
            if (guni < 0) guni = guns.Length - 1;
            RCPSelectGun(guni);
        }
    }
    protected virtual void FixedUpdate()
    {
        if (isOwner) LocalMove();
    }
    private void LocalMove()
    {
        if (Screen.lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = _Cam.transform.TransformDirection(moveDirection);
            moveDirection.y = 0;
            moveDirection.Normalize();
            this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.deltaTime * angularvel);

            this.rigidbody.AddForce(moveDirection * Time.deltaTime * force);
        }
    }
    protected override void OnConsole(string s)  
    {
        if (s == "spec" && isOwner)
        {
            if (_Cam.spectator == true)
            {
                _Cam.spectator = false;
                RPCSpawn();
            }
            else
                if (_Cam.spectator == false && enabled)
                {
                    print("u are now spectator");
                    RPCSpec();
                }

        }
    }
    [RPC]
    public void RPCSetTeam(int t)
    {
        CallRPC(true, t);
        team =   (Team)t;
        
    }
    public override void OnSetID()
    {
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;        
        _Spawn.players.Add(OwnerID.Value, this);
    }
    [RPC]
    public void RCPSelectGun(int i)
    {
        CallRPC(true, i);
        foreach (GunBase gb in guns)
            gb.DisableGun();
        guns[i].EnableGun();      
        
    }
    [RPC]
    private void RPCSpec()
    {
        Show(false);
        _Cam.spectator = true;

    }
    
    void OnCollisionEnter(Collision collisionInfo)
    {
        Base b = collisionInfo.gameObject.GetComponent<Base>();
        if (b != null && isOwner && !b.isOwner &&
            b is box && !(b is Player) && !(b is Zombie) &&
            collisionInfo.impactForceSum.sqrMagnitude > 150 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
        {
            killedyby = b.OwnerID;
            RPCSetLife(Life - (int)collisionInfo.impactForceSum.sqrMagnitude / 2);
        }
    }
    [RPC]
    void RPCSetNick(string nick)
    {
        CallRPC(true, nick);
        Nick = nick;
    }
    [RPC]
    public override void RPCSetLife(int NwLife)
    {
        CallRPC(true, NwLife);
        if (isOwner)
            blood.Hit(Mathf.Abs(NwLife - Life));
        if (killedyby == null || _Spawn.players[killedyby.Value].team != team)
            Life = NwLife;

        if (NwLife < 0)
            RPCDie();

    }
    public override void RPCDie()
    {

        Transform a = (Transform)Instantiate(bloodexp, transform.position, Quaternion.identity);
        Destroy(a.gameObject, 5);
        a.parent = _Spawn.effects;
        if (isOwner)
        {
            _TimerA.AddMethod(10000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if (p.OwnerID == killedyby || !killedyby.HasValue)
                {
                    if (p.isOwner || !killedyby.HasValue)
                    {                        
                        _Loader.rpcwrite(_LocalPlayer.Nick + " died byself");
                        _LocalPlayer.RPCSetFrags(-1);
                    }
                    else if (p.team != _LocalPlayer.team)
                    {
                        _Loader.rpcwrite(p.Nick + " killed " + _LocalPlayer.Nick);
                        p.RPCSetFrags(+1);
                    }
                    else
                    {
                        _Loader.rpcwrite(p.Nick + " friendly fired " + _LocalPlayer.Nick);
                        p.RPCSetFrags(-1);
                        
                    }
                }
            Screen.lockCursor = false;
        }
        
        Show(false);
    }

    [RPC]
    public void RPCSetFrags(int i)
    {
        CallRPC(true, i);
        frags += i; 
    }
    public static Vector3 Clamp(Vector3 velocityChange,float maxVelocityChange)
    {
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }
    public override Vector3 SpawnPoint()
    {
        Transform t= _Spawn.transform.Find(team.ToString());
        return t.GetChild(UnityEngine.Random.Range(0, t.childCount)).transform.position;
    }
}
