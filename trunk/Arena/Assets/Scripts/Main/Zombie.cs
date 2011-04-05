using System;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using doru;

public class Zombie : Shared
{
    
    AnimationState walk { get { return an["walk"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState hitstay { get { return an["hit"]; } }
    AnimationState upperHit { get { return an["upperHit"]; } }
    AnimationState idle { get { return an["idle"]; } }
    public Player selected;
    TimerA timer = new TimerA();
    
    public override void Awake()
    {
        //an.AddClip(hitstay.clip, "upperHit");
        //foreach (var t in upperbody)
        //    upperHit.AddMixingTransform(t);
        
        if (NotInstance()) return;
        base.Awake();
        _Game.Zombies.Add(this);
        AddToNetwork();
        an.wrapMode = WrapMode.Loop;
        upperHit.wrapMode = WrapMode.Clamp;
        idle.wrapMode = walk.wrapMode = WrapMode.Loop;
        upperHit.layer = 1;
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
        
        hittm -= Time.deltaTime;
        name = "Zombie" + "+" + GetId() + "+" + (selected == _PlayerOwn ? "Owner" : "");
        if (timer.TimeElapsed(100))
            if (selected != _PlayerOwn && (_PlayerOther == null || Vector3.Distance(pos, _PlayerOwn.pos) < Vector3.Distance(pos, _PlayerOther.pos)))
            {
                networkView.RPC("RPCSelectPlayer", RPCMode.All, _PlayerOwn.id);
            }
        
        if (selected != null)
        {
            var ztopl = (selected.pos - pos).normalized;
            ztopl.y = 0;

            var dist = Vector3.Distance(selected.pos, pos);
            float speed = _Game.SpeedCurv.Evaluate(dist);

            var nm = controller.velocity.magnitude /_Game.SpeedCurv.Evaluate(_Game.SpeedCurv.length);
        

            if (dist > 1f)
                controller.SimpleMove(ztopl * speed);

            bool stay = nm < .1f;
            if (stay)            
                Fade(idle);
            else if (dist < 5)
                Fade(run);
            else
                Fade(walk);

            if (dist < 2f && !upperHit.enabled)
                an.CrossFade(stay ? hitstay.name : upperHit.name);
            walk.speed = Mathf.Sqrt(Mathf.Sqrt(.3f * speed));
            run.speed = Mathf.Sqrt(Mathf.Sqrt(.1f * speed));
            model.transform.rotation = Quaternion.LookRotation(ztopl);
        }
        timer.Update();
    }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (selected == _PlayerOwn || stream.isReading)
        {
            if (stream.isWriting)
            {
                syncPos = pos;
                syncRot = rot;
                //syncVel = controller.velocity;
            }
            stream.Serialize(ref syncPos);
            stream.Serialize(ref syncRot);
            //stream.Serialize(ref syncVel);
            if (stream.isReading)
            {
                if (syncPos == Vector3.zero)
                { }
                else
                {
                    pos = syncPos;
                    rot = syncRot;
                }
                //vel = syncVel;
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
        selected = _PlayerOwn.id == id ? _PlayerOwn : _PlayerOther;
    }
    

}