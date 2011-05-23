using System;
using UnityEngine;


public class RopeEnd : bs
{
    public Vector3? oldpos;
    public GameObject clothPrefab;
    GameObject cloth;
    InteractiveCloth Cloth;
    Player pl;
    bool attached;
    public Wall AttachedTo;
    Vector3 offs;
    Vector3 SyncPos;
    Vector3 SyncVel;

    public override void Awake()
    {
        pl = transform.parent.GetComponent<Player>();
        transform.parent = null;
    }
    void Start()
    {
        EnableRope(false);
    }
    void Update()
    {
        if (Cloth != null)
            Cloth.stretchingStiffness = Mathf.Clamp(Cloth.stretchingStiffness + Time.deltaTime*2, 0, 1);
        UpdateHitTest();
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
    void FixedUpdate()
    {        
        if (attached)
            pos = AttachedTo.transform.TransformPoint(offs);
    }
    [RPC]
    public void Throw(Vector3 pos ,Vector3 dir)
    {
        if (!enabled)
        {
            this.oldpos = this.transform.position = pos;
            EnableRope(true);
            dir = dir.normalized;
            this.rigidbody.velocity = pl.rigidbody.velocity + dir * 40;
        }
    }
    void OnColl(Vector3 point, Collider t)
    {
        AttachedTo = t.transform.GetComponentInParrent<Wall>();
        if (AttachedTo != null && AttachedTo.attachRope)
        {
            offs = AttachedTo.transform.InverseTransformPoint(pos);
            attached = true;
            Cloth.DetachFromCollider(pl.RopeColl);
            Cloth.AttachToCollider(pl.RopeColl, false, true);
            rigidbody.isKinematic = true;
        }
        if (!attached)
            EnableRope(false);

        var p = t.transform.GetComponentInParrent<PhysAnim>();
        
        if (p != null)
            if (p.PlayOnRopeHit && networkView.isMine)
                p.RPCPlay();
    }
    public void EnableRope(bool enable)
    {
        if (cloth != null)
            Destroy(cloth.gameObject);
        
        enabled = enable;
        rigidbody.isKinematic = false;
        attached = false;
        
        rigidbody.useGravity = enable;
        rigidbody.velocity = Vector3.zero;
        if (enable)
        {
            cloth = ((GameObject)Instantiate(clothPrefab, pl.pos, Quaternion.identity));            
            Cloth = cloth.GetComponentInChildren<InteractiveCloth>();
            oldpos = pos = cloth.transform.Find("s2").position;
            Cloth.AttachToCollider(pl.RopeColl,false,false);
            Cloth.AttachToCollider(this.collider, false, true);
        }
    }
    void OnCollisionEnter(Collision c)
    {
        OnColl(c.transform.position, c.collider);
    }
    [RPC]
    public void Hide()
    {
        EnableRope(false);
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (rigidbody.isKinematic) return;
        if (stream.isWriting)
        {
            SyncPos = rigidbody.position;
            SyncVel = rigidbody.velocity;
        }
        stream.Serialize(ref SyncPos);
        stream.Serialize(ref SyncVel);
        if (stream.isReading)
        {
            rigidbody.position = SyncPos;
            rigidbody.velocity = SyncVel;
        }
    }
}    
