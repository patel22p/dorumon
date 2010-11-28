using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public int defcount;
    public string _Name ="None";
    internal Transform cursor;
    public int group=1;
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
    }
    protected virtual void FixedUpdate()
    {
    }

}
