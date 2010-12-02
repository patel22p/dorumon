using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public bool laser;
    public int defcount;
    public string _Name ="None";
    public List<Transform> cursor = new List<Transform>();
    public int group { get { return int.Parse(transform.parent.name); } }
    internal float patronsleft = 0;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void Init()
    {

        //if (transform.Find("cursor") != null && cursor.Count == 0)
        //    cursor.Add(transform.Find("cursor"));
        base.Init();
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
