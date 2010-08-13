using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    public Transform blood;
    float rigidbodyPushForce=20;
    public Transform impact;
    
    
    protected override void Hit(RaycastHit hit )
    {

        base.Hit(hit);
        Destroy(Instantiate(impact, hit.point, transform.rotation), 3);        
        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(rigidbodyPushForce * transform.TransformDirection(Vector3.forward), hit.point);
        }        
        
        Destroy(gameObject);
        IPlayer iplayer = Root(hit.collider.gameObject).GetComponent<IPlayer>();
        if (iplayer as CarController != null)
        {
            Transform a;
            a = (Transform)Instantiate(decal, hit.point, Quaternion.LookRotation(hit.normal));
            a.parent = ((CarController)iplayer).effects;                
        }
        if (iplayer as Player != null)
        {
            Transform a;
            Destroy(a = (Transform)Instantiate(blood, hit.point, transform.rotation),10);
            a.parent = _Spawn.effects;
        }
        if (iplayer != null && !iplayer.isdead && iplayer.isOwner)
        {            
            iplayer.killedyby = OwnerID.Value;
            iplayer.RPCSetLife(iplayer.Life - damage);
        }
        
    }

    protected override void FixedUpdate()
    {

        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }


}