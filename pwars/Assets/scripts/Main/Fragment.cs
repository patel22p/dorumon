using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Fragment : Base2
{
    void Start()
    {
        _Game.fragments.Add(this);
    }
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.impactForceSum.magnitude > 10 && transform.childCount > 0 && gameObject.active)
    //        BreakAndDestroy();
    //}
    public void Explosion(Vector3 pos, float power, float radius)
    {
        if (rigidbody != null)
            rigidbody.AddExplosionForce(power, pos, radius);
        if (transform.childCount > 0 && Vector3.Distance(transform.position, pos) < radius)
        {
            Break();
            foreach (Transform t in transform)
            {
                t.rigidbody.AddExplosionForce(power, pos, radius);
                Fragment f = t.GetComponent<Fragment>();

                if (f != null)
                {
                    //print("exp");
                    f.Explosion(pos, power, radius);
                }
            }
            transform.DetachChildren();
            Destroy(this.gameObject);

        }
    }
    public void BreakAndDestroy()
    {
        Break();
        if (transform.childCount > 0)
        {
            transform.DetachChildren();
            Destroy(this.gameObject);
        }
    }
    private void Break()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform t in transform)
            {                
                t.gameObject.active = true;
                Rigidbody r = t.gameObject.AddComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    r.velocity = rigidbody.velocity;
                    r.angularVelocity = rigidbody.angularVelocity;
                    r.mass = rigidbody.mass;
                }
                //foreach (ContactPoint cp in collision.contacts)
                //    r.AddExplosionForce(20, cp.point, 20);
            }
            
        }
    }
}
