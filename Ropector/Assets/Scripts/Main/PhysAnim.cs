using UnityEngine;
using System.Collections;
using System.Linq;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using System.Collections.Generic;
public class PhysAnim : bs{

    
    Transform RiggidTransform;
    Transform[] animTrs;
    Transform[] riggidTrs;

    public override void Awake()
    {
        base.Awake();
    }
    public void Start()
    {
        var AnimTransform = this.transform;
        animTrs = AnimTransform.GetTransforms().Skip(1).ToArray();
        RiggidTransform = ((Transform)Instantiate(AnimTransform, pos, rot));
        Destroy(RiggidTransform.GetComponent<PhysAnim>());

        riggidTrs = RiggidTransform.GetTransforms().Skip(1).ToArray();
        for (int i = 0; i < animTrs.Length; i++)
        {
            riggidTrs[i].gameObject.AddComponent<PhysAnimObj>().original = animTrs[i];
            animTrs[i].gameObject.AddComponent<PhysAnimObj>().original = riggidTrs[i];
        }

        RiggidTransform.parent = this.transform.parent;
        
        foreach (var t in riggidTrs)
        {
            if (t.animation != null && t.animation.clip != null)
            {
                Destroy(t.animation);
                var rg = t.gameObject.AddComponent<Rigidbody>();
                rg.mass = .5f;
                rg.useGravity = false;
                rg.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            }
        }
        foreach (var t in animTrs)
        {
            foreach (var a in t.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation) && !(a is PhysAnimObj)))
                Destroy(a);            
        }
    }
    void FixedUpdate()
    {
        for (int i = 0; i < animTrs.Length; i++)
        {
            var a = animTrs[i];
            var b = riggidTrs[i];
            var ac = a.GetComponent<PhysAnimObj>();
            var bc = b.GetComponent<PhysAnimObj>();
            if (b.rigidbody != null)
            {
                var pc = a.position - b.position;
                var rc = Mathf.DeltaAngle(bc.localrot.eulerAngles.z, ac.localrot.eulerAngles.z);

                var s = Selection.activeGameObject;
                if (s == b.gameObject || s == a.gameObject)
                    Debug.Log(rc);
                b.rigidbody.velocity = pc * 10 * b.rigidbody.mass;
                b.rigidbody.angularVelocity = new Vector3(0, 0, rc) * b.rigidbody.mass; 
            }
        }
    }
}
