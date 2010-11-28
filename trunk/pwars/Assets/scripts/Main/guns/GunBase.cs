using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public int defcount;
    public string _Name ="None";
    internal Transform cursor;
    internal Quaternion rotation;    
    internal float patronsleft = 0;
    protected override void Awake()
    {
        cursor = transform.Find("cursor");
        base.Awake();
    }
    public virtual void DisableGun()
    {
        Show(false);
    }
    public virtual void EnableGun()
    {
        Show(true);
    }
    public void Reset()
    {
        patronsleft = defcount;
    }
    protected virtual void Update()
    {
        UpdateAim();
    }
    protected virtual void FixedUpdate()
    {
        UpdateAim();
    }
    protected virtual void UpdateAim()
    {
        if (isOwner) rotation = _Cam.transform.rotation;
        this.transform.rotation = rotation;
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (isOwner) rotation = _Cam.transform.rotation;
        stream.Serialize(ref rotation);
        transform.rotation = rotation;
    }


}
