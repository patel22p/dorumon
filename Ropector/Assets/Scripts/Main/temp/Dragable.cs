using System;
using UnityEngine;





public class Dragable : bs
{
    public SpringJoint spr;

    public GameObject cursor;

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
        if (Input.GetMouseButton(0))
            this.rigidbody.AddExplosionForce(-100 * Time.deltaTime, cursor.transform.position, 10);
    }
    void OnMouseUp()
    {
        Destroy(spr);
    }
}