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
    float freezedt;
    [FindAsset]
    public AudioClip heal;
    public Team? team
    {
        get
        {
            if (OwnerID == -1) return null;
            else return _Game.players[OwnerID.GetHashCode()].team;
        }
    }    
    //public bool dead { get { return !Alive; } set { Alive = !value; } }
    public bool Alive = true;
    public void SetLayer(GameObject g)
    {
        _TimerA.AddMethod(delegate
        {
            g.layer = LayerMask.NameToLayer(_localPlayer.isEnemy(OwnerID) ? "Enemy" : "Ally"); 
        });
        
    }
    public override void Awake()
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
        SetLayer(gameObject);
        base.Start();
    }
    public float isGrounded;
    protected virtual void OnCollisionEnter(Collision collisionInfo)
    {
        if (Alive && isController)
        {
            Box b = collisionInfo.gameObject.GetComponent<Box>();
            if (b != null && isEnemy(b.OwnerID) && collisionInfo.rigidbody.velocity.magnitude > 20)
            {
                RPCSetLife(Life - (int)collisionInfo.rigidbody.velocity.magnitude * 10, b.OwnerID);
            }
        }
    }
    public bool frozen;

    public virtual void RPCSetFrozen(bool value)
    {
        if(value)
            freezedt = .5f;
        if (value != frozen)
           CallRPC("SetFrozen", value);
    }
    [RPC]
    public virtual void SetFrozen(bool value)
    {        
        frozen = value;
    }
    protected override void Update()
    {
        if (isController)
        {
            freezedt -= Time.deltaTime;
            if (freezedt < 0 && frozen)
                RPCSetFrozen(false);
        }

        isGrounded +=Time.deltaTime;
        base.Update();
    }
    public void RPCHeal(float life) { CallRPC("Heal", life); }
    [RPC]
    public void Heal(float life)
    {
        PlaySound(heal);
        if (isController)
            RPCSetLife(Life + 10, -1);
    }
    public virtual void RPCSetLife(float NwLife, int killedby)
    {
        if (!Alive) return;
        if (isController)
        {
            if (isEnemy(killedby) || NwLife > Life)
            {
                Life = Math.Min(NwLife, maxLife);                
                CallRPC("SetLife", Life, killedby);
                if (this == _localPlayer)
                {
                    if (killedby == _localPlayer.OwnerID && NwLife < Life) _localPlayer.score += Math.Abs(Life - NwLife) / 100;
                    _GameWindow.Hit(Mathf.Abs(Life - NwLife) * 2);
                }
                if(killedby!=-1)
                    RPCSetFrozen(true);
            }
            
            if (Life <= 0)
                RPCDie(killedby);
        }
    }
    [RPC]
    public virtual void SetLife(float NwLife, int killedby)
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
    public void RPCDie(int killedby) { CallRPC("Die", killedby); }
    [RPC]
    public abstract void Die(int killedby);
}
