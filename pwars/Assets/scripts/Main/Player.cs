using System.Linq;
using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public enum Team 
{
    Red,
    Blue,
    None
}
public class Player : Destroible
{
    private float multikilltime;
    private int multikill;
    
    private int selectedgun;
    private float defMaxLife;
    internal MapItem mapItem;
    internal float mapItemTm;
    internal float MapItemInterval;
    internal List<Vector3> plPathPoints = new List<Vector3>();
    internal new Team team = Team.None;
    internal float Score = 1;
    internal bool haveLight;
    internal bool haveTimeBomb;
    internal UserView user = new UserView();
    internal bool haveAntiGravitation;
    internal int fps;
    internal int ping;
    internal int errors;
    internal int deaths;
    internal bool spawned;
    internal int frags;
    internal float defmass = 1;
    public float Energy = 30;
    public int SpeedUpgrate=1;
    public int LifeUpgrate=1;
    public int PowerUpgrate=1;
    
    [FindAsset] public AudioClip death;
    [FindAsset] public AudioClip alive;
    [FindAsset] public AudioClip ForceField;
    
    [FindTransform("speedparticles")] public ParticleEmitter speedparticles;
    [FindTransform("Guns")] public Transform guntr;
    [FindAsset] public GameObject staticField;
    [GenerateEnums("GunType")] public List<GunBase> guns = new List<GunBase>();
    [FindAsset]
    public GameObject Turret;

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
        this.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Point).enabled = false;         
        Energy = 10;
    }
    public override void Awake()
    {
        Debug.Log("player awake " + networkView.isMine);
        defMaxLife = MaxLife;
        defmass = rigidbody.mass;
        if (networkView.isMine)
        {
            _localPlayer = this;
            RPCSetOwner();
            user = _Loader.UserView;
            RPCSetUserView(Serialize(user, UserView.xml));
            _Game.RPCWriteMessage("Player Connected: " + nick);
            ResetSpawn();
        }
        base.Awake();
    }
    public override void Start()
    {
        Debug.Log("player Start " + name);
        rigidbody.maxAngularVelocity = 3000;
        _TimerA.AddMethod(100, delegate
        {
            if (_Game.mapSettings.zombi)
                Score = (_Game.stage + 1) * mapSettings.pointsPerStage;
            slowdowntime = _Game.mapSettings.slow ? 1 : 0;
        });
        base.Start();
    }
    public void RPCSetUserView(byte[] s)
    {
        CallRPC("SetUserView", s);
    }
    [RPC]
    public void SetUserView(byte[] data) //userinfo
    {
        if (!isOwner)
            user = (UserView) Deserialize(data, UserView.xml);
        AliveMaterial = new Material(_Loader.playerTextures[user.MaterialId]);
        if (user.MaterialId == 1)
        {
            Debug.Log("loading texture" + user.BallTextureUrl);
            var w = new WWW(Menu.webserver + "image.php?image=" + user.BallTextureUrl);
            _TimerA.AddMethod(() => w.isDone, delegate
                                                  {
                                                      AliveMaterial.SetTexture("_MainTex", w.texture);
                                                      if (Alive) model.renderer.sharedMaterial = AliveMaterial;
                                                  });
        }
    }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        base.OnPlayerConnectedBase(np);
        RPCSetUserView(Serialize(user, UserView.xml));
        RPCSetTeam((int) team);
        RPCSetAlive(Alive);
        RPCSetFrags(frags, Score);
        RPCSetDeaths(deaths);
        RPCSetFanarik(fanarik.enabled);
        RPCSelectGun(selectedgun);
        RPCSetLifeUpgrate(LifeUpgrate);
        RPCSetSpeedUpgrate(SpeedUpgrate);
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
        RPCSetAlive(false);
        var g = GetSpawn(team);
        if (g == null)
            g = GetSpawn(Team.None);
        transform.position = g.transform.position;
        transform.rotation = Quaternion.identity;
    }
    public MapTag GetSpawn(Team team)
    {
        return _Game.spawns.Where(a => a.SpawnType.ToString().ToLower() == (team + "spawn").ToLower()).Random();
    }
    public void LocalSelectGun(int id)
    {
        if (guns.Count(a => a.group == id && a.patronsLeft > 0) == 0 && !debug) return;
        bool foundfirst = false;
        bool foundnext = false;
        for (int i = selectedgun; i < guns.Count; i++)
            if (guns[i].group == id && (guns[i].patronsLeft > 0 || debug))
            {
                if (foundfirst)
                {
                    selectedgun = i;
                    foundnext = true;
                    break;
                }
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
    [FindAsset("change")] public AudioClip changeSound;
    public void RPCSelectGun(int i)
    {
        CallRPC("SelectGun", i);
    }
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
        MaxLife = LifeUpgrate * defMaxLife;
        if (!Alive && fanarik.enabled) fanarik.enabled = false;
        UpdateAim();

        

        if (_TimerA.TimeElapsed(100))
        {
            if (plPathPoints.Count == 0 || Vector3.Distance(pos, plPathPoints.Last()) > 1)
            {
                plPathPoints.Add(pos);
                if (plPathPoints.Count > 20) plPathPoints.RemoveAt(0);
            }
        }


        multikilltime -= Time.deltaTime;
        if (this.rigidbody.velocity.magnitude > 20)
        {
            speedparticles.worldVelocity = this.rigidbody.velocity/10;
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
    private float scrolintrvl;
    
    private void LocalUpdate()
    {
        
        Energy += Time.deltaTime * PowerUpgrate / 6;
        Energy = Math.Min(100, Energy);
        scrolintrvl += Time.deltaTime;
        if (lockCursor && Alive)
        {
            UpdateMapItems();
            if (DebugKey(KeyCode.L))
                RPCSetFrags(30, 10);

            bool sh = Input.GetKey(KeyCode.LeftShift);
            if (sh != shift) RPCSetStreff(sh);
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && scrolintrvl > .1f)
            {
                PrevGun();
                scrolintrvl = 0;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && scrolintrvl > .1f)
            {
                NextGun();
                scrolintrvl = 0;
            }


            if (_TimerA.TimeElapsed(500))
            {
                var nears = players.Union(_Game.towers.Cast<Destroible>()).Where(a => a != null && a != this && !a.isEnemy(OwnerID) && Vector3.Distance(a.pos, pos) < 2);
                foreach (var near in nears)                    
                        if (Input.GetKey(KeyCode.H) && near.Life < near.MaxLife)
                            near.RPCHeal(20);
            }

            if (Input.GetKeyDown(KeyCode.Q) && (Energy > 20 || !build))
            {
                Energy -= 20;
                RPCSetShield();
            }
            if (Input.GetKeyDown(KeyCode.X) && (gun.towerScore != -1 && Score > gun.towerScore) && !new[] { GunType.physxgun }.Contains((GunType)gun.guntype))
            {
                Score -= gun.towerScore;
                Tower t = ((GameObject)Network.Instantiate(Turret, pos + rot * Vector3.forward * 3 + Vector3.up, rot, (int)GroupNetwork.Tower)).GetComponent<Tower>();
                t.RPCSetType(gun.guntype);                
                t.RPCSetOwner(OwnerID);
            }
            
                if (Input.GetKeyDown(KeyCode.G))
                {
                    var near = players.Where(a => a != null && a != this && Vector3.Distance(pos, a.pos) < 10).OrderBy(a => Vector3.Distance(pos, a.pos)).FirstOrDefault(); ;
                    if (near != null)
                    {                        
                        if (Score >= 1)
                        {
                            near.RPCGiveMoney(1);
                            Score -= 1;                            
                        }
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
                _Game.RPCSetTimeBomb(Time.timeScale*0.5f);
            }
            

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Energy > 10 || !build)
                {
                    Energy -= 10;
                    RPCJump();
                }
            }
            if ((haveLight || debug) && Input.GetKeyDown(KeyCode.R))
            {
                RPCSetFanarik(!fanarik.enabled);
            }
        }
    }
    public void RPCSetShield()
    {
        CallRPC("SetShield");
    }
    [RPC]
    public void SetShield()
    {
        var g = (GameObject) Instantiate(staticField, pos, rot);
        foreach (var p in players.Where(a => a != null))
            Physics.IgnoreCollision(g.GetComponentInChildren<Collider>(), p.collider, true);
        audio.PlayOneShot(ForceField);
        g.transform.parent = this.transform;
        Destroy(g, 10);
    }
    private void UpdateMapItems()
    {
        var mi = _Game.mapitems.Where(a => a.enabled && (
            a.buttons.Any(b => Vector3.Distance(b.position, pos) < a.Distance) ||
            a.boundings.Any(b => b.collider.bounds.Contains(a.pos)))
            ).OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
        if (mi != null)
        {
            mapItem = mi;
            mapItemTm = .5f;
        }
        MapItemInterval -= Time.deltaTime;
        mapItemTm -= Time.deltaTime;
        if (mapItemTm > 0 && MapItemInterval < 0 && mapItem.Check())
        {
            _GameWindow.CenterText.text = mapItem.text +
                                          (mapItem.Score > 0 ? (", costs " + mapItem.Score + " Money") : "");
            if ((Input.GetKeyDown(KeyCode.F) || mapItem.autoTake) && (Score >= mapItem.Score - 1 || debug))
                mapItem.LocalCheckOut();
        }
        else
            _GameWindow.CenterText.text = "";
    }
    
    public bool shift;
    public void RPCSetStreff(bool value)
    {
        CallRPC("SetStreff", value);
    }
    [RPC]
    public void SetStreff(bool value)
    {
        shift = value;
    }
    
    
    public void RPCSetFanarik(bool v)
    {
        CallRPC("SetFanarik", v);
    }
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
    private struct tmp
    {
        public float angle;
        public Destroible b;
    }
    public void UpdateAim()
    {
        if (Alive)
        {
            if (isOwner) syncRot = _Cam.rot;
            guntr.rotation = syncRot;

            var pos = gun.cursor[0].position;
            var ps = _Game.players.Union(_Game.zombies.Cast<Destroible>())
                .Where(a => a != null && a.Alive && a.isEnemy(OwnerID)); //aimbot
            var obj = ps.Select(a => new tmp { angle = Vector3.Angle(syncRot * Vector3.forward, a.pos - pos), b = a }).Where(a => a.angle < 30).OrderBy(a => Vector3.Distance(a.b.pos, pos)).FirstOrDefault();
            if (obj.b != null)
            {
                var r = Quaternion.LookRotation(obj.b.pos - pos);
                var y = Quaternion.RotateTowards(syncRot, r, 2).eulerAngles;
                var zx = Quaternion.RotateTowards(syncRot, r, 10).eulerAngles;
                guntr.rotation = Quaternion.Euler(topdown ? zx.x : y.x, y.y, topdown ? zx.z : y.z);
            }
            Laser();
        }
        else
            laserRender.enabled = false;
    }
    private void Laser()
    {
        Ray r = gun.GetRay();
        RaycastHit h = new RaycastHit() { point = r.origin + r.direction * 100 };
        if (Physics.Raycast(r, out h, 100, ~(1 << LayerMask.NameToLayer("Glass"))))
            h.collider.gameObject.SendMessage("Aim", this, SendMessageOptions.DontRequireReceiver);            

        if ((gun.laser || debug || _Game.mapSettings.haveALaser))
        {
            laserRender.enabled = true;
            laserRender.SetPosition(0, r.origin);
            laserRender.SetPosition(1, h.point);
        }
        else
            laserRender.enabled = false;
    }
    private Vector3 moveForce;
    protected virtual void FixedUpdate()
    {
        moveForce *= .80f;
        if (isOwner)
        {
            var agr = Physics.gravity != _Game.gravity;
            Vector3 md = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (!lockCursor) md = Vector3.zero;
            var e = _Cam.rot.eulerAngles;
            md = Quaternion.Euler(new Vector3(agr ? e.x : 0, e.y, 0)) * md;
            md.Normalize();

            if (agr)
            {
                this.rigidbody.AddForce(new Vector3(md.x, md.y, md.z) * 10);
                this.rigidbody.velocity *= .98f;
            }
            else
                if (shift && !frozen && (Energy > 0 || !build))
                {
                    moveForce += md * 2.6f; //forcemove
                    this.rigidbody.angularVelocity = new Vector3(md.z, 0, -md.x) * 16 / Mathf.Sqrt(Mathf.Sqrt(rigidbody.mass));

                    Energy -= Time.deltaTime * 2f;
                    var v = this.rigidbody.velocity;
                    v.x = moveForce.x;
                    v.z = moveForce.z;
                    v = v / Mathf.Sqrt(rigidbody.mass);
                    if (Physics.gravity == _Game.gravity)
                        v.y *= Mathf.Sqrt(rigidbody.mass);
                    rigidbody.velocity = v;
                }
                else
                {
                    this.rigidbody.AddTorque(new Vector3(md.z, 0, -md.x) * 1 * 5);
                    if (isGrounded > .2f)
                        this.rigidbody.AddForce(new Vector3(md.x, md.y, md.z) * 10);
                }
            if (frozen)
                this.rigidbody.velocity *= .95f;
        }
    }
    public void RPCJump()
    {
        CallRPC("Jump");
    }
    [RPC]
    public void Jump()
    {
        transform.rigidbody.MovePosition(rigidbody.position + new Vector3(0, 1, 0));
        rigidbody.velocity = (_Cam.rot * new Vector3(0, 0, 40) * rigidbody.mass * fdt);
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
        for (var i = selectedgun + 1; i < guns.Count; i++)
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
    public void RPCSetTeam(int t)
    {

        CallRPC("SetTeam", t);
    }
    [RPC]
    public void SetTeam(int t)
    {
        print(pr);
        team = (Team) t;
        _TimerA.AddMethod(delegate
        {
            if (Alive)
                SetLayer(gameObject);
            else
                SetLayer(gameObject, LayerMask.NameToLayer("DeadPlayer"));
        });
    }
    [RPC]
    public void RPCSetDeaths(int d)
    {
        deaths = d;
    }
    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        if (!Alive) return;
        if (isOwner && collisionInfo.relativeVelocity.y > 20)
            RPCPowerExp(this.transform.position);
        base.OnCollisionEnter(collisionInfo);
    }
    [FindAsset("powerexp")] public AudioClip powerexpSound;
    [FindAsset("wave")] public GameObject WavePrefab;
    [FindAsset("bowling")] public AudioClip bowling;
    [FindAsset("nitrojump")] public AudioClip nitrojumpSound;
    [FindAsset] public AudioClip givemoney;
    [FindAsset] public GameObject Explosion;
    public void RPCPowerExp(Vector3 v)
    {
        CallRPC("PowerExp", v);
    }
    [RPC]
    public void PowerExp(Vector3 v)
    {
        PlaySound(powerexpSound, 1);
        GameObject g = (GameObject) Instantiate(WavePrefab, v, Quaternion.Euler(90, 0, 0));       
        if (isOwner)
            _Cam.exp = 1f;

        Destroy(g, 1.6f);
    }
    [FindAsset("Detonator-Base")] public GameObject detonator;
    public override void RPCDie(int killedby)
    {
        base.RPCDie(killedby);
    }
    [RPC]
    public override void Die(int killedby)
    {
        if (!Alive) return;
        Destroy(Instantiate(detonator, transform.position, Quaternion.identity), .4f);
        deaths++;
        if (isOwner)
        {
            if (killedby == -1)
            {
                _Game.RPCWriteMessage(nick + " died ");
            }
            else if (OwnerID == killedby)
            {
                _Game.RPCWriteMessage(_localPlayer.nick + " Killed self ");
                players[killedby].AddFragsLocal(-1, -.5f * _Game.mapSettings.pointsPerPlayer);
            }
            else if (team != players[killedby].team || _Game.mapSettings.DeathMatch)
            {
                _Game.RPCWriteMessage(players[killedby].nick + " kill " + nick);
                players[killedby].AddFragsLocal(+1, _Game.mapSettings.pointsPerPlayer);
                players[killedby].RPCPlayKillAnim(OwnerID);
            }
            else
            {
                _Game.RPCWriteMessage(_localPlayer.nick + " kill " + nick);
                players[killedby].AddFragsLocal(-1, -1 * _Game.mapSettings.pointsPerPlayer);
            }
        }
        

        if (isOwner)
        {
            if (!_Game.mapSettings.zombi)
                _TimerA.AddMethod(10000, delegate { ResetSpawn(); RPCSetAlive(true); });

            RPCSetAlive(false);
        }
    }
    public void RPCPlayKillAnim(int killedby) { CallRPC("PlayKillAnim", killedby); }
    [RPC]
    public void PlayKillAnim(int killedby)
    {
        PlayTextAnimation(_Cam.ActionText, "You Killed " + players[killedby].nick + " +" + _Game.mapSettings.pointsPerPlayer);
    }
    public Material AliveMaterial;
    public Material deadMaterial;
    public void RPCSetAlive(bool v)
    {
        CallRPC("SetAlive", v);
    }
    [RPC]
    public void SetAlive(bool value)
    {
        if (value)
            spawned = true;
        Debug.Log(name + " Alive " + value);
        
        
        Alive = value;
        if (spawned)
            audio.PlayOneShot(Alive ? alive : death, 2);
        RPCSetFanarik(false);
        model.renderer.sharedMaterial = value ? AliveMaterial : deadMaterial;
        ResetPlayer();
        if (isOwner)
        {
            foreach (GunBase gb in guns)
                gb.DisableGun();
            selectedgun = -1;
            NextGun();
            RPCSetFrozen(false);
            RPCSetTeam((int)(_Game.mapSettings.Team && _TeamSelectWindow.Teams != "" ? _TeamSelectWindow.Teams.Parse<Team>() : Team.None));
        }
        Life = MaxLife;
    }
    private void ResetPlayer()
    {
        foreach (GunBase gunBase in guns.Concat(guns))
            gunBase.Reset();

        if (!_Game.mapSettings.zombi)
            Score = _Game.mapSettings.StartMoney;
        if(build)
            SpeedUpgrate = LifeUpgrate = PowerUpgrate = 1;
        haveAntiGravitation = haveLight = haveTimeBomb = false;
        shift = false;       
        Energy = 20;
    }
    public void RPCSetFrags(int i, float score)
    {
        CallRPC("SetFrags", i, score);
    }
    [RPC]
    public void SetFrags(int i, float sc)
    {
        frags = i;
        Score = sc;
    }
    public void AddFragsLocal(int i, float sc)
    {
        if (isOwner)
        {
            if (multikilltime > 0) //double score
                multikill += i;
            else
                multikill = 0;
            multikilltime = 1;

            if (multikill >= 2)
            {
                if (gun is GunPhysix && multikill >= 5)
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

                PlayTextAnimation(_Cam.ScoreText, "x" + (multikill + 1));
            }
            
        }
        frags += i;
        Score += sc;
        RPCSetFrags(frags, Score);
    }
    [FindAsset("toasty")] public AudioClip[] multikillSounds;
    public void RPCSetSpeedUpgrate(int value)
    {
        CallRPC("SetSpeedUpgrate", value);
    }
    [RPC]
    public void SetSpeedUpgrate(int value)
    {
        SpeedUpgrate = value;
    }
    public void RPCSetLifeUpgrate(int value)
    {
        CallRPC("SetLifeUpgrate", value);
    }
    [RPC]
    public void SetLifeUpgrate(int value)
    {
        LifeUpgrate = value;
    }
    public void RPCGiveMoney(int money)
    {
        CallRPC("GiveMoney", money);
    }
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
        get { return guntr.rotation; }
        set { guntr.rotation = value; }
    }
    public override void RPCSetLifeLocal(float NwLife, int killedby)
    {
        base.RPCSetLifeLocal(NwLife, killedby);
    }
    [RPC]
    public override void SetLife(float NwLife, int killedby)
    {        
        base.SetLife(NwLife, killedby);
    }
    private void OnCollisionStay(Collision collisionInfo)
    {
        isGrounded = 0;
    }
    internal GunBase gun
    {
        get { return guns[selectedgun]; }
    }
    internal string nick
    {
        get { return user.guest ? "[G]" + user.nick : user.nick; }
    }
    internal bool topdown { get { return _Cam.topdown; } set { _Cam.topdown = value; } }
    internal MapSetting mapSettings
    {
        get { return _Loader.mapSettings; }
        set { _Loader.mapSettings = value; }
    }
}