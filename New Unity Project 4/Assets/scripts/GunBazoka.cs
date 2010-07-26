using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{
    
    
    
    protected override void OnUpdate()
    {
        base.OnUpdate();        
    }
    
        
    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }
    
    public LayerMask Default;
    protected override void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);        
    }

    

    [RPC]
    private void RPCShoot(Vector3 pos,Quaternion rot)
    {        
        CallRPC(pos, rot);
        ((Transform)Instantiate(_Patron, pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}