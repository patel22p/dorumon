using System.Linq;
using UnityEngine;


using System.Collections;
using doru;
using System.Collections.Generic;
using System;
public enum Team : int { Red, Blue, None }
[Serializable]
public class Player : Destroible
{
    float shownicktime;
    public List<Vector3> plPathPoints = new List<Vector3>();
    public TextMesh title;
    public float nitro;
    public new bool dead { get { return !Alive && spawned; } }
    public new Team team = Team.None;
    public float force;
    public int score;
    public bool haveLight;
    public bool haveTimeBomb;
    public float freezedt;
    public int guni;
    public int fps;
    public int ping;
    public int deaths;
    new public string nick;
    public bool spawned;
    public int frags;
    [PathFind("speedparticles")]
    public ParticleEmitter speedparticles;
    [PathFind("Guns")]
    public Transform guntr;
    [GenerateEnums("GunType")]
    public List<GunBase> guns = new List<GunBase>();
    public int selectedgun;
    public GunBase gun { get { return guns[selectedgun]; } }
    public float defmass;
    [PathFind("Sphere")]
    public GameObject model;
    public override void Init()
    {        
        base.Init();
        gameObject.layer = LayerMask.NameToLayer("Player");
        guns = guntr.GetChild(0).GetComponentsInChildren<GunBase>().ToList();
        shared = false;
        title = transform.GetComponentInChildren<TextMesh>();
        laserRender = root.GetComponentInChildren<LineRenderer>();        
        fanarik = this.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Spot);
        nitro = 10;
    }
    void OnCollisionStay(Collision collisionInfo)
    {
        isGrounded = 0;
    }

    protected override void Awake()
    {
        
        AliveMaterial = model.renderer.sharedMaterial;        
        Debug.Log("player awake");
        defmass = rigidbody.mass;
        this.rigidbody.maxAngularVelocity = 3000;
        if (networkView.isMine)
        {
            RPCSetOwner();
            RPCSetUserInfo(LocalUserV.nick);
            RPCSpawn();
        }
        //speedparticles = transform.Find("speedparticles").GetComponent<ParticleEmitter>();

        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);        
        RPCSetUserInfo(nick);
        RPCSetFrags(frags, score);
        RPCSetDeaths(deaths);
        RPCSetTeam((int)team);
        RPCSpawn();        
        RPCSetAlive( Alive);
        RPCSetFanarik(fanarik.enabled);
        RPCSelectGun( selectedgun);        
        //if (spawned && dead) networkView.RPC("RPCDie", np, -1);
    }
    public override void OnSetOwner()
    {
        print("set owner" + OwnerID);
        if (isOwner)
        {
            tag = name = "LocalPlayer";
        }
        else
            name = "RemotePlayer" + OwnerID;
        _Game.players[OwnerID] = this;        
    }
    public void RPCSpawn() { CallRPC("Spawn"); }
    [RPC]
    public void Spawn()
    {        
        print(pr + "+" + OwnerID);        
        if (isOwner)
        {
            RPCSetTeam((int)team);
            RPCSetAlive(Alive);
            ResetSpawn();
        }        
    }
    public override void ResetSpawn()
    {
        base.ResetSpawn();
        transform.position = GameObject.FindGameObjectWithTag("Spawn" + team.ToString()).transform.position;
        transform.rotation = Quaternion.identity;
    }
    public void LocalSelectGun(int id)
    {
        if (guns.Count(a => a.group == id && a.patronsLeft > 0) == 0 && !debug) return;
        bool foundfirst = false;
        bool foundnext = false;
        for (int i = selectedgun; i < guns.Count; i++)
            if (guns[i].group == id && (guns[i].patronsLeft > 0 || debug))
            {
                if (foundfirst) { selectedgun = i; foundnext = true; break; }
                foundfirst = true;
            }
        if (!foundnext)
            for (int i = 0; i < guns.Count; i++)
                if (guns[i].group == id && (guns[i].patronsLeft > 0 || debug))
                {
                    selectedgun = i;
                    break;
                }
        
        RPCSelectGun(selectedgun);
    }
    [LoadPath("change")]
    public AudioClip changeSound;
    public static Transform Root(string tag,Transform t2)
    {
        Transform t = t2;
        while(t.parent != null && t.parent.tag != tag)
            t = t.parent;

        return t;
    }
    public void RPCSelectGun(int i) { CallRPC("SelectGun", i); }
    [RPC]
    public void SelectGun(int i)
    {        
        PlaySound(changeSound);
        selectedgun = i;
        foreach (GunBase gb in guns)
            gb.DisableGun();


        if (Alive)
            guns[selectedgun].EnableGun();
    }

    protected override void Update()
    {
      
        if (!Alive && fanarik.enabled) fanarik.enabled = false;
        UpdateAim();
        if (isOwner)
            nitro += Time.deltaTime / 5;
        
        UpdateTitle();

        if (_TimerA.TimeElapsed(100))
        {
            if (plPathPoints.Count == 0 || Vector3.Distance(pos, plPathPoints.Last()) > 1)
            {
                plPathPoints.Add(pos);
                if (plPathPoints.Count > 10) plPathPoints.RemoveAt(0);
            }
        }
        
        
        multikilltime -= Time.deltaTime;
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
            freezedt -= Time.deltaTime;
        LocalUpdate();
        base.Update();
        //UpdateLightmap(model.renderer.materials);
    }



    private void LocalUpdate()
    {
        if (isOwner && lockCursor && Alive)
        {
            //NextGun(Input.GetAxis("Mouse ScrollWheel"));
            if (_TimerA.TimeElapsed(200))
            {
                if (Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.G))
                    foreach (var a in players.Union(_Game.towers.Cast<Destroible>()).Where(a => a != null && a != this && Vector3.Distance(a.pos, pos) < 10))
                    {
                        if (Input.GetKey(KeyCode.H) && a.Life < a.maxLife)
                        {
                            a.RPCSetLife(a.Life + 2, -1);
                        }
                        if (Input.GetKey(KeyCode.G) && a is Player)
                        {
                            var p = ((Player)a);
                            if (score > 10)
                            {
                                p.RPCSetFrags(p.frags, p.score + 10);
                                score -= 10;
                            }
                        }
                    }
            }

            SelectGun();
            if (Input.GetKey(KeyCode.LeftShift))
                this.transform.rotation = Quaternion.identity;
            if (Input.GetKeyDown(KeyCode.T) && (haveTimeBomb || debug))
            {
                _TimerA.AddMethod(5000, delegate { _Game.RPCSetTimeBomb(1); });
                _Game.RPCSetTimeBomb(Time.timeScale * 0.5f);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (nitro > 10 && isGrounded < 1 || !build)
                {
                    nitro -= 10;
                    RPCJump();
                }
            }
            if ((haveLight || debug) && Input.GetKeyDown(KeyCode.R))
            {
                RPCSetFanarik(!fanarik.enabled);                
            }
        }
    }
    private void UpdateTitle()
    {
        if (OwnerID != -1 && (team == Team.Red || team == Team.Blue))
            title.renderer.material.color = team == Team.Red ? Color.red : Color.blue;
        else
            title.renderer.material.color = Color.white;
        if (shownicktime > 0)
            title.text = nick + ":" + Life;
        else
            title.text = "";

        shownicktime -= Time.deltaTime;
    }

    public void RPCSetFanarik(bool v) { CallRPC("SetFanarik",v); }
    [RPC]
    public void SetFanarik(bool value)
    {
        if(value) haveLight = true;
        fanarik.enabled = value;
    }
    public Light fanarik;
    private void SelectGun()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            LocalSelectGun(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            LocalSelectGun(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            LocalSelectGun(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            LocalSelectGun(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            LocalSelectGun(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            LocalSelectGun(6);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            LocalSelectGun(7);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            LocalSelectGun(8);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            LocalSelectGun(9);
    }
    public LineRenderer laserRender;

    public void UpdateAim()
    {
        if (isOwner) syncRot = _Cam.transform.rotation;
        guntr.rotation = syncRot;

        Ray r = new Ray(gun.cursor[0].position, gun.rot * new Vector3(0, 0, 1));
        RaycastHit h = new RaycastHit() { point = r.origin + r.direction * 100 };
        Physics.Raycast(r, out h, 100);        
        if ((gun.laser || debug) && Alive && selectedgun != (int)GunType.physxgun)
        {
            laserRender.enabled = true;
            laserRender.SetPosition(0, r.origin);
            laserRender.SetPosition(1, h.point);
        }
        else
            laserRender.enabled = false;


    }
    
    protected virtual void FixedUpdate()
    {
        if (isOwner) FixedLocalMove();
        //UpdateAim();
    }
    private void FixedLocalMove()
    {
        if (lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = _Cam.transform.TransformDirection(moveDirection);
            moveDirection.y = 0;
            moveDirection.Normalize();


            Vector3 v = this.rigidbody.velocity;
            float slw = freezedt > 0 ? .5f : 1f;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.rigidbody.angularVelocity = Vector3.zero;
                this.rigidbody.AddForce(moveDirection * 0.02f / Time.timeScale * force * 7*slw);
                v.x *= .65f;
                v.z *= .65f;
                this.rigidbody.velocity = v;
            }
            else
            {
                this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * 0.02f / Time.timeScale * 300 * slw);
            }
        }
    }
    public void RPCJump() { CallRPC("Jump"); }
    [RPC]
    public void Jump()
    {        
        transform.rigidbody.MovePosition(rigidbody.position + new Vector3(0, 1, 0));
        rigidbody.AddForce(_Cam.transform.rotation * new Vector3(0, 0, 1000) / Time.timeScale);
        PlaySound(nitrojumpSound);
    }
    [LoadPath("nitrojump")]
    public AudioClip nitrojumpSound;
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
            RPCSelectGun(guni);
        }
    }

    public void RPCSetTeam(int t) { CallRPC("SetTeam",t); }
    [RPC]
    public void SetTeam(int t)
    {
        print(pr);        
        team = (Team)t;
    }
    [RPC]
    public void RPCSetDeaths(int d) { deaths = d; }
    
    
    
    protected override void OnCollisionEnter(Collision collisionInfo)
    {        
        if (!Alive) return;
        
        if (isOwner && collisionInfo.relativeVelocity.y > 30)
            RPCPowerExp(this.transform.position);
        base.OnCollisionEnter(collisionInfo);
    }
    [LoadPath("powerexp")]
    public AudioClip powerexpSound;
    [LoadPath("wave")]
    public GameObject WavePrefab;
    [LoadPath("bowling")]
    public AudioClip bowling;
    public void RPCPowerExp(Vector3 v) { CallRPC("PowerExp",v); }        
    [RPC]
    public void PowerExp(Vector3 v)
    {        
        PlaySound(powerexpSound, 4);
        
        GameObject g = (GameObject)Instantiate(WavePrefab, v, Quaternion.Euler(90, 0, 0));
        Explosion e = g.AddComponent<Explosion>();
        e.OwnerID = OwnerID;
        e.self = this;
        e.exp = 3000;
        e.radius = 8;
        e.damage = 200;
        if(isOwner)
            _Cam.exp = 2;
        
        Destroy(g, 1.6f);
    }


    [RPC]
    public override void SetLife(int NwLife, int killedby)
    {
        if (!Alive) return;
        if (isOwner)
            _GameWindow.Hit(Mathf.Abs(Life - NwLife) * 2);

        freezedt = (Life - NwLife) / 20;

        if (isEnemy(killedby) || NwLife > Life)
            Life = Math.Min(NwLife, 100);

        if (Life <= 0 && isOwner)
            RPCDie(killedby);

    }


    public void RPCSetUserInfo(string nick) { CallRPC("SetUserInfo", nick); }
    [RPC]
    public void SetUserInfo(string nick)
    {        
        this.nick = nick;
    }
    [LoadPath("Detonator-Base")]
    public GameObject detonator;

    [RPC]
    public override void Die(int killedby)
    {
        
        if (!Alive) return;
        print(pr);
        Detonator dt = this.detonator.GetComponent<Detonator>();
        dt.autoCreateForce = false;
        dt.size = 3;
        Instantiate(dt, transform.position, Quaternion.identity);
        var exp = dt.gameObject.AddComponent<Explosion>();
        exp.self = this;

        deaths++;
        if (isOwner)
        {
            if (!mapSettings.zombi) _TimerA.AddMethod(10000, delegate { RPCSetAlive(true); });
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                if (p.OwnerID == killedby)
                {
                    if (p.isOwner)
                    {
                        _Game.RPCWriteMessage(_localPlayer.nick + " Killed self ");
                        _localPlayer.AddFrags(-1, -5);
                    }
                    else if (p.team != _localPlayer.team || mapSettings.DM)
                    {
                        _Game.RPCWriteMessage(p.nick + " kill " + _localPlayer.nick);
                        p.AddFrags(+1, 20);
                    }
                    else
                    {
                        _Game.RPCWriteMessage(p.nick + " kill " + _localPlayer.nick);
                        p.AddFrags(-1, -10);
                    }
                }
            }
            if (killedby == -1)
            {
                _Game.RPCWriteMessage(_localPlayer.nick + " died ");
                _localPlayer.AddFrags(-1, -5);
            }
            lockCursor = false;
            RPCSetAlive(false);
        }
    }
    public Material AliveMaterial;
    public Material deadMaterial;


    public void RPCSetAlive(bool v) { CallRPC("SetAlive", v); }
    [RPC]
    public void SetAlive(bool value)
    {
        Debug.Log(name + " Alive " + value);
        foreach (var t in GetComponentsInChildren<Transform>())
            t.gameObject.layer = value ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("DeadPlayer");

        Alive = value;
        RPCSetFanarik(false);
        if(value)
            spawned = true;
        model.renderer.sharedMaterial = value? AliveMaterial:deadMaterial;                
        foreach (GunBase gunBase in guns.Concat(guns))
            gunBase.Reset();
        if (isOwner)
            LocalSelectGun(1);
        Life = maxLife;
        freezedt = 0;

    }

    float multikilltime;
    int multikill;
    public void AddFrags(int i, int sc)
    {
        RPCSetFrags(frags + i, score + sc);
    }
    [LoadPath("toasty")]
    public AudioClip[] multikillSounds;

    public void RPCSetFrags(int i, int sc) { CallRPC("SetFrags",i, sc); }
    [RPC]
    public void SetFrags(int i, int sc)
    {
        if (isOwner)
        {
            if (multikilltime > 0)
                multikill += (i - frags);
            else
                multikill = 0;
            multikilltime = 1;


            if (multikill >= 1)
            {
                if (gun is GunPhysix && multikill > 3)
                    PlaySound(bowling, 3);
                else
                    PlayRandSound(multikillSounds, 5);
                
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

    
    public override Quaternion rot
    {
        get
        {
            return guntr.rotation;
        }
        set
        {
            guntr.rotation = value;
        }
    }

}