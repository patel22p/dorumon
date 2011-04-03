using System;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using doru;

public class Zombie : Shared
{
    
    AnimationState walk { get { return an["walk"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState idle { get { return an["idle"]; } }
    public Player selected;
    TimerA timer = new TimerA();
    public override void Awake()
    {
        if (Check()) return;

        base.Awake();
        _Game.Zombies.Add(this);
        AddToNetwork();
        an.wrapMode = WrapMode.Loop;
        idle.wrapMode = walk.wrapMode = WrapMode.Loop;
    }
    public void Start()
    {
        networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
    }
    float fspeed;
    public void Update()
    {
        
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
            float normspeed = speed / _Game.SpeedCurv.Evaluate(_Game.SpeedCurv.length);
            controller.SimpleMove(ztopl * speed);
            
            if (normspeed < .1f)
                Fade(idle, 1);
            if (dist < 5)
                Fade(run, 1);
            else
                Fade(walk, 1);

            //fspeed = Mathf.Lerp(speed, fspeed, .5f);
            //Debug.Log(speed);
            run.speed = walk.speed = .3f * speed;
            model.transform.rotation = Quaternion.LookRotation(ztopl);

            //if (selected == _PlayerOwn)
            //{
            //    var v = (_PlayerOwn.pos - pos).normalized;
            //}
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