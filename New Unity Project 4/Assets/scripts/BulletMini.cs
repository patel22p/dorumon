﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    float rigidbodyPushForce=20;
    public Transform impact;
    protected override void Hit(RaycastHit hit )
    {        
        
        Destroy(Instantiate(impact, hit.point, transform.rotation), 3);
        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(rigidbodyPushForce * transform.TransformDirection(Vector3.forward), hit.point);
        }
         
        Destroy(gameObject);
        IPlayer iplayer = hit.collider.GetComponent<IPlayer>();        
        if (iplayer != null && !iplayer.isdead && iplayer.isOwner)
        {
            iplayer.killedyby = OwnerID.Value;
            iplayer.RPCSetLife(iplayer.Life - damage);
        }
    }

    protected override void OnFixedUpdate()
    {
        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.OnFixedUpdate();
    }


}