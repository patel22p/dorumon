using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using doru;
using System.Text.RegularExpressions;
using System.Linq;

public class Box : Shared
{
#if(UNITY_EDITOR && UNITY_STANDALONE_WIN)
    public override void Init()
    {
        base.Init();
        rigidbody.mass = 5;

        if (collider.sharedMaterial == null)
            collider.sharedMaterial = Base2.FindAsset<PhysicMaterial>("box");                        
    }
#endif

    protected override void Start()
    {        
        _Game.boxes.Add(this);
        base.Start();
    }
    protected virtual void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.impactForceSum.magnitude > 10 && _TimerA.TimeElapsed(10))
            foreach (ContactPoint cp in collisionInfo.contacts)
                _Game.particles[(int)ParticleTypes.particle_metal].Emit(cp.point, Quaternion.identity, -rigidbody.velocity / 4);
    }
    protected virtual void OnCollisionEnter(Collision coll)
    {
        if (coll.impactForceSum.magnitude > 10)
            audio.PlayOneShot(soundcollision, .5f);
    }

}
