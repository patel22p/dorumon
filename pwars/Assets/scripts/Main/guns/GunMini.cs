using UnityEngine;
using System.Collections;
public class GunMini : GunBase
{
    public void Start()
    {
        _Name = "Mиниган";
        fire =transform.GetComponentsInChildren<ParticleEmitter>();
        
    }
    ParticleEmitter[] fire;    
    public Light light1;
    public Transform gilza;
    public Transform gilzaPlaceHolder;
    [RPC]
    protected override void RPCShoot(Vector3 vector3, Quaternion quaternion)
    {        
        CallRPC(vector3, quaternion);
        foreach (ParticleEmitter p in fire)
            p.Emit();
        light1.enabled = true;
        _TimerA.AddMethod(100, delegate {
            light1.enabled = false;
        });
        PlaySound("ARshoot1a_2D",.2f);
        ((GameObject)Instantiate(Load("BulletMini"), vector3, quaternion)).GetComponent<Base>().OwnerID = OwnerID;
    }
}