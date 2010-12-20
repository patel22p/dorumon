using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Fragment : MonoBehaviour
{
    public List<Transform> child = new List<Transform>();
    void Awake()
    {
        if(renderer.lightmapIndex == -1)
            Base.UpdateLightmap(renderer.materials, transform.position);
    }
    public void Explosion(Vector3 pos, float power, float radius)
    {
        if (rigidbody != null)
            rigidbody.AddExplosionForce(power, pos, radius);


        if (Vector3.Distance(transform.position, pos) < radius)
        {
            BreakAndDestroy();
            foreach (Transform t in child)
            {
                if (t.rigidbody != null)
                    t.rigidbody.AddExplosionForce(power, pos, radius);
                Fragment f = t.GetComponent<Fragment>();

                f.Explosion(pos, power, radius / 2);
            }
        }
    }
    public GameObject partcl;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.impactForceSum.magnitude > 20 && transform.childCount > 0 && gameObject.active)
        {
            if (rigidbody != null)
                rigidbody.velocity = -collision.impactForceSum / 2;
            BreakAndDestroy();
        }
    }
    public void BreakAndDestroy()
    {
        Break();
        if (child.Count > 0)
            Destroy(this.gameObject);
    }
    public int level;
    bool broken;
    private void Break()
    {
        if (broken) Debug.Log("already broken" + level + "+" + name);
        broken = true;
        transform.DetachChildren();
        foreach (Transform t in child)
        {
            t.gameObject.active = true;
            t.transform.position += Random.insideUnitSphere / 10;
            Destroy(t.gameObject, 10);
            GameObject g = (GameObject)Instantiate(partcl, t.position, Quaternion.LookRotation(Random.onUnitSphere));
            g.transform.parent = t;
            Destroy(g, .7f);
            if (level > 0)
            {
                Rigidbody r = t.gameObject.AddOrGet<Rigidbody>();
                if (rigidbody != null)
                {

                    r.velocity = rigidbody.velocity;
                    r.angularVelocity = rigidbody.angularVelocity;
                    r.mass = rigidbody.mass * .8f;

                }
            }
        }
    }
}
