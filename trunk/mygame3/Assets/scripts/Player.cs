using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;

public enum Team : int { None, ata, def }
public class Player : IPlayer
{
    internal CarController car;
    
    internal new Team team { get { return userview.team; }  set { userview.team=value; } }    
    
    public Transform bloodexp;
    public float force = 400;
    
    
    internal float freezedt;
    public float tormoza = 1.5f;
    public ParticleEmitter frozenRender;
    public string Nick { get { return userview.nick; } }
    public int frags { get { return userview.frags; } set { userview.frags = value; } }
    
    public z0Vk.user userview;
    GuiBlood blood { get { return GuiBlood._This; } }
    protected override void Start()
    {
        base.Start();
        if (networkView.isMine)
        {
            _localiplayer = _LocalPlayer = this;
            RPCSetOwner();
            RPCSpawn();
        }
    }
    
    [RPC]
    public void RPCSpawn()
    {
        CallRPC(false);
        
        Show(true);
        if (isOwner)
        {
            RPCSelectGun(1);
            RPCSetTeam((int)_Spawn.team);
        }
        foreach (GunBase gunBase in guns)
            gunBase.Reset();
        Life = 100;
        freezedt = 0;
        transform.position = SpawnPoint();
        transform.rotation = Quaternion.identity;
    }
    int guni;

    
    //public Player serverPl
    //{
    //    get
    //    {
    //        print(Network.connections[0]);
    //        return Network.isServer ? _LocalPlayer : _Spawn.players[Network.connections[0]];
    //    }
    //}

    protected override void Update()
    {
        if (freezedt >= 0)
        {
            freezedt -= Time.deltaTime * 5;
            frozenRender.emit = true;
        }
        else frozenRender.emit = false;
        //this.transform.Find("Sphere").rotation = Quaternion.Euler(this.rigidbody.angularVelocity);

        

        if (isOwner && lockCursor)
        {
            NextGun(Input.GetAxis("Mouse ScrollWheel"));
            if (Input.GetKeyDown(KeyCode.Alpha1))
                RPCSelectGun(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                RPCSelectGun(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                RPCSelectGun(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                RPCSelectGun(3);
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (nitro > 10 || !build)
                {
                    nitro -= 10;
                    RCPJump();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
                rigidbody.angularVelocity = Vector3.zero;
            }

        }
        base.Update();
    }

    [RPC]
    private void RCPJump()
    {
        CallRPC(false);
        rigidbody.AddForce(_Cam.transform.rotation * new Vector3(0, 0, 1000));
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
            RPCSelectGun(guni);
        }
    }
    protected virtual void FixedUpdate()
    {
        if (isOwner) LocalMove();
    }
    
    private void LocalMove()
    {
        if (DebugKey(KeyCode.G))
        {
            RPCSetFrags(20);
            RPCSetLife(-200, -1);
        }
        //f400,a20,4,4,6,min,max
        if (lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = _Cam.transform.TransformDirection(moveDirection);
            moveDirection.y = 0;
            moveDirection.Normalize();
            
            Vector3 v =this.rigidbody.velocity;            
            float angle ;
            if (moveDirection == Vector3.zero || rigidbody.velocity == Vector3.zero)
                angle = 1;
            else
            {
                Quaternion a = Quaternion.LookRotation(rigidbody.velocity);
                Quaternion b = Quaternion.LookRotation(moveDirection);
                angle = 1 + (Quaternion.Angle(a, b) / 180 * 4);
            }
            
            this.rigidbody.maxAngularVelocity = v.magnitude / tormoza;
            this.rigidbody.AddForce(moveDirection * Time.deltaTime * force * angle * (frozenRender.emit || Input.GetKey(KeyCode.Space) ? .5f : 1));
        }
    }

    [RPC]
    public void RPCSetTeam(int t)
    {
        print("set team");
        CallRPC(false, t);
        team = (Team)t;
    }
    public override bool dead { get { return !enabled && car == null; } }
    public override void OnSetOwner()
    {
        print("set owner");
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
        _Spawn.players.Add(OwnerID, this);
        userview = userviews[OwnerID];
    }
    internal int selectedgun;
    [RPC]
    public void RPCSelectGun(int i)
    {        
        CallRPC(false, i);
        selectedgun = i;
        foreach (GunBase gb in guns)
            gb.DisableGun();
        guns[i].EnableGun();

    }
    [RPC]
    private void RPCSpec()
    {
        Show(false);
    }
    public override void Dispose()
    {
        players.Remove(networkView.owner.GetHashCode());
        base.Dispose();
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        box b = collisionInfo.gameObject.GetComponent<box>();
        if (b != null && isOwner && b.OwnerID != -1 && (b.isOwner || players[b.OwnerID].team != team || dm) &&
            !(b is Player) && !(b is Zombie) &&
            collisionInfo.impactForceSum.sqrMagnitude > 150 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
        {
            RPCSetLife(-Math.Min(80, (int)collisionInfo.impactForceSum.sqrMagnitude / 2), b.OwnerID);
        }
    }
    
    const int life = 100;
    [RPC]
    public override void RPCHealth()
    {
        CallRPC(false);
        if (Life < life)
            Life += 10;
        if (freezedt > 0)
            freezedt = 0;
        guns[0].bullets += 10;
    }

    [RPC]
    public override void RPCSetLife(int NwLife, int killedby)
    {
        CallRPC(false, NwLife, killedby);
        if (isOwner)
            blood.Hit(Mathf.Abs(NwLife) * 2);

        if (isEnemy(killedby))
        {
            if (killedby == -1 || dm)
                Life += NwLife;
            else
            {
                if (zombi)
                    freezedt -= NwLife;
                else if (tdm)
                    Life += NwLife;
            }
        }
        if (Life < 0 && isOwner)
            RPCDie(killedby);

    }
    [RPC]
    public override void RPCDie(int killedby)
    {
        CallRPC(false, killedby);
        Base a = ((Transform)Instantiate(bloodexp, transform.position, Quaternion.identity)).GetComponent<Base>();
        a.Destroy(10000);
        a.transform.parent = _Spawn.effects;
        userview.deaths++;
        if (isOwner)
        {
            if (!zombi && !zombisurive) _TimerA.AddMethod(10000, RPCSpawn);            
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                if (p.OwnerID == killedby)
                {
                    if (p.isOwner)
                    {
                        _cw.rpcwrite(_LocalPlayer.Nick + lc.dbsf);
                        _LocalPlayer.RPCSetFrags(-1);
                    }
                    else if (p.team != _LocalPlayer.team || dm)
                    {
                        rpcwrite(p.Nick + lc.kld + _LocalPlayer.Nick);
                        p.RPCSetFrags(+1);
                    }
                    else
                    {
                        rpcwrite(p.Nick + lc.ff + _LocalPlayer.Nick);
                        p.RPCSetFrags(-1);

                    }
                }
            }
            if (killedby == -1)
            {
                rpcwrite(_LocalPlayer.Nick + lc.scw.ToString());
                _LocalPlayer.RPCSetFrags(-1);
            }

            lockCursor = false;
        }
        Show(false);

    }

    [RPC]
    public void RPCSetFrags(int i)
    {
        CallRPC(false, i);
        frags += i;
    }
    public static Vector3 Clamp(Vector3 velocityChange, float maxVelocityChange)
    {
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }
    public override Vector3 SpawnPoint()
    {
        Transform t = _Spawn.transform.Find(team.ToString());

        return t.GetChild(UnityEngine.Random.Range(0, t.childCount)).transform.position;
    }
    [RPC]
    public void RPCCarIn()
    {
        CallRPC(false);

        Show(false);
    }
}
