using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;
public enum Team : int { Red, Blue, Spectator, None }
[Serializable]
public class Player : IPlayer
{
    public CarController car;
    public new Team team { get { return userView.team; } set { userView.team = value; } }
    public float force; 
    public float freezedt; 
    public int guni;
    new public string nick { get { return userView.nick; } } 
    public int frags { get { return userView.frags; } set { userView.frags = value; } }
    public UserView userView;
    public ParticleEmitter speedparticles;
    const int life = 100;
    protected override void Awake() 
    {  
        force = 400;
        speedparticles = transform.Find("speedparticles").GetComponent<ParticleEmitter>(); 
        base.Awake();
    }

    protected override void Start()
    {        
        
        base.Start();        
        if (networkView.isMine)
        {
            _Game._localiplayer = _Game._localPlayer = this;
            print(pr); 
            RPCSetOwner();
            RPCSpawn();            
        }
    }

    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        networkView.RPC("RPCSetTeam", np, (int)team);
        networkView.RPC("RPCSpawn", np);
        networkView.RPC("RPCSelectGun", np, selectedgun);
        networkView.RPC("RPCSetFrags", np, frags);
        networkView.RPC("RPCSetDeaths", np, userView.deaths);
        if (dead) networkView.RPC("RPCDie", np, -1);
        if (car != null) networkView.RPC("RPCCarIn", np);
    }
    
    public override void OnSetOwner()
    {
        print("set owner" + OwnerID);
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
        _Game.players[OwnerID] = this;
        userView = userViews[OwnerID];
        _Game.WriteMessage(userView.nick + " зашел в игру ");
    }

    [RPC]
    public void RPCSpawn()
    {
        print(pr);
        CallRPC();
        Show(true);
        if (isOwner)
        {
            RPCSelectGun(1);
            RPCSetTeam((int)_Game.team);
            transform.position = SpawnPoint();
            transform.rotation = Quaternion.identity;
        }
        foreach (GunBase gunBase in guns)
            gunBase.Reset();
        Life = life;
        freezedt = 0;

    }
    
    

    protected override void Update()
    {

        multikilltime-= Time.deltaTime;
        if (this.rigidbody.velocity.magnitude > 30)
        {
            speedparticles.worldVelocity = this.rigidbody.velocity / 10;
            if (_TimerA.TimeElapsed(100))
            {
                speedparticles.transform.rotation = Quaternion.identity;
                speedparticles.Emit();
            }
        }
        if (freezedt >= 0)
            freezedt -= Time.deltaTime * 5;
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
            if (Input.GetKeyDown(KeyCode.Alpha5))
                RPCSelectGun(4);
            if (Input.GetKey(KeyCode.LeftShift))
                this.transform.rotation = Quaternion.identity;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (nitro > 10 || !build)
                {
                    nitro -= 10;
                    RCPJump();
                }
            }            
        }
        base.Update();
    }
    protected virtual void FixedUpdate()
    {
        if (isOwner) LocalMove();
    }

    private void LocalMove()
    {

        if (lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = _Cam.transform.TransformDirection(moveDirection);
            moveDirection.y = 0;
            moveDirection.Normalize();

            Vector3 v = this.rigidbody.velocity;            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.rigidbody.angularVelocity = Vector3.zero;
                this.rigidbody.AddForce(moveDirection * Time.fixedDeltaTime * force * 15);                
                v.x *= .65f;
                v.z *= .65f;
                this.rigidbody.velocity = v;
            }
            else
            {
                this.rigidbody.maxAngularVelocity = v.magnitude / 1.1f;
                this.rigidbody.AddForce(moveDirection * Time.fixedDeltaTime * force * 2 * (freezedt > 0 ? .5f : 1));
            }
        }
    }
    [RPC]
    private void RCPJump()
    {
        CallRPC();
        transform.rigidbody.MovePosition(rigidbody.position + new Vector3(0, 5, 0));
        rigidbody.AddForce(_Cam.transform.rotation * new Vector3(0, 0, 1000));        
        PlaySound("nitrojump");
    }
    public void NextGun(float a)
    {
        if (a != 0)
        {                        
            if (a > 0)
                guni++;
            if (a < 0)
                guni--;
            if (guni > guns.Length - 1) guni = 0;
            if (guni < 0) guni = guns.Length - 1;
            RPCSelectGun(guni);
        }
    }
    
    
    [RPC]
    public void RPCSetTeam(int t)
    {
        print(pr);
        CallRPC(t); 
        team = (Team)t;
    }
    [RPC]
    public void RPCSetDeaths(int d) { LocalUserV.deaths = d; }
    public override bool dead { get { return !enabled && car == null; } }
     
    [RPC]
    private void RPCSpec()
    {
        Show(false);
    }
    public override void Dispose()
    {
        base.Dispose();
        players[networkView.owner.GetHashCode()] = null;
    }
    
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.relativeVelocity.y > 30)
            RPCPowerExp(this.transform.position);

        Box b = collisionInfo.gameObject.GetComponent<Box>();
        if (b != null && isOwner && b.OwnerID != -1 && (b.isOwner || players[b.OwnerID].team != team || mapSettings.DM) &&
            !(b is Player) && !(b is Zombie) &&
            collisionInfo.rigidbody.velocity.magnitude > 20
            //&& rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude
            )
        {
            RPCSetLife(Life - (int)collisionInfo.rigidbody.velocity.magnitude * 2, b.OwnerID);
        }
    }
    [RPC]
    private void RPCPowerExp(Vector3 v)
    {
        CallRPC(v);
        PlaySound("powerexp", 4);
        GameObject g = (GameObject)Instantiate(Load("wave"), v, Quaternion.Euler(90, 0, 0));
        Explosion e = g.AddComponent<Explosion>();
        e.OwnerID = OwnerID;
        e.self = this;
        e.exp = 2000;
        e.maxdistance= 200;        
        _Cam.ran = 1;                
        Destroy(g, 1.6f);
    }

    public override void Health()
    { 
        if (Life < life)
            Life += 10;
        RPCSetLife(Life, -1);
        if (freezedt > 0)
            freezedt = 0;
    }
    [RPC]
    public override void RPCSetLife(int NwLife, int killedby)
    {
        CallRPC(NwLife, killedby);
        if (isOwner)
            _GameWindow.Hit(Mathf.Abs(Life - NwLife) * 2);
        

        if (isEnemy(killedby))
            Life = NwLife;

        if (Life <= 0 && isOwner)
            RPCDie(killedby);

    }
    [RPC]
    public override void RPCDie(int killedby)
    {
        print(pr);
        CallRPC(killedby);
        Instantiate(Resources.Load("Detonator/Prefab Examples/Detonator-Chunks"), transform.position, Quaternion.identity);
        userView.deaths++;
        if (isOwner)
        {
            if (!mapSettings.TeamZombiSurvive && !mapSettings.ZombiSurvive) _TimerA.AddMethod(10000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                if (p.OwnerID == killedby)
                {
                    if (p.isOwner)
                    {

                        _Game.RPCWriteMessage(_localPlayer.nick + " Умер сам ");
                        _localPlayer.SetFrags(-1);
                    }
                    else if (p.team != _localPlayer.team || mapSettings.DM)
                    {
                        _Game.RPCWriteMessage(p.nick + " Убил " + _localPlayer.nick);
                        p.SetFrags(+1);
                    }
                    else
                    {
                        _Game.RPCWriteMessage(p.nick + " Убил союзника " + _localPlayer.nick);
                        p.SetFrags(-1);

                    }
                }
            }
            if (killedby == -1)
            {
                _Game.RPCWriteMessage(_localPlayer.nick + " Погиб ");
                _localPlayer.SetFrags(-1);
            }

            lockCursor = false;
        }
        Show(false);

    }
    
    float multikilltime;
    int multikill;
    public void SetFrags(int i)
    {
        if (multikilltime > 0)
            multikill++;
        else
            multikill = 0;
        multikilltime = 3;
        
        if (multikill >= 1)
        {
            PlayRandSound("toasty");
            if (isOwner)
            {
                _Cam.ScoreText.text = "x" + multikill;
                _Cam.ScoreText.animation.Play();
            }
        }
        RPCSetFrags(frags + i);
    }
    [RPC]
    public void RPCSetFrags(int i)
    {        
        CallRPC(i);
        frags = i;
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
        print(team);
        Transform t = _Game.transform.Find(team.ToString());
        return t.GetChild(UnityEngine.Random.Range(0, t.childCount)).transform.position;
    }
    [RPC]
    public void RPCCarIn()
    {
        CallRPC();
        Show(false);
    }
}
