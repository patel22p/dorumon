using System;
using System.Linq;
using System.Text.RegularExpressions;
using doru;
using UnityEngine;

public class Zombie : Shared
{

    AnimationState walk { get { return an["walk"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState hitStayAnim { get { return an["hit"]; } }
    AnimationState HitUpperAnim { get { return an["upperHit"]; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState damage { get { return an["damage"]; } }
    AnimationState death { get { return an["death"]; } }
    public Player controlby;
    TimerA timer = new TimerA();

    public override void Awake()
    {        
        if (NotInstance()) return;
        life = 50;
        base.Awake();
        _Game.Zombies.Add(this);
        AddToNetwork();
        an.wrapMode = WrapMode.Loop;
        damage.wrapMode = HitUpperAnim.wrapMode = WrapMode.Clamp;
        HitUpperAnim.wrapMode = WrapMode.Clamp;
        hitStayAnim.wrapMode = WrapMode.Clamp;
        death.wrapMode = WrapMode.ClampForever;
        idle.wrapMode = walk.wrapMode = WrapMode.Loop;

        death.layer = damage.layer = HitUpperAnim.layer = hitStayAnim.layer = 1;        

        damage.speed = 3;
        //foreach (var t in upperbody)
        //    upperHit.AddMixingTransform(t);
    }
    public void Start()
    {        
        networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
    }
    float fspeed;
    float hittm;
    float blendtm;
    
    public void Update()
    {
        if (Alive)
        {
            UpdateOther();
            UpdateMove();
            timer.Update();
        }
        else
            controller.SimpleMove(Vector3.zero);
    }

    private void UpdateMove()
    {
        
        if (controlby != null)
        {
            var ztopl = (controlby.pos - pos).normalized;
            ztopl.y = 0;
            var dist = Vector3.Distance(controlby.pos, pos);
            float speed = _Game.SpeedCurv.Evaluate(dist);
            var nm = controller.velocity.magnitude / _Game.SpeedCurv.Evaluate(_Game.SpeedCurv.length);

            if (dist > 1f && !damage.enabled && controlby.Alive && !hitStayAnim.enabled)
                controller.SimpleMove(ztopl * speed);

            bool stay = nm < .1f;

            if (!controlby.Alive)
                Fade(idle);
            else if (damage.enabled) { }
            else if (dist < 2f)
                Hit(stay);
            else if (dist < 5)
                Fade(run);
            else
                Fade(walk);

            walk.speed = Mathf.Sqrt(Mathf.Sqrt(.3f * speed));
            run.speed = Mathf.Sqrt(Mathf.Sqrt(.1f * speed));
            if (ztopl != Vector3.zero)
            {
                rot = Quaternion.Lerp(Quaternion.LookRotation(ztopl), rot, .68f);
            }
            
                
        }
    }

    private void Hit(bool stay)
    {
        if (!HitUpperAnim.enabled && !hitStayAnim.enabled)
        {
            if (trigger.colliders.Contains(_PlayerOwn) && _PlayerOwn.Alive)
                _PlayerOwn.networkView.RPC("Damage", RPCMode.All, 10);
            Debug.Log(stay);
            an.CrossFade(stay ? hitStayAnim.name : HitUpperAnim.name);
        }        
        
    }

    private void UpdateOther()
    {
        hittm -= Time.deltaTime;
        name = "Zombie" + "+" + GetId() + "+" + (controlby == _PlayerOwn ? "Owner" : "");
        if (timer.TimeElapsed(100))
            if (controlby != _PlayerOwn && _PlayerOwn.Alive && (_PlayerOther == null || !_PlayerOther.Alive || Vector3.Distance(pos, _PlayerOwn.pos) < Vector3.Distance(pos, _PlayerOther.pos)))
                networkView.RPC("RPCSelectPlayer", RPCMode.All, _PlayerOwn.id);
        if (life <= 0 && controlby == _PlayerOwn)
            networkView.RPC("Die", RPCMode.All);
    }
    [RPC]
    private void Die()
    {
        _PlayerOwn.killed++;
        if (!Alive) return;
        if (Network.isServer)
            _Loader.timer.AddMethod(10000, delegate()
            {
                Network.RemoveRPCs(this.networkView.viewID);
                Network.Destroy(this.gameObject);
            });
        SetLayer(LayerMask.NameToLayer("Dead"));
        Alive = false;
        Fade(death);
    }
    public bool dead;

    [RPC]
    public void Damage(int value)
    {
        if (!Alive) return;
        life -= value;
        damage.time = 0;
        Fade(damage);
    }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (controlby == _PlayerOwn || stream.isReading)
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
        controlby = _PlayerOwn.id == id ? _PlayerOwn : _PlayerOther;
    }


}