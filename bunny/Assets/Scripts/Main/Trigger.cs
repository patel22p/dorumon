using System;
using UnityEngine;
using System.Collections.Generic;
public class Trigger : bs
{
    public List<bs> triggers = new List<bs>();
    void OnTriggerEnter(Collider other)
    {
        var bs = other.gameObject.GetComponent<bs>();
        if (!triggers.Contains(bs) && bs != null)
            triggers.Add(bs);
    }

    void OnTriggerExit(Collider other)
    {
        var bs = other.gameObject.GetComponent<bs>();
        triggers.Remove(bs);
    }
}