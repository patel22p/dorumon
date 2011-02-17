using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

public class GunBase : bs
{
    internal bool laser;
    public float ves;
    public Texture2D GunPicture;
    public int guntype;
    public float score;
    public float towerScore = -1;// { get { return score * 5; } }
    public Player player;
    public List<Transform> cursor = new List<Transform>();
    public float patronsLeft = 0;
    public string Text;
    public GameObject gunModel;
    public override void Awake()
    {
        base.Awake();
    }
    public override void InitValues()
    {
        Text = name;
        base.InitValues();
    }
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    public override void Init()
    {
        networkView.stateSynchronization = NetworkStateSynchronization.Off;
        networkView.observed = null;
        if (GunPicture == null)
            GunPicture = Base2.FindAsset<Texture2D>(name);
        player = root.GetComponent<Player>();
        //if (transform.Find("cursor") != null && cursor.Count == 0)
        //    cursor.Add(transform.Find("cursor"));
        var t = this.transform.GetTransforms().Skip(1).FirstOrDefault(a=>a.name != "cursor");        
        if (t != null)
            gunModel = t.gameObject;
        base.Init();
    }
#endif
    public virtual void DisableGun()
    {
        Show(false);
    }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        RPCSetLaser(laser);
        base.OnPlayerConnectedBase(np);
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
        patronsLeft = _Game.mapSettings.patrons[guntype];
        laser = false;
    }
    protected virtual void Update()
    {
        if (isOwner)
        {
            if (GunPicture != null && player != null && isOwner)
                _GameWindow.gunTexture.texture = GunPicture;
        }
        if (player != null)
        {
            if (enabled)
                player.rigidbody.mass = player.defmass + ves * player.defmass - (player.SpeedUpgrate * .07f);
            player.UpdateAim();
        }
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
    internal int group { get { return int.Parse(transform.parent.name); } }
    public Ray GetRay()
    {
        var p = cursor[0].position;
        var pl = new Plane(rot * Vector3.forward, pos);
        p -= rot * Vector3.forward * pl.GetDistanceToPoint(p);
        var r = new Ray(p, rot * new Vector3(0, 0, 1));
        return r;
    }

}
