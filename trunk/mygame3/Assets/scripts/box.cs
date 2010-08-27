using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using doru;


[AddComponentMenu("_Box")]
public class box : Base
{
    public Transform bounds;
    protected virtual void Start()
    {        
        _Spawn.dynamic.Add(this);
        spawnpos = transform.position;
        if (!(this is Player))
        {
            rigidbody.angularDrag = 30;
            if (Network.peerType == NetworkPeerType.Disconnected) return;
            if (!Network.isServer)
                networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        }
        bounds = GameObject.Find("bounds").transform;
    }
    protected Vector3 spawnpos;
    public virtual Vector3 SpawnPoint()
    {
        return spawnpos;
    }
    protected virtual void Update()
    {
        if (selected != -1)
            id = selected.GetHashCode();
        else
            id = -2;

        if (bounds != null && !bounds.collider.bounds.Contains(this.transform.position) && enabled)
        {
            transform.position = SpawnPoint();
            rigidbody.velocity = Vector3.zero;
        }

        if (this is Player) return;

        if (Network.isServer)
        {
            float min = float.MaxValue;
            IPlayer nearp = null;
            foreach (IPlayer p in _Spawn.iplayers)
            {
                if (p.enabled && p.OwnerID != -1)
                {
                    float dist = Vector3.Distance(p.transform.position, this.transform.position);
                    if (min > dist)
                        nearp = p;
                    min = Math.Min(dist, min);
                }
            }

            if (nearp == null || nearp.OwnerID == -1) return;
            if (selected != nearp.OwnerID)
            {
                networkView.RPC("SetController", RPCMode.AllBuffered, nearp.OwnerID);
            }
        }
    }

    public bool isController { get { return selected == Network.player.GetHashCode(); } }
    Vector3 pos;
    Quaternion rot;  
    Vector3 velocity;
    Vector3 angularVelocity;

    public int selected = -1;
    public int id = -3;
    public bool zombiealive { get { return (this is Zombie && ((Zombie)this).Alive); } }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {
            lock ("ser")
            {
                if (stream.isWriting)
                {
                    pos = rigidbody.position;
                    rot = rigidbody.rotation;
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                }
                stream.Serialize(ref pos);
                stream.Serialize(ref velocity);
                if (!zombiealive)
                {
                    stream.Serialize(ref rot);                    
                    stream.Serialize(ref angularVelocity);
                }
                if (stream.isReading && pos != default(Vector3))
                {
                    rigidbody.position = pos;
                    rigidbody.velocity = velocity;
                    if (!zombiealive)
                    {
                        rigidbody.rotation = rot;                        
                        rigidbody.angularVelocity = angularVelocity;
                    }
                }
            }
        }
    }

}
