using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    //public Transform blood;
    //public Transform bloodmat1;
    //public Transform impact;
    
    public float exp=50;
    public float exprad = 2; 
    protected override void Hit(RaycastHit hit )
    {

        base.Hit(hit);
        
        Transform b = Root(hit.collider.gameObject.transform);
        if (b.rigidbody != null)
            b.rigidbody.AddForceAtPosition(transform.rotation * new Vector3(0, 0, exp), hit.point);
 
        
        Destroy(gameObject);
        IPlayer iplayer = Root(hit.collider.gameObject).GetComponent<IPlayer>();
        //if (iplayer as CarController != null)
        //{
        //    Transform a;
        //    a = (Transform)Instantiate(decal, hit.point, Quaternion.LookRotation(hit.normal));
        //    a.parent = ((CarController)iplayer).effects;                
        ////}
        if (iplayer as Player != null || iplayer as Zombie != null)
            Instantiate(Resources.Load("Impact"), hit.point, transform.rotation);
        else if (iplayer as IPlayer != null)
            Destroy(Instantiate(Resources.Load("particle_metal"), hit.point, transform.rotation),1);
        else
            Destroy(Instantiate(Resources.Load("particle_concrete"), hit.point, transform.rotation),1);
        

        //if (iplayer as Zombie != null)
        //{
        //    Transform a;
        //    Destroy(a = (Transform)Instantiate(bloodmat1, hit.point, transform.rotation), 10);
        //     a.parent =iplayer.transform;
        //}

        if (iplayer != null && iplayer.isController && !iplayer.dead)
        {
            iplayer.RPCSetLife(-damage, OwnerID);
        }
        
    }

    protected override void FixedUpdate()
    {

        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }


}