using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBazoka : BulletBase
{
    
    public Transform exp;
    protected NetworkPlayer killby;
    public float maxdistance = 30;
    public float maxdamage = 60;
    public float velocity;

    protected override void FixedUpdate()
    {
        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }


    protected override void Hit(RaycastHit h)
    {
        Vector3 vector3 = h.point;
        Object dt = (Object)Instantiate(exp, vector3, Quaternion.identity);
        Destroy(dt, 10);
        Destroy(gameObject);        
        float dist = Vector3.Distance(LocalPlayer.transform.position, vector3);
        int damage = (int)(Mathf.Max(maxdistance - dist, 0) * maxdamage);        
        if (!LocalPlayer.isdead && damage != 0)
        {            
            LocalPlayer.killedyby = OwnerID.Value;           
            LocalPlayer.RPCSetLife(LocalPlayer.Life - damage);
        }
    }

}
