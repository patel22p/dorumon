using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public abstract class IPlayer : Base
{
    public Transform title;
    public NetworkPlayer killedyby;
    public int Life;
    public bool isdead { get { return !enabled; } }
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    
    
    [RPC]
    protected override void OnUpdate()
    {

        if (OwnerID != null && !isOwner && _localPlayer.team == Player.players[OwnerID.Value].team)
            title.GetComponent<TextMesh>().text = Player.players[OwnerID.Value].Nick;
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
        CallRPC(true,NwLife);        
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;
    }
    public abstract Vector3 SpawnPoint();
    [RPC]
    public abstract void RPCDie();

}
