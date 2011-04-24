using System;
using System.Linq;
using UnityEngine;
[AddComponentMenu("Game/GlowOnHit")]
public class GlowOnHit : bs
{
    //PhysAnimObj this;
    void Start()
    {
        //this = this.GetComponent<PhysAnimObj>();
    }
    //bool hit;
    void Update()
    {

        var c = this.renderer.material.color;
        //if (c.a <= 0) hit = false;
        c.a -= Time.deltaTime;
        this.renderer.material.color = c;
    }
    void OnCollisionEnter(Collision coll)
    {
        //this = this.GetComponent<PhysAnimObj>();
        //Debug.Log("Hit");
        //hit = true;
        var c = this.renderer.material.color;
        c.a = 1;
        this.renderer.material.color = c;
    }
}