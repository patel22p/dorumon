using System;
using UnityEngine;

public class RopeEnd : MonoBehaviour
{
    void OnCollisionEnter(Collision coll)
    {
        if (gameObject.GetComponent<FixedJoint>() == null)
        {
            var fx = gameObject.AddComponent<FixedJoint>();
            fx.connectedBody = coll.rigidbody;
        }
    }
    public void Reset()
    {
        var fx = this.GetComponent<FixedJoint>();
        if(fx!=null) 
            GameObject.Destroy(fx);
    }
}