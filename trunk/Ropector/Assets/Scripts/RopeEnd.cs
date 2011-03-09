using System;
using UnityEngine;

public class RopeEnd : MonoBehaviour
{
    
    void OnCollisionEnter(Collision coll)
    {
        OnColl(coll.contacts[0].point,coll.transform);
    }
    public InteractiveCloth cl;
    void OnColl(Vector3 point,Transform t)
    {
        this.rigidbody.isKinematic = true;
        transform.parent = t;
        transform.position = point;
        enabled = false;
    }
    public void Disable()
    {
        rigidbody.isKinematic = false;
        transform.parent = null;
    }
    public bool isFlying { get { return enabled; } }
    public Vector3? oldpos;

    void Update()
    {
        this.rigidbody.AddForce(new Vector3(0, -2000, 0) * Time.deltaTime);
        if (oldpos != null && oldpos != transform.position)
        {
            var r = new Ray(oldpos.Value, transform.position - oldpos.Value);
            RaycastHit h;
            if (Physics.Raycast(r, out h, Vector3.Distance(transform.position, oldpos.Value), 1 << LayerMask.NameToLayer("Level") | 1 << LayerMask.NameToLayer("Default")))
                OnColl(h.point,h.transform);

        }
        oldpos = transform.position;
    }
 
}