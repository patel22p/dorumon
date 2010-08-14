using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using doru;


public class Box : Base
{
    protected virtual void Start()
    {
        if (Network.peerType == NetworkPeerType.Disconnected) return;
        if (!Network.isServer)
        {
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());

        }
    }
    public float cardist;
    public float pldist;
    protected virtual void FixedUpdate()
    {

        if (Network.isServer && OwnerID == null)
        {
            float min = float.MaxValue;
            IPlayer nearp = null;
            foreach (IPlayer p in GameObject.FindObjectsOfType(typeof(IPlayer)))
            {
                if (p.enabled && p.OwnerID != null)
                {
                    float dist = Vector3.Distance(p.transform.position, this.transform.position);
                    if (min > dist)
                        nearp = p;
                    min = Math.Min(dist, min);
                    if (p is CarController) cardist = dist;
                    if (p is Player) pldist = dist;
                }
            }
            if (nearp == null) return;
            NetworkView nw = GetNetworkView(nearp.OwnerID.Value);

            if (nw.observed == null)
            {
                print("set Controll " + nw.isMine + " " + this);
                nw.RPC("SetController", RPCMode.AllBuffered);
            }
        }
    }
}
