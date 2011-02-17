using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public enum MapItemType { none, door, lift, jumper, shop, money, speed, laser, trap, spotlight, timewarp, teleport, speedupgrate, lifeupgrate, antigravitation, energy, life }
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
[AddComponentMenu("MapItem")]
public class MapItem : bs
{
    bool isCheckOutCalled;
    [FindAsset("checkout")]
    public AudioClip opendoor;
    public AudioClip checkOutSound;
    public bool payonce;
    public bool hide = false;
    public bool lookat;
    public bool autoTake;
    public Vector3 Jumper = new Vector3(0, 1000, 0);
    public Vector2 Speed;
    public float Distance = 10;
    public int itemsLeft = 1;
    public bool endless;
    public Transform teleport;
    public string itemtype;
    public string gunType;
    public int bullets = 1000;
    public string text = "";
    public Transform[] buttons = new Transform[0];
    public Collider[] boundings = new Collider[0];
    public float RespawnTm = 1;    
    [FindAsset("Player")]
    public Player playerPrefab;
    [FindAsset("wave")]
    public GameObject wavePrefab;
#if (UNITY_EDITOR && UNITY_STANDALONE_WIN)
    public override void Init()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.gameObject.isStatic = false;
            t.gameObject.layer = LayerMask.NameToLayer("MapItem");
        }
        var g = gameObject;
        
        if (g.rigidbody != null)
            DestroyImmediate(g.rigidbody);
        if (g.GetComponents<NetworkView>().Count() > 1)
            DestroyImmediate(g.GetComponents<NetworkView>().Last());

        if (!inited)
        {
            //g.AddComponent<NetworkView>();
            //g.AddComponent<AudioSource>(); 
            //if(g.rigidbody)
            //    g.rigidbody.isKinematic = true;
            inited = true;
        }
        if (this.networkView.observed != null)
            this.networkView.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        else
            this.networkView.stateSynchronization = NetworkStateSynchronization.Off;

        foreach (var a in g.GetComponentsInChildren<Animation>())
        {
            a.animatePhysics = true;
            a.playAutomatically = false;
        }
        if (itemType == MapItemType.shop)
        {
            var gun = playerPrefab.guns[guni];
            var cur = transform.Find("cursor");
            if (cur != null && cur.childCount == 0)
            {
                var g2 = (GameObject)Instantiate(gun.gunModel, cur.transform.position, cur.transform.rotation);
                g2.transform.parent = cur;
            }
            //if (gun is Gun)
                //bullets = (int)(20 / ((Gun)gun).interval);
        }
        base.Init();        
    }
    public override void InitValues()
    {
        checkOutSound = opendoor;
        if (buttons.Length == 0)
            buttons = new Transform[] { this.transform };
        if (itemType == MapItemType.shop)
        {
            text = "" + gunType;
            endless = true;
            hide = true;
            switch (gunType.Parse<GunType>())
            {
                case GunType.gravitygranate: //gravgun                    
                    score = 150;
                    bullets = 5;
                    RespawnTm = 60 * 3;
                    break;
                case GunType.minigun:
                    score = 220;
                    break;
                case GunType.railgun:
                    score = 150;
                    break;
                case GunType.bazoka:
                    score = 150;
                    break;
                case GunType.ak:
                    score = 100;
                    break;
                case GunType.granate:
                    score = 80;
                    break;
                case GunType.physxgun:
                    score = 80;
                    bullets = 50;
                    break;
                case GunType.uzi:
                    score = 70;
                    break;
                case GunType.shotgun:
                    score = 50;
                    break;
                case GunType.pistol:
                    score = 20;
                    break;
                default:
                    score = 100;
                    break;
            }
            //playerPrefab.guns[(int)gunType.Parse<GunType>()].score = Score;            
        }
        
        if (itemType == MapItemType.timewarp)
        {
            text = "Time Warp, Press T to use";
            endless = true;
            hide = true;
            score = 70;
        }
        if (itemType == MapItemType.spotlight)
        {
            endless = true; score = 20; text = "Take SpotLight";
        }
        if (itemType == MapItemType.laser)
        {
            endless = true; score = 50; text = "Take Laser aim";
        }
        if (itemType == MapItemType.antigravitation)
        {
            endless = true;
            score = 50;
            text = "Take Zero Gravitation";
        }
        if (itemType == MapItemType.timewarp)
        {
            endless = true;
            score = 50;
            text = "Take TimeBomb";
        }
        if (itemType == MapItemType.life)
        {
            endless = true;
            score = 50;
            RespawnTm = 10000;
            bullets = 45;
            text = "Take Life";
        }
        if (itemType == MapItemType.energy)
        {
            endless = true;
            score = 200;
            bullets = 10;
            text = "Take Energy";
        }
        if (itemType == MapItemType.speedupgrate)
        {
            endless = true;
            score = 300;
            text = "Upgrate speed";
        }
        if (itemType == MapItemType.lifeupgrate)
        {
            endless = true;
            score = 350;
            text = "Upgrate life";
        }

        if (itemType == MapItemType.trap)
        {
            lookat = true;
            bullets = 1000;
            endless = true;
            score = 20;
            hide = false;
            text = "Trap";
        }
        if (itemType == MapItemType.lift)
        {
            hide = false;
            text = "elevator";
            payonce = true;
            Distance = 0;
        }
        if (itemType == MapItemType.teleport)
        {
            checkOutSound = Base2.FindAsset<AudioClip>("teleport");
            endless = true;
            text = "Teleport";            
            score = 40;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "Door";
        }
        if (itemType == MapItemType.speed)
        {
            checkOutSound = Base2.FindAsset<AudioClip>("speed");
            Distance = 0;
            autoTake = false;
            endless = true;
            text = "Speed up";
        }

        if (itemType == MapItemType.money)
        {
            hide = true;
            RespawnTm = 30000;
            if (Score == 0) score = -20;
            text = "Take money";
        }

        if (itemType == MapItemType.jumper)
        {
            text = "Jumper";
            endless = true;
            Distance = 0;
        }

        base.InitValues();
    }
