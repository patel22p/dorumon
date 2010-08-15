using System;
using System.Collections.Generic;

using UnityEngine;

public class GunPhysix : GunBase
{
    public float gravitaty = 1;
    public float radius = 50;
    public float exp = 2000;
    public float expradius = 40;

    public bool power;
    protected override void FixedUpdate()
    {
        if (power)
        {
            foreach (Box b in GameObject.FindObjectsOfType(typeof(Box)))
                if (b.GetType() == typeof(Box))
                {
                    b.rigidbody.AddExplosionForce(-gravitaty, cursor.position, radius);
                    b.rigidbody.velocity *= .97f;
                    b.OwnerID = Root(this.gameObject).GetComponent<Player>().OwnerID;
                }
        }
        base.Update();
    }

    [RPC]
    public void RPCSetPower(bool enable) 
    {
        CallRPC(true,enable);
        power = enable;
        if (!enable)
        {            
            foreach (Box b in GameObject.FindObjectsOfType(typeof(Box)))
                if (typeof(Box) == b.GetType() && Vector3.Distance(b.transform.position, cursor.position) < expradius)
                {
                    b.rigidbody.AddForce(this.transform.rotation  * new Vector3(0,0,exp));
                }

        }
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
