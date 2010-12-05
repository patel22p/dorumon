using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Fragment : Base
{
    public bool first;
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
                    f.Explosion(pos, power, radius/2);
                }
            }
            transform.DetachChildren();
            Destroy(this.gameObject);

        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.impactForceSum.magnitude > 10 && transform.childCount > 0 && gameObject.active)
            BreakAndDestroy();
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
                    r.mass = rigidbody.mass *.8f;
                }
            }
            
        }
    }
}
