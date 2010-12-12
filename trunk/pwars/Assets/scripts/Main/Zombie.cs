using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public class Zombie : Destroible
{
    public float zombieBite;
    public float speed = .3f;
    public float up = 1f;
    float seekPath;
    public bool move;
    [LoadPath("scream")]
    public AudioClip[] screamSounds;
    [LoadPath("gib")]
    public AudioClip[] gibSound;
    [LoadPath("Zombie")]
    public AudioClip[] ZombieSound;
    [PathFind("zombieAlive")]
    public GameObject AliveZombie;
    [PathFind("zombieDead")]
    public GameObject DeadZombie;

    public Seeker seeker;
    public float zombieBiteDist = 3;
    Vector3[] pathPoints;
    public Vector3 oldpos;
    public override void Init()
    {
        base.Init();
        seeker = this.GetComponent<Seeker>();
        if (seeker == null) Debug.Log("Could not find seeker");
        velSync = angSync = false;
        posSync = rotSync = true;
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        if(!_Loader.disablePathFinding) seeker.enabled = true;
        _Game.zombies.Add(this);
        base.Start();
        
    }
    [RPC]
    public void RPCSetup(float zombiespeed, float zombieLife)
    {
        if(CallRPC(zombiespeed, zombieLife)) return;
        Alive = true;
        Sync = true;
        transform.position = SpawnPoint();
        gameObject.layer = LayerMask.NameToLayer("Default");
        _TimerA.AddMethod(UnityEngine.Random.Range(0, 1000), PlayRandom);        
        AliveZombie.renderer.enabled = true;
        DeadZombie.renderer.enabled = false;
        speed = zombiespeed;
        transform.localScale = Vector3.one * Mathf.Max(zombieLife / 100f, 1f);
        Life = (int)zombieLife;
    }
    [RPC]
    public override void RPCDie(int killedby)
    {
        if (!Alive) { return; }        
        Sync = false;
        Alive = false;
        gameObject.layer = LayerMask.NameToLayer("HitLevelOnly");
        if (isController) if(CallRPC(killedby)) return;
        PlayRandSound(gibSound);
        AliveZombie.renderer.enabled = false;
        DeadZombie.renderer.enabled = true;

        if (isController)
        {
            foreach (Player p in players)
                if (p != null && p.OwnerID == killedby)
                    p.SetFrags(+1, 1);
        }
    }
    float tiltTm;
    protected override void Update()
    {
        base.Update();
        if (!Alive || selected == -1) return;
        zombieBite += Time.deltaTime;
        var ipl = Nearest();
        if (ipl != null)
        {
            Vector3 pathPointDir;
            Vector3 zToPlDir = ipl.transform.position - pos;
            if (zToPlDir.magnitude > zombieBiteDist)
            {
                pathPointDir = (zToPlDir.magnitude < 10 && Mathf.Abs(zToPlDir.y) < 1) ? zToPlDir : (GetPlayerPathPoint(ipl) ?? GetNextPathPoint(ipl) ?? zToPlDir);
                Debug.DrawLine(pos, pos + pathPointDir);
                pathPointDir.y = 0;
                rot = Quaternion.LookRotation(pathPointDir.normalized);
                move = true;
                tiltTm += Time.deltaTime;
                if (tiltTm > 10 && isController)
                {
                    tiltTm = 0;
                    if (Vector3.Distance(oldpos, pos) < 1)
                        pos = SpawnPoint();
                    oldpos = pos;
                }
            }
            else
            {
                move = false;
                tiltTm = 0;
                if (zombieBite > 1)
                {
                    zombieBite = 0;
                    if (ipl is Player)
                        PlayRandSound(screamSounds);
                    if (build && isController) ipl.RPCSetLife(ipl.Life - 10 - _Game.stage, -1);
                }
            }
        }
        else
        {
            move = false;
            tiltTm = 0;
        }
    }
    private Player Nearest()
    {
        Player pl = players.Where(b => b != null && b.Alive).OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
        return pl;
    }
    void FixedUpdate()
    {
        if (move && Alive)
        {
            if (rigidbody.velocity.magnitude < 10)
            {
                Vector3 v = rigidbody.velocity;
                v.x = v.z = 0;
                rigidbody.velocity = v;
            }
            pos += rot * new Vector3(0, 0, speed * Time.fixedDeltaTime);
        }
        
    }
    private Vector3? GetPlayerPathPoint(Player ipl)
    {
        var pathPoints = ipl.plPathPoints;
        return FindNextPoint(pathPoints);
    }
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
                if (nearest.magnitude < 8)
                    found = true;
            }
        }
        if (found)
            while(true)
            {
                ni++;
                if (ni >= points.Count) break;
                //if (Vector3.Distance(points[ni], pos) > 2)
                return points[ni] - pos;
            }

        return null;
    }
    private Vector3? GetNextPathPoint(Destroible ipl)
    {
        if (_Loader.disablePathFinding) return null;
        if ((seekPath -= Time.deltaTime) < 0)
        {
            seeker.StartPath(this.transform.position, ipl.transform.position);
            seekPath = UnityEngine.Random.Range(.5f, 1);
        }

        return FindNextPoint(pathPoints);
        
    }
    void PathComplete(Vector3[] points)
    {
        pathPoints = points;
    }
    
    private void PlayRandom()
    {
        if (this != null && Alive)
        {
            _TimerA.AddMethod(UnityEngine.Random.Range(5000, 50000), PlayRandom);            
            PlayRandSound(ZombieSound);
            
        }
    }
    public override Vector3 SpawnPoint()
    {
        GameObject[] gs = GameObject.FindGameObjectsWithTag("SpawnZombie");
        Player pl = Nearest(); 
        if (pl == null) return gs.First().transform.position;
        var neargs  = gs.Where(a => Vector3.Distance(a.transform.position, pl.pos) < 50 && Math.Abs(a.transform.position.y - pl.pos.y) < 3);
        Debug.Log(neargs.Count());
        return (neargs.Random() ??
            gs.OrderBy(a => Vector3.Distance(a.transform.position, pl.pos)).First()
            ).transform.position;        
    }
    [RPC]
    public override void RPCSetLife(int NwLife, int killedby)
    {
        base.RPCSetLife(NwLife, killedby);
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        
        if (dead) return;
        Base b = collisionInfo.gameObject.GetComponent<Base>();
        if (b != null && b is Box && !(b is Zombie) && Alive && isController &&
            collisionInfo.impactForceSum.magnitude > 20)
        {            
            RPCSetLife(Life - Math.Max((int)collisionInfo.impactForceSum.sqrMagnitude * 5, 200), b.OwnerID);
            if(_SettingsWindow.Blood)
                _Game.particles[1].Emit(transform.position, Quaternion.identity, rigidbody.velocity);
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        networkView.RPC("RPCSetup", np, (float)speed, (float)Life);
        if(!Alive) networkView.RPC("RPCDie", np, -1);
    }
}
