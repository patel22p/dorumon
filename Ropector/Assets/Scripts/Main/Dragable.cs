using System;
using UnityEngine;





public class Dragable : bs
{
    public SpringJoint spr;
    [FindTransform(scene = true)]
    public GameObject cursor;
    [FindTransform(scene = true)]
    public GameObject Plane;
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Default"));
        spr = gameObject.AddComponent<SpringJoint>();
        spr.anchor = this.transform.InverseTransformPoint(cursor.transform.position);
        spr.connectedBody = cursor.rigidbody;
        Plane.transform.position = hit.point;
    }
    void OnMouseDrag()
    {

        renderer.material.color -= Color.white * Time.deltaTime;
    }
    void OnMouseUp()
    {
        Destroy(spr);
    }
} 