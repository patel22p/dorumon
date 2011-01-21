using UnityEngine;
using System.Collections;

public class Explosion : Base
{

    public Shared self;
    public AnimationCurve damage;
    public float exp = 500;
    public float radius = 4;


    protected override void Start()
    {
        foreach (Destroible ip in GameObject.FindObjectsOfType(typeof(Destroible)))
        {
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            if (ip != self && dist < radius && ip.isController && ip.Alive)
            {
                if (ip.isOwner)
                    _Cam.exp = 1;
                ip.RPCSetLife(ip.Life - damage.Evaluate(dist) * mapSettings.damageFactor, OwnerID);
            }
        }
        //_TimerA.AddMethod(delegate
        //{
        foreach (Shared b in GameObject.FindObjectsOfType(typeof(Shared)))
            if (b != self && (!(b is Player) || ((Player)b).Alive))
            {
                b.rigidbody.AddExplosionForce(exp * b.rigidbody.mass * fdt, transform.position, radius);
            }
        //});
    }
}
