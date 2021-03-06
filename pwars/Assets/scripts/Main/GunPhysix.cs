﻿using System.Linq;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class GunPhysix : GunBase
{
    private float holdtm;
    private float shoottm;
    internal bool power;
    public float radius = 50;
    public AnimationCurve gravitaty;
    [FindTransform("Object01")]
    public Renderer gloweffect;
    public AnimationCurve release;
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
    public void RPCShoot() { CallRPC("Shoot"); }
    [RPC]
    public void Shoot()
    {
        bool boxes = false;
        foreach (Box b in _Game.boxes.Where(b => b != null))
        {
            var d = Vector3.Distance(b.pos, curspos);
            if (d < 8)
            {
                b.OwnerID = this.root.GetComponent<Player>().OwnerID;
                b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, 6000 * b.rigidbody.mass) * fdt);
                boxes = true;
            }
        }
        var v = curspos;
        if (!boxes)
        {
            RaycastHit h;
            var ray = new Ray(pos, rot * new Vector3(0, 0, 1));
            if (Physics.Raycast(ray, out h, 10, 1 << LayerMask.NameToLayer("Level")) || Antigrav)
            {
                if (Antigrav)
                {
                    ray.origin = pos;
                    ray.direction = rot * Vector3.forward;
                }
                if (h.distance < 4)
                {
                    v = ray.origin;
                    var vd = ray.direction.normalized;
                    //vd.x 
                    player.rigidbody.velocity = (vd * -20 * fdt / ((player.rigidbody.mass)));
                }
            }
        }
        root.audio.PlayOneShot(superphys_launch3);
        Destroy(Instantiate(wavePrefab, v, transform.rotation), 1.36f);
    }
    public Vector3 curspos;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (power)
        {
            patronsLeft -= Time.deltaTime;
            holdtm += Time.deltaTime;

            var boxes = _Game.boxes.Where(b => b != null && Vector3.Distance(b.pos, pos + transform.forward * 14) < 15);
            float size = boxes.Sum(a => a.collider.bounds.size.sqrMagnitude);
            curspos = cursor[0].position + (transform.forward * size / 120) + (transform.forward * 6);

            var bc = _Game.towers.Where(a => a != null && a.Alive && Vector3.Distance(a.pos, curspos) < 10).OrderBy(a => Vector3.Distance(a.pos, curspos)).FirstOrDefault();
            if (bc != null)
            {
                bc.rigidbody.velocity = (curspos - bc.pos) * 5;
                bc.rigidbody.angularVelocity = Vector3.zero;
                bc.physxguntm = 1;
                bc.rot = rot;
            }
            else
            {
                foreach (Box b in boxes)
                {
                    if (b.rigidbody.velocity.magnitude < 1)
                        b.OwnerID = this.root.GetComponent<Player>().OwnerID;
                    b.rigidbody.velocity *= .95f;
                    b.rigidbody.AddExplosionForce(-1f * b.rigidbody.mass, curspos, radius, 0, ForceMode.VelocityChange);
                    b.rigidbody.angularVelocity *= .95f;
                    b.rigidbody.angularVelocity += Vector3.one / 4; //physrot gunphys physgun
                }
            }
            
            audio.pitch = Math.Min(0.1f + (holdtm / 200), .2f);
            if (!audio.isPlaying) audio.Play();
        }
        else
        {
            audio.Stop();
            holdtm = 0;
        }
        if (gloweffect != null)
        {
            var m = gloweffect.material;
            var c = m.color;
            c.a = holdtm;
            c.g = holdtm;
            c.b = holdtm;
            m.color = c;
        }
    }
    private bool HitTest(Transform j, float dist)
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Vector3 dir = rot * new Vector3(0, 0, dist);
        j.gameObject.layer = LayerMask.NameToLayer("HitTest");
        bool hit = (Physics.CheckCapsule(pos, dir + pos, 1, 1 << LayerMask.NameToLayer("HitTest")));
        j.gameObject.layer = 0;
        return hit;
    }
    public void RPCSetPower(bool e) { CallRPC("SetPower", e); }
    [RPC]
    public void SetPower(bool power)
    {
        audio.Stop();
        this.power = power;
    }
    protected override void Update()
    {
        shoottm -= Time.deltaTime;
        if (isOwner && enabled && lockCursor)
        {
            if (Input.GetMouseButtonDown(0) && (patronsLeft > 0 || debug))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0) || (patronsLeft <= 0 && !debug))
            {
                RPCSetPower(false);
                if (holdtm < .2f && (patronsLeft > 2 || debug) && shoottm < 0)
                {
                    shoottm = .3f;
                    patronsLeft -= 2;
                    RPCShoot();
                }
            }
        }
        base.Update();

    }
    public override void DisableGun()
    {
        RPCSetPower(false);
        base.DisableGun();
    }
}
