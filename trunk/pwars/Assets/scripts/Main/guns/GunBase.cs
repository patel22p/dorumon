using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : Base
{
    public bool laser;
    public float patronsDefaultCount;
    public Texture2D GunPicture;    
    public Player player;
    public List<Transform> cursor = new List<Transform>();
    public int group { get { return int.Parse(transform.parent.name); } }
    public float patronsLeft = 0;
    public string Text;
    public GameObject gunModel;
    protected override void Awake()
    {
        Text = name;
        base.Awake();
    }
    public Ray GetRay()
    {
        return new Ray(cursor[0].position, rot * new Vector3(0, 0, 1));
    }
#if UNITY_STANDALONE_WIN
    public override void Init()
    {
        if (GunPicture == null)
            GunPicture = Base2.FindAsset<Texture2D>(name);
        patronsDefaultCount = patronsLeft;
        player = root.GetComponent<Player>();
        //if (transform.Find("cursor") != null && cursor.Count == 0)
        //    cursor.Add(transform.Find("cursor"));
        if (gunModel == null && this.GetComponentInChildren<Renderer>() != null) gunModel = this.GetComponentInChildren<Renderer>().gameObject;
        base.Init();
    }
#endif
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
        if (isOwner)
        {
            if (GunPicture != null && player != null && isOwner)
                _GameWindow.gunTexture.texture = GunPicture;
        }
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
