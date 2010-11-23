using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using doru;


[AddComponentMenu("_Box")]
public class Box : Base
{
    public Transform bounds;
    public bool isController { get { return selected == Network.player.GetHashCode(); } }
    Vector3 pos;
    Quaternion rot;

    Vector3 velocity;
    Vector3 angularVelocity;
    public int selected;
    public bool zombieAlive { get { return (this is Zombie && ((Zombie)this).Alive); } }

    protected override void Awake()
    {
        selected = -1 ;
        if (Network.peerType == NetworkPeerType.Disconnected)
            enabled = false;
        bounds = GameObject.Find("bounds").transform;
        base.Awake();
    }
    void OnServerInitialized() { Enable(); }
    void OnConnectedToServer() { Enable(); }
    void Enable() { enabled = true; }
    protected virtual void Start()
    {
        if (this.GetType() == typeof(Box))
        {
            rigidbody.mass = 10;
            //rigidbody.angularDrag = 20;
        }
        _Game.dynamic.Add(this);
        spawnpos = transform.position;
        if (!(this is Player))
            if (!Network.isServer)
                networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());

    }
    void OnCollisionEnter(Collision infO)
    {
        if (infO.impactForceSum.magnitude > 10)
            PlaySound("Collision1");
    }
    void OnCollisionStay(Collision collisionInfo)
    {

        if (this.GetType() == typeof(Box) && _SettingsWindow.Sparks)
            if (collisionInfo.impactForceSum.magnitude > 10 && _TimerA.TimeElapsed(10))
                foreach (ContactPoint cp in collisionInfo.contacts)                    
                    _Game.Emit(_Game.metalSparkEmiters, _Game.metalSpark, cp.point, Quaternion.identity, -rigidbody.velocity / 4);
    }
    public float tsendpackets;
    [RPC]
    public void RPCShow(bool value)
    {
        CallRPC(value);
        Show(value);
    }

    protected virtual void Update()
    {
        tsendpackets-=Time.deltaTime;
        if (bounds != null && !bounds.collider.bounds.Contains(this.transform.position) && enabled)
        {
            transform.position = SpawnPoint();
            rigidbody.velocity = Vector3.zero;
        }

        if (!(this is Player) && Network.isServer)
            ControllerUpdate();
    }
    void ControllerUpdate()
    {

        float min = float.MaxValue;
        IPlayer nearp = null;
        foreach (IPlayer p in _Game.iplayers)
            if (p != null)
            {
                if (p.enabled && p.OwnerID != -1)
                {
                    float dist = Vector3.Distance(p.transform.position, this.transform.position);
                    if (min > dist)
                        nearp = p;
                    min = Math.Min(dist, min);
                }
            }

        if (nearp != null && nearp.OwnerID != -1 && selected != nearp.OwnerID)
            networkView.RPC("SetController", RPCMode.All, nearp.OwnerID);

    }


    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        if (OwnerID != -1) networkView.RPC("RPCSetOwner", np, OwnerID);
        if (selected != -1) networkView.RPC("SetController", np, selected);
        networkView.RPC("RPCShow", np, enabled);
        base.OnPlayerConnected1(np);        
    }

    [RPC]
    void RPCSetOwner(int owner)
    {
        CallRPC(owner);
        SetController(owner);
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetOwner();
        }
    }
    [RPC]
    public void SetController(int owner)
    {
        lock ("ser")
            ((Box)this).selected = owner;

    }
    [RPC]
    public void RPCResetOwner()
    {
        CallRPC();
        Debug.Log("_ResetOwner");
        ((Box)this).selected = 0;
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = -1;

    }
    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {

        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)GroupNetwork.RPCAssignID;
        nw.observed = this;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }
    public void RPCSetOwner()
    {
        RPCSetOwner(Network.player.GetHashCode());
    }


    protected Vector3 spawnpos;
    public virtual Vector3 SpawnPoint()
    {
        return spawnpos;
    }



    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {
            if (this.GetType() != typeof(Zombie) || tsendpackets<0)
                lock ("ser")
                {
                    tsendpackets = 1;
                    if (stream.isWriting)
                    {
                        pos = rigidbody.position;
                        rot = rigidbody.rotation;
                        velocity = rigidbody.velocity;
                        angularVelocity = rigidbody.angularVelocity;
                    }
                    stream.Serialize(ref pos);
                    stream.Serialize(ref velocity);
                    if (!zombieAlive)
                    {
                        stream.Serialize(ref rot);
                        stream.Serialize(ref angularVelocity);
                    }
                    if (stream.isReading && pos != default(Vector3))
                    {
                        rigidbody.position = pos;
                        rigidbody.velocity = velocity;
                        if (!zombieAlive)
                        {
                            rigidbody.rotation = rot;
                            rigidbody.angularVelocity = angularVelocity;
                        }
                    }
                }
        }
    }
}
