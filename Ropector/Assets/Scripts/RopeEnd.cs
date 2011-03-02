using System;
using UnityEngine;

public class RopeEnd : MonoBehaviour
{
    internal bool katched;
    void OnCollisionEnter(Collision col)
    {
        
        Debug.Log("hit");
        katched = true;
        oldpos = col.contacts[0].point;
    }

    Vector3 oldpos;
    void Update()
    {
        if (katched)
        {
            transform.position = oldpos;
            rigidbody.velocity = Vector3.zero;
        }
    }
}