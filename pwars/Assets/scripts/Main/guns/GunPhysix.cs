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
    Player pl;
    
    public void Start()
    {
        pl = root.GetComponent<Player>();
        _Name = "Грави Пушка";

        audio.clip = (AudioClip)Resources.Load("sounds/PowerGun");

    }

    protected override void FixedUpdate()
    {
        if (power)
        {
            if (energy < exp) energy += 80;
            foreach (Transform t in _Game.jumpers)
            {
                Jumper j = t.GetComponent<Jumper>();
                if (HitTest(t, j.distance))
                {
                    Vector3 v = transform.rotation * j.Magnet;
                    v.x *= j.multiplier.x;
                    v.z *= j.multiplier.x;
                    v.y *= j.multiplier.y;
                    pl.rigidbody.AddForce(v * scalefactor * pl.rigidbody.mass);

                }
            }
            foreach (Base b in _Game.dynamic)
            {
                if (!(b is IPlayer))
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

    [RPC]
    public void RPCSetPower(bool e)
    {
        CallRPC(e);
        power = e;
        if (!e)
        {
            //this.GetComponents<AudioSource>()[0].Stop();
            bool any = false;

            foreach (Base b in _Game.dynamic)
                if (!(b is IPlayer) && Vector3.Distance(b.transform.position, cursor[0].position) < expradius)
                {
                    b.rigidbody.angularDrag = 2;
                    b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, energy * scalefactor * b.rigidbody.mass));
                    Destroy(Instantiate(Load("wave"), cursor[0].position, transform.rotation), 1.36f);
                    any = true;
                }
            if (energy > 300 && any)
                PlaySound("superphys_launch3");

            foreach (Transform t in _Game.jumpers)
            {
                Jumper j = t.GetComponent<Jumper>();
                if (HitTest(t,.3f))
                {
                    PlaySound("superphys_launch3");
                    pl.rigidbody.AddForce(j.Release * scalefactor * pl.rigidbody.mass);
                    GameObject g = (GameObject)Instantiate(Load("wave"), j.transform.position, j.transform.rotation * Quaternion.Euler(90, 0, 0));
                    Destroy(g, 1.6f);
                }
            }
            energy = 0;

        }
        //else
        //    this.GetComponents<AudioSource>()[0].Play();
    }

    protected override void Update()
    {
        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0))
                RPCSetPower(false);
        }
        base.Update();

    }



}
