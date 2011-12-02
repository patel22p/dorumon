using UnityEngine;
using System.Collections;

public class RopeEnd : Bs {

    public InteractiveCloth cloth;
    internal float stiffness;
    public Collider col;
    void OnCollisionEnter(Collision collision)
    {
        cloth.DetachFromCollider(col);
        cloth.AttachToCollider(col, false, true);
        cloth.stretchingStiffness = stiffness;
        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
    }
}
