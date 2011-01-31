using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
using System.Collections;


public abstract class Destroible : Shared
{
    public bool CanFreeze;
    public float maxLife = 100;
    public float Life;
    public GameObject model;
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
    public void SetLayer(int layer)
    {
        foreach (var t in transform.GetTransforms())
            t.gameObject.layer = layer;
    }
    public void SetActive(GameObject g, bool value)
    {
        foreach (var a in g.transform.GetTransforms())
            a.gameObject.active= value;
    }
    public void SetLayer(GameObject g)
    {
        var la = LayerMask.NameToLayer(_localPlayer.isEnemy(OwnerID) ? "Enemy" : "Ally");
        foreach (var a in g.transform.GetTransforms())
            a.gameObject.layer = la;
    }
    public void SetLayer(GameObject g, LayerMask la)
    {        
        foreach (var a in g.transform.GetTransforms())
            a.gameObject.layer = la;
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
    public override void Start()
    {
        //SetLayer(model);
        base.Start();
    }
    public float isGrounded;
    protected virtual void OnCollisionEnter(Collision collisionInfo)
    {
        if (Alive && isController)
        {
            Box b = collisionInfo.gameObject.GetComponent<Box>();
            if (b != null && (this.isEnemy(b.OwnerID) || OwnerID == _localPlayer.OwnerID) && collisionInfo.rigidbody.velocity.magnitude > 20)
                RPCSetLife(Life - (int)collisionInfo.rigidbody.velocity.magnitude * 10 * mapSettings.damageFactor, b.OwnerID);
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
            //if (isEnemy(killedby) || NwLife > Life)
            {
                if (this == _localPlayer && NwLife < Life)
                    _GameWindow.Hit();
                Life = Math.Min(NwLife, maxLife);                
                CallRPC("SetLife", Life, killedby);
                
                if(CanFreeze)
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
    public virtual void RPCDie(int killedby) { CallRPC("Die", killedby); }
    [RPC]
    public abstract void Die(int killedby);
}
