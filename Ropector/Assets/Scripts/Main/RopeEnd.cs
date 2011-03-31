using System;
using UnityEngine;

public class RopeEnd : bs
{
    public Vector3? oldpos;
    float tmRope;
    public float RopeFactor = 1;
    public float ObjectMagnetFactor = 1;
    public LineRenderer line;
    
    void Start()
    {
        EnableRope(false);
        RopeFactor = 1000f;
        ObjectMagnetFactor = 500;
    }
    void Update()
    {
        
        
        HitTest();
        line.SetPosition(0, Player.pos);
        line.SetPosition(1, this.pos);
    }
    void FixedUpdate()
    {
        Gravity();
    }
    public override void AlwaysUpdate()
    {
        tmRope -= Time.deltaTime;
        base.AlwaysUpdate();
    }
    
    void OnCollisionEnter(Collision coll)
    {
        OnColl(coll.contacts[0].point, coll.transform);
    }

    private void Gravity()
    {
        var v = Player.transform.position - this.transform.position;
        
        if (this.attached)
        {
            var r = this.transform.parent.GetComponentInParrent<Rigidbody>(); //interactive
            if (r != null && !r.isKinematic)
            {
                if(v.magnitude > 3)
                    r.AddForceAtPosition(v * this.ObjectMagnetFactor * Time.deltaTime / Mathf.Sqrt(v.magnitude), this.transform.position);
            }
            
            var m = v.magnitude - 4*AttachedTo.RopeLength;
            if (m > 0)
            {
                var fctr = AttachedTo.RopeForce;
                var vn = v.normalized;
                var va = new Vector3(fctr.x * vn.x, fctr.y * vn.y * .6f, fctr.z * vn.z);
                Player.rigidbody.AddForce(va * -400 * m * Time.deltaTime);
                Player.rigidbody.position -= v * Time.deltaTime * .5f;
                Player.rigidbody.AddForce(Vector3.up * Time.deltaTime * 20);
            }
        }
        else
            this.rigidbody.AddForce(v * this.RopeFactor * Time.deltaTime);
        if (v.magnitude > 27)
            EnableRope(false);
    }
    //shoot
    public void MouseDown(Vector3 dir)
    {
        if (!enabled)
        {
            tmRope = 1;
            this.oldpos = this.transform.position = Player.pos;
            EnableRope(true);                        
            this.rigidbody.velocity = dir * 100;
        }
        
    }

    public void MouseUp()
    {
        EnableRope(false);
    } 


    private void HitTest()
    {
        if (attached) return;
        
        this.rigidbody.AddForce(new Vector3(0, -2000, 0) * Time.deltaTime);
        if (oldpos != null && oldpos != transform.position)
        {
            var r = new Ray(oldpos.Value, transform.position - oldpos.Value);
            RaycastHit h;

            if (Physics.Raycast(r, out h, Vector3.Distance(transform.position, oldpos.Value), ~(1 << LayerMask.NameToLayer("Player"))))
                OnColl(h.point, h.transform);
        }
        oldpos = transform.position;
    }
    bool attached { get { return rigidbody.isKinematic; } }
    public Wall AttachedTo;
    void OnColl(Vector3 point, Transform t)
    {
        AttachedTo= t.gameObject.GetComponent<Wall>();
        if (AttachedTo != null && AttachedTo.attachRope)
        {            
            this.rigidbody.isKinematic = true;
            transform.parent = t;
            transform.position = point;
        }
        if (!rigidbody.isKinematic)
        {
            transform.position = point + (Player.pos - this.pos).normalized;
            rigidbody.angularVelocity = rigidbody.velocity = Vector3.zero;
        }
        //else
        //    EnableRope(false);
    }
    
    public void EnableRope(bool enableCloth)
    {        
        this.gameObject.active = enableCloth;
        enabled = enableCloth;
        oldpos = Player.pos;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        rigidbody.isKinematic = false;
        transform.parent = null;
        //Update();
    }
}