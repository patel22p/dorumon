using UnityEngine;
using System.Collections;

public class Explosion : Base {

    public float maxdistance = 9;
    public int maxdamage = 40;
	// Use this for initialization
    protected override void OnStart()
    {

        float dist = Vector3.Distance(LocalIPlayer.transform.position, transform.position);

        if (!LocalIPlayer.isdead && dist < maxdistance)
        {
            LocalIPlayer.killedyby = OwnerID.Value;
            LocalIPlayer.RPCSetLife(LocalIPlayer.Life - maxdamage);
        }

        Tower t = Find<Tower>();
        dist = Vector3.Distance(t.transform.position, transform.position);
        if (dist < maxdistance)
            t.RPCSetLife(t.Life - maxdamage);
    }


}
