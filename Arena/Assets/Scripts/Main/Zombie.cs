using System;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using doru;

public class Zombie : Shared
{
    
    public Player selected;
    TimerA timer = new TimerA();
    public override void Awake()
    {
        _Game.Zombies.Add(this);
        AddToNetwork();
    }
    public void Start()
    {
        networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
    }
    
    public void Update()
    {
        name = "Zombie" + "+" + GetId() + (selected == _PlayerOwn ? "Owner" : "");

        if (timer.TimeElapsed(100))
            if (selected != _PlayerOwn && (_PlayerOther == null || Vector3.Distance(pos, _PlayerOwn.pos) < Vector3.Distance(pos, _PlayerOther.pos)))
            {
                RPCSelectPlayer(_PlayerOwn.id);
            }
        if (selected != null)
        {
            var v = (_PlayerOwn.pos - pos).normalized;
            controller.SimpleMove(v);
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
                    Debug.Log("Sync ErroR");
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
    public void RPCSelectPlayer(int id)
    {
        selected = _PlayerOwn.id == id ? _PlayerOwn : _PlayerOther;
        Debug.Log("Select Player " + id);
    }
    

}