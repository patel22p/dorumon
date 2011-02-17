
using System.Linq;
using UnityEngine;
using System.Collections;
using System;
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]

public class Tower : Destroible
{
    [FindAsset("Detonator-Base")]
    public Detonator dt;
    public Gun gun;
    [FindTransform("cursor")]
    public GameObject cursor;
    public GunType gunType;
    public float lifePerSecond = .5f;    
    public override void Init()
    {
        model = GetComponentInChildren(typeof(Renderer)).gameObject;
        base.Init();
    }
    public bool Its(params GunType[] gts)
    {
        return gts.Contains(gunType);
    }
    public override void Start()
    {        
        base.Start();
        Life = 100;
        if (Its(GunType.pistol))
            lifePerSecond = .5f;
        if (Its(GunType.uzi))
            lifePerSecond = .2f;        
        if (Its(GunType.railgun))
        {
            Life = 200;
            angle = 180;
            lifePerSecond = .2f;
        }
        _Game.towers.Add(this);
        if (isController)
            networkView.RPC("InstanciateGun", RPCMode.AllBuffered, Network.AllocateViewID(),(int)gunType);            
    }
    
    [RPC]
    public void InstanciateGun(NetworkViewID id,int gt)
    {
        gunType = (GunType)gt;
        var o = Instantiate(_Game.playerPrefab.GetComponent<Player>().guns[(int)gunType], cursor.transform.position, Quaternion.identity);        
        gun = (Gun)o;
        gun.networkView.viewID = id;
        gun.player = null;
        gun.OwnerID = OwnerID;
        gun.transform.parent = this.cursor.transform;
        gun.EnableGun();
        
        SetLayer(gameObject);
    }
    public override void Awake()
    {
        base.Awake();
    }
    public float angle=30;
    public float physxguntm;
    protected override void Update()
    {
        physxguntm -= Time.deltaTime;
        if (isController)
        {
            
            if (_TimerA.TimeElapsed(1000))
                RPCSetLifeLocal(Life - lifePerSecond, -1);

            gun.tm -= Time.deltaTime;
            if (gun.tm < 0 && physxguntm < 0)
            {
                gun.tm = gun.interval;
                
                var bd = _Game.players.Union(_Game.towers.Cast<Destroible>())
                    .Union(_Game.zombies.Cast<Destroible>())
                    .Where(a => a != null && a.isEnemy(OwnerID) && a.Alive);

                var b = bd.Where(a => Math.Abs(clamp(rot.eulerAngles.y - Quaternion.LookRotation(a.pos - pos).eulerAngles.y)) < angle);
                var z = b.OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
                
                if (z != null)
                {
                    var gunpos = gun.cursor[0].transform.position;
                    Ray r = new Ray(gunpos, z.pos - gunpos);

                    if (!Physics.Raycast(r, Vector3.Distance(z.pos, gunpos), 1 << LayerMask.NameToLayer("Level")))
                    {
                        RPCShoot(Quaternion.LookRotation(r.direction));
                    }
                }
                else
                    gun.rot = rot;
            }
        }
        base.Update();
    }
    public void RPCShoot(Quaternion q) { CallRPC("Shoot", q); }
    [RPC]
    public void Shoot(Quaternion q)
    {
        gun.rot = q;
        gun.Shoot();
    }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        RPCShow(enabled);
        base.OnPlayerConnectedBase(np);
    }
    [RPC]
    public override void Die(int killedby)
    {
        _Game.towers.Remove(this);
        Alive = false;
        dt.autoCreateForce = false;
        GameObject g = (GameObject)Instantiate(dt.gameObject, pos, rot);
        Destroy(g, .6f);
        //var e = g.AddComponent<Explosion>();
        //e.exp = 3000;
        //e.radius = 8;        
        RPCShow(false);

    }
    public void RPCSetType(int type) { CallRPC("SetType",type); }
    [RPC]
    public void SetType(int type)
    {
        gunType = (GunType)type;
    }
}