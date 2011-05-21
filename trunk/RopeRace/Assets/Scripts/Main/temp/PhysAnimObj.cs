//using UnityEngine;
//using System.Collections;
////using UnityEditor;
//using System.Linq;


//public class PhysAnimObj : bs
//{
//    public Animation anim;
//    Quaternion oldRot;
//    private Transform AnimObj;
//    public WrapMode wrapMode;
    

//    public override void Awake()
//    {
//        base.Awake();        
//        oldRot = this.rot;
//    }
//    public float strength = 1;
//    public override void Init()
//    {
//        if (animation != null && animation.clip && anim == null)
//            anim = animation;
//        base.Init();
//    }
//    public virtual void Start()
//    {        
//        if (anim != null)
//        {
//            if (wrapMode != WrapMode.Default)
//                animation.wrapMode = wrapMode;

//            rigidbody.useGravity = false;
//            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
//            rigidbody.centerOfMass = Vector3.zero;
//            AnimObj = ((Transform)Instantiate(this.transform, pos, rot));            
//            AnimObj.parent = this.transform.parent;
//            anim = AnimObj.GetComponent<PhysAnimObj>().anim;
//            foreach (var a in AnimObj.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation)))
//                Destroy(a);
//            Destroy(animation);
//        }
//    }

//    void FixedUpdate()
//    {
//        if (AnimObj != null)
//        {
//            var pc = AnimObj.position - this.pos;            
//            var rc = Mathf.DeltaAngle((rot * Quaternion.Inverse(oldRot)).eulerAngles.z,(AnimObj.rotation * Quaternion.Inverse(oldRot)).eulerAngles.z);
//            this.rigidbody.velocity = pc * 5 * strength;
//            this.rigidbody.angularVelocity = new Vector3(0, 0, rc);
//        }
//    }

//}