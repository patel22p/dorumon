using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
public enum ZombieType { Normal, Speed, Life }
public class Zombie : Destroible
{
    internal ZombieType[] priority = new ZombieType[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ZombieType.Life, ZombieType.Speed, ZombieType.Speed, ZombieType.Speed };
    public ZombieType zombieType;
    public float zombieBite;    
    public float speed = .3f;
    public float up = 1f;
    float seekPathtm;
    public bool move;
    [FindAsset("scream")]
    public AudioClip[] screamSounds;
    [FindAsset("gib")]
    public AudioClip[] gibSound;
    [FindAsset("Zombie")]
    public AudioClip[] ZombieSound;
    [FindTransform("zombieAlive")]
    public GameObject AliveZombie;
    [FindTransform("zombieDead")]
    public GameObject DeadZombie;
    [FindTransform("zombieBig")]
    public GameObject BigZombie;
    [FindTransform("zombieBigDead")]
    public GameObject BigDeadZombie;
    [FindTransform("zombieFastDead")]
    public GameObject FastDeadZombie;
    [FindTransform("zombieFast")]
    public GameObject FastZombie;

    public Seeker seeker;
    
    Vector3[] pathPoints;
    public Vector3 oldpos;
    public AnimationCurve zombieSpeedCurve;
    public AnimationCurve zombieLifeCurve;
    public override void Init()
    {        
        base.Init();
        seeker = this.GetComponent<Seeker>();
        
        if (seeker == null) Debug.Log("Could not find seeker");
        velSync = angSync = true;
        posSync = rotSync = true;
        updateLightmapInterval = 500;
        
    }
    public override void Awake()
    {
        seeker.debugPath = _Loader.debugPath;
        base.Awake();        
    }
    public override void Start()
    {        
        ResetSpawnTm();
        _Game.zombies.Add(this);
        base.Start();

    }
    public void CreateZombie(int stage)
    {   
        zombieType = priority.Random();
        var speed = zombieSpeedCurve.Evaluate(stage) * _Game.mapSettings.zombieSpeedFactor;
        speed = Random.Range(speed, speed * .8f);
        var life = zombieLifeCurve.Evaluate(stage) * _Game.mapSettings.zombieLifeFactor;
        life = Random.Range(life, life * .3f);
        if (zombieType == ZombieType.Life) { speed *= .7f; life *= 5; }
        if (zombieType == ZombieType.Speed) { speed *= 1.5f; life *= .7f; }
        RPCSetup(speed, life, (int)zombieType);
    }
    public void RPCSetup(float zombiespeed, float zombieLife, int priority) { CallRPC("Setup", zombiespeed, zombieLife, priority); }
    [RPC]
    public void Setup(float zombiespeed, float zombieLife, int priority)
    {
        
        Alive = true;
        Sync = true;
        ResetSpawn();
        SetLayer(gameObject);
        _TimerA.AddMethod(UnityEngine.Random.Range(0, 1000), PlayRandom);
        zombieType = (ZombieType)priority;
        SetAliveModel(zombieType, true);
        CanFreeze = zombieType != ZombieType.Life;
        speed = zombiespeed;
        MaxLife = Life = zombieLife;
        transform.localScale = Vector3.one * Math.Min(Mathf.Max(Mathf.Sqrt(zombieLife) / 10, 1f), 3);
    }

