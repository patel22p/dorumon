using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BulletGranate : BulletBase
{
    public float velocity;
    public float gravitaty = 1;
    public float scalefactor = 10;
    public float radius = 50;
    protected override void Start()
    {
        base.Start();
        _TimerA.AddMethod(3000, Bum);
        _Game.dynamic.Add(this);
    }
    protected override void FixedUpdate()
    {        
        foreach (var b in _Game.dynamic.Union(_Game.zombies.Cast<Base>()))
        {
            if (b.GetType() == typeof(Box) || b is Zombie)
            {
                b.rigidbody.AddExplosionForce(-gravitaty * scalefactor * b.rigidbody.mass, transform.position, radius);
                b.rigidbody.angularDrag = 30;
                b.rigidbody.velocity *= .97f;
                b.OwnerID = OwnerID;
            }
        }
    }

    public void Bum()
    {
        
        GameObject o;
        Destroy(o = (GameObject)Instantiate(Resources.Load("Detonator/Prefab Examples/Detonator-Sounds"), this.transform.position, Quaternion.identity), 10);
        o.AddComponent<Explosion>().exp = 300;
        o.GetComponent<Detonator>().size = 8f;
        o.GetComponent<Detonator>().autoCreateForce = false;
        o.GetComponent<Explosion>().OwnerID = OwnerID;
        Transform a;
        Destroy((a = (Transform)Instantiate(_Game.decal, this.transform.position, Quaternion.identity)), 10);
        a.parent = _Game.effects;
        _Game.dynamic.Remove(this);
        Destroy(gameObject);        
    }
    //protected override void Hit(RaycastHit hit)
    //{
    //    base.Hit(hit);
    //    Vector3 vector3 = hit.point - this.transform.rotation * new Vector3(0, 0, 1);
    

    //    o.GetComponent<Explosion>().OwnerID = OwnerID;

    //    if (hit.collider.gameObject.isStatic)
    //    {
    
    //    }

    //    Destroy(gameObject);        
    //}

}
