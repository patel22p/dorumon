using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunPhysix : GunBase
{

    public float radius = 50;
    public float exp = 2000;
    public float expradius = 40;
    public float gravitaty = 1;
    public float scalefactor = 10;
    public float energy;
    public bool power;
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void FixedUpdate()
    {
        if (power)
        {
            patronsLeft-=Time.fixedDeltaTime;

            if (energy < exp) energy += 80;            
            foreach (Base b in _Game.boxDerived)
            {
                if (!(b is Destroible))
                {
                    b.rigidbody.AddExplosionForce(-gravitaty * scalefactor * b.rigidbody.mass, cursor[0].position, radius);
                    b.rigidbody.angularDrag = 30;
                    b.rigidbody.velocity *= .97f;
                    b.OwnerID = this.root.GetComponent<Player>().OwnerID;
                    AudioSource a = audio;
                    a.pitch = 0.1f + (energy / exp / 20);
                    if (!a.isPlaying) a.Play();
                }
            }
        }
        else
            audio.Stop();

    }

    private bool HitTest(Transform j,float dist)
    {        
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Vector3 dir = rot * new Vector3(0, 0, dist);
        j.gameObject.layer = LayerMask.NameToLayer("HitTest");
        bool hit = (Physics.CheckCapsule(pos, dir + pos, 1, 1 << LayerMask.NameToLayer("HitTest")));
        j.gameObject.layer = 0;
        return hit;
    }
    public override void Init()
    {
        base.Init();
        audio.clip = (AudioClip)FindObjectsOfTypeIncludingAssets(typeof(AudioClip)).FirstOrDefault(a => a.name == "PowerGun");
    }
    [LoadPath("wave")]
    public GameObject wavePrefab;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    public void RPCSetPower(bool e) { CallRPC("SetPower",e); }
    [RPC]
    void SetPower(bool e)
    {        
        power = e;
        if (!e)
        {
            bool any = false;
            foreach (Base b in _Game.boxDerived)
                if (!(b is Destroible) && Vector3.Distance(b.transform.position, cursor[0].position) < expradius)
                {
                    b.rigidbody.angularDrag = 2;
                    b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, energy * scalefactor * b.rigidbody.mass));                    
                    any = true;
                }
            if (energy > 300 && any)
            {
                root.audio.PlayOneShot(superphys_launch3);
                Destroy(Instantiate(wavePrefab, cursor[0].position, transform.rotation), 1.36f);                
            }

            energy = 0;

        }
    }

    protected override void Update()
    {
        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0) && (patronsLeft > 0 || debug))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0) || (patronsLeft <= 0 && !debug))
                RPCSetPower(false);
        }
        base.Update();

    }



}
