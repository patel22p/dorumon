using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{

    void Start()
    {
        _Name = "Базука";
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
        CallRPC(pos, rot);
        PlaySound("rocklf1a",.5f);
        ((GameObject)Instantiate(Resources.Load("Prefabs/BulletBazoka"), pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}