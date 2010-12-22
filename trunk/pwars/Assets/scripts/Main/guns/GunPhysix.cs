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
    public float holdtm;
    public bool power;
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void FixedUpdate()
    {
        if (power)
        {
            patronsLeft -= Time.fixedDeltaTime;
            holdtm += Time.fixedDeltaTime;
            {
                var p2 = cursor[0].position;
                var b2 = _Game.towers.Where(a => a != null && a.Alive && Vector3.Distance(a.pos, p2) < 10).OrderBy(a => Vector3.Distance(a.pos, p2)).FirstOrDefault();
                if (b2 != null)
                {

                    b2.rigidbody.velocity = (p2 - b2.pos) * 5;
                    b2.rot = rot;
                    b2.rigidbody.angularVelocity = Vector3.zero;
                }
            }
            foreach (Base b in _Game.boxes.Where(b => b != null))
            {
                b.rigidbody.AddExplosionForce(-gravitaty * scalefactor * b.rigidbody.mass, cursor[0].position, radius);
                b.rigidbody.angularDrag = 30;
                b.rigidbody.velocity *= .97f;
                b.OwnerID = this.root.GetComponent<Player>().OwnerID;
                AudioSource a = audio;
                a.pitch = Math.Min(0.1f + (holdtm / 200), .2f);
                if (!a.isPlaying) a.Play();
            }
        }
        else
        {
            audio.Stop();
            holdtm = 0;
        }

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
    public void SetPower(bool power)
    {
        this.power = power;
    }
    public void RPCShoot() { CallRPC("Shoot"); }
    [RPC]
    public void Shoot()
    {
        foreach (Base b in _Game.boxes.Cast<Base>().Where(b => b != null))
            if (Vector3.Distance(b.pos, cursor[0].position) < expradius)
            {
                b.rigidbody.angularDrag = 2;
                b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, exp * scalefactor * b.rigidbody.mass) / Time.timeScale);
            }
            root.audio.PlayOneShot(superphys_launch3);
            Destroy(Instantiate(wavePrefab, cursor[0].position, transform.rotation), 1.36f);

    }
    protected override void Update()
    {
        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0) && (patronsLeft > 0 || debug))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0) || (patronsLeft <= 0 && !debug))
            {
                if (holdtm < .2f && (patronsLeft > 5 || debug))
                {
                    patronsLeft -= 5;
                    RPCShoot();
                }
                RPCSetPower(false);
            }
        }
        base.Update();

    }



}
