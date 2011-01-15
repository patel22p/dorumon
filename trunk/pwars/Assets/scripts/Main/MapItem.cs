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
public class MapItem : Base, IAim
{
    public float score;
    public float Score
    {
        get
        {
            return score * scorefactor * ((itemType == MapItemType.shop && _localPlayer.guns[guni].patronsLeft == -1) ? 3 : 1);
        }
    }
    [FindAsset("checkout", overide = true)]
    public AudioClip opendoor;
    public bool payonce;
    public bool hide;
    public bool lookat;
    public bool autoTake;
    float scorefactor = .5f;
    public Vector3 Jumper = new Vector3(0, 1000, 0);
    public Vector2 Speed;
    public float distance = 10;
    public int itemsLeft = 1;
    public bool endless;
    public Transform teleport;
    public string itemtype;
    MapItemType itemType { get { return itemtype.Parse<MapItemType>(); } }
    public string gunType;
    public int guni { get { return (int)gunType.Parse<GunType>(); } }
    public bool isCheckOutCalled;
    public int bullets = 1000;
    public string text = "";
    public float TmOn;
    public int respawnTm;
    public float tmCheckOut;
    [FindAsset("Player")]
    public GameObject playerPrefab;
    [FindAsset("superphys_launch3")]
    public AudioClip superphys_launch3;
    [FindAsset("wave")]
    public GameObject wavePrefab;
    public override void Init()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.gameObject.isStatic = false;
            t.gameObject.layer = LayerMask.NameToLayer("MapItem");
        }
        var g = gameObject;
        if (!inited)
        {
            g.AddComponent<NetworkView>();
            g.AddComponent<AudioSource>();
            g.AddComponent<Rigidbody>();
            g.networkView.observed = g.animation;
            g.rigidbody.isKinematic = true;
            inited = true;
        }

        foreach (var a in g.GetComponentsInChildren<Animation>())
        {
            a.animatePhysics = true;
            a.playAutomatically = true;
            //a.clip.wrapMode = WrapMode.Loop;
            //foreach (AnimationState b in a)
            //    b.wrapMode = WrapMode.Loop;            
        }
        if (itemType == MapItemType.trap)
        {
            lookat = true;
            bullets = 1000;
            endless = true;
            score = 20;
        }
        if (itemType == MapItemType.shop)
        {
            var gun = playerPrefab.GetComponent<Player>().guns[guni];
            var cur = transform.Find("cursor");
            if (cur != null && cur.childCount == 0)
            {
                var g2 = (GameObject)Instantiate(gun.gunModel, cur.transform.position, cur.transform.rotation);
                g2.transform.parent = cur;
            }
            if (gun is Gun)
            {
                bullets = (int)(20 / ((Gun)gun).interval);
            }
            text = "Press F to buy " + gunType;
            endless = !autoTake;
            hide = autoTake;
            if (autoTake)
                distance = 1;
        }

        if (itemType == MapItemType.shop)
        {
            distance = 2;
            switch (gunType.Parse<GunType>())
            {
                case GunType.ak:
                    score = 100;
                    break;
                case GunType.bazoka:
                    score = 120;
                    break;
                case GunType.granate:
                    score = 80;
                    break;
                case GunType.gravitygranate:
                    score = 200;
                    break;
                case GunType.minigun:
                    score = 160;
                    break;
                case GunType.physxgun:
                    score = 80;
                    bullets = 10;
                    break;
                case GunType.pistol:
                    score = 10;
                    break;
                case GunType.railgun:
                    score = 110;
                    break;
                case GunType.shotgun:
                    score = 60;
                    break;
                case GunType.staticField:
                    score = 100;
                    break;
                case GunType.uzi:
                    score = 80;
                    break;
                default:
                    score = 100;
                    break;
            }

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
            score = 200;
            text = "Take Antigravitation";
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
            bullets = 45;
            text = "Take Life";
        }
        if (itemType == MapItemType.energy)
        {
            endless = true;
            score = 50;
            bullets = 65;
            text = "Take Energy";
        }
        if (itemType == MapItemType.speedupgrate)
        {
            endless = true;
            score = 400;
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
            text = "Trap";
        }
        if (itemType == MapItemType.lift)
        {
            text = "elevator";
            payonce = true;
            distance = 0;
        }

        
        if (itemType == MapItemType.teleport)
        {
            opendoor = Base2.FindAsset<AudioClip>("teleport");
            endless = true;
            text = "Teleport";
            foreach (var a in this.GetComponentsInChildren<Collider>())
                a.isTrigger = true;
            score = 40;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "Door";
        }
        if (itemType == MapItemType.speed)
        {
            opendoor = Base2.FindAsset<AudioClip>("speed");
            distance = 0;
            autoTake = false;
            endless = true;
            text = "Speed up";
        }

        if (itemType == MapItemType.money)
        {
            hide = true;
            respawnTm = 30000;
            distance = 1;
            if (Score == 0) score = -20;
            text = "Take money";
        }

        if (itemType == MapItemType.jumper)
        {
            text = "Jumper";
            endless = true;
            distance = 0;
        }
        
            


        foreach (var a in g.GetComponentsInChildren<Renderer>().Distinct())
        {
            var go = a.gameObject;
            if (a.collider != null)
                DestroyImmediate(a.collider);
            if (itemType == MapItemType.door || itemType == MapItemType.teleport || itemType == MapItemType.speed || itemType == MapItemType.lift)
                go.AddComponent<BoxCollider>();
        }

        base.Init();
    }
    protected override void Awake()
    {
        foreach (var a in gameObject.GetComponentsInChildren<Animation>())
            a.wrapMode = WrapMode.Once;

        if (itemType == MapItemType.shop)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.material.shader = Shader.Find("Self-Illumin/Diffuse");
        }

        base.Awake();
    }
    protected override void Start()
    {
        if (animation != null && animation.clip != null && animation.clip.name == "Take 001" && Network.isServer)
            animation.Stop();

    }



    public void Aim()
    {
        if (lookat && Check())
        {
            TmOn = .5f;
        }
    }
    bool Check()
    {
        if (!_localPlayer.Alive) return false;
        if (itemType == MapItemType.life && _localPlayer.Life == _localPlayer.maxLife)
            return false;
        if (itemType == MapItemType.laser && _localPlayer.gun.laser) return false;
        if (itemType == MapItemType.timewarp && _localPlayer.haveTimeBomb) return false;
        if (itemType == MapItemType.spotlight && _localPlayer.haveLight) return false;
        if (itemType == MapItemType.antigravitation && _localPlayer.haveAntiGravitation) return false;
        if (itemType == MapItemType.lifeupgrate && _localPlayer.lifeUpgrate >= 6) return false;
        if (itemType == MapItemType.speedupgrate && _localPlayer.lifeUpgrate >= 3) return false;
        if (!endless && itemsLeft <= 0) return false;
        if (tmCheckOut >= 0) return false;
        if (animation != null && animation.clip.name == "Take 001" && animation.isPlaying) return false;
        return true;
    }
    void Update()
    {
        tmCheckOut -= Time.deltaTime;
        if (TmOn > 0)
            TmOn -= Time.deltaTime;

        foreach (var t in transform.GetTransforms())
            if (Vector3.Distance(t.position, _localPlayer.pos) < distance && Check())
            {
                TmOn = .5f;
                teleport = t;
            }

        if (TmOn > 0 && Check())
        {
            if ((Input.GetKeyDown(KeyCode.F) || autoTake) && (_localPlayer.score >= Score || debug))
            {                
                TmOn = -1;
                LocalCheckOut();
            }

            if (text != "" && _GameWindow.CenterText.text == "")
                _GameWindow.CenterText.text = text + (Score > 0 ? (", costs " + Score + " Money") : "");
        }
        else if (TmOn < 0)
        {
            _GameWindow.CenterText.text = "";
            TmOn = 0;
        }
    }
    bool isbought { get { return Score == 0; } }

    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        if (isCheckOutCalled)
        {
            RPCCheckOut(itemsLeft);

            if (animation != null && animation.clip.name == "Take 001")
            {
                animation.Play();
            }
        }

        //if (animation != null && animation["Take 001"] != null)
        //    RPCSetTime(animation["Take 001"].time);
        base.OnPlayerConnected1(np);
    }
    //public void RPCSetTime(float time) { CallRPC("SetTime", time); }
    //[RPC]
    //private void SetTime(float time)
    //{
    //    if (time != 0) Debug.Log(name + " animation" + time);
    //    animation["Take 001"].time = time;
    //}
    void OnCollisionStay(Collision c)
    {
        if (itemType == MapItemType.trap)
        {
            var ipl = c.gameObject.GetComponent<Destroible>();
            if (_TimerA.TimeElapsed(100) && ipl != null && ipl.isController)
                ipl.RPCSetLife(ipl.Life - bullets, -1);
        }

        if (c.gameObject == _localPlayer.gameObject && Check())
            TmOn = .5f;
    }
    public void LocalCheckOut()
    {
        {
            RPCCheckOut(itemsLeft - 1);
            if (itemType == MapItemType.speed && isbought)
                _localPlayer.rigidbody.AddTorque(new Vector3(Speed.y, 0, Speed.x));

            if (itemType == MapItemType.teleport)
            {

                foreach (var a in transform.Cast<Transform>().OrderBy(a => Random.Range(-1, 1)))
                {
                    if (a != teleport)
                    {
                        _localPlayer.transform.position = a.position;
                    }
                }
            }
            if (itemType == MapItemType.jumper && isbought)
            {
                _localPlayer.gun.patronsLeft -= 10;
                _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass * fdt);
                GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
                PlaySound(superphys_launch3);
                Destroy(g, 1.6f);
            }

            tmCheckOut = 4;
            _localPlayer.score -= Score;

            if (itemType == MapItemType.shop)
            {
                _localPlayer.guns[this.guni].patronsLeft += this.bullets;
                _localPlayer.RPCSelectGun(this.guni);
            }
            if (itemType == MapItemType.energy)
            {
                _localPlayer.nitro += bullets;
            }
            if (itemType == MapItemType.life)
            {
                _localPlayer.RPCSetLife(Math.Min(_localPlayer.Life + this.bullets, _localPlayer.maxLife), -1);
                Debug.Log(_localPlayer.maxLife);
                Debug.Log(_localPlayer.Life);
                
            }
            if (itemType == MapItemType.laser)
            {
                _localPlayer.gun.RPCSetLaser(true);
            }
            if (itemType == MapItemType.spotlight)
                _localPlayer.haveLight = true;
            if (itemType == MapItemType.lifeupgrate)
                _localPlayer.RPCSetLifeUpgrate(_localPlayer.lifeUpgrate + 1);
            if (itemType == MapItemType.speedupgrate)
                _localPlayer.RPCSetSpeedUpgrate(_localPlayer.speedUpgrate + 1);
            if (itemType == MapItemType.timewarp)
                _localPlayer.haveTimeBomb = true;
            if (itemType == MapItemType.antigravitation)
                _localPlayer.haveAntiGravitation = true;

            if (animation != null && animation.clip != null)
                RPCPlay();
        }
    }

    public void RPCPlay() { CallRPC("Play"); }
    [RPC]
    public void Play()
    {
        animation.Play();
    }


    public void RPCCheckOut(int i) { CallRPC("CheckOut", i); }
    [RPC]
    public void CheckOut(int i)
    {
        itemsLeft = i;
        Debug.Log("check out " + name + " " + i);
        isCheckOutCalled = true;
        if (payonce) { score = 0; endless = true; payonce = false; }
        if (opendoor != null)
            audio.PlayOneShot(opendoor, 10);

        if (hide && itemsLeft <= 0)
        {
            this.Show(false);
            if (respawnTm > 0) _TimerA.AddMethod(respawnTm, delegate { this.Show(true); itemsLeft = 1; });
        }

    }
}
