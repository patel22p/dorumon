using System.Linq;
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
    public new Team team;
    public float force;
    public int score;
    public float freezedt; 
    public int guni;
    public int fps;
    public int ping;
    public int deaths;
    new public string nick;
    public bool spawned;
    public int frags;
    public ParticleEmitter speedparticles;
    const int life = 100;
    public Transform guntr;

    [GenerateEnums("GunType")]
    public List<GunBase> gunsR = new List<GunBase>();
    public List<GunBase> gunsL = new List<GunBase>();
    public List<GunBase> guns { get { return LeftGun ? gunsL : gunsR; } }
    public bool LeftGun = true;

    protected override void Awake() 
    {
        Debug.Log("player awake");
        guntr = this.transform.Find("Guns");
        gunsL = guntr.GetChild(1).GetComponentsInChildren<GunBase>().ToList();
        gunsR = guntr.GetChild(0).GetComponentsInChildren<GunBase>().ToList();
        print("guns count" + gunsL.Count);
        print("guns count" + gunsR.Count);
        if (!build)
        {
            foreach (var g in gunsL.Concat(gunsR))
                g.defcount = 999999;
        }
        
        this.rigidbody.maxAngularVelocity = 40;
        if (networkView.isMine)
        {
            _Game._localiplayer = _Game._localPlayer = this;
            RPCSetOwner();
            RPCSetUserInfo(LocalUserV.nick);
            this.RPCShow(false);
        }
        speedparticles = transform.Find("speedparticles").GetComponent<ParticleEmitter>();
        
        base.Awake();
    }

    
    protected override void Start()
    {
        base.Start();                
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        networkView.RPC("RPCSetUserInfo", np, nick);
        
        networkView.RPC("RPCSetFrags", np, frags,score);
        networkView.RPC("RPCSetDeaths", np, deaths);
        if (spawned)
        {
            networkView.RPC("RPCSetTeam", np, (int)team);        
            networkView.RPC("RPCSpawn", np);
            networkView.RPC("RPCSelectGun", np, selectedgun, LeftGun);        
            if (spawned && dead) networkView.RPC("RPCDie", np, -1);
            if (car != null) networkView.RPC("RPCCarIn", np);
        }
    }
    public override void OnSetOwner()
    {
        print("set owner" + OwnerID);
        if (isOwner)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
        _Game.players[OwnerID] = this;
        _Game.WriteMessage(nick + " зашел в игру ");
    }
    [RPC]
    public void RPCSpawn()
    {
        spawned = true;
        print(pr);
        CallRPC();
        
        Show(true);
        if (isOwner)
        {
            RPCSetTeam((int)team);
            RPCSelectGun(1, true);
            RPCSelectGun(1, false);
            transform.position = SpawnPoint();
            transform.rotation = Quaternion.identity;
        }
        foreach (GunBase gunBase in gunsR.Concat(gunsL))
            gunBase.Reset();

        Life = life;
        freezedt = 0;

    }
    public override  Vector3 SpawnPoint()
    {        
        GameObject[] gs = GameObject.FindGameObjectsWithTag(team.ToString());
        return gs.OrderBy(a => Vector3.Distance(a.transform.position, transform.position)).First().transform.position;        
    }
    public void SelectGun(int id)
    {
        if (guns.Count(a => a.group == id && a.patronsleft > 0) == 0) return;
        bool foundfirst = false;
        bool foundnext=false;
        for (int i = selectedgun; i < guns.Count; i++)
            if (guns[i].group == id && guns[i].patronsleft > 0)
            {
                if (foundfirst) { selectedgun = i; foundnext = true; break; }
                foundfirst = true;
            }
        if (!foundnext)
            for (int i = 0; i < guns.Count; i++)
                if (guns[i].group == id && guns[i].patronsleft > 0)
                {
                    selectedgun = i;
                    break;
                }
        
        RPCSelectGun(selectedgun,LeftGun);
    }
    [RPC]
    public void RPCSelectGun(int i,bool left)
    {
        CallRPC(i,left);
        PlaySound("change");
        selectedgun = i;
        foreach (GunBase gb in (left ? gunsL : gunsR))
            gb.DisableGun();

        (left ? gunsL : gunsR)[selectedgun].EnableGun();
    }
    protected override void Update()
    {
        UpdateOnPoint();

        if (DebugKey(KeyCode.Keypad1))
            RPCSetLife(-1, -1);
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
            //NextGun(Input.GetAxis("Mouse ScrollWheel"));
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SelectGun(1);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                SelectGun(2);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                SelectGun(3);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                SelectGun(4);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                SelectGun(5);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                SelectGun(6);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                SelectGun(7);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                SelectGun(8);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                SelectGun(9);
            if (Input.GetKeyDown(KeyCode.Q))
                LeftGun = true;
            if (Input.GetKeyDown(KeyCode.E))
                LeftGun = false;
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
        UpdateAim();
        base.Update();
    }
    protected virtual void UpdateAim()
    {
        if (isOwner) syncRot = _Cam.transform.rotation;
        guntr.rotation = syncRot;
    }
    private void UpdateOnPoint()
    {
        _GameWindow.CenterText.text = "";
        Collider c = RayCast(1 << LayerMask.NameToLayer("Level"), 10).collider;
        if (c != null)
        {
            MapItem item = c.gameObject.transform.parent.GetComponent<MapItem>() ?? c.gameObject.transform.GetComponent<MapItem>();
            if (item != null && item.enabled)
            {
                _GameWindow.CenterText.text = item.title();
                if (Input.GetKeyDown(KeyCode.B) && (score > item.score || !build))
                {
                    
                    item.CheckOut();
                }
            }
        }
    }
    protected virtual void FixedUpdate()
    {
        if (isOwner) LocalMove();
        UpdateAim();
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
                this.rigidbody.AddForce(moveDirection * Time.fixedDeltaTime * force * 7);                
                v.x *= .65f;
                v.z *= .65f;
                this.rigidbody.velocity = v;
            }
            else
                this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.fixedDeltaTime * 300);
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
            if (guni > guns.Count - 1) guni = 0;
            if (guni < 0) guni = guns.Count - 1;
            RPCSelectGun(guni,LeftGun);
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
    public void RPCSetDeaths(int d) { deaths = d; }
    public override bool dead { get { return !enabled && spawned && car == null; } }
     
    [RPC]
    private void RPCSpec()
    {
        Show(false);
    }
    
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (isOwner && collisionInfo.relativeVelocity.y > 30)
            RPCPowerExp(this.transform.position);

        Box b = collisionInfo.gameObject.GetComponent<Box>();
        if (b != null && isOwner && b.OwnerID != -1 && (b.isOwner || players[b.OwnerID].team != team || mapSettings.DM) &&
            !(b is Player) && !(b is Zombie) &&
            collisionInfo.rigidbody.velocity.magnitude > 20
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
        _Cam.exp = 2;                
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
        

        if (isEnemy(killedby) || NwLife > Life)
            Life = Math.Min(NwLife,100);

        if (Life <= 0 && isOwner)
            RPCDie(killedby);

    }

    [RPC]
    private void RPCSetUserInfo(string nick)
    {

        CallRPC(nick);
        this.nick = nick;
    }

    [RPC]
    public override void RPCDie(int killedby)
    {
        print(pr);
        CallRPC(killedby);
        Instantiate(Resources.Load("Detonator/Prefab Examples/Detonator-Chunks"), transform.position, Quaternion.identity);
        deaths++;
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
                        _localPlayer.SetFrags(-1,-5);
                    }
                    else if (p.team != _localPlayer.team || mapSettings.DM)
                    {
                        _Game.RPCWriteMessage(p.nick + " Убил " + _localPlayer.nick);
                        p.SetFrags(+1,20);
                    }
                    else
                    {
                        _Game.RPCWriteMessage(p.nick + " Убил союзника " + _localPlayer.nick);
                        p.SetFrags(-1,-10);

                    }
                }
            }
            if (killedby == -1)
            {
                _Game.RPCWriteMessage(_localPlayer.nick + " Погиб ");
                _localPlayer.SetFrags(-1,-5);
            }

            lockCursor = false;
        }
        Show(false);

    }
    
    float multikilltime;
    int multikill;
    public void SetFrags(int i,int sc)
    {
        RPCSetFrags(frags + i, score + sc);
    }
    [RPC]
    public void RPCSetFrags(int i, int sc)
    {
        CallRPC(i, sc);

        if (isOwner)
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
                    _Cam.ScoreText.text = "x" + (multikill + 1);
                    _Cam.ScoreText.animation.Play();
                }
            }
        }


        frags = i;
        score = sc;
    

    }
    public static Vector3 Clamp(Vector3 velocityChange, float maxVelocityChange)
    {

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {
            //if (stream.isReading || this.GetType() != typeof(Zombie) || tsendpackets<0)
            lock ("ser")
            {
                tsendpackets = 1;
                if (stream.isWriting)
                {
                    syncPos = rigidbody.position;
                    syncRot = guntr.rotation;
                    syncVelocity = rigidbody.velocity;
                    syncAngularVelocity = rigidbody.angularVelocity;
                }
                stream.Serialize(ref syncPos);
                stream.Serialize(ref syncVelocity);
                stream.Serialize(ref syncRot);
                stream.Serialize(ref syncAngularVelocity);

                if (stream.isReading)
                {
                    rigidbody.position = syncPos;
                    rigidbody.velocity = syncVelocity;
                    rigidbody.rotation = syncRot;
                    rigidbody.angularVelocity = syncAngularVelocity;
                }
            }
        }
    }

    [RPC]
    public void RPCCarIn()
    {
        CallRPC();
        Show(false);
    }
}
