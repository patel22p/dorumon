using System;
using System.Linq;
using UnityEngine;
[AddComponentMenu("Game/Wall")]
public class Wall : PhysAnimObj
{
    public bool die;
    //bool dieOnHit = true;
    public bool attachRope = true;
    public Vector3 RopeForce = new Vector3(1, 1f, 1);
    public float RopeLength = 1f;
    public float SpeedTrackFactor;
    public Vector3 bounchyForce;
    public Collider[] Ignore;
    public PhysAnimObj[] animationToPlay;
    public float animationSpeedFactor = 1;
    public float AnimationOffsetFactor;

    public override void Start()
    {

        base.Start();
        if (Ignore != null)
            foreach (Collider a in this.transform.GetTransforms().Where(a => a.collider != null).Select(a => a.collider))
                foreach (Collider b in Ignore)
                    Physics.IgnoreCollision(a, b);

        if (Network.isServer)
            if (anim != null && anim.clip != null && animationState.enabled && animationState != null)
                _Game.timer.AddMethod(3000, delegate
            {
                networkView.RPC("AnimState", RPCMode.Others, animationState.enabled, animationState.time);
            });

    }
    float oldoffset;
    void Update()
    {
        if (anim != null && anim.clip != null)
            animationState.speed = animationSpeedFactor;
        if (anim != null && AnimationOffsetFactor != 0 && AnimationOffsetFactor != oldoffset)
        {
            animationState.time = (this.x *  AnimationOffsetFactor) % animationState.length;
            oldoffset = AnimationOffsetFactor;
        }
    }
    AnimationState animationState { get { return anim.Cast<AnimationState>().FirstOrDefault(); } }
    [RPC]
    void AnimState(bool enabled, float time)
    {
        animationState.enabled = enabled;
        animationState.time = time;
    }

    public override void Init()
    {
        base.Init();
        
        Debug.Log("Wall Init");
        if (this.networkView == null)
            this.gameObject.AddComponent<NetworkView>();
        this.networkView.observed = null;
        this.networkView.stateSynchronization = NetworkStateSynchronization.Off;
        if (anim && rigidbody == null)
            gameObject.AddComponent<Rigidbody>();
        if (anim != null)
            renderer.lightmapIndex = -1;

        if (rigidbody != null)
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
        
    }
    
    
    void OnCollisionEnter(Collision coll)
    {
        if(PlayOnPlayerHit)
            OnHit();

        if (bounchyForce != Vector3.zero)
        {
            var f = coll.impactForceSum.magnitude * 10;
            coll.rigidbody.AddForce(bounchyForce.x * f, bounchyForce.y * f, bounchyForce.z * f);
        }
    }
    public bool PlayOnRopeHit = true;
    public bool PlayOnPlayerHit;
    public void OnHit()
    {
        if (networkView != null)
            networkView.RPC("PlayAnim", RPCMode.All);        
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        _Game.timer.AddMethod(2000, delegate { SetState(player); });
    }
    private void SetState(NetworkPlayer player)
    {
        if (anim != null && animationState.enabled && animationState != null && Network.isServer && Network.connections.Contains(player))
            networkView.RPC("AnimState", player, animationState.enabled, animationState.time);
    }


    [RPC]
    private void PlayAnim()
    {
        //Debug.Log("Play");
        foreach (var a in animationToPlay)
            a.anim.Play();

        if (animation != null)
            this.animation.Play();
        else
        {
            var pha = this.GetComponent<Wall>();
            if (pha != null && pha.anim != null)
                pha.anim.Play();
        }
        
    }
}