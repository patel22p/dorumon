using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using doru;
using System.Text.RegularExpressions;


public class Box : Base
{
    public bool isController { get { return selected == Network.player.GetHashCode(); } }
    public Vector3 syncPos;
    public Quaternion syncRot;
    public Vector3 syncVelocity;
    public Vector3 syncAngularVelocity;
    protected Vector3 spawnpos;
    public bool velSync = true, posSync = true, rotSync = true, angSync = true, Sync = true;
    public int selected = -1;
    public float tsendpackets;
    public bool shared=true;
    [LoadPath("Collision1")]
    public AudioClip soundcollision;
    protected override void Awake()
    {        
        base.Awake();
        _Game.boxDerived.Add(this);
    }
    public override void Init()
    {
        gameObject.isStatic = false;
        gameObject.AddOrGet<NetworkView>().observed = this;
        gameObject.AddOrGet<Rigidbody>();
        if (collider is MeshCollider)
        {
            ((MeshCollider)collider).convex = true;
            rigidbody.centerOfMass = transform.worldToLocalMatrix.MultiplyPoint(collider.bounds.center);
        }
        
        
        base.Init();
    }
    protected virtual void Start()
    {
        spawnpos = transform.position;
        if (shared)
            if (!Network.isServer)
                networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());

    }
    void OnCollisionEnter(Collision coll)
    {
        if (this.GetType()==typeof(Box) && coll.impactForceSum.magnitude > 10)
            audio.PlayOneShot(soundcollision);
    }
    void OnCollisionStay(Collision collisionInfo)
    {

        if (this.GetType() == typeof(Box))
            if (collisionInfo.impactForceSum.magnitude > 10 && _TimerA.TimeElapsed(10))
                foreach (ContactPoint cp in collisionInfo.contacts)                    
                     _Game.particles[(int)ParticleTypes.particle_metal].Emit(cp.point, Quaternion.identity, -rigidbody.velocity / 4);
    }
    protected virtual void Update()
    {
        tsendpackets-=Time.deltaTime;
        if (_Game.bounds != null && !_Game.bounds.collider.bounds.Contains(this.transform.position) && enabled)
        {
            transform.position = SpawnPoint();
            rigidbody.velocity = Vector3.zero;
        }

        if (shared && Network.isServer)
            ControllerUpdate();
    }
    void ControllerUpdate()
    {
        float min = float.MaxValue;
        IPlayer nearp = null;
        foreach (IPlayer p in _Game.iplayers)
            if (p != null)
            {
                if (p.Alive && p.OwnerID != -1)
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
        ((Box)this).selected = -1;
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = -1;

    }
    [RPC]
    public void RPCAddNetworkView(NetworkViewID id)
    {
        var ss = networkView.stateSynchronization;
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)GroupNetwork.RPCAssignID;
        nw.observed = this;
        nw.stateSynchronization = ss;
        nw.viewID = id;
        name += "+" + Regex.Match(nw.viewID.ToString(), @"\d+").Value;
    }
    public void RPCSetOwner()
    {
        RPCSetOwner(Network.player.GetHashCode());
    }
    public virtual Vector3 SpawnPoint()
    {
        return spawnpos;
    }    
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled || !Sync) return;
        if (selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {
            if (stream.isReading || this.GetType() != typeof(Zombie) || tsendpackets<0)
            lock ("ser")
            {
                tsendpackets = .3f;
                if (stream.isWriting)
                {
                    syncPos = pos;
                    syncRot = rot;
                    syncVelocity = rigidbody.velocity;
                    syncAngularVelocity = rigidbody.angularVelocity;
                }
                if (posSync) stream.Serialize(ref syncPos);
                if (velSync) stream.Serialize(ref syncVelocity);
                if (rotSync) stream.Serialize(ref syncRot);
                if (angSync) stream.Serialize(ref syncAngularVelocity);
                if (stream.isReading)//&& syncPos != default(Vector3)
                {
                    if (posSync) pos = syncPos;
                    if (velSync) rigidbody.velocity = syncVelocity;
                    if (rotSync) rot = syncRot;
                    if (angSync) rigidbody.angularVelocity = syncAngularVelocity;
                }
            }
        }
    }
}
