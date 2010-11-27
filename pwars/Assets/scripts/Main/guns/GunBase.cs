using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public string _Name;
    public Transform cursor;
    public float patronsleft = -1;
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
}
