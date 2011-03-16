using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Button : bs
{
    public Rigidbody key;
    public Animation anim;        
    void OnTriggerEnter(Collider col)
    {
        Debug.Log(col.name);
        if (col.rigidbody == key)
            anim.Play();
    }
}