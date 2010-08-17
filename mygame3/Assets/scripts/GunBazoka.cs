using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{
    
    
    
    protected override void Update()
    {
        base.Update();        
    }
    
        
    protected override void FixedUpdate()
    {

        base.FixedUpdate();
    }
    
    
    protected override void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);        
    }

    

    [RPC]
    private void RPCShoot(Vector3 pos,Quaternion rot)
    {
        GetComponentInChildren<AudioSource>().Play();
        CallRPC(false,pos, rot);
        ((Transform)Instantiate(_Patron, pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}