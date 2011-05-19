using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using doru;


public class AnimHelper : bs
{

    
    public Animation Anim;
    public bool PlayOnPlayerHit;
    public bool PlayOnRopeHit;
    public WrapMode wrapMode;
    TimerA timer = new TimerA();
    public float animationSpeedFactor = 1;
    public float TimeOffsetFactor;
    AnimationState animationState { get { return Anim.Cast<AnimationState>().FirstOrDefault(); } }

    float oldoffset;

    public override void Awake()
    {
        
        if (Anim == null) Anim = this.GetComponentInChildren<Animation>();
        Anim.wrapMode = wrapMode;
        if (wrapMode == WrapMode.Default)
        {
        }
        else
        {
            Anim.playAutomatically = true;
            Anim.Play();
        }
        OnTime();
        base.Awake();
        
    }

    
    
    void OnTime()
    {
        animationState.time = ((float)_Loader.networkTime) + (this.transform.position.x * TimeOffsetFactor);
        animationState.speed = animationSpeedFactor;
    }

    public void Update()
    {        
        timer.Update();
    }
}