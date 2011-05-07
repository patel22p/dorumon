using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;


public class PhysAnim : Tool
{
    
    
    internal List<Transform> rigidBoddies = new List<Transform>();
    internal List<Transform> trs = new List<Transform>();
    internal Quaternion[] oldRot;
    public float strength = 1;

    void Start()
    {

        

        Transform AnimObj = ((Transform)Instantiate(this.transform, pos, rot));
        AnimObj.transform.parent = transform.parent;
        trs = AnimObj.GetComponentInChildren<Transform>().Cast<Transform>().ToList();
        oldRot = new Quaternion[trs.Count];
        for (int i = 0; i < trs.Count; i++)
            oldRot[i] = trs[i].rotation;
        
        foreach (Transform t in this.GetComponentInChildren<Transform>())
        {            
            var r = t.gameObject.AddComponent<Rigidbody>();
            r.useGravity = false;
            r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            r.centerOfMass = Vector3.zero;
            rigidBoddies.Add(t);
            
            
        }
        Destroy(animation);
        foreach (var a in AnimObj.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation)))
            Destroy(a);
    }
    void FixedUpdate()
    {
        for (int i = 0; i < trs.Count; i++)
        {
            var AnimObj = trs[i];
            var RiggidObj = rigidBoddies[i];
            
            var pc = AnimObj.position - RiggidObj.position;
            var rc = Mathf.DeltaAngle((RiggidObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z, (AnimObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z);
            RiggidObj.rigidbody.velocity = pc * 5 * strength;
            RiggidObj.rigidbody.angularVelocity = new Vector3(0, 0, rc);
        }
        
    }
}