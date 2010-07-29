using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public class IPlayer :Base
{
    public TimerA _TimerA { get { return Find<GuiHelper>().timer; } }
    public bool isdead { get { return !enabled; } }
    public NetworkPlayer killedyby;
            
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
