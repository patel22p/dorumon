using UnityEngine;
using System.Collections;
public class GunMini : GunBase
{

    public void Start()
    {
        _Name = lc.gunmini.ToString();
        fire =transform.GetComponentsInChildren<ParticleEmitter>();
        
    }
    ParticleEmitter[] fire;    
    public Light light1;
    public Transform gilza;
    public Transform gilzaPlaceHolder;
    [RPC]
    protected override void RPCShoot(Vector3 vector3, Quaternion quaternion)
    {        
        CallRPC(false, vector3, quaternion);
        //if (gilza != null)
        //{
        //    GameObject a = ((Transform)Instantiate(gilza, gilzaPlaceHolder.position, gilzaPlaceHolder.rotation)).gameObject;
        //    a.transform.parent = _Spawn.effects;
        //    Destroy(a, 1);
        //}
        foreach (ParticleEmitter p in fire)
            p.Emit();
        light1.enabled = true;
        _TimerA.AddMethod(100, delegate {
            light1.enabled = false;
        });
        root.audio.PlayOneShot((AudioClip)Resources.Load(" sounds/ARshoot1a_2D"));
        ((Transform)Instantiate(_Patron, vector3, quaternion)).GetComponent<Base>().OwnerID = OwnerID;
    }
}