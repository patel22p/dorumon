using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public bool laser;
    public float patronsDefaultCount;
    public Player player;
    public List<Transform> cursor = new List<Transform>();
    public int group { get { return int.Parse(transform.parent.name); } }
    public float patronsLeft = 0;
    
    protected override void Awake()
    {
        base.Awake();
    }
    public override void Init()
    {
        patronsDefaultCount = patronsLeft;
        player = root.GetComponent<Player>();
        //if (transform.Find("cursor") != null && cursor.Count == 0)
        //    cursor.Add(transform.Find("cursor"));
        base.Init();
    }
    public virtual void DisableGun()
    {
        Show(false);
    }

    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        RPCSetLaser(laser);
        base.OnPlayerConnected1(np);
    }



    public void RPCSetLaser(bool value) { CallRPC("SetLaser", value); }
    [RPC]
    public void SetLaser(bool value)
    {
        laser = value;
    }

    public virtual void EnableGun()
    {
        Show(true);
    }
    public void Reset()
    {
        patronsLeft = patronsDefaultCount;
        
    }
    protected virtual void Update()
    {
        if(player!=null)
            player.UpdateAim();
    }
    protected virtual void FixedUpdate()
    {
        if (player != null)
            player.UpdateAim();
    }
    public override string ToString()
    {
        return name;
    }

}
