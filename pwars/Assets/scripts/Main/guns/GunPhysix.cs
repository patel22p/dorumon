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
    public bool power;
    public void Start()
    {
        _Name = "Грави Пушка";

        audio.clip = (AudioClip)Resources.Load("sounds/PowerGun");

    }

    protected override void FixedUpdate()
    {
        if (power)
        {
            if (bullets < exp) bullets += 80;

            foreach (Base b in _Game.dynamic)
            {
                if (!(b is IPlayer))
                {
                    b.rigidbody.AddExplosionForce(-gravitaty * scalefactor * b.rigidbody.mass, cursor.position, radius);
                    b.rigidbody.angularDrag = 30;
                    b.rigidbody.velocity *= .97f;
                    b.OwnerID = this.root.GetComponent<Player>().OwnerID;
                    AudioSource a = audio;
                    a.pitch = 0.1f + (bullets / exp / 20);
                    if (!a.isPlaying) a.Play();
                }
            }
        }
        else
            audio.Stop();
        base.FixedUpdate();

    }
    
    void OnTriggerStay(Collider c)
    {
        Jumper j = c.GetComponent<Jumper>();
        if (j != null)
        {
            if (power)
            {
                Player p = root.GetComponent<Player>();
                p.rigidbody.AddForce(transform.rotation * new Vector3(0, 0, 50) * scalefactor * p.rigidbody.mass);
            }
        }
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
                if (!(b is IPlayer) && Vector3.Distance(b.transform.position, cursor.position) < expradius)
                {
                    b.rigidbody.angularDrag = 2;
                    b.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, bullets * scalefactor * b.rigidbody.mass));
                    Destroy(Instantiate(Load("wave"), cursor.position, transform.rotation), 1.36f);
                    any = true;
                }
            if (bullets > 300 && any)
                PlaySound("superphys_launch3");

            foreach (Transform t in _Game.jumpers)
                if (IsPointed(jumper, 100))
                {
                    Player p = root.GetComponent<Player>();
                    p.rigidbody.AddForce(transform.rotation * new Vector3(0, 0, -3000) * scalefactor * p.rigidbody.mass);                    
                }
            bullets = 0;

        }
        //else
        //    this.GetComponents<AudioSource>()[0].Play();
    }

    public LayerMask jumper;
    protected override void LocalUpdate()
    {
        if (isOwner && enabled)
        {
            if (Input.GetMouseButtonDown(0))
                RPCSetPower(true);
            else if (Input.GetMouseButtonUp(0))
                RPCSetPower(false);
        }

    }



}
