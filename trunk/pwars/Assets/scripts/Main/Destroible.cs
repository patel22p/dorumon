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
    public bool Alive;
    
    
    protected override void Awake()
    {        
        base.Awake();
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        base.OnPlayerConnected1(np);        
    }
    protected override void Start()
    {
        _Game.destroyables.Add(this);
        base.Start();
    }
    
    [RPC]
    public virtual void RPCSetLife(int NwLife, int killedby)
    {
        if (dead) return;        
        if(isController) if(CallRPC(NwLife,killedby)) return;

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
    [RPC]
    public abstract void RPCDie(int killedby);
}
