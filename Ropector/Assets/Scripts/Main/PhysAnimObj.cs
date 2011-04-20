using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[AddComponentMenu("Game/PhysicsAnimation")]
//[RequireComponent(typeof(Rigidbody))]
public class PhysAnimObj : bs
{
    
    Quaternion oldRot;
    internal Transform AnimObj;
    public WrapMode wrapMode;
    public override void Awake()
    {
        base.Awake();
        oldRot = this.rot;
    }
    public float strength = 1;
    public void Start()
    {
        if(animation!=null)
            animation.wrapMode = wrapMode;
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        
        AnimObj = ((Transform)Instantiate(this.transform, pos, rot));
        AnimObj.parent = this.transform.parent;
        foreach (var a in AnimObj.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation)))
            Destroy(a);  
        
        Destroy(animation);
    }

    void FixedUpdate()
    {        
        if (this.rigidbody != null)
        {
            var pc = AnimObj.position - this.pos;
            var rc = Mathf.DeltaAngle((rot * Quaternion.Inverse(oldRot)).eulerAngles.z, (AnimObj.rotation * Quaternion.Inverse(oldRot)).eulerAngles.z);
            this.rigidbody.velocity = pc * 5 * this.rigidbody.mass*strength;
            this.rigidbody.angularVelocity = new Vector3(0, 0, rc) * this.rigidbody.mass;
        }

    }

}
