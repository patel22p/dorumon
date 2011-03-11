using System;
using UnityEngine;

public class RopeEnd : bs
{
    public Vector3? oldpos;
    float tmRope;
    //bool enabled = true;
    public float RopeFactor = 1;
    public float ShootFactor = 1;
    public float ObjectMagnetFactor = 1;
    public LineRenderer line;
    public override void AlwaysUpdate()
    {
        tmRope -= Time.deltaTime;
        base.AlwaysUpdate();
    }
    void Update()
    {
        Gravity();
        HitTest();
        line.SetPosition(0, Player.pos);
        line.SetPosition(1, this.pos);
    }
    public override void InitValues()
    {
        
        RopeFactor = 1000f;
        ShootFactor = 700f;
        ObjectMagnetFactor = 500;
        base.InitValues();
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
            else if (v.magnitude > 3)
                Player.rigidbody.AddForce(v * -1 * this.ShootFactor * Time.deltaTime / Mathf.Sqrt(v.magnitude));
        }
        else
            this.rigidbody.AddForce(v * this.RopeFactor * Time.deltaTime);
        if (v.magnitude > 27)
            EnableRope(false);
    }
    //shoot
    public void MouseClick()
    {
        if (!enabled && tmRope < 0)
        {
            Debug.Log("RopeEnable");
            tmRope = 1;
            this.oldpos = this.transform.position = Player.pos;
            EnableRope(true);
            var dir = Cam.cursor.transform.position - Player.pos;
            dir = dir.normalized;
            this.rigidbody.velocity = dir * 100;

        }
        else if (enabled)
        {
            Debug.Log("RopeDisable");
            EnableRope(false);
        }
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
    void OnColl(Vector3 point, Transform t)
    {
        this.rigidbody.isKinematic = true;
        transform.parent = t;
        transform.position = point;
    }
    public void EnableRope(bool value)
    {
        this.gameObject.active = value;
        enabled = value;
        oldpos = Player.pos;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        rigidbody.isKinematic = false;
        transform.parent = null;
        //Update();
    }
}