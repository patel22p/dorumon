using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMini : BulletBase
{
    public float velocity = 100;
    public int damage=34;
    public float rigidbodyPushForce=20;
    public Transform impact;
    protected override void Hit(RaycastHit hit )
    {

        Destroy(Instantiate(impact, hit.point, transform.rotation), 5);
        if (hit.rigidbody)
        {
            Trace.Log(rigidbodyPushForce);
            hit.rigidbody.AddForceAtPosition(rigidbodyPushForce * transform.TransformDirection(Vector3.forward), hit.point);
        }
        Destroy(gameObject);

        Player localPlayer = hit.collider.GetComponent<Player>();
        if (localPlayer != null && !localPlayer.isdead && localPlayer.isMine)
        {
            localPlayer.killedyby = OwnerID.Value;
            localPlayer.RPCSetLife(localPlayer.Life - damage);
        }
    }

    protected override void FixedUpdate()
    {
        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }


}