using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;

public enum Team:int { ata, def }
public class Player : IPlayer {
    internal Team team;
    public float flyForce = 300;
    public Transform bloodexp;
    public float force = 400;
    public float angularvel = 600;
    public float maxVelocityChange = 10.0f;
    public string Nick;
    public int score;
    Cam _cam { get { return Find<Cam>(); } }
    Blood blood { get { return Find<Blood>(); } }
    GameObject boxes { get { return GameObject.Find("box"); } }
    public static Dictionary<NetworkPlayer, Player> players = new Dictionary<NetworkPlayer, Player>();
    void Start()
    {
        if (started || !levelLoaded) return;
        started = true;

        if (networkView.isMine)
        {
            RPCSetNick(GuiConnection.Nick);
            RPCSetOwner();            
            
            RPCSpawn();
            RPCSetTeam((int)team);
            _cam.localplayer = this;
        }
    }
    [RPC]
    public void RPCSpawn()
    {
        Show(true);
        CallRPC(true);
        RCPSelectGun(1);
        foreach (GunBase gunBase in gunlist)
            gunBase.Reset();
        Life = 100;
        transform.position = SpawnPoint();
    }
    protected override void Update()
    {
        if (!started) return;

        if (isOwner && Screen.lockCursor)
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
                RCPSelectGun(1);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                RCPSelectGun(2);
            if (Input.GetKey(KeyCode.Space))
            {
                rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        base.Update();
    }
    void FixedUpdate()
    {
        if (!started) return;
        if (isOwner) LocalMove();
    }
    private void LocalMove()
    {
        if (Screen.lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = _cam.transform.TransformDirection(moveDirection);
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
    private void RPCSetTeam(int t)
    {        
        CallRPC(true , t);
        team =   (Team)t;
        this.renderer.material.color = team == Team.ata?Color.red:Color.blue;
    }
    public override void OnSetID()
    {
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
        players.Add(OwnerID.Value, this);
    }
    [RPC]
    private void RCPSelectGun(int i)
    {
        CallRPC(true,i);
        foreach (GunBase gb in gunlist)
            gb.DisableGun();
        gunlist[i-1].EnableGun();      
        
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

        if (b != null && isOwner &&
            b.OwnerID != null && !b.isOwner && b is CarController &&
            collisionInfo.impactForceSum.sqrMagnitude > 50 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
        {
            RPCSetLife(Life - (int)collisionInfo.impactForceSum.sqrMagnitude / 2);
        }

    }
    [RPC]
    void RPCSetNick(string nick)
    {
        CallRPC(true,nick);
        Nick = nick;
    }
    [RPC]
    public override void RPCSetLife(int NwLife)
    {        
        CallRPC(true,NwLife);
        if (isOwner)
            blood.Hit(Mathf.Abs(NwLife - Life));
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;

    }
    public override void RPCDie()
    {
        Transform a;
        Destroy(a=(Transform)Instantiate(bloodexp, transform.position, Quaternion.identity),5);
        a.parent = GameObject.Find("effects").transform;
        if (isOwner)
        {
            _TimerA.AddMethod(2000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if (p.OwnerID == killedyby)
                {
                    if (p.isOwner)
                    {                        
                        _Loader.rpcwrite(_localPlayer.Nick + " died byself");
                        _localPlayer.networkView.RPC("RPCSetScore", RPCMode.AllBuffered, _localPlayer.score - 1);
                    }
                    else if (p.team != _localPlayer.team)
                    {
                        _Loader.rpcwrite(p.Nick + " killed " + _localPlayer);
                        p.networkView.RPC("RPCSetScore", RPCMode.AllBuffered, p.score + 1);
                    }
                    else
                    {
                        _Loader.rpcwrite(p.Nick + " friendly fired " + _localPlayer);
                        p.networkView.RPC("RPCSetScore", RPCMode.AllBuffered, p.score - 1);
                    }
                }
        }
        
        Show(false);
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
    public override Vector3 SpawnPoint()
    {
        Transform t= spawn.transform.Find(team.ToString());
        return t.GetChild(UnityEngine.Random.Range(0, t.childCount)).transform.position;
    }
}
