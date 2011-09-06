using System;
using System.Linq;
using System.Text.RegularExpressions;
using doru;
using UnityEngine;

public class Zombie : Shared
{
    public Vector3 vel;
    TimerA timer = new TimerA();
    float fspeed;
    float hitTm;
    float blendTm;
    public bool dead;

    AnimationState walk { get { return an["walk"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState hitStayAnim { get { return an["hit"]; } }
    AnimationState HitUpperAnim { get { return an["upperHit"]; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState damage { get { return an["damage"]; } }
    AnimationState death { get { return an["death"]; } }
    public Player controlBy;

    public override void Awake()
    {        
        if (NotInstance()) return;
        base.Awake();
        InitOther();
        InitAnimations();
    }
    private void InitAnimations()
    {
        an.wrapMode = WrapMode.Loop;
        damage.wrapMode = HitUpperAnim.wrapMode = WrapMode.Clamp;
        HitUpperAnim.wrapMode = WrapMode.Clamp;
        hitStayAnim.wrapMode = WrapMode.Clamp;
        death.wrapMode = WrapMode.ClampForever;
        idle.wrapMode = walk.wrapMode = WrapMode.Loop;

        death.layer = damage.layer = HitUpperAnim.layer = hitStayAnim.layer = 1;

        damage.speed = 3;
    }
    private void InitOther()
    {
        _Game.alwaysUpdate.Add(this);
        life = 50;
        _Game.Zombies.Add(this);
        AddToNetwork();
    }
    public void Start()
    {        
        
        networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
    }
    public void Update()
    {
        UpdateOther();
        UpdateMove();
        timer.Update();
    }
    private void UpdateOther()
    {
        hitTm -= Time.deltaTime;
        name = "Zombie" + "+" + GetId() + "+" + (controlBy == _PlayerOwn ? "Owner" : "");
        if (timer.TimeElapsed(100))
            if (controlBy != _PlayerOwn && _PlayerOwn.Alive && (_PlayerOther == null || !_PlayerOther.Alive || Vector3.Distance(pos, _PlayerOwn.pos) < Vector3.Distance(pos, _PlayerOther.pos)))
                networkView.RPC("RPCSelectPlayer", RPCMode.All, _PlayerOwn.id);
        if (life <= 0 && controlBy == _PlayerOwn)
            networkView.RPC("Die", RPCMode.All);
    }
    private void UpdateMove()
    {
        if (controlBy != null)
        {
            Vector3 zToPl = (controlBy.pos - pos).normalized;
            zToPl.y = 0;
            float dist = Vector3.Distance(controlBy.pos, pos);
            float speed = _Game.SpeedCurv.Evaluate(dist);
            bool hit = dist < 1.5f;
            if (!hit && !damage.enabled && controlBy.Alive)
                controller.SimpleMove(zToPl * speed);

            if (!controlBy.Alive)
                Fade(idle);
            else if (damage.enabled) { }
            else if (hit)
                Hit();
            else if (dist < 5)
                Fade(run);
            else
                Fade(walk);

            walk.speed = Mathf.Sqrt(Mathf.Sqrt(.3f * speed));
            run.speed = Mathf.Sqrt(Mathf.Sqrt(.1f * speed));
            if (zToPl != Vector3.zero)
                rot = Quaternion.Lerp(Quaternion.LookRotation(zToPl), rot, .68f);
        }
    }
    public override void AlwaysUpdate()
    {
        if (!enabled)
            controller.SimpleMove(vel);
        vel *= .89f;
    }
    public void FixedUpdate()
    {        
        controller.Move(vel);
    }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (controlBy == _PlayerOwn || stream.isReading)
        {
            if (stream.isWriting)
            {
                syncPos = pos;
            }
            stream.Serialize(ref syncPos);
            if (stream.isReading)
            {
                if (syncPos == Vector3.zero)
                { }
                else
                {
                    pos = syncPos;
                }
            }
        }
    }
    private void Hit()
    {
        if (!HitUpperAnim.enabled)
        {
            timer.AddMethod(600, delegate
            {
                if (HitUpperAnim.enabled)
                    if (trigger.colliders.Contains(_PlayerOwn) && _PlayerOwn.Alive)
                        _PlayerOwn.networkView.RPC("Damage", RPCMode.All, 10);
            });
            Fade(idle);
            an.CrossFade(HitUpperAnim.name);
        }        
    }
    [RPC]
    private void Die()
    {
        if (!Alive) return;
        _PlayerOwn.killed++;
        if (controlBy == _PlayerOwn)
            _Loader.timer.AddMethod(10000, delegate()
            {
                _Game.alwaysUpdate.Remove(this);
                Network.Destroy(this.gameObject);
            });
        SetLayer(LayerMask.NameToLayer("Dead"));
        enabled = Alive = false;
        Fade(death);
    }
    [RPC]
    public void Damage(int value, float vel, int id)
    {
        this.vel = (pos - _Game.players[id].pos).normalized * vel;
        if (!Alive) return;
        life -= value;
        damage.time = 0;
        Fade(damage);
    }
    [RPC]
    public void AddNetworkView(NetworkViewID id)
    {
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)NetworkGroup.Zombie;
        nw.observed = this;
        nw.stateSynchronization = NetworkStateSynchronization.Unreliable;
        nw.viewID = id;
    }
    [RPC]
    public void RPCSelectPlayer(int id)
    {
        controlBy = _PlayerOwn.id == id ? _PlayerOwn : _PlayerOther;
    }
    
}