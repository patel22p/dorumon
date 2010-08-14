using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using doru;

public class Box : Base
{
    void Start()
    {
        if (Network.peerType == NetworkPeerType.Disconnected) return;
        if (!Network.isServer)
        {
            networkView.RPC("RPCAddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());

        }
    }
    void FixedUpdate()
    {

        if (Network.isServer && OwnerID == null)
        {
            float min = float.MaxValue;
            Player nearp = null;
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                float dist = Vector3.Distance(p.transform.position, this.transform.position);
                if (min > dist)
                    nearp = p;
                min = Math.Min(dist, min);
            }
            if (nearp == null) return;
            NetworkView nw = GetNetworkView(nearp.networkView.owner);
            if (nw.observed == null)
            {
                nw.RPC("SetController", RPCMode.AllBuffered);
            }
        }
    }
}
