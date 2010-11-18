using UnityEngine;
using System.Collections;
public class GunGranate : GunBase
{

    public void Start()
    {
        _Name = "Граната";
        print();
    }
    [RPC]
    protected override void RPCShoot(Vector3 vector3, Quaternion quaternion)
    {
        PlaySound("sounds/granate",3);
        Base b = ((GameObject)Instantiate(Load("Prefabs/bulletgranate"), vector3, quaternion)).GetComponent<Base>();
        b.OwnerID = OwnerID;
        b.rigidbody.AddForce(this.transform.rotation * force);
    }
    public Vector3 force = new Vector3(0, -1000, 20000);
}