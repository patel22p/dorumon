using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using doru;


public class AnimHelper : MonoBehaviour
{

    public WrapMode wrapMode;
    TimerA timer = new TimerA();
    public float animationSpeedFactor = 1;
    public float AnimationOffsetFactor;
    AnimationState animationState { get { return anim.Cast<AnimationState>().FirstOrDefault(); } }
    Animation anim { get { return animation; } }
    float oldoffset;
    public void Start()
    {
        animation.wrapMode = wrapMode;
        if (Network.isServer)
            if (animation.playAutomatically)
                timer.AddMethod(3000, delegate
                {
                    networkView.RPC("AnimState", RPCMode.Others, animationState.enabled, animationState.time);
                });

    }
    
    [RPC]
    void AnimState(bool enabled, float time)
    {
        animationState.enabled = enabled;
        animationState.time = time;
    }

    public void Update()
    {
        if (anim != null && anim.clip != null)
            animationState.speed = animationSpeedFactor;
        if (anim != null && AnimationOffsetFactor != 0 && AnimationOffsetFactor != oldoffset)
        {
            animationState.time = (this.transform.position.x * AnimationOffsetFactor) % animationState.length;
            oldoffset = AnimationOffsetFactor;
        }

        timer.Update();
    }
}