using UnityEngine;
using System.Collections;

public class Zombie : IPlayer
{
    [RPC]
    public override void RPCDie(int killedby)
    {
        CallRPC(true, killedby);
        PlayRandSound(gibsound);
        if (!Alive) { return; }
        _Spawn.zombies.Remove(this);
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", deadTexture);
        Alive = false;
        if (isController)
            foreach (Player p in players.Values)
            {
                if (p.OwnerID == killedby)
                {
                    p.RPCSetFrags(+1);
                }
            }
        
    }
    public override bool dead { get { return !Alive; } }
    internal bool Alive = true;
    public Texture deadTexture;
    public AudioClip[] zombiesound;
    public AudioClip[] gibsound;
    protected override void Start()
    {
        rigidbody.angularDrag = 5;
        base.Start();
    }
    
    private void PlayRandom()
    {
        if (Alive)
        {
            _TimerA.AddMethod(Random.Range(5000, 50000), PlayRandom);            
            PlayRandSound(zombiesound);
        }
    }

    
    float zombiewait = 2;
    float zombiebite;
    protected override void Update()
    {
        zombiebite += Time.deltaTime;
        base.Update();
        if (!Alive) return;
        if ((zombiewait -= Time.deltaTime) < 0 && selected != -1)
        {
            Player pl = _Spawn.players[selected];
            IPlayer ipl = pl.car != null ? (IPlayer)pl.car : pl;
            if (ipl.enabled)
            {
                Vector3 v3 = ipl.transform.position - transform.position;
                v3.y = 0;
                if (v3.sqrMagnitude > 10)
                {
                    r = Quaternion.LookRotation(v3.normalized);
                    p += r * new Vector3(0, 0, speed * Time.deltaTime);
                    oldpos = p;
                }
                else if (ipl.isOwner && zombiebite>1)
                {
                    zombiebite = 0;
                    ipl.RPCSetLife(-10, -1);
                }
            }
        }
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (dead) return;
        Base b = collisionInfo.gameObject.GetComponent<Base>();
        if (b != null && b is box && !(b is Player) && Alive &&
            collisionInfo.impactForceSum.sqrMagnitude > 150 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
        {

            RPCSetLife(-(int)collisionInfo.impactForceSum.sqrMagnitude / 2, b.OwnerID);
        }
    }
    public float speed = .3f;
    public float up = 1f;
    public Vector3 oldpos;
    public Quaternion r { get { return this.rigidbody.rotation; } set { this.rigidbody.rotation = value; } }
    public Vector3 p { get { return this.rigidbody.position; } set { this.rigidbody.position = value; } }


    [RPC]
    public void RPCSetup(float zombiespeed, int zombieLife)
    {
        _TimerA.AddMethod(Random.Range(0, 1000), PlayRandom);        
        CallRPC(true, zombiespeed, zombieLife);
        _Spawn.zombies.Add(this);
        speed = zombiespeed;
        Life = zombieLife;
    }
}
