using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[AddComponentMenu("Game/PhysicsAnimation")]
//[RequireComponent(typeof(Rigidbody))]
public class PhysAnimObj : bs
{
    
    Quaternion oldRot;
    internal Transform PhysObj;
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
                
        PhysObj = ((Transform)Instantiate(this.transform, pos, rot));
        PhysObj.parent = this.transform.parent;

        PhysObj.rigidbody.useGravity = false;
        PhysObj.rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        PhysObj.rigidbody.mass = this.rigidbody.mass;
        var w = PhysObj.GetComponent<Wall>();
        if (w != null) w.Anim = this.animation;
        foreach (var a in this.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation) && !(a is PhysAnimObj) && !(a is NetworkView)))
            Destroy(a);

        if (PhysObj.networkView != null)
        {
            Destroy(PhysObj.networkView);
            //PhysObj.networkView.stateSynchronization = NetworkStateSynchronization.Off;
            //PhysObj.networkView.observed = null;
            //if(Network.isServer)
            //    networkView.RPC("AllocateID", RPCMode.AllBuffered, Network.AllocateViewID());
        }        
        if (PhysObj.animation != null)
            Destroy(PhysObj.animation);
        Destroy(PhysObj.GetComponent<PhysAnimObj>());
    }
    [RPC]
    void AllocateID(NetworkViewID id)
    {
        PhysObj.networkView.viewID = id;
    }
    void FixedUpdate()
    {
        if (PhysObj.rigidbody != null)
        {
            var pc = this.pos - PhysObj.position;

            var rc = Mathf.DeltaAngle((PhysObj.rotation * Quaternion.Inverse(oldRot)).eulerAngles.z, (rot * Quaternion.Inverse(oldRot)).eulerAngles.z);
            PhysObj.rigidbody.velocity = pc * 5 * PhysObj.rigidbody.mass * strength;
            PhysObj.rigidbody.angularVelocity = new Vector3(0, 0, rc) * PhysObj.rigidbody.mass;
        }

    }

}
