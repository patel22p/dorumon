using System;
using UnityEngine;





public class Dragable : bs //this drag effect only applies to be in menu 
{
    public SpringJoint spr;
    [FindTransform()]
    public GameObject cursor;
    [FindTransform()]
    public GameObject Plane;
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Default"));
        Plane.transform.position = hit.point;
    }
    void Update()
    {
        if(Input.GetMouseButton(0))
            this.rigidbody.AddExplosionForce(-100 * Time.deltaTime , cursor.transform.position, 10);
    }
    void OnMouseUp()
    {
        Destroy(spr);
    }
} 