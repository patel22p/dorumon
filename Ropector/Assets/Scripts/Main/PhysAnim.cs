using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;


[AddComponentMenu("Game/Anim")]

public class PhysAnim : AnimHelper
{
    
    public List<Transform> rigidBoddies = new List<Transform>();
    internal List<Transform> trs = new List<Transform>();
    internal Quaternion[] oldRot;
    public float strength = 1;

    public void Start()
    {
        if (!editor)
        {
            Transform AnimObj = ((Transform)Instantiate(this.transform, pos, rot));

            foreach (var a in AnimObj.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation)))
                Destroy(a);
            Destroy(Anim);

            Anim = AnimObj.animation;

            AnimObj.transform.parent = transform.parent;
            Anim = AnimObj.GetComponent<PhysAnim>().Anim;

            trs = AnimObj.GetComponentInChildren<Transform>().Cast<Transform>().ToList();
            oldRot = new Quaternion[trs.Count];
            for (int i = 0; i < trs.Count; i++)
                oldRot[i] = trs[i].rotation;

            if (rigidBoddies.Count == 0) rigidBoddies = transform.Cast<Transform>().ToList();
            foreach (Transform t in rigidBoddies)
            {
                var r = t.gameObject.AddComponent<Rigidbody>();
                r.useGravity = false;
                r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                r.centerOfMass = Vector3.zero;
            }            
        }
    }
    void FixedUpdate()
    {
        if (editor) return;
        for (int i = 0; i < trs.Count; i++)
        {
            var AnimObj = trs[i];
            var RiggidObj = rigidBoddies[i];

            var pc = AnimObj.position - RiggidObj.position;
            var z1 = (RiggidObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z;
            var z2 = (AnimObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z;
            var rc = Mathf.DeltaAngle(z1, z2);
            RiggidObj.rigidbody.velocity = pc * 5 * strength;
            RiggidObj.rigidbody.angularVelocity = new Vector3(0, 0, rc);
        }
        
    }
}