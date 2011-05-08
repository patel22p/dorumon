using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using doru;


public class AnimHelper : Tool
{

    internal Animation anim;
    public Animation Anim { get { return anim == null ? animation : anim; } }
    
    public WrapMode wrapMode;
    TimerA timer = new TimerA();
    public float animationSpeedFactor = 1;
    public float AnimationOffsetFactor;
    AnimationState animationState { get { return Anim.Cast<AnimationState>().FirstOrDefault(); } }
    
    float oldoffset;
    public override void Awake()
    {
        animation.wrapMode = wrapMode;
        if (Network.isServer)
            if (animation.playAutomatically)
                timer.AddMethod(3000, delegate
                {
                    networkView.RPC("AnimState", RPCMode.Others, animationState.enabled, animationState.time);
                });
        base.Awake();
    }
    
    [RPC]
    void AnimState(bool enabled, float time)
    {
        animationState.enabled = enabled;
        animationState.time = time;
    }

    public void Update()
    {
        //if (Anim != null && Anim.clip != null)
        animationState.speed = animationSpeedFactor;
        if (AnimationOffsetFactor != 0 && AnimationOffsetFactor != oldoffset)
        {
            animationState.time = (this.transform.position.x * AnimationOffsetFactor) % animationState.length;
            oldoffset = AnimationOffsetFactor;
        }

        timer.Update();
    }
}