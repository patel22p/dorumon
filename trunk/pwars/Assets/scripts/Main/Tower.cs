using System.Linq;
using UnityEngine;
using System.Collections;
using System;
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class Tower : Destroible
{
    [LoadPath("Detonator-Base")]
    public Detonator dt;
    public Gun gun;
    public bool disableZombieAtack;
    //[PathFind("cursor")]
    public GameObject cursor;
    public int guni=-1;
    public override void Init()
    {
        model = GetComponentInChildren(typeof(Renderer)).gameObject;
        range = 90;
        base.Init();
    }
    public GameObject model;

    protected override void Start()
    {
        base.Start();
        
        _Game.towers.Add(this);
        

        if (isController)
        {
            _TimerA.AddMethod(delegate
            {
                if (guni != -1)
                {
                    networkView.RPC("InstanciateGun", RPCMode.AllBuffered, Network.AllocateViewID());                    
                }
            });
        }
    }
    [RPC]
    private void InstanciateGun(NetworkViewID id)
    {
        Debug.Log("instgun " + id);
        var o = Instantiate(_Game.playerPrefab.GetComponent<Player>().guns[guni], cursor.transform.position, Quaternion.identity);
        gun = (Gun)o;
        gun.networkView.viewID = id;
        gun.player = null;
        gun.transform.parent = this.cursor.transform;
        gun.EnableGun();
    }
    protected override void Awake()
    {
        base.Awake();
    }
    public float range=20;
    protected override void Update()
    {
        //UpdateLightmap(model.renderer.materials);
        if (gun != null && _TimerA.TimeElapsed((int)(gun.interval * 1000f)))
        {
            var b = _Game.zombies.Where(a => a != null && a.Alive && clamp(rot.eulerAngles.y - Quaternion.LookRotation(a.pos - pos).eulerAngles.y) < range);            
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
    
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        RPCShow(enabled);
        base.OnPlayerConnected1(np);
    }
    [RPC]
    public override void Die(int killedby)
    {
        Alive = false;
        dt.autoCreateForce = false;
        GameObject g = (GameObject)Instantiate(dt.gameObject, pos, rot);

        var e = g.AddComponent<Explosion>();
        e.exp = 3000;
        e.radius = 8;
        e.damage = 50;
        RPCShow(false);

    }
}