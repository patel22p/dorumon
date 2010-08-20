using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    public Transform blood;
    public Transform impact;
    public float exp=10;
    public float exprad = 2;
    protected override void Hit(RaycastHit hit )
    {

        base.Hit(hit);
        Destroy(Instantiate(impact, hit.point, transform.rotation), 3);
        Transform b = Root(hit.collider.gameObject.transform);
        if (b.rigidbody != null)
            b.rigidbody.AddForceAtPosition(transform.rotation * new Vector3(0, 0, exp), hit.point);
 
        
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
        if (iplayer != null && iplayer.isController && !iplayer.isdead)
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