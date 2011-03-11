using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Button : bs
{
    public bs button;
    public Animation anim;
    public float massFactor = .5f;
    public float prSmoth;    
    void Update()
    {
         
        var max = 1;
        var pr = Mathf.Max(0, (max - massFactor * trigger.Where(b => b.transform.GetComponentInParrent<Rigidbody>() != null).Sum(a => a.transform.GetComponentInParrent<Rigidbody>().mass)));
        prSmoth = (prSmoth*30 + pr ) / 31;
        //Debug.Log(prSmoth);
        button.lpos = Vector3.up * prSmoth;        
        if (pr == 0 && anim != null)
            anim.Play();
    }
    public override void Init()
    {
        base.Init();
    }

    public override void InitValues()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Button");
        rigidbody.isKinematic = true;
        base.InitValues();
    }
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
        //Debug.Log("enter"+col.gameObject.name);

        collisions.Add(col.gameObject);
    }
    void OnCollisionExit(Collision col)
    {
        //Debug.Log("exit" + col.gameObject.name);
        collisions.Remove(col.gameObject);
    }
}