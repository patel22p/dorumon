using System;
using System.Collections.Generic;
using UnityEngine;
using doru;
public class IPlayer :Base
{
    TimerA _TimerA { get { return Find<GuiFpsCounter>().timer; } }
    public bool isdead { get { return !enabled; } }
    public NetworkPlayer killedyby;
    public int Life;
    public Player localPlayer { get { return Find<Player>("LocalPlayer"); } }
    [RPC]
    public virtual void RPCSetLife(int NwLife)
    {
        CallRPC(NwLife);        
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;

    }

    public virtual void RPCSpawn()
    {
        

    }
    public GunBase[] gunlist { get { return this.GetComponentsInChildren<GunBase>(); } }
    public virtual void RPCDie()
    {

        if (isMine)
        {
            _TimerA.AddMethod(2000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if (p.OwnerID == killedyby)
                {
                    if (p.isMine)
                        localPlayer.networkView.RPC("RPCSetScore", RPCMode.All, localPlayer.score - 1);
                    else
                        p.networkView.RPC("RPCSetScore", RPCMode.All, p.score + 1);
                }
        }
        Show(false);

    }

}
