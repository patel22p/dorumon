using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
using System.Collections;


[Serializable]
public abstract class Destroible : Box
{
    public int Life;
    public Team? team
    {
        get
        {
            if (OwnerID == -1) return null;
            else return _Game.players[OwnerID.GetHashCode()].team;
        }
    }    
    public bool dead { get { return !Alive; } set { Alive = !value; } }
    public bool Alive = true;
    
    
    protected override void Awake()
    {        
        base.Awake();
    }

    public override void Init()
    {
        if (Life == 0) Life = 100;
        base.Init();
    }
    protected override void Start()
    {
        _Game.destroyables.Add(this);
        base.Start();
    }
    public float isGrounded;
    protected override void OnCollisionStay(Collision collisionInfo)
    {
        isGrounded = 0;
    }
    protected override void Update()
    {
        isGrounded +=Time.deltaTime;
        base.Update();
    }
    public void RPCSetLife(int NwLife, int killedby) { if (isController)CallRPC("SetLife", NwLife, killedby); }

    [RPC]    
    public virtual void SetLife(int NwLife, int killedby)
    {
        if (dead) return;        
        if (isEnemy(killedby) || NwLife > Life)
            Life = NwLife;

        if (Life <= 0 && isController)
            RPCDie(killedby);
    }
    public virtual bool isEnemy(int killedby)
    {
        if (this is Zombie) return true;
        if (killedby == OwnerID) return true;
        if (killedby == -1) return true;
        if (mapSettings.DM) return true;
        if (killedby != -1 && players[killedby] != null && players[killedby].team != team) return true;        
        return false;    
    }
    
    public void RPCDie(int killedby) { if (isController) CallRPC("Die", killedby); }
    [RPC]
    public abstract void Die(int killedby);
}
