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
        bounds = GameObject.Find("bounds").transform;
        spawnpos = transform.position;
        rigidbody.angularDrag = 10;
        if (Network.peerType == NetworkPeerType.Disconnected) return;
        if (!Network.isServer)
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
    }
    public float cardist;
    public float pldist;
    protected Vector3 spawnpos;
    public virtual Vector3 SpawnPoint()
    {
        return spawnpos;
    }
    protected virtual void Update()
    {
        if (bounds!=null && !bounds.collider.bounds.Contains(this.transform.position) && enabled)
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
                if (p.enabled && p.OwnerID != null)
                {
                    float dist = Vector3.Distance(p.transform.position, this.transform.position);
                    if (min > dist)
                        nearp = p;
                    min = Math.Min(dist, min);                    
                }
            }
            if (nearp == null || nearp.OwnerID == null) return;
         //   NetworkView nw = GetNetworkView(nearp.OwnerID.Value);            
            if (GetComponent<NetworkRigidbody>().selected != nearp.OwnerID)
                networkView.RPC("SetController", RPCMode.AllBuffered, nearp.OwnerID.Value);
        }
    }


}
