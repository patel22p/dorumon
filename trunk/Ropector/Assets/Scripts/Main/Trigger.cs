using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Trigger : bs
{    
    public List<GameObject> trigger = new List<GameObject>();
    public List<GameObject> collisions = new List<GameObject>();
    void OnTriggerEnter(Collider col)
    {
        if (!trigger.Contains(col.gameObject))
            trigger.Add(col.gameObject);
    }
    void OnTriggerExit(Collider col)
    {
        trigger.Remove(col.gameObject);
    }
    void OnCollisionEnter(Collision col)
    {
        collisions.Add(col.gameObject);
    }
    void OnCollisionExit(Collision col)
    {
        collisions.Remove(col.gameObject);
    }
}