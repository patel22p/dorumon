using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;


//[AddComponentMenu("Game/Anim")]
public class PhysAnim : AnimHelper
{

    public override void Init()
    {
        if (networkView == null)
            gameObject.AddComponent<NetworkView>();
        
        if (wrapMode == WrapMode.Loop)
        {
            networkView.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
            networkView.observed = this;
        }
        else
        {
            networkView.stateSynchronization = NetworkStateSynchronization.Off;
            networkView.observed = null;
        }
        
        Anim.animatePhysics = true;

        if (rigidbody == null)
            gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        base.Init();
    }
    void Update()
    {
        animationState.time = Mathf.Lerp(animationState.time, _Game.networkTime * animationSpeedFactor, .96f);
        animationState.speed = animationSpeedFactor;
    }
}   