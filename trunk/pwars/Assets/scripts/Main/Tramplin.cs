using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Tramplin : Base
{
    Transform tramplin;
    public CubeGrav jump;
    void Start()
    {
        
        tramplin = this.transform.parent;

    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.name == "LocalPlayer")
        {
            Transform t = (Transform)Instantiate(jump.transform);
            t.parent = tramplin;
        }

    }
}
