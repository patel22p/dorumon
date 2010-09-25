using UnityEngine;
using System.Collections;

public class GunHealth : GunBase
{
    public bool power;
    public Renderer render;
    public Light render1;
    void Start()
    {
        _Name = lc.heathgun.ToString();
    }
    protected override void Update()
    {
        if (power)
        {
            foreach (IPlayer p in _Spawn.iplayers)
                if (_TimerA.TimeElapsed(500) && isOwner && !p.isOwner && !(p is Zombie) && p.enabled && Vector3.Distance(p.transform.position, this.cursor.position) < 3)
                {
                    p.RPCHealth();
                }

            render.enabled = render1.enabled = true;
        }
        else
            render.enabled = render1.enabled = false;
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
        CallRPC(false, enable);
        power = enable;
    }

}
