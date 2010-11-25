using UnityEngine;
using System.Collections;

public class Explosion : Base
{
    public Box self;
    public int maxdamage;
    public float exp = 500;
    public float radius = 4;
    void Start()
    {
        maxdamage = 61;
        foreach (IPlayer ip in GameObject.FindObjectsOfType(typeof(IPlayer)))
        {            
            
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            if (ip != self && dist < radius && ip.isController && !ip.dead)
            {                
                if (ip.isOwner)
                    _Cam.exp = 1;                
                ip.RPCSetLife(ip.Life - maxdamage,OwnerID);
            }
        }
        foreach (Box b in GameObject.FindObjectsOfType(typeof(IPlayer)))
            if (b != self)
            {
                print("pos" + b.transform.position);
                b.rigidbody.AddExplosionForce(exp, transform.position, radius);
            }
    }
    


}
