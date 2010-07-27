using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public class IPlayer :Base
{
    public TimerA _TimerA { get { return Find<GuiFpsCounter>().timer; } }
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

  
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    public virtual void RPCDie()
    {        
    }

}
