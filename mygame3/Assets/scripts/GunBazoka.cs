using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{

    void Start()
    {
        _Name = lc.baz.ToString();
    }
    
    protected override void Update()
    {
        base.Update();        
    }
    
        
    protected override void FixedUpdate()
    {

        base.FixedUpdate();
    }
    

    [RPC]
    protected override void RPCShoot(Vector3 pos,Quaternion rot)
    {
        
        CallRPC(false, pos, rot);
        root.audio.PlayOneShot((AudioClip)Resources.Load(" sounds/rocklf1a"));        
        ((Transform)Instantiate(_Patron, pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}