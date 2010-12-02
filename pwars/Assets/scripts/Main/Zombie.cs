using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public class Zombie : IPlayer
{
    public float zombieBite;
    public float speed = .3f;
    //public override bool dead { get { return !Alive; } }        
    //new public bool Alive;
    public float up = 1f;
    float seekPath;
    public bool move;
    [LoadPath("scream")]
    public AudioClip[] screamSounds;
    [LoadPath("gib")]
    public AudioClip[] gibSound;
    [LoadPath("Zombie")]
    public AudioClip[] ZombieSound;
    [LoadPath("Skin/Images/zombie.png")]
    public Texture zombieImage;
    [LoadPath("Skin/Images/zombiedead.png")]
    public Texture zombieDeadImage;
    public Seeker seeker;
    public float zombieBiteDist = 3;
    Vector3[] pathPoints;
    public override void Init()
    {
        base.Init();
        seeker = this.GetComponent<Seeker>();
        if (seeker == null) Debug.Log("Could not find seeker");
        velSync = rotSync = angSync = false;
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
        transform.position = SpawnPoint();
        gameObject.layer = LayerMask.NameToLayer("Default");
        _TimerA.AddMethod(UnityEngine.Random.Range(0, 1000), PlayRandom);
        CallRPC(zombiespeed, zombieLife);
        Alive = true;
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", zombieImage);
        speed = zombiespeed;
        transform.localScale = Vector3.one * Mathf.Max(zombieLife / 100f, 1f);
        Life = (int)zombieLife;
    }
    public Vector3 oldpos;
    protected override void Update()
    {
        zombieBite += Time.deltaTime;
        base.Update();
        
        if (!Alive || selected==-1) return;
        IPlayer ipl = players.Where(b => b != null && b.Alive).OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();

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
                if (_TimerA.TimeElapsed(2000) && isController)
                {
                    if (Vector3.Distance(oldpos, pos) < 2)
                        pos = SpawnPoint();
                    oldpos = pos;
                }
            }
            else if (ipl.isOwner && zombieBite > 1)
            {
                move = false;
                zombieBite = 0;
                if (ipl is Player)
                    PlayRandSound(screamSounds);
                if (build && isController) ipl.RPCSetLife(ipl.Life - 10 - _Game.stage, -1);
            }
        }
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
    
    private Vector3? GetPlayerPathPoint(IPlayer ipl)
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
            if (i != 0) Debug.DrawLine(points[i - 1], points[i]);
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
            for (; ni < points.Count; ni++)
            {
                if (Vector3.Distance(points[ni], pos) > 5)
                    return points[ni] - pos;
            }

        return null;
    }
    private Vector3? GetNextPathPoint(IPlayer ipl)
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
    [RPC]
    public override void RPCDie(int killedby)
    {
        
        gameObject.layer = LayerMask.NameToLayer("HitLevelOnly");
        if (isController) CallRPC(killedby);
        PlayRandSound(gibSound);
        if (!Alive) { return; }
        Alive = false;
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", zombieDeadImage);        
        if (isController)
        {            
            foreach (Player p in players)
                if (p != null && p.OwnerID == killedby)
                    p.SetFrags(+1, 1);
        }        
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
        
        return gs.OrderBy(a => Vector3.Distance(a.transform.position, transform.position)).Take(3).NextRandom().transform.position;        
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
