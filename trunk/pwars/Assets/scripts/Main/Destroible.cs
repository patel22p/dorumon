using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
using System.Collections;

public abstract class Destroible : Shared
{
    internal float isGrounded;
    public bool Alive = true;

    public float Life;
    internal bool frozen;
    private float freezedt;
    public float MaxLife = 100;
    public GameObject model;
    public float slowdowntime = 1;
    [FindAsset] public AudioClip heal;
    [FindTransform]
    public TextMesh title;

    public Team? team
    {
        get
        {
            if (OwnerID == -1) return null;
            else return _Game.players[OwnerID.GetHashCode()].team;
        }
    }

    private void UpdateTitle()
    {
        if (OwnerID != -1 && (team == Team.Red || team == Team.Blue))
            title.renderer.material.color = (team == Team.Red ? Color.red : Color.blue) * .5f;
        else
            title.renderer.material.color = Color.green * .5f;

        if (!_localPlayer.isEnemy(OwnerID))
            title.text = "id:" + isController + " " + players[OwnerID].nick + ":" + (int)Life;        
    }

    public void SetLayer(int layer)
    {
        foreach (var t in transform.GetTransforms())
            t.gameObject.layer = layer;
    }

    public void SetActive(GameObject g, bool value)
    {
        foreach (var a in g.transform.GetTransforms())
            a.gameObject.active = value;
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
        base.Start();
    }

    protected virtual void OnCollisionEnter(Collision collisionInfo)
    {
        if (Alive && isController)
        {
            Box b = collisionInfo.gameObject.GetComponent<Box>();
            if (b != null)
            {
                if ((this.isEnemy(b.OwnerID) || b.OwnerID == _localPlayer.OwnerID) &&
                    collisionInfo.rigidbody.velocity.magnitude > 20)
                    RPCSetLifeLocal(
                        Life - (int)collisionInfo.rigidbody.velocity.magnitude * 10 * _Game.mapSettings.damageFactor,
                        b.OwnerID);
            }
        }
    }

    public virtual void RPCSetFrozen(bool value)
    {
        if (slowdowntime == 0) return;
        if (value)
            freezedt = slowdowntime;
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

        if (title != null && _localPlayer != this)
            UpdateTitle();        

        isGrounded += Time.deltaTime;
        base.Update();
    }

    public void RPCHeal(float life)
    {
        CallRPC("Heal", life);
    }

    [RPC]
    public void Heal(float life)
    {
        PlaySound(heal);
        if (isController)
            RPCSetLifeLocal(Life + 10, -1);
    }

    public virtual void RPCSetLifeLocal(float NwLife, int killedby)
    {
        if (!Alive) return;
        if (isController)
        {
            if (debug && !(this is Zombie)) NwLife = Math.Max(1, NwLife);
            if (this == _localPlayer && NwLife < Life)
                _Cam.Hit();
            Life = Math.Min(NwLife, MaxLife);            
            RPCSetLife(Life, killedby);
            //if (slowdowntime != 0)
            //    RPCSetFrozen(true);

            if (Life <= 0)
                RPCDie(killedby);
        }
    }

    private void RPCSetLife(float NwLife, int killedby)
    {
        CallRPC("SetLife", Life, killedby);
    }

    [RPC]
    public virtual void SetLife(float NwLife, int killedby)
    {
        Life = NwLife;
    }
    public static bool isenemycheck;
    public virtual bool isEnemy(int id)
    {        
        if (this is Zombie) return true; if (isenemycheck) Debug.Log("-1");
        if (id == -1) return true; if (isenemycheck) Debug.Log("owner");
        if (id == OwnerID) return false; if (isenemycheck) Debug.Log("death");
        if (_Game.mapSettings.DeathMatch) return true; if (isenemycheck) Debug.Log("hz");
        if (id != -1 && players[id] != null && players[id].team != team) return true;
        return false;
    }

    public virtual void RPCDie(int killedby)
    {
        CallRPC("Die", killedby);
    }

    [RPC]
    public abstract void Die(int killedby);
}
