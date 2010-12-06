using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public bool laser;
    public int patronsDefaultCount;
    
    public List<Transform> cursor = new List<Transform>();
    public int group { get { return int.Parse(transform.parent.name); } }
    public int patronsLeft = 0;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void Init()
    {
        patronsDefaultCount = patronsLeft;
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
        patronsLeft = build ? patronsDefaultCount : 99999;
        
    }
    protected virtual void Update()
    {
    }
    protected virtual void FixedUpdate()
    {
    }
    public override string ToString()
    {
        return name;
    }

}