    private void SetAliveModel(ZombieType zt, bool alive)
    {
        bool big = zt == ZombieType.Life;
        bool fast = zt == ZombieType.Speed;
        bool norm = zt == ZombieType.Normal;
        foreach (var a in DeadZombie.GetComponentsInChildren<Renderer>())
            a.enabled = norm && !alive;
        foreach (var a in AliveZombie.GetComponentsInChildren<Renderer>())
            a.enabled = norm && alive;
        foreach (var a in BigZombie.GetComponentsInChildren<Renderer>())
            a.enabled = big && alive;
        foreach (var a in BigDeadZombie.GetComponentsInChildren<Renderer>())
            a.enabled = big && !alive;
        foreach (var a in FastZombie.GetComponentsInChildren<Renderer>())
            a.enabled = fast && alive;
        foreach (var a in FastDeadZombie.GetComponentsInChildren<Renderer>())
            a.enabled = fast && !alive;
    }
    public override void RPCDie(int killedby)
    {
        base.RPCDie(killedby);
    }
    [RPC]
    public override void Die(int killedby)
    {
        if (!Alive) { return; }        
        Sync = false;
        Alive = false;
        _TimerA.AddMethod(delegate
        {
            SetLayer(LayerMask.NameToLayer("HitLevelOnly"));
        });
        if (Game.sendto == null)
            PlayRandSound(gibSound);
        SetAliveModel(zombieType, false);
        if (killedby == _localPlayer.OwnerID)
            _localPlayer.AddFrags(1, _Game.mapSettings.pointsPerZombie );
    }
    public float tiltTm;
    public float spawninTM;
    public new Quaternion rot;
    Vector3? zToPl(Destroible ipl)
    {        
        return ipl.transform.position - pos;        
    }
    public enum ZState { none, GetRay, GetPath, ZToPl }
    ZState state = ZState.none;
    float stm;
    public void RPCSetzsL(ZState z)
    {
        if (state != z && isController)
            RPCSetzs((int)z);
    }
    public void RPCSetzs(int z) { CallRPC("Setzs", z); }
    [RPC]
    public void Setzs(int z)
    {
        stm = 0;
        state = (ZState)z;
    }
    public Vector3? GetPoint()
    {
        var ipl = Nearest();
        Vector3? pnt = null;
        if (state == ZState.GetRay || stm > 1)
        {
            pnt = pnt ?? GetRay(ipl);
            if (pnt != null) { RPCSetzsL(ZState.GetRay); return pnt; }
        }

        if (state == ZState.GetPath || stm > 1)
        {
            pnt = pnt ?? GetNextPathFindPoint(ipl);
            if (pnt != null) { RPCSetzsL(ZState.GetPath); return pnt; }
        }

        if (state == ZState.ZToPl || stm > 10)
        {
            pnt = pnt ?? zToPl(ipl);
            if (pnt != null) { RPCSetzsL(ZState.ZToPl); return pnt; }
        }
        return null;
    }
    protected override void Update()
    {
        base.Update();        
        
        if(isController)
            if (rigidbody.velocity.magnitude > 5 * transform.localScale.x || Physics.gravity != _Game.gravity || Time.timeScale != 1) RPCSetFrozen(true);

        if (_Loader.stopZombies && _TimerA.TimeElapsed(100)) RPCSetFrozen(true);

        zombieBite += Time.deltaTime;

        if (isController)
            stm += Time.deltaTime;
        else
            stm = 0;

        seekPathtm -= Time.deltaTime;

        if (!Alive || selected == -1 || frozen) return;
        var ipl = Nearest();
        if (ipl != null && ipl.Alive)
        {
            Vector3? nwp =  _Loader.disablePathFinding ? zToPl(ipl) : GetPoint();
            if (nwp == null)
            {
                move = false;
                tiltTm += Time.deltaTime;
            }
            else
            {
                Vector3 pathPointDir = nwp.Value;
                Debug.DrawLine(pos, pos + pathPointDir);
                pathPointDir.y = 0;
                rot = Quaternion.LookRotation(pathPointDir.normalized);
                if (Vector3.Distance(ipl.transform.position, pos) > 1.5f)
                {
                    move = true;
                    tiltTm += Time.deltaTime;
                }
                else
                    Bite(ipl);
            }
        }
        else
        {
            move = false;
            tiltTm = 0;
        }
        
        if (tiltTm > spawninTM && isController)
        {
            tiltTm = 0;
            if (Vector3.Distance(oldpos, pos) / spawninTM < .3f)
                ResetSpawn();
            oldpos = pos;
        }

    }
    private void Bite(Destroible ipl)
    {
        move = false;
        tiltTm = 0;
        if (zombieBite > 1)
        {
            zombieBite = 0;
            PlayRandSound(screamSounds);
            ipl.RPCSetLife(ipl.Life - Math.Min(_Game.mapSettings.zombieDamage, _Game.stage + 1), -1);
        }
    }
    private Vector3? GetRay(Destroible ipl)
    {        
        var r = new Ray(pos, ipl.pos - pos);
        RaycastHit h;
        if (!Physics.Raycast(r, out h, Vector3.Distance(ipl.pos, pos), 1 << LayerMask.NameToLayer("Level")))
            return ipl.pos - pos;
        else
            return null;

    }
    Destroible nearest;
    private Destroible Nearest()
    {
        if(nearest == null || _TimerA.TimeElapsed(100))            
            nearest = players.Where(a => a != null && a.Alive).OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
        return nearest;
    }
    public float speedadd;
    void FixedUpdate()
    {
        if (Alive && !frozen)
        {
            transform.rotation = rot;
            if (move)
            {
                Vector3 v = rigidbody.velocity;
                v.x = v.z = 0;
                rigidbody.velocity = v;
                rigidbody.angularVelocity = Vector3.zero;
                speedadd = (_Game.stageTime / 10) * _Game.mapSettings.zombieSpeedGrowFactor;
                var t = rot * new Vector3(0, 0, 1) * .5f * (speed + speedadd) * Time.deltaTime * Time.timeScale * Time.timeScale;
                Ray r = new Ray(pos, t);
                if (Physics.Raycast(r, .3f, 1 << LayerMask.NameToLayer("Level")))
                    t.y++;
                pos += t;                
            }
        }
    }
    //private Vector3? GetPlayerPathPoint(Destroible ipl)
    //{        
    //    if (ipl is Player)
    //    {
    //        var pathPoints = ((Player)ipl).plPathPoints;
    //        var np = FindNextPoint(pathPoints);
    //        return np;
    //    }
    //    return null;
    //}
    private Vector3? FindNextPoint(IList<Vector3> points)
    {
        if (points == null || points.Count == 0) return null;
        Vector3 nearest = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        bool found = false;
        int ni = 0;
        for (int i = 0; i < points.Count; i++)
        {
            //if (i != 0) //Debug.DrawLine(points[i - 1], points[i]);
            Vector3 newp = points[i] - pos;
            if (newp.magnitude < nearest.magnitude)
            {
                nearest = newp;
                ni = i;
                if (nearest.magnitude < 6)
                    found = true;
            }
        }
        if (found)
            while(true)
            {
                ni++;
                if (ni >= points.Count) break;
                if (Vector3.Distance(points[ni], pos) > 3)
                    return points[ni] - pos;
            }

        return null;
    }
    private Vector3? GetNextPathFindPoint(Destroible ipl)
    {
        if (_Loader.disablePathFinding) return null;
        if (seekPathtm < 0)
        {
            seeker.StartPath(this.transform.position, ipl.transform.position);
            seekPathtm = UnityEngine.Random.Range(3f, 6);
        }
        return FindNextPoint(pathPoints);
        
    }
    void PathComplete(Vector3[] points)
    {
        pathPoints = points;
    }
    public override void RPCSetFrozen(bool value)
    {
        base.RPCSetFrozen(value);
    }
    [RPC]
    public override void SetFrozen(bool value)
    {
        tiltTm = 0;
        base.SetFrozen(value);
    }
    private void PlayRandom()
    {
        if (this != null && Alive)
        {
            _TimerA.AddMethod(UnityEngine.Random.Range(2000, 15000), PlayRandom);
            audio.pitch = 1.3f / transform.localScale.x;
            PlayRandSound(ZombieSound, transform.localScale.x);
            
        }
    }
    public override void ResetSpawn()
    {
        ResetSpawnTm();
        MapTag[] loc = _Game.spawns.Where(a => a.SpawnType == SpawnType.ZombieSpawnLocation).ToArray();
        MapTag[] spawns = _Game.spawns.Where(a => a.SpawnType == SpawnType.zombie).ToArray();        
        
        Destroible pl = Nearest();
        if (pl == null)
        {
            pos = spawns.First().transform.position;
        }
        else
        {
            var spawn = spawns.Where(a => loc.Any(b => b.collider.bounds.Contains(pl.pos) && b.collider.bounds.Contains(a.transform.position))).Random();
            var o = spawns.OrderBy(a => Vector3.Distance(a.transform.position, pl.pos));
            pos = (spawn ?? o.FirstOrDefault(a => Math.Abs(a.transform.position.y - pl.pos.y) < 3) ?? o.First()
                ).transform.position;
        }
        rot = Quaternion.identity;
        rigidbody.velocity = Vector3.zero;
    }
    public void ResetSpawnTm()
    {
        spawninTM = Random.Range(5f, 20f);
    }
    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        base.OnCollisionEnter(collisionInfo);
    }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        base.OnPlayerConnectedBase(np);
        RPCSetup((float)speed, (float)MaxLife, (int)zombieType);
        RPCSetzs((int)state);
        if (!Alive)
        {
            Debug.Log("send zombie rpc die" + np);
            RPCDie(-1);
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
