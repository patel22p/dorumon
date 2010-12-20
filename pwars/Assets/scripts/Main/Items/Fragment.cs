using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Fragment : MonoBehaviour
{
    public bool first;
    public List<Transform> child = new List<Transform>();
    public void Explosion(Vector3 pos, float power, float radius)
    {
        if (rigidbody != null)
            rigidbody.AddExplosionForce(power, pos, radius);
        
        if (transform.childCount > 0 && Vector3.Distance(transform.position, pos) < radius)
        {
            Break();
            foreach (Transform t in child)
            {
                t.rigidbody.AddExplosionForce(power, pos, radius);
                Fragment f = t.GetComponent<Fragment>();
                if (f != null)
                {
                    f.Explosion(pos, power, radius/2);
                }
            }
            Destroy(this.gameObject);
        }
    }
    
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.impactForceSum.magnitude > 20 && transform.childCount > 0 && gameObject.active)
        {            
            BreakAndDestroy();
        }
    }
    public void BreakAndDestroy()
    {
        Break();
        Destroy(this.gameObject);
    }
    private void Break()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform t in child)
            {
                t.gameObject.active = true;
                //t.renderer.material.color = renderer.material.color;
                //t.parent = _Game.effects.transform;
                Destroy(t.gameObject, 5);
                
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
