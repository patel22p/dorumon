using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using doru;

[AddComponentMenu("Game/AnimHelper")]
public class AnimHelper : bs
{


    public Animation Anim { get { return animation; } }
    public bool PlayOnPlayerHit;
    public bool PlayOnRopeHit;
    public WrapMode wrapMode;
    //TimerA timer = new TimerA();
    public float animationSpeedFactor = 1;
    public float TimeOffsetFactor;
    public AnimationState animationState { get { return Anim.Cast<AnimationState>().FirstOrDefault(); } }

    float oldoffset;

    public override void Awake()
    {
        //if (!enabled) { Debug.Log("Test"+gameObject.name); }
        //if (Anim == null) Anim = this.GetComponentInChildren<Animation>();
        Anim.wrapMode = wrapMode;
        Anim.playAutomatically = wrapMode != WrapMode.Default;
        if (!Anim.playAutomatically)
            Anim.Stop();
        else
            Anim.Play();


        //if (networkView == null) enabled = false;
        base.Awake();
    }
    //public override void Init()
    //{
    //    if (networkView == null)
    //        gameObject.AddComponent<NetworkView>();
        
    //    //networkView.observed = null;
    //    //if (wrapMode == WrapMode.Loop)
    //    //{
    //    //    networkView.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
    //    //    networkView.observed = this;
    //    //}
    //    //else
    //    //{
    //    //    networkView.stateSynchronization = NetworkStateSynchronization.Off;
    //    //    networkView.observed = null;
    //    //}
    //    //if(networkView != null)

    //    Anim.animatePhysics = true;        
    //    if (rigidbody == null)
    //        gameObject.AddComponent<Rigidbody>();
    //    rigidbody.isKinematic = true;
    //    base.Init();
    //}


    //float time;
    //void Update()
    //{
    //    //if (!networkView.isMine)
    //    animationState.time = Mathf.Lerp(animationState.time, _Game.networkTime * animationSpeedFactor, .96f);
    //    animationState.speed = animationSpeedFactor;
    //}
    //void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    //{
    //    if (stream.isWriting)
    //        time = animationState.time;
    //    stream.Serialize(ref time);
    //}

    [RPC]
    void Play()
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