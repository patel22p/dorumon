using System;
using UnityEngine;

public class RopeEnd : MonoBehaviour
{
    //void OnCollisionEnter(Collision coll)
    //{
    //    //Debug.Log("coll");    
    //    //if (gameObject.GetComponent<FixedJoint>() == null)
    //    //{
    //    //    var fx = gameObject.AddComponent<FixedJoint>();
    //    //    fx.connectedBody = coll.rigidbody;
    //    //}
    //}
    void OnCollisionEnter(Collision coll)
    {
        OnColl(coll.contacts[0].point);
    }
    void OnColl(Vector3 point)
    {
        this.rigidbody.isKinematic = true;
        //this.rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        transform.position = point;
        enabled = false;
    }
    public Vector3? oldpos;

    void Update()
    {

        if (oldpos != null && oldpos != transform.position)
        {
            var r = new Ray(oldpos.Value, transform.position);
            RaycastHit h;
            if (Physics.Raycast(r, out h, Vector3.Distance(transform.position, oldpos.Value), 1 << LayerMask.NameToLayer("Level")))
            {
                Debug.Log("hit");
                OnColl(h.point);
            }

        }
        oldpos = transform.position;
    }
    //void OnCollisionStay(Collision collisionInfo)
    //{
    //    Debug.Log("test");
    //}
    //public void Disable()
    //{
    //    //var fx = this.GetComponent<FixedJoint>();
    //    //if(fx!=null) 
    //    //    GameObject.Destroy(fx);
    //}
}