using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using doru;

[AddComponentMenu("Game/AnimHelper")]
public class AnimHelper : bs
{

    
    public Animation Anim;
    public bool PlayOnPlayerHit;
    public bool PlayOnRopeHit;
    public WrapMode wrapMode;
    public float animationSpeedFactor = 1;
    public float TimeOffsetFactor;
    public AnimationState animationState { get { return Anim.Cast<AnimationState>().FirstOrDefault(); } }
    float oldoffset;
    
    public override void Awake()
    {
        if (Anim == null) Anim = this.GetComponentInChildren<Animation>();
        Anim.wrapMode = wrapMode;
        Anim.playAutomatically = wrapMode != WrapMode.Default;
        if (!Anim.playAutomatically) 
            Anim.Stop();
        else
            Anim.Play();

        animationState.speed = animationSpeedFactor;
        base.Awake();
    }

    [RPC]
    public void Play()
    {
        Anim.Play();
    }   
    
    internal void RPCPlay()
    {
        if (this.networkView != null)
        {
            if (networkView.isMine) this.networkView.RPC("Play", RPCMode.All);
        }
        else
            this.Play();
    }
}