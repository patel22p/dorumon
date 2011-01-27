using System.Linq;
using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public enum Team : int { Red, Blue, None }
interface IAim { void Aim(Player p); }
[Serializable]
public class Player : Destroible, IAim
{
    public MapItem mapItem;
    public float mapItemTm;
    public float MapItemInterval;
    float shownicktime;
    public List<Vector3> plPathPoints = new List<Vector3>();
    public TextMesh title;
    public float nitro;
    public new Team team = Team.None;
    public float speed;
    public float Score = 1;
    public bool haveLight;
    public bool haveTimeBomb;
    internal UserView user = new UserView();
    public int speedUpgrate;
    public int lifeUpgrate;
    public bool haveAntiGravitation;
    public int guni;
    public int fps;
    public int ping;
    public int deaths;
    public string nick { get { return user.nick; } }
    public bool spawned;
    public int frags;
    public float defMaxLife;
    [FindTransform("speedparticles")]
    public ParticleEmitter speedparticles;
    [FindTransform("Guns")]
    public Transform guntr;
    [GenerateEnums("GunType")]
    public List<GunBase> guns = new List<GunBase>();
    public int selectedgun;
    public GunBase gun { get { return guns[selectedgun]; } }
    public float defmass;
    [FindTransform("Sphere")]
    public GameObject model;
    public override void Init()
    {
        base.Init();
        guns = guntr.GetChild(0).GetComponentsInChildren<GunBase>().ToList();
        for (int i = 0; i < guns.Count; i++)
            guns[i].guntype = i;   
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
    public void RPCSetUserView(byte[] s) { CallRPC("SetUserView", s); }
    [RPC]
    public void SetUserView(byte[] data)
    {
        if (!isOwner)
            user = (UserView)Deserialize(data, UserView.xml);

        AliveMaterial = _Loader.playerTextures[user.MaterialId];
        Debug.Log("dsdsdsds" + model.renderer.material.name);
    }
    public override void Awake()
    {
        CanFreeze = mapSettings.freezeOnBite;
        AliveMaterial = model.renderer.sharedMaterial;
        Debug.Log("player awake");
        defmass = rigidbody.mass;
        defMaxLife = maxLife;        
        Score = _Loader.mapSettings.StartMoney + _Game.stage * 10 * _Game.scorefactor.Evaluate(_Game.stage);
        this.rigidbody.maxAngularVelocity = 3000;
        if (networkView.isMine)
        {
            _localPlayer = this;
            _Game.RPCWriteMessage("Player Connected: " + nick);            
            RPCSetOwner();
            user = _Loader.UserView;
            RPCSetUserView(Serialize(user, UserView.xml));
            ResetSpawn();
        }        
        base.Awake();
    }
    protected override void Start()
    {        
        base.Start();
    }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        base.OnPlayerConnectedBase(np);
        RPCSetUserView(Serialize(user, UserView.xml));
        RPCSetTeam((int)team);
        RPCSetAlive(Alive);
        RPCSetFrags(frags, Score);
        RPCSetDeaths(deaths);
        RPCSetFanarik(fanarik.enabled);
        RPCSelectGun(selectedgun);
        RPCSetLifeUpgrate(lifeUpgrate);
        RPCSetSpeedUpgrate(speedUpgrate);
        //if (spawned && dead) networkView.RPC("RPCDie", np, -1);
    }
    public override void OnSetOwner()
    {
        enabled = true;
        print("set owner" + OwnerID);
        if (isOwner)
            tag = name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
        _Game.players[OwnerID] = this;
        base.OnSetOwner();
    }
    public override void ResetSpawn()
    {
        Debug.Log(name + "Reset Spawn");
        base.ResetSpawn();
        Debug.Log((team + "spawn"));
        RPCSetAlive(false);
        transform.position = _Game.spawns.Where(a => a.SpawnType.ToLower() == (team + "spawn").ToLower()).Random().transform.position;
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
    [FindAsset("change")]
    public AudioClip changeSound;
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
        maxLife = defMaxLife + (lifeUpgrate * 100);
        if (!Alive && fanarik.enabled) fanarik.enabled = false;
        UpdateAim();

        if (!isOwner)
            UpdateTitle();

        if (shift)
            this.transform.rotation = Quaternion.identity;

        if (_TimerA.TimeElapsed(100))
        {
            if (plPathPoints.Count == 0 || Vector3.Distance(pos, plPathPoints.Last()) > 1)
            {
                plPathPoints.Add(pos);
                if (plPathPoints.Count > 20) plPathPoints.RemoveAt(0);
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

        if (isOwner)
            LocalUpdate();
        base.Update();
        
    }
    public bool shift;
    public void RPCSetStreff(bool value) { CallRPC("SetStreff", value); }
    [RPC]
    public void SetStreff(bool value)
    {
        shift = value;
    }
    private void LocalUpdate()
    {

        UpdateMapItems();

        nitro += Time.deltaTime / 5;
        if (lockCursor && Alive)
        {
            bool sh = Input.GetKey(KeyCode.LeftShift);
            if (sh != shift) RPCSetStreff(sh);
            if (Input.GetAxis("Mouse ScrollWheel") > 0) PrevGun();
            if (Input.GetAxis("Mouse ScrollWheel") < 0) NextGun();

            if (_TimerA.TimeElapsed(500) && Input.GetKey(KeyCode.H))
                foreach (var a in Nearest())
                    if (Input.GetKey(KeyCode.H) && a.Life < a.maxLife)
                        a.RPCHeal(20);

            foreach (var a in Nearest())
                if (Input.GetKeyDown(KeyCode.G) && a is Player)
                {
                    var p = ((Player)a);
                    if (Score >= 1)
                    {
                        p.RPCGiveMoney(1);
                        Score -= 1;
                    }
                }
            SelectGun();
            if (Input.GetKeyDown(KeyCode.Y) && (haveAntiGravitation || debug))
            {
                haveAntiGravitation = false;
                _TimerA.AddMethod(15000, delegate { _Game.RPCSetGravityBomb(false); });
                _Game.RPCSetGravityBomb(true);
            }
            if (Input.GetKeyDown(KeyCode.T) && (haveTimeBomb || debug))
            {
                haveTimeBomb = false;
                _TimerA.AddMethod(10000, delegate { _Game.RPCSetTimeBomb(1); });
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

    private IEnumerable<Destroible> Nearest()
    {
        return players.Union(_Game.towers.Cast<Destroible>()).Where(a => a != null && a != this && Vector3.Distance(a.pos, pos) < 10);
    }

    private void UpdateMapItems()
    {
        var mi = _Game.mapitems.Where(a => Vector3.Distance(a.pos, pos) < a.Distance && a.enabled).OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
        if (mi != null)
        {
            mapItem = mi;
            mapItemTm = .5f;
        }
        MapItemInterval -= Time.deltaTime;
        mapItemTm -= Time.deltaTime;
        if (mapItemTm > 0 && MapItemInterval < 0)
        {
            _GameWindow.CenterText.text = mapItem.text + (mapItem.Score > 0 ? (", costs " + mapItem.Score + " Money") : "");
            if ((Input.GetKeyDown(KeyCode.F) || mapItem.autoTake) && (Score >= mapItem.Score - 1 || debug) && mapItem.Check())
                mapItem.LocalCheckOut();
        }
        else
            _GameWindow.CenterText.text = "";

    }
    private void UpdateTitle()
    {
        if (OwnerID != -1 && (team == Team.Red || team == Team.Blue))
            title.renderer.material.color = (team == Team.Red ? Color.red : Color.blue) * 1f;
        else
            title.renderer.material.color = Color.white * 1;

        if (shownicktime > 0 || !_localPlayer.isEnemy(OwnerID))
            title.text = nick + ":" + Life;
        else
            title.text = "";

        shownicktime -= Time.deltaTime;
    }
    public void Aim(Player p)
    {
        if (p.isOwner)
            shownicktime = 3;
    }
    public void RPCSetFanarik(bool v) { CallRPC("SetFanarik", v); }
    [RPC]
    public void SetFanarik(bool value)
    {
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
        if (Alive)
        {
            if (isOwner) syncRot = _Cam.transform.rotation;
            guntr.rotation = syncRot;
            

            Ray r = gun.GetRay();
            RaycastHit h = new RaycastHit() { point = r.origin + r.direction * 100 };
            if (Physics.Raycast(r, out h, 100, ~(1 << LayerMask.NameToLayer("Glass"))))
            {
                var aim = h.collider.gameObject.transform.GetMonoBehaviorInParrent() as IAim;
                if (aim != null)
                    aim.Aim(this);
            }

            if ((gun.laser || debug) && selectedgun != (int)GunType.physxgun)
            {
                laserRender.enabled = true;
                laserRender.SetPosition(0, r.origin);
                laserRender.SetPosition(1, h.point);
            }
            else
                laserRender.enabled = false;
        }
        else
            laserRender.enabled = false;
    }
    Vector3 moveForce;
    protected virtual void FixedUpdate()
    {
        moveForce *= .80f;
        if (isOwner && lockCursor)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); 
            moveDirection = _Cam.transform.TransformDirection(moveDirection);
            if (Physics.gravity == _Game.gravity)
                moveDirection.y = 0;
            moveDirection.Normalize();            
            Vector3 v = this.rigidbody.velocity;
            if (shift && !frozen && (nitro > 0 || !build))
            {
                moveForce += moveDirection * Time.deltaTime * 8; //forcemove
                this.rigidbody.angularVelocity = Vector3.zero;
                this.rigidbody.AddForce(moveForce * fdt * speed * 450);
                v.x *= 0;
                v.z *= 0;
                nitro -= Time.deltaTime * 1.5f;
                if (Physics.gravity != _Game.gravity)
                    v.y *= 0;
                this.rigidbody.velocity = v;
            }
            else
            {
                
                this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * speed * 5);
            }
            if (frozen) this.rigidbody.velocity *= .85f;
        }
    }

    public void RPCJump() { CallRPC("Jump"); }
    [RPC]
    public void Jump()
    {
        transform.rigidbody.MovePosition(rigidbody.position + new Vector3(0, 1, 0));
        rigidbody.AddForce(_Cam.transform.rotation * new Vector3(0, 0, 1000) * rigidbody.mass * fdt);
        PlaySound(nitrojumpSound);
    }

    public void PrevGun()
    {

        for (int i = selectedgun - 1; i >= 0; i--)
        {
            if (guns[i].patronsLeft > 0 || debug)
            {
                RPCSelectGun(i);
                return;
            }
        }
        for (int i = guns.Count - 1; i > 0; i--)
        {
            if (guns[i].patronsLeft > 0 || debug)
            {
                RPCSelectGun(i);
                return;
            }
        }
    }
    public void NextGun()
    {

        for (int i = selectedgun + 1; i < guns.Count; i++)
        {
            if (guns[i].patronsLeft > 0 || debug)
            {
                RPCSelectGun(i);
                return;
            }
        }
        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i].patronsLeft > 0 || debug)
            {
                RPCSelectGun(i);
                return;
            }
        }
    }
    public void RPCSetTeam(int t) { CallRPC("SetTeam", t); }
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

        if (isOwner && collisionInfo.relativeVelocity.y > 20)
            RPCPowerExp(this.transform.position);
        base.OnCollisionEnter(collisionInfo);
    }
    [FindAsset("powerexp")]
    public AudioClip powerexpSound;
    [FindAsset("wave")]
    public GameObject WavePrefab;
    [FindAsset("bowling")]
    public AudioClip bowling;
    [FindAsset("nitrojump")]
    public AudioClip nitrojumpSound;
    [FindAsset]
    public AudioClip givemoney;
    [FindAsset]
    public GameObject Explosion;
    public void RPCPowerExp(Vector3 v) { CallRPC("PowerExp", v); }
    [RPC]
    public void PowerExp(Vector3 v)
    {
        PlaySound(powerexpSound, 4);

        GameObject g = (GameObject)Instantiate(WavePrefab, v, Quaternion.Euler(90, 0, 0));
        GameObject exp = (GameObject)Instantiate(Explosion, v, Quaternion.identity);
        exp.transform.parent = g.transform;
        var e = exp.GetComponent<Explosion>();
        e.OwnerID = OwnerID;
        e.self = this;
        e.exp = 5000;
        e.radius = 10;

        if (isOwner)
            _Cam.exp = 2;

        Destroy(g, 1.6f);
    }
    [FindAsset("Detonator-Base")]
    public GameObject detonator;
    [RPC]
    public override void Die(int killedby)
    {
        if (!Alive) return;
        Destroy(Instantiate(detonator, transform.position, Quaternion.identity), .4f);
        deaths++;
        if (killedby == _localPlayer.OwnerID)
        {
            if (OwnerID == _localPlayer.OwnerID)
            {
                _Game.WriteMessage(_localPlayer.nick + " Killed self ");
                _localPlayer.AddFrags(-1, -.5f);
            }
            else if (team != _localPlayer.team || mapSettings.DM)
            {
                _Game.WriteMessage(_localPlayer.nick + " kill " + nick);
                _localPlayer.AddFrags(+1, 2);
            }
            else
            {
                _Game.WriteMessage(_localPlayer.nick + " kill " + nick);
                _localPlayer.AddFrags(-1, -1);
            }
        }
        if (killedby == -1)
        {
            _Game.WriteMessage(nick + " died ");
        }

        if (isOwner)
        {
            if (!mapSettings.zombi) _TimerA.AddMethod(10000, delegate { RPCSetAlive(true); });
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
        _TimerA.AddMethod(delegate
        {
            foreach (var t in GetComponentsInChildren<Transform>())
                if (value)
                    SetLayer(t.gameObject);
                else
                    t.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        });
        Alive = value;

        RPCSetFanarik(false);
        if (value)
            spawned = true;
        model.renderer.sharedMaterial = value ? AliveMaterial : deadMaterial;
        foreach (GunBase gunBase in guns.Concat(guns))
            gunBase.Reset();
        if (isOwner)
            LocalSelectGun(1);
        Life = maxLife;
        RPCSetFrozen(false);

    }
    float multikilltime;
    int multikill;
    public void AddFrags(int i, float sc)
    {
        if (isOwner)
        {
            if (multikilltime > 0)
                multikill += i;
            else
                multikill = 0;
            multikilltime = 1;

            if (multikill >= 1)
            {
                if (gun is GunPhysix)
                {
                    if (!audio.isPlaying)
                    {
                        audio.clip = bowling;
                        audio.volume = 3;
                        audio.Play();
                    }
                }
                else
                    PlayRandSound(multikillSounds, 5);

                _Cam.ScoreText.text = "x" + (multikill + 1);
                _Cam.ScoreText.animation.Play();
            }
            frags += i;
            Score += sc;
        }
        RPCSetFrags(frags, Score);
    }
    [FindAsset("toasty")]
    public AudioClip[] multikillSounds;
    public void RPCSetFrags(int i, float score) { CallRPC("SetFrags", i, score); }
    [RPC]
    public void SetFrags(int i, float sc)
    {
        frags = i;
        Score = sc;
    }
    public void RPCSetSpeedUpgrate(int value) { CallRPC("SetSpeedUpgrate", value); }
    [RPC]
    public void SetSpeedUpgrate(int value)
    {
        speedUpgrate = value;
    }

    public void RPCSetLifeUpgrate(int value) { CallRPC("SetLifeUpgrate", value); }
    [RPC]
    public void SetLifeUpgrate(int value)
    {
        lifeUpgrate = value;
    }
    public void RPCGiveMoney(int money) { CallRPC("GiveMoney", money); }
    [RPC]
    public void GiveMoney(int money)
    {
        PlaySound(givemoney);
        if (isOwner)
            RPCSetFrags(frags, Score + money);
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
    public override void RPCSetLife(float NwLife, int killedby)
    {
        base.RPCSetLife(NwLife, killedby);
    }
    [RPC]
    public override void SetLife(float NwLife, int killedby)
    {
        base.SetLife(NwLife, killedby);
    }
}