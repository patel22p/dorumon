using UnityEngine;
using System.Collections;

public class Zombie : IPlayer
{
    
    public override void Die(NetworkPlayer killedyby)
    {        
        if (!enabled) { Debug.Log("Zombie AlreadY Dead"); return; }
        if (Network.isServer)
            _TimerA.AddMethod(3000, RPCDestroy);
        this.transform.Find("zombie").renderer.materials[2].SetTexture("_MainTex", dead);
        enabled = false;
        foreach (Player p in players.Values)
        {
            if (p.OwnerID == killedyby)
            {                
                p.RPCSetFrags(+1);
            }
        }
    }


    [RPC]
    void RPCDestroy()
    {
        CallRPC(true);
        Destroy();
    }
    public Texture dead;
    protected override void Start()
    {
        _Spawn.zombies.Add(this);
        base.Start();
    }
    public override void Dispose()
    {
        _Spawn.zombies.Remove(this);
        base.Dispose();
    }
    protected override void Update()
    {
        base.Update();
        if (selected != null)
        {
            Player pl = _Spawn.players[selected.Value];
            IPlayer ipl = pl.car != null ? (IPlayer)pl.car : pl;
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
                ipl.RPCSetLife(-10,de);
            }
        }
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        Base b = collisionInfo.gameObject.GetComponent<Base>();
        if (b != null && b is box && !(b is Player) && enabled &&
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
        speed = zombiespeed;
        Life = zombieLife;
    }
}
