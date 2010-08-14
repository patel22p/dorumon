using UnityEngine;
using System.Collections;

public class Explosion : Base
{

    public float maxdistance = 9;
    public int maxdamage = 40;
    
    void Start()
    {

        foreach (IPlayer ip in GameObject.FindObjectsOfType(typeof(IPlayer)))
        {
            float dist = Vector3.Distance(ip.transform.position, transform.position);
            if (dist < maxdistance && ip.isOwnerOrNull && !ip.isdead)
            {
                ip.killedyby = OwnerID.Value;
                ip.RPCSetLife(ip.Life - maxdamage);
            }
        }
        //Tower t = Find<Tower>();
        //dist = Vector3.Distance(t.transform.position, transform.position);
        //if (dist < maxdistance)
        //    t.RPCSetLife(t.Life - maxdamage);
    }


}
