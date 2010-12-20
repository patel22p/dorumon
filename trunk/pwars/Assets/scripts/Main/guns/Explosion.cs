using UnityEngine;
using System.Collections;

public class Explosion : Base
{
    public Shared self;
    public int damage;
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
                ip.RPCSetLife(ip.Life - damage,OwnerID);
            }
        }
        _TimerA.AddMethod(delegate
        {
            foreach (Shared b in GameObject.FindObjectsOfType(typeof(Shared)))
                if (b != self)
                {
                    b.rigidbody.AddExplosionForce(exp * b.rigidbody.mass, transform.position, radius);
                }
        });
        foreach (Fragment f in FindObjectsOfType(typeof(Fragment)))
        {

            if(f!=null)
                f.Explosion(transform.position, exp, radius);
        }
    }
}
