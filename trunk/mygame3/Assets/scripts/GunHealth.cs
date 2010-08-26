using UnityEngine;
using System.Collections;

public class GunHealth : GunBase
{
    public bool power;
    public Renderer render;
    protected override void Update()
    {
        if (power)
        {
            foreach (IPlayer p in _Spawn.iplayers)
                if (!(p is Zombie) && p.enabled && Vector3.Distance(p.transform.position, this.cursor.position) < 3)
                {
                    if (!p.isOwner && _TimerA.TimeElapsed(500)) p.RPCHealth();
                }
            render.enabled = true;
        }
        else
            render.enabled = false;
        base.Update();
    }
    protected override void LocalUpdate()
    {

        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0))
                RPCSetPower(false);
        }

    }
    [RPC]
    public void RPCSetPower(bool enable)
    {
        CallRPC(true, enable);
        power = enable;
    }

}
