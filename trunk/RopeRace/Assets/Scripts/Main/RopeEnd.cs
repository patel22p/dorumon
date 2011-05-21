using System;
using UnityEngine;


public class RopeEnd : bs
{
    public Vector3? oldpos;
    public GameObject clothPrefab;
    GameObject cloth;
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
        UpdateCloth();
        UpdateHitTest();
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

    private void UpdateGravity()
    {
        var v = pl.transform.position - this.transform.position;
        
        if (this.attached)
        {
            var r = this.transform.GetComponentInParrent<Rigidbody>(); 
            if (r != null && !r.isKinematic)
            {
                Debug.Log("Test");
                if(v.magnitude > 3)
                    r.AddForceAtPosition(v * 50  / Mathf.Sqrt(v.magnitude) / Time.timeScale, this.transform.position);
            }

            var m = Mathf.Clamp(Mathf.Sqrt(v.magnitude * 5) - (4 * AttachedTo.RopeLength), 0, 10);

            if (m > 0)
            {
                var fctr = AttachedTo.RopeForce;
                var vn = v.normalized;
                var va = new Vector3(fctr.x * vn.x, fctr.y * vn.y * .6f, fctr.z * vn.z);
                pl.rigidbody.AddForce(va * -400 * m * Time.deltaTime);
                pl.rigidbody.position -= v * Time.deltaTime * .5f;
                //pl.rigidbody.AddForce(Vector3.up * Time.deltaTime * 20);
            }
        }
        else
            this.rigidbody.AddForce(v * 500 * Time.deltaTime / Time.timeScale);
    }

    [RPC]
    public void Throw(Vector3 pos ,Vector3 dir)
    {
        if (!enabled)
        {
            this.oldpos = this.transform.position = pos;
            EnableRope(true);
            dir = dir.normalized;
            this.rigidbody.velocity = dir * 40;
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

            if (Physics.Raycast(r, out h, Vector3.Distance(transform.position, oldpos.Value), _Game.RopeColl))
                OnColl(h.point, h.collider);
        }
        oldpos = transform.position;
    }
    bool attached { get { return rigidbody.isKinematic; } }
    public Wall AttachedTo;
    void OnColl(Vector3 point, Collider t)
    {
        AttachedTo= t.transform.GetComponentInParrent<Wall>();
        if (AttachedTo != null && AttachedTo.attachRope)
        {            
            this.rigidbody.isKinematic = true;
            transform.parent = t.transform;
            transform.position = point;
        }
        if (!rigidbody.isKinematic)
            EnableRope(false);

        var p = t.transform.GetComponentInParrent<PhysAnim>();
        
        if (p != null)
        {
            if (p.PlayOnRopeHit)
                p.RPCPlay();
        }

    }
    
    public void EnableRope(bool enable)
    {
        
        if (cloth != null)
            Destroy(cloth);
        if (enable)
            cloth = (GameObject)Instantiate(clothPrefab,pos,Quaternion.identity);            
        enabled = enable;
        //oldpos = pos;
        rigidbody.isKinematic = false;
        rigidbody.useGravity = enable;
        rigidbody.velocity = Vector3.zero;
        transform.parent = null;
    }
}