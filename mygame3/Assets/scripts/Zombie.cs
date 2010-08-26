using UnityEngine;
using System.Collections;

public class Zombie : IPlayer
{
    
    public override void Die(int killedby)
    {        
        if (!Alive) { return; }
        _Spawn.zombies.Remove(this);
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", deadTexture);
        Alive = false;
        foreach (Player p in players.Values)
        {
            if (p.OwnerID == killedby)
            {                
                p.RPCSetFrags(+1);
            }
        }
        
    }

    internal bool Alive = true;
    public Texture deadTexture;
    protected override void Start()
    {
        
        base.Start();
    }
    float zombiewait = 2;
    protected override void Update()
    {                
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
                if (v3.sqrMagnitude > 6)
                {
                    r = Quaternion.LookRotation(v3.normalized);
                    p += r * new Vector3(0, 0, speed * Time.deltaTime);
                    oldpos = p;
                }
                else if (ipl.isOwner && _TimerA.TimeElapsed(1000))
                {
                    ipl.RPCSetLife(-10, -1);
                }
            }
        }
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (!Alive) return;
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
        CallRPC(true, zombiespeed, zombieLife);
        _Spawn.zombies.Add(this);
        speed = zombiespeed;
        Life = zombieLife;
    }
}
