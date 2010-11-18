using UnityEngine;
using System.Collections;
using System;

public class Zombie : IPlayer
{
    float zombieWait = 2;
    float zombieBite;
    public float speed = .3f;
    public override bool dead { get { return !Alive; } }
    public bool Alive = true;
    public float up = 1f;
    public Vector3 oldpos;
    public Quaternion r { get { return this.rigidbody.rotation; } set { this.rigidbody.rotation = value; } }
    public Vector3 p { get { return this.rigidbody.position; } set { this.rigidbody.position = value; } }
    protected override void Start()
    {
        rigidbody.angularDrag = 5;
        rigidbody.mass = .5f;
        base.Start();
    }
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
                Vector3 v3 = ipl.transform.position - transform.position;
                v3.y = 0;
                if (v3.sqrMagnitude > 10)
                {
                    r = Quaternion.LookRotation(v3.normalized);
                    rigidbody.MovePosition( p + r * new Vector3(0, 0, speed * Time.deltaTime));
                    oldpos = p;
                }
                else if (ipl.isOwner && zombieBite > 1)
                {
                    zombieBite = 0;
                    if (ipl is Player)
                        PlayRandSound("sounds/scream");
                    if (build) ipl.RPCSetLife(ipl.Life - 10 - _Game.stage, -1);
                }
            }
        }
    }
    
    [RPC]
    public override void RPCDie(int killedby)
    {
        
        if (isController) CallRPC(killedby);
        PlayRandSound("sounds/gib");
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
    protected override void OnEnable()
    {        
        _Game.zombies.Add(this); base.OnEnable();
    }
    //protected override void OnDisable() { _Game.zombies.Remove(this); base.OnDisable(); }
    
    private void PlayRandom()
    {
        if (this != null && Alive)
        {
            _TimerA.AddMethod(UnityEngine.Random.Range(5000, 50000), PlayRandom);            
            PlayRandSound("sounds/Zombie");
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
        if (b != null && b is Box && !(b is Player) && Alive && isController &&
            collisionInfo.impactForceSum.magnitude > 15 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
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