#endif
    public override void Awake()
    {        
        _Game.mapitems.Add(this);        
        base.Awake();
    }
    public void Aim(Player p)
    {        
        if (lookat && Check() && p.isOwner)
        {
            p.mapItemTm = .5f;
            p.mapItem = this;
        }
    }
    public bool Check()
    {

        if (!_localPlayer.Alive) return false; //Debug.Log(1);
        if (itemType == MapItemType.life && _localPlayer.Life == _localPlayer.MaxLife)
            return false; //Debug.Log(2);
        if (itemType == MapItemType.laser && _localPlayer.gun.laser) return false; //Debug.Log(3);
        if (itemType == MapItemType.timewarp && _localPlayer.haveTimeBomb) return false; //Debug.Log(4);
        if (itemType == MapItemType.spotlight && _localPlayer.haveLight) return false; //Debug.Log(5);
        if (itemType == MapItemType.antigravitation && _localPlayer.haveAntiGravitation) return false; //Debug.Log(6);
        if (itemType == MapItemType.lifeupgrate && _localPlayer.LifeUpgrate >= 3) return false; //Debug.Log(7);
        if (itemType == MapItemType.speedupgrate && _localPlayer.SpeedUpgrate >= 5) return false; //Debug.Log(8);
        if (itemType == MapItemType.energy && _localPlayer.PowerUpgrate > 3) return false; //Debug.Log(9);
        if (!endless && itemsLeft <= 0) return false; //Debug.Log(10);
        if (_localPlayer.MapItemInterval >= 0) return false; //Debug.Log(11);
        if (animation != null && animation.clip.name == "Take 001" && animation.isPlaying) return false; //Debug.Log(12);
        return true;
    }
    void Update()
    {

    }
    bool isbought { get { return Score == 0; } }
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        if (isCheckOutCalled)
            RPCCheckOut(itemsLeft);

        base.OnPlayerConnectedBase(np);
    }
    
    public void LocalCheckOut()
    {
        
        RPCCheckOut(itemsLeft - 1);
        if (itemType == MapItemType.speed && isbought)
            _localPlayer.rigidbody.AddTorque(new Vector3(Speed.y, 0, Speed.x));

        if (itemType == MapItemType.teleport)
        {
            _localPlayer.transform.position = teleport.position;
        }
        if (itemType == MapItemType.jumper && isbought)
        {
            _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass * fdt);
            GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
            Destroy(g, 1.6f);
        }

        _localPlayer.MapItemInterval = 1;
        _localPlayer.Score = Math.Max(_localPlayer.Score - Score, 0);

        if (itemType == MapItemType.shop)
        {
            _localPlayer.guns[this.guni].patronsLeft += this.bullets;
            _localPlayer.RPCSelectGun(this.guni);
        }
        if (itemType == MapItemType.energy)
        {
            _localPlayer.PowerUpgrate += 1;
        }
        if (itemType == MapItemType.life)
        {
            _localPlayer.RPCSetLifeLocal(Math.Min(_localPlayer.Life + this.bullets, _localPlayer.MaxLife), -1);
        }
        if (itemType == MapItemType.laser)
        {
            _localPlayer.gun.RPCSetLaser(true);
        }
        if (itemType == MapItemType.spotlight)
            _localPlayer.haveLight = true;
        if (itemType == MapItemType.lifeupgrate)
            _localPlayer.RPCSetLifeUpgrate(_localPlayer.LifeUpgrate + 1);
        if (itemType == MapItemType.speedupgrate)
            _localPlayer.RPCSetSpeedUpgrate(_localPlayer.SpeedUpgrate + 1);
        if (itemType == MapItemType.timewarp)
            _localPlayer.haveTimeBomb = true;
        if (itemType == MapItemType.antigravitation)
            _localPlayer.haveAntiGravitation = true;

        if (animation != null && animation.clip != null)
            RPCPlay();
    }
    public void RPCPlay() { CallRPC("Play"); }
    [RPC]
    public void Play()
    {       
        foreach(Animation a in this.GetComponentsInChildren<Animation>())
            a.Play();
    }
    public void RPCCheckOut(int i) { CallRPC("CheckOut", i); }
    [RPC]
    public void CheckOut(int i)
    {        
        itemsLeft = i;
        if (checkOutSound != null)
            audio.PlayOneShot(checkOutSound, 10);

        Debug.Log("check out " + name + " " + i);
        isCheckOutCalled = true;
        if (payonce) { score = 0; endless = true; payonce = false; }        

        if (hide && itemsLeft <= 0)
        {
            this.Show(false);
            if (RespawnTm > 0) _TimerA.AddMethod((int)(RespawnTm * 1000), delegate { this.Show(true); itemsLeft = 1; });
        }

    }
    MapItemType itemType { get { return itemtype.Parse<MapItemType>(); } }
    internal int guni { get { return (int)gunType.Parse<GunType>(); } }
    public float score;
    public float Score
    {
        get
        {
            //_Game.scorefactor.Evaluate(_Game.stage) 
            return (int)(score * .1f * ((itemType == MapItemType.shop && _localPlayer.guns[guni].patronsLeft == -1) ? 3 : 1));
        }
    }
}
