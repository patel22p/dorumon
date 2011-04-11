using System;
using UnityEngine;

public class RopeEnd : bs
{
    public Vector3? oldpos;
    float tmRope;
    public float RopeFactor = 1;
    public float ObjectMagnetFactor = 1;
    public LineRenderer line;
    [FindAsset("cloth")]
    public GameObject clothPrefab;
    public GameObject cloth;
    void Start()
    {
        EnableRope(false);
    }
    void Update()
    {
        if (cloth != null)
        {
            var fx1 = cloth.transform.Find("s1");
            fx1.transform.position = Player.pos;
            var fx2 = cloth.transform.Find("s2");
            fx2.transform.position = this.transform.position;
        }
        
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
    public override void InitValues()
    {
        RopeFactor = 1000f;
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
            
            var m = v.magnitude - 4*AttachedTo.RopeLength;
            if (m > 0)
            {
                var fctr = AttachedTo.RopeForce;
                var vn = v.normalized;
                var va = new Vector3(fctr.x * vn.x, fctr.y * vn.y * .6f, fctr.z * vn.z);
                var f = .05f;
                Player.veloticy += va * -400 * m * Time.deltaTime * f;
                Player.veloticy += Vector3.up * Time.deltaTime * 20 * f;
                //Player.rigidbody.AddForce(va * -400 * m * Time.deltaTime);
                //Player.rigidbody.position -= v * Time.deltaTime * .5f;
                //Player.rigidbody.AddForce(Vector3.up * Time.deltaTime * 20);
            }
        }
        else
            this.rigidbody.AddForce(v * this.RopeFactor * Time.deltaTime);
        if (v.magnitude > 27)
            EnableRope(false);
    }
    public bool ropedown;
    public void MouseDown()
    {
        if (!enabled)
        {
            ropedown = true;
            tmRope = 1;
            this.oldpos = this.transform.position = Player.pos;
            EnableRope(true);
            var dir = Cam.cursor.transform.position - Player.pos;
            dir = dir.normalized;
            this.rigidbody.velocity = dir * 100;
        }
        
    }

    public void MouseUp()
    {
        ropedown = false;
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

        if (cloth != null)
            Destroy(cloth);
        if (enableCloth)
        {
            cloth = (GameObject)Instantiate(clothPrefab,Player.pos,Quaternion.identity);            
        }
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