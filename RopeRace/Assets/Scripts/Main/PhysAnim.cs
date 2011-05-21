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
    public bool unrotate;
    public float mass = 1;
    public float strengthRot = 1;
    public void Start()
    {        
        SetupAnim();
        SetupNw();
    }

    private void SetupAnim()
    {
        if (rigidBoddies.Count == 0) rigidBoddies = transform.Cast<Transform>().ToList();
        Transform AnimObj = ((Transform)Instantiate(this.transform, pos, rot));
        foreach (var a in AnimObj.GetComponentsInChildren<Component>().Where(a => !(a is Transform) && !(a is Animation)))
            Destroy(a);
        Destroy(Anim);
        Anim = AnimObj.animation;
        AnimObj.transform.parent = transform.parent;
        Anim = AnimObj.GetComponent<PhysAnim>().Anim;

        trs = AnimObj.GetComponent<PhysAnim>().rigidBoddies;

        oldRot = new Quaternion[trs.Count];
        for (int i = 0; i < trs.Count; i++)
            oldRot[i] = trs[i].rotation;
        foreach (Transform t in rigidBoddies)
        {
            var r = t.gameObject.AddComponent<Rigidbody>();
            r.useGravity = false;
            r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            r.mass = mass;
            r.centerOfMass = Vector3.zero;
        }
    }

    private void SetupNw()
    {
        //if (wrapMode == WrapMode.Loop)
        //{
        //    networkView.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        //    networkView.observed = Anim;
        //}
        //else
        //{
            networkView.stateSynchronization = NetworkStateSynchronization.Off;
            networkView.observed = null;
        //}
    }
    public override void Init()
    {
        if (networkView == null)
            gameObject.AddComponent<NetworkView>();
        //if (wrapMode == WrapMode.Loop)
        //    networkView.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        //else
        networkView.stateSynchronization = NetworkStateSynchronization.Off;
        networkView.observed = null;
        base.Init();
    }
    
    void FixedUpdate()
    {
        for (int i = 0; i < trs.Count; i++)
        {
            var AnimObj = trs[i];
            var RiggidObj = rigidBoddies[i];

            var pc = AnimObj.position - RiggidObj.position;
            var z1 = (RiggidObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z;            
            var z2 = (AnimObj.rotation * Quaternion.Inverse(oldRot[i])).eulerAngles.z;
            if (unrotate) z2 = 0;
            var rc = Mathf.DeltaAngle(z1, z2);
            RiggidObj.rigidbody.velocity = pc * 5 * strength;
            RiggidObj.rigidbody.angularVelocity = new Vector3(0, 0, rc * strengthRot * .5f);
        }        
    }
    void Update()
    {
        if (wrapMode == WrapMode.Loop)
            animationState.time = Mathf.Lerp(animationState.time, _Game.networkTime * animationSpeedFactor, .20f);        
    }
}   