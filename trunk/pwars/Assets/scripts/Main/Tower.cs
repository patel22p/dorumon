
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
    //[PathFind("cursor")]
    public GameObject cursor;
    public string gunType = "";
    public override void Init()
    {
        if (Life == 0) Life = 100;
        model = GetComponentInChildren(typeof(Renderer)).gameObject;
        base.Init();
    }    
    public override void Start()
    {
        base.Start();
        _Game.towers.Add(this);
        if (isController)
        {
            _TimerA.AddMethod(delegate
            {
                if (gunType != "")
                {
                    networkView.RPC("InstanciateGun", RPCMode.AllBuffered, Network.AllocateViewID());                    
                }
            });
        }
    }
    
    [RPC]
    public void InstanciateGun(NetworkViewID id)
    {
        var o = Instantiate(_Game.playerPrefab.GetComponent<Player>().guns[(int)gunType.Parse<GunType>()], cursor.transform.position, Quaternion.identity);
        gun = (Gun)o;
        gun.networkView.viewID = id;
        gun.player = null;
        gun.transform.parent = this.cursor.transform;
        gun.EnableGun();
    }
    public override void Awake()
    {
        base.Awake();
    }
    public float range=20;
    protected override void Update()
    {
        //UpdateLightmap(model.renderer.materials);
        gun.tm -= Time.deltaTime;

        if (gun.tm < 0)
        {
            gun.tm = gun.interval;

            var b = _Game.players.Union(_Game.zombies.Cast<Destroible>())
                .Where(a => a != null && a.isEnemy(OwnerID) && a.Alive 
                    && Math.Abs(clamp(rot.eulerAngles.y - Quaternion.LookRotation(a.pos - pos).eulerAngles.y)) < range);            
            var z = b.OrderBy(a => Vector3.Distance(a.pos, pos)).FirstOrDefault();
            if (z != null)
            {
                Ray r = new Ray(gun.pos, z.pos - gun.pos);

                if (!Physics.Raycast(r, Vector3.Distance(z.pos, gun.pos), 1 << LayerMask.NameToLayer("Level")))
                {
                    gun.rot = Quaternion.LookRotation(r.direction);
                    gun.RPCShoot();
                }
            }
            else
                gun.rot = rot;

        }
        base.Update();
    }
    public override bool isEnemy(int killedby)
    {
        return true;
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
        var e = g.AddComponent<Explosion>();
        e.exp = 3000;
        e.radius = 8;        
        RPCShow(false);

    }
}