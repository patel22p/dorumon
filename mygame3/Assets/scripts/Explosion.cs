using UnityEngine;
using System.Collections;

public class Explosion : Base
{

    public float maxdistance = 9;
    internal int maxdamage = 61;
    public float r=.3f;
    void Start()
    {
        
        foreach (IPlayer ip in GameObject.FindObjectsOfType(typeof(IPlayer)))
        {            
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            if (dist < maxdistance && ip.isController && !ip.dead)
            {
                if (ip.isOwner)
                    _Cam.ran = r;                
                ip.RPCSetLife(- maxdamage,OwnerID);
            }
        }
    }


}
