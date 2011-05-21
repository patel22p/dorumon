using System;
using UnityEngine;


public class RopeEnd : bs
{
    public Vector3? oldpos;
    public GameObject clothPrefab;
    GameObject cloth;
    InteractiveCloth Cloth;
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
        if (Cloth != null)
        {
            Cloth.stretchingStiffness = Mathf.Clamp(Cloth.stretchingStiffness + Time.deltaTime*2, 0, 1);
            //Cloth.stretchingStiffness = Mathf.Clamp(Cloth.stretchingStiffness + (1 - Cloth.stretchingStiffness) * Time.deltaTime, 0, 1);
            //Debug.Log(Cloth.stretchingStiffness);
        }
        //if (this.ched)
        //    pos = oldpos.Value;
        UpdateHitTest();
    }
    void FixedUpdate()
    {
        //if (j1 != null)
        //{
        //    j1.rigidbody.velocity = pl.rigidbody.velocity;
        //    j1.rigidbody.angularVelocity = pl.rigidbody.angularVelocity;
        //}
        //this.transform.rotation = Quaternion.identity;
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


    bool attached;
    public Wall AttachedTo;
    void OnColl(Vector3 point, Collider t)
    {
        Debug.Log("Coll");
        AttachedTo= t.transform.GetComponentInParrent<Wall>();
        if (AttachedTo != null && AttachedTo.attachRope)
        {
            //this.rigidbody.constraints = RigidbodyConstraints.FreezePosition;
            attached = true;
            this.rigidbody.isKinematic = true;
            //transform.parent = t.transform;
            //transform.position = point;
        }
        if (!attached)
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
            Destroy(cloth.gameObject);
        

        enabled = enable;
        
        attached = false;
        this.rigidbody.isKinematic = false;
        //rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
        rigidbody.useGravity = enable;
        rigidbody.velocity = Vector3.zero;
        transform.parent = null;
        if (enable)
        {
            cloth = ((GameObject)Instantiate(clothPrefab, pl.pos, Quaternion.identity));            
            Cloth = cloth.GetComponentInChildren<InteractiveCloth>();
            oldpos = pos = cloth.transform.Find("s2").position;
            Cloth.AttachToCollider(pl.RopeColl,false,true);
            Cloth.AttachToCollider(this.collider, false, true);
            
        }
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log("Coll");
        OnColl(c.transform.position, c.collider);
    }
    [RPC]
    public void Hide()
    {
        EnableRope(false);
    }     
    
}    
