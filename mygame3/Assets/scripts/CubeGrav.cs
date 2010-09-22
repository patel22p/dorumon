using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CubeGrav : Base
{
    GameObject s;

    void Start()
    {
        s = _LocalPlayer.gameObject;
        s.rigidbody.velocity = Vector3.zero;
    }

    void Update()
    {
        if (!this.animation.isPlaying) Destroy(this.gameObject);
    }
    
    void FixedUpdate()
    {        
        AnimationClip ac = new AnimationClip();

        s.rigidbody.AddExplosionForce(-force, transform.position, 1000);
    }
    public float force = 100;
}
