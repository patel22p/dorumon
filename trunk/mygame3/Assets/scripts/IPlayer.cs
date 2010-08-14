using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public abstract class IPlayer : Box
{
    public Transform title;
    public Transform mesh;
    public NetworkPlayer killedyby;
    public int Life;
    public bool isdead { get { return !enabled; } }
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    
    
    
    protected virtual void Update()
    {
        Team team = Team.ata;
        if (mesh != null)
        {
            if (OwnerID != null)
            {
                team = _Spawn.players[OwnerID.Value].team;
                mesh.renderer.material.color = team == Team.ata ? Color.red : Color.blue;
            }
            else
                mesh.renderer.material.color = Color.white;
        }

        if (OwnerID != null && !isOwner && _localPlayer != null && _localPlayer.team == team)
            title.GetComponent<TextMesh>().text = _Spawn.players[OwnerID.Value].Nick;
        else
            title.GetComponent<TextMesh>().text = "";
        if (!GameObject.Find("bounds").collider.bounds.Contains(this.transform.position) && !isdead)
        {
            transform.position = SpawnPoint();
            rigidbody.velocity = Vector3.zero;
        }
    }
    [RPC]
    public virtual void RPCSetLife(int NwLife)
    {
        CallRPC(true, NwLife);        
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;
    }
    public abstract Vector3 SpawnPoint();
    [RPC]
    public abstract void RPCDie();

}
