using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunPhysix : GunBase
{

    public float radius = 50;
    public float exp = 2000;
    public float expradius = 40;
    public AnimationCurve gravitaty;
    public float scalefactor = 10;
    public float holdtm;    
    public bool power;
    [FindAsset("wave")]
    public GameObject wavePrefab;
    [FindAsset("superphys_launch3")]
    public AudioClip superphys_launch3;

#if(UNITY_EDITOR && UNITY_STANDALONE_WIN)
    public override void Init()
    {
        base.Init();
        audio.clip = Base2.FindAsset<AudioClip>("PowerGun");
    }
#endif

    public override void Awake()
    {
        base.Awake();
    }
    
    protected override void FixedUpdate()
    {
        if (power)
        {
            patronsLeft -= Time.deltaTime;
            holdtm += Time.deltaTime;
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
            var boxes = _Game.boxes.Where(b => b != null && Vector3.Distance(b.pos, cursor[0].position)<20);
            var count = boxes.Count();
            var mpos = pos + (transform.forward * count * 2) + (transform.forward * 8);
            foreach (Base b in boxes)
            {
                var f = 600;
                var d = Vector3.Distance(b.pos, mpos);
                //b.rigidbody.AddExplosionForce(fdt*-gravitaty.Evaluate(d) * f * .3f * scalefactor * b.rigidbody.mass, mpos, radius);
                b.rigidbody.AddForce((b.pos - mpos).normalized * scalefactor * b.rigidbody.mass * -gravitaty.Evaluate(d) * f * fdt);
                //b.rigidbody.angularVelocity *= .6f;
                b.rigidbody.velocity *= Math.Min(.1f * d, 1);
                b.OwnerID = this.root.GetComponent<Player>().OwnerID;
            }
            audio.pitch = Math.Min(0.1f + (holdtm / 200), .2f);
            if (!audio.isPlaying) audio.Play();
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
        bool boxes = false;
        foreach (Base b in _Game.boxes.Cast<Base>().Where(b => b != null))
            if (Vector3.Distance(b.pos, cursor[0].position) < expradius)
            {
                b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, exp * scalefactor * b.rigidbody.mass) * fdt);
                boxes = true;
            }
        var v = cursor[0].position;
        if (!boxes)
        {
            RaycastHit h;
            var ray = new Ray(pos, rot * new Vector3(0, 0, 1));            
            if (Physics.Raycast(ray, out h, 10, 1 << LayerMask.NameToLayer("Level")))
            {
                var d = 20 - h.distance;
                if (d > 0)
                {
                    v = h.point;
                    player.rigidbody.AddForce(ray.direction * -50 * d * fdt);
                }
            }
        }
        root.audio.PlayOneShot(superphys_launch3);
        Destroy(Instantiate(wavePrefab, v, transform.rotation), 1.36f);


    }
    protected override void Update()
    {
        if (isOwner && enabled && lockCursor)
        {
            if (Input.GetMouseButtonDown(0) && (patronsLeft > 0 || debug))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0) || (patronsLeft <= 0 && !debug))
            {
                RPCSetPower(false);
                if (holdtm < .2f && (patronsLeft > 2 || debug))
                {
                    patronsLeft -= 2;
                    RPCShoot();
                }                
            }
        }
        base.Update();

    }



}
