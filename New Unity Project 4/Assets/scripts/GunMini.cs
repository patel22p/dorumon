using UnityEngine;
using System.Collections;
public class GunMini : GunBase
{
    
    
    
    protected override void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);        
    }

    [RPC]
    private void RPCShoot(Vector3 vector3, Quaternion quaternion)
    {
        CallRPC(false,vector3, quaternion);
        ((Transform)Instantiate(_Patron, vector3, quaternion)).GetComponent<Base>().OwnerID = OwnerID;
    }
}