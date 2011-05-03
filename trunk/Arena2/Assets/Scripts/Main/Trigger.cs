using System;
using UnityEngine;
using System.Collections.Generic;
public class Trigger : bs
{
    public List<bs> colliders = new List<bs>();
    void OnTriggerEnter(Collider other)
    {
        var bs = other.gameObject.GetComponent<bs>();
        if (!colliders.Contains(bs) && bs != null)
            colliders.Add(bs);
    }

    void OnTriggerExit(Collider other)
    {
        var bs = other.gameObject.GetComponent<bs>();
        colliders.Remove(bs);
    }
}