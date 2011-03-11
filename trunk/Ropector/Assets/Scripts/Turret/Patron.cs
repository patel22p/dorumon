using System;
using UnityEngine;

public class Patron : bs
{
    public Vector3 force;
    public Vector3? oldpos;
    public void Update()
    {
        if (Player == null) return;
        if (oldpos != null && oldpos != transform.position)
        {
            var r = new Ray(oldpos.Value, transform.position - oldpos.Value);
            RaycastHit h;

            if (Physics.Raycast(r, out h, Vector3.Distance(transform.position, oldpos.Value), ~(1 << LayerMask.NameToLayer("Player"))))
                OnColl(h.point, h.transform);
        }
        oldpos = transform.position;
    }
    void OnColl(Vector3 point, Transform t)
    {
        
    }
}