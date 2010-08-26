using System;

using System.Collections.Generic;
using UnityEngine;
using doru;
public abstract class IPlayer : box
{
    public Transform title;
    public Transform mesh;

    public int Life;
    public Transform CamPos;
    public bool isdead { get { return !enabled; } }
    public GunBase[] guns;
    internal Team? team
    {
        get
        {

            if (OwnerID == -1) return null;
            else return _Spawn.players[OwnerID.GetHashCode()].team;
        }
    }
    [RPC]
    public virtual void RPCHealth()
    {        
        
    }
    public override void Dispose()
    {
        _Spawn.iplayers.Remove(this);
        base.Dispose();
    }
    void Awake()
    {
        guns = this.GetComponentsInChildren<GunBase>(); 
    }
    protected override void Start()
    {
        
        _Spawn.iplayers.Add(this);
        base.Start();
    }

    float shownicktime;
    bool IsPointed()
    {
        RaycastHit rah = ScreenRay();
        if (rah.collider == null) return false;
        return rah.collider.gameObject == this.gameObject;
    }
    protected override void Update()
    {
        if (isOwner)
            nitro += Time.deltaTime / 5;

        if (mesh != null)
        {
            if (OwnerID != -1 && (team == Team.ata || team == Team.def))
                mesh.renderer.material.color = team == Team.ata ? Color.red : Color.blue;
            else
                mesh.renderer.material.color = Color.white;
        }

        if (title != null && _TimerA.TimeElapsed(50))
        {
            
            if (!isOwner && _LocalPlayer != null && ((_LocalPlayer.team == team && !dm) || IsPointed()))
                shownicktime = 2;
            if (OwnerID != -1 && shownicktime > 0)
                title.GetComponent<TextMesh>().text = _Spawn.players[OwnerID].Nick + ":" + Life;
            else
                title.GetComponent<TextMesh>().text = "";
        }
        
        shownicktime -= Time.deltaTime;
        base.Update();        
    }
    public float nitro =10;
    [RPC]
    public virtual void RPCSetLife(int NwLife, int killedby)
    {
        
        if (!enabled) return;        
        CallRPC(true, NwLife,killedby);

        if (isEnemy(killedby))
            Life += NwLife;

        if (Life < 0)
            Die(killedby);
    }
    public bool isEnemy(int killedby) { return killedby != OwnerID && (killedby == -1 || players[killedby].team != team || dm); }
    [RPC]
    public abstract void Die(int killedby);


    
}
