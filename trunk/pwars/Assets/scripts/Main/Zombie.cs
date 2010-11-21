using UnityEngine;
using System.Collections;
using System;

public class Zombie : IPlayer
{
    float zombieWait = 0;
    float zombieBite;
    public float speed = .3f;
    public override bool dead { get { return !Alive; } }
    public bool Alive = true;
    public float up = 1f;
    public Vector3 oldpos;
    public Quaternion r { get { return this.rigidbody.rotation; } set { this.rigidbody.rotation = value; } }
    public Vector3 p { get { return this.rigidbody.position; } set { this.rigidbody.position = value; } }
    Seeker seeker;
    protected override void Start()
    {
        if(_Game.enablePathFinding) seeker = this.gameObject.AddComponent<Seeker>();
        _Game.zombies.Add(this);
        rigidbody.angularDrag = 5;
        rigidbody.mass = .5f;
        base.Start();
    }
    
    public float zombieBiteDist = 3;
    [RPC]
    public void RPCSetup(float zombiespeed, float zombieLife)
    {
        _TimerA.AddMethod(UnityEngine.Random.Range(0, 1000), PlayRandom);
        CallRPC(zombiespeed, zombieLife);
        Alive = true;
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", (Texture2D)Resources.Load("Images/zombie"));
        speed = zombiespeed;
        transform.localScale = Vector3.one * Mathf.Max(zombieLife / 100f, 1f);
        Life = (int)zombieLife;
        _Game.AliveZombies.Add(this);
    }
    float seekPath;
    protected override void Update()
    {        
        zombieBite += Time.deltaTime;
        base.Update();

        if (!Alive) return;
        if ((zombieWait -= Time.deltaTime) < 0 && selected != -1)
        {
            Player pl = _Game.players[selected];
            IPlayer ipl = pl.car != null ? (IPlayer)pl.car : pl;
            if (ipl.enabled)
            {
                
                Vector3 pathPointDir;
                Vector3 zToPlDir = ipl.transform.position - p;

                if (zToPlDir.magnitude > zombieBiteDist)
                {
                    //Debug.DrawLine(transform.position, ipl.transform.position);
                    RaycastHit hitInfo;
                    bool shit = Physics.Raycast(new Ray(p, zToPlDir.normalized), out hitInfo, Vector3.Distance(p, ipl.transform.position), _Loader.LevelMask);
                    if (!shit || !_Game.enablePathFinding || (pathPointDir = GetNextPathPoint(ipl)) == default(Vector3))
                        pathPointDir = zToPlDir;
                    Debug.DrawLine(p, p + pathPointDir);
                    pathPointDir.y = 0;
                    r = Quaternion.LookRotation(pathPointDir.normalized);
                    transform.position += r * new Vector3(0, 0, speed * Time.deltaTime);
                    oldpos = p;
                }
                else if (ipl.isOwner && zombieBite > 1)
                {
                    zombieBite = 0;
                    if (ipl is Player)
                        PlayRandSound("scream");
                    if (build) ipl.RPCSetLife(ipl.Life - 10 - _Game.stage, -1);
                }

            }
        }
    }

    private Vector3 GetNextPathPoint(IPlayer ipl)
    {
        if ((seekPath -= Time.deltaTime) < 0)
        {
            seeker.StartPath(this.transform.position, ipl.transform.position);
            seekPath = UnityEngine.Random.Range(1, 3);
        }
        if (pathPoints == null) return default(Vector3);
        int offset = 2;
        Vector3 nearest = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        int ni=-2;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 newp = pathPoints[i] - transform.position;
            if (newp.magnitude < nearest.magnitude) { nearest = newp; ni = i; }
        }
        return ni == -2 || pathPoints.Length - 1 <= (ni + offset) ? default(Vector3) : pathPoints[ni + offset] - transform.position;
    }

    Vector3[] pathPoints;
    void PathComplete(Vector3[] points)
    {
        pathPoints = points;
    }
    [RPC]
    public override void RPCDie(int killedby)
    {
        
        if (isController) CallRPC(killedby);
        PlayRandSound("gib");
        if (!Alive) { return; }
        Alive = false;
        _Game.AliveZombies.Remove(this);
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", (Texture2D)Resources.Load("Images/zombiedead"));        
        if (isController)
        {            
            foreach (Player p in players)
                if (p != null && p.OwnerID == killedby)
                    p.SetFrags(+1);
        }        
    }
    private void PlayRandom()
    {
        if (this != null && Alive)
        {
            _TimerA.AddMethod(UnityEngine.Random.Range(5000, 50000), PlayRandom);            
            PlayRandSound("Zombie");
        }
    }
    
    [RPC]
    public override void RPCSetLife(int NwLife, int killedby)
    {
        zombieWait = .1f;
        base.RPCSetLife(NwLife, killedby);
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        
        if (dead) return;
        Base b = collisionInfo.gameObject.GetComponent<Base>();
        if (b != null && b is Box && Alive && isController &&
            collisionInfo.impactForceSum.magnitude > 30)
        {            
            RPCSetLife(Life - Math.Max((int)collisionInfo.impactForceSum.sqrMagnitude * 3, 200), b.OwnerID);
            if(_SettingsWindow.Blood)
                _Game.Emit(_Game.BloodEmitors, _Game.Blood, transform.position, Quaternion.identity, rigidbody.velocity);
        }
    }

    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);
        networkView.RPC("RPCSetup", np, (float)speed, (float)Life);
        if(!Alive) networkView.RPC("RPCDie", np, -1);
        if (dead) networkView.RPC("RPCRemove", np);
    }    

}
