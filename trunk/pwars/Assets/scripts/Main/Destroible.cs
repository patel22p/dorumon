using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
using System.Collections;


[Serializable]
public abstract class Destroible : Shared
{
    public float maxLife = 100;
    public float Life;
    public float freezedt;
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
        base.Start();
    }
    public float isGrounded;
    
    protected virtual void OnCollisionEnter(Collision collisionInfo)
    {
        if (Alive && isController)
        {
            Box b = collisionInfo.gameObject.GetComponent<Box>();
            if (b != null && isEnemy(b.OwnerID) && collisionInfo.rigidbody.velocity.magnitude > 50)
            {
                RPCSetLife(Life - (int)collisionInfo.rigidbody.velocity.magnitude * 10, b.OwnerID);
            }
        }
    }
    protected override void Update()
    {
        isGrounded +=Time.deltaTime;
        base.Update();
    }

    public void RPCSetLife(float NwLife, int killedby)
    {
        if (dead) return;
        if (isController)
        {
            if (isEnemy(killedby) || NwLife > Life)
            {
                Life = Math.Min(NwLife, maxLife);
                if (_localPlayer == this) Life = Math.Max(Life, 1);
                CallRPC("SetLife", Life, killedby);
                if (this == _localPlayer)
                {
                    if (killedby == _localPlayer.OwnerID && NwLife < Life) _localPlayer.score += Math.Abs(Life - NwLife) / 100;
                    _GameWindow.Hit(Mathf.Abs(Life - NwLife) * 2);
                    freezedt = (Life - NwLife) / 20;
                }
            }
            
            if (Life <= 0)
                RPCDie(killedby);
        }
    }

    [RPC]
    public void SetLife(float NwLife, int killedby)
    {
        Life = NwLife;
    }


    public virtual bool isEnemy(int id)
    {
        if (this is Zombie) return true;        
        if (id == -1) return true;        
        if (id == OwnerID) return false;
        if (mapSettings.DM) return true;
        if (id != -1 && players[id] != null && players[id].team != team) return true;        
        return false;    
    }
    
    public void RPCDie(int killedby) { if (isController) CallRPC("Die", killedby); }
    [RPC]
    public abstract void Die(int killedby);
}
