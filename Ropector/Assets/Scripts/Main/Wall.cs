using System;
using System.Linq;
using UnityEngine;
[AddComponentMenu("Game/Wall")]
public class Wall : bs
{
    public bool attachRope = true;
    public Vector3 RopeForce = new Vector3(1, 1f, 1);
    public float RopeLength = 1f;
    
    public Vector3 bounchyForce;
    public Vector3 ClickForce;
    public float SpeedFactor;
    //public bool playOnHit;
    public Collider[] Ignore;
    void Start()
    {
        if (Ignore != null)
            foreach (Collider a in this.transform.GetTransforms().Where(a => a.collider != null).Select(a => a.collider))
                foreach (Collider b in Ignore)
                    Physics.IgnoreCollision(a, b);
    }
    public override void Init()
    {
        if (rigidbody != null)
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
        base.Init();
    }
    
    void OnTriggerEnter(Collider coll)
    {
        Player.Trigger = this;
    }
    void OnTriggerExit(Collider coll)
    {
        if(Player.Trigger == this)
            Player.Trigger = null;
    }
    void OnCollisionEnter(Collision coll)
    {
        if(PlayOnPlayerHit)
            OnHit();

        if (bounchyForce != Vector3.zero)
        {
            var f = coll.impactForceSum.magnitude * 10;
            coll.rigidbody.AddForce(bounchyForce.x * f, bounchyForce.y * f, bounchyForce.z * f);
        }
    }
    public bool PlayOnRopeHit = true;
    public bool PlayOnPlayerHit;
    public void OnHit()
    {               
        var ph = this.GetComponent<PhysAnimObj>();
        if (ph != null)
        {
            var anim = ph.AnimObj.animation;
            if (anim != null && anim.clip != null)
                anim.Play();
        }
        else if (animation != null && animation.clip != null)
            this.animation.Play();
    }
}