using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public class IPlayer :Box
{
    public TimerA _TimerA { get { return Find<GuiHelper>().timer; } }
    public bool isdead { get { return !enabled; } }
    public NetworkPlayer killedyby;

    protected Vector3 spawnpos;
    public int Life;
    public Player localPlayer { get { return Find<Player>("LocalPlayer"); } }
    [RPC]
    public virtual void RPCSetLife(int NwLife)
    {
        CallRPC(true,NwLife);        
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;
    }
    public static Spawn spawn { get { return Find<Spawn>(); } }
    public static Vector3 SpawnPoint()
    {
        return spawn.transform.GetChild(UnityEngine.Random.Range(0, spawn.transform.childCount)).transform.position;
    }

    protected override void OnUpdate()
    {
        if (!GameObject.Find("bounds").collider.bounds.Contains(this.transform.position) && !isdead)
        {
            transform.position = SpawnPoint();
            rigidbody.velocity = Vector3.zero;
        }
    }
    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        Base b =collisionInfo.gameObject.GetComponent<Base>();
        
        if (b!=null &&
            b.OwnerID != null && !b.isOwner &&
            collisionInfo.rigidbody != null &&
            collisionInfo.impactForceSum.sqrMagnitude > 50 &&
            rigidbody.velocity.magnitude < collisionInfo.rigidbody.velocity.magnitude)
        {
            RPCSetLife(Life - (int)collisionInfo.impactForceSum.sqrMagnitude / 2);
        }

    }
  
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    public virtual void RPCDie()
    {        
    }

}
