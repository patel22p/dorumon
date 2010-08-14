using System;
using System.Collections.Generic;

using UnityEngine;

public class GunPhysix : GunBase
{
    public float gravitaty = 1;
    public float radius = 50;
    protected override void Update()
    {
        if (power)
        {
            foreach (Box b in GameObject.FindObjectsOfType(typeof(Box)))
                if (!b.isOwner)
                {
                    b.rigidbody.AddExplosionForce(-gravitaty, cursor.position, radius);
                }
        }
        base.Update();
    }

    bool power;
    protected override void FixedUpdate()
    {

        base.FixedUpdate();
    }

    [RPC]
    public void RPCSetPower(bool enable)
    {
        CallRPC(true,enable);
        power = enable;
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

    

}
