using UnityEngine;
using System.Collections;
public class GunGranate : GunBase
{
    BulletGranate bulletGranate;
    public void Start()
    {        
        _Name = "Граната";
    }
    protected override void Awake()
    {
        bulletGranate = transform.Find("bulletgranate").GetComponent<BulletGranate>();
    }
    protected override void LocalShoot()
    {
        if (!bulletGranate.enabled)
        {
            base.LocalShoot();
        }
    }
    [RPC]
    protected override void RPCShoot(Vector3 vector3, Quaternion quaternion)
    {
        CallRPC(vector3, quaternion);
        PlaySound("granate", 3);
        bulletGranate.transform.parent = null;
        bulletGranate.rigidbody.isKinematic = false;
        bulletGranate.rigidbody.detectCollisions = true;
        bulletGranate.enabled = true;
        bulletGranate.OwnerID = OwnerID;
        bulletGranate.StartGranate();
        
        bulletGranate.rigidbody.AddForce(this.transform.rotation * force);
    }
    public override void onShow(bool enabled)
    {
        base.onShow(enabled);
    }
    public Vector3 force = new Vector3(0, -1000, 20000);
}