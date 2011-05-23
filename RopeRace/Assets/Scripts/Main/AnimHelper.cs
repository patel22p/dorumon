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
        Init();
        Anim.wrapMode = wrapMode;
        if (!Anim.playAutomatically)
            Anim.Stop();
        else
            Play();
        animationState.speed = animationSpeedFactor;
        base.Awake();
    }
    public override void Init()
    {
        if (Anim == null) Anim = this.GetComponentInChildren<Animation>();
        if (Anim == null)
            Debug.Log(name);
        else
            Anim.playAutomatically = wrapMode == WrapMode.Loop;
        base.Init();
    }
    [RPC]
    public void Play()
    {
        //Debug.Log("Play");
        //Anim.Play();
        foreach (AnimationState a in Anim)
        {
            Anim.Blend(a.name, 1, 0);
        }
    }   
    
    internal void RPCPlay()
    {
        if (this.networkView != null)
        {
            //if (networkView.isMine)
            //{
                this.Play();
                this.networkView.RPC("Play", RPCMode.Others);
            //}
        }
        else
            this.Play();
    }
}