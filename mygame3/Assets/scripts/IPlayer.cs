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
    public GunBase[] guns { get { return this.GetComponentsInChildren<GunBase>(); } }
    internal Team? team
    {
        get
        {
            if (OwnerID == null) return null;
            else return _Spawn.players[OwnerID.Value].team;
        }
    }
    
    public override void Dispose()
    {
        _Spawn.iplayers.Remove(this);
    }
    protected override void Start()
    {
        _Spawn.iplayers.Add(this);
        base.Start();
    }

    //float shownicktime;
    bool IsPointed()
    {
        RaycastHit rah = ScreenRay();
        if (rah.collider == null) return false;
        return rah.collider.gameObject == this.gameObject;
    }
    protected override void Update()
    {
        if(isOwner)
            nitro += Time.deltaTime / 5;

        if (mesh != null)
        {
            if (OwnerID != null && (team == Team.ata || team == Team.def))
                mesh.renderer.material.color = team == Team.ata ? Color.red : Color.blue;
            else
                mesh.renderer.material.color = Color.white;
        }
        
        if (title != null && _TimerA.TimeElapsed(5000))
        {
            if (OwnerID != null && !isOwner && _LocalPlayer != null && ((_LocalPlayer.team == team && !dm) || IsPointed()))
                title.GetComponent<TextMesh>().text = _Spawn.players[OwnerID.Value].Nick;
            else
                title.GetComponent<TextMesh>().text = "";
        }
        base.Update();
        
    }
    public float nitro =10;
    [RPC]
    public virtual void RPCSetLife(int NwLife, NetworkPlayer killedby)
    {
        
        if (!enabled) return;        
        CallRPC(true, NwLife,killedby);

        if (killedby == de || players[killedby].team != team)
            Life += NwLife;

        if (Life < 0)
            Die(killedby);
    }
    
    [RPC]
    public abstract void Die(NetworkPlayer killedby);


    
}
