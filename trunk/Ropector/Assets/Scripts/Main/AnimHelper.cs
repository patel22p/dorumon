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
            Anim.playAutomatically = false;
            Anim.Stop();
        }
        base.Awake();
    }

    public override void OnPlayerCon(NetworkPlayer player)
    {
        //if (Network.isServer)
        //    networkView.RPC("AnimState", RPCMode.Others, animationState.enabled, animationState.time);
        //base.OnPlayerCon(player);
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
        if (TimeOffsetFactor != 0 && TimeOffsetFactor != oldoffset)
        {
            animationState.time = (this.transform.position.x * TimeOffsetFactor) % animationState.length;
            oldoffset = TimeOffsetFactor;
        }

        timer.Update();
    }
}