using UnityEngine;
using System.Collections;

public class Explosion : bs
{

    public Shared self;
    public AnimationCurve damage;
    public float exp = 500;
    public float radius = 4;
    public float DamageFactor = 1;

    public void Start()
    {
        foreach (Destroible ip in GameObject.FindObjectsOfType(typeof(Destroible)))
        {
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            if (ip != self && dist < radius && ip.isController && ip.Alive)
            {
                if (ip.isOwner)
                    _Cam.exp = 1;
                ip.RPCSetLife(ip.Life - damage.Evaluate(dist) * DamageFactor / 5 * mapSettings.damageFactor, OwnerID);
            }
        }
        //_TimerA.AddMethod(delegate
        //{
        foreach (Shared b in GameObject.FindObjectsOfType(typeof(Shared)))
            if (b != self && (!(b is Player) || ((Player)b).Alive))
            {
                b.rigidbody.AddExplosionForce(exp * b.rigidbody.mass / b.GetComponentInChildren<Collider>().bounds.size.sqrMagnitude * 3 * fdt, transform.position, radius);
            }
        //});
    }
}
