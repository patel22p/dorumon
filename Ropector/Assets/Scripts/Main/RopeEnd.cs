using System;
using UnityEngine;

public class RopeEnd : bs
{
    public Vector3? oldpos;
    float tmRope;

    public LineRenderer line;
    [FindAsset("cloth")]
    public GameObject clothPrefab;
    public GameObject cloth;
    Player pl;
    public override void Awake()
    {
        pl = transform.parent.GetComponent<Player>();
    }
    void Start()
    {
        EnableRope(false);
    }
    void Update()
    {
        name = "Rope: " + ToString(); 
        UpdateCloth();
        UpdateHitTest();
        UpdateOther();
    }

    private void UpdateOther()
    {
        line.SetPosition(0, pl.pos);
        line.SetPosition(1, this.pos);
    }

    private void UpdateCloth()
    {
        if (cloth != null)
        {
            var fx1 = cloth.transform.Find("s1");
            fx1.transform.position = pl.pos;
            var fx2 = cloth.transform.Find("s2");
            fx2.transform.position = this.transform.position;
        }
    }
    void FixedUpdate()
    {
        UpdateGravity();
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

    private void UpdateGravity()
    {
        var v = pl.transform.position - this.transform.position;
        
        if (this.attached)
        {
            var r = this.transform.parent.GetComponent<Rigidbody>(); 
            if (r != null && !r.isKinematic)
            {
                Debug.Log("Interactive");
                if(v.magnitude > 3)
                    r.AddForceAtPosition(v * 50  / Mathf.Sqrt(v.magnitude) / Time.timeScale, this.transform.position);
            }

            var m = Mathf.Min(10, Mathf.Sqrt(v.magnitude * 5) - 4 * AttachedTo.RopeLength);

            if (m > 0)
            {
                var fctr = AttachedTo.RopeForce;
                var vn = v.normalized;
                var va = new Vector3(fctr.x * vn.x, fctr.y * vn.y * .6f, fctr.z * vn.z);
                pl.rigidbody.AddForce(va * -400 * m * Time.deltaTime);
                pl.rigidbody.position -= v * Time.deltaTime * .5f;
                pl.rigidbody.AddForce(Vector3.up * Time.deltaTime * 20);
            }
        }
        else
            this.rigidbody.AddForce(v * 500 * Time.deltaTime / Time.timeScale);
    }

    [RPC]
    public void Throw(Vector3 dir)
    {
        if (!enabled)
        {
            tmRope = 1;
            this.oldpos = this.transform.position = pl.pos;
            EnableRope(true);
            
            dir = dir.normalized;
            this.rigidbody.velocity = dir * 100;
        }
        
    }

    [RPC]
    public void Hide()
    {
        EnableRope(false);
    } 


    private void UpdateHitTest()
    {
        if (attached) return;
               
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
        if (AttachedTo != null && AttachedTo.PlayOnRopeHit)
            AttachedTo.OnHit();
        if (AttachedTo != null && AttachedTo.attachRope)
        {            
            this.rigidbody.isKinematic = true;
            transform.parent = t;
            transform.position = point;
        }
        if (!rigidbody.isKinematic)
            EnableRope(false);
    }
    
    public void EnableRope(bool enable)
    {
        if (cloth != null)
            Destroy(cloth);
        if (enable)
            cloth = (GameObject)Instantiate(clothPrefab,pl.pos,Quaternion.identity);            

        //this.gameObject.active = enable;
        enabled = enable;
        oldpos = pl.pos;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        rigidbody.isKinematic = false;
        transform.parent = null;
        //Update();
    }
}