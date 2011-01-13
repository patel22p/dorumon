using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;
public enum MapItemType { none, door, lift, jumper, shop, money, speed, laser, health, trap, spotlight, clock, teleport, speedupgrate, lifeupgrate, timebomb, antigravitation }
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class MapItem : Base,IAim
{
    public float score;
    [FindAsset("checkout", overide = true)]
    public AudioClip opendoor;
    public bool payonce;
    public bool hide;
    public bool lookat;
    public bool autoTake;
    public Vector3 Jumper = new Vector3(0, 1000, 0);
    public Vector2 Speed;
    public float distance=10;
    public int itemsLeft = 1;
    public bool endless;
    public Transform teleport;
    public MapItemType itemType;
    public string gunType;
    public int guni { get { return (int)gunType.Parse<GunType>(); } }    
    public bool isCheckOutCalled;
    public int bullets = 1000;
    public string text = "";
    public float TmOn;
    public int respawnTm;
    public string[] param { get { return name.Split(','); } }
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
        ParseItemType();
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
            score = 2;
        }
        if (itemType == MapItemType.clock)
        {
            text = "Time Warp, Press T to use";
            endless = true;
            hide = true;
            score = 5;
            respawnTm = 1000;
        }
        if (itemType == MapItemType.spotlight)
        {
            endless = true; score = 1; text = "Take SpotLight";
        }
        if (itemType == MapItemType.laser)
        {
            endless = true; score = 5; text = "Take Laser aim";
        }
        if (itemType == MapItemType.antigravitation)
        {
            endless = true;
            score = 100;
            text = "Take Antigravitation";
        }
        if (itemType == MapItemType.timebomb)
        {
            endless = true;
            score = 50;
            text = "Take TimeBomb";
        }
        if (itemType == MapItemType.speedupgrate)
        {
            itemsLeft = 3;
            hide = true;
            score = 200;
            text = "Upgrate speed";
        }
        if (itemType == MapItemType.lifeupgrate)
        {
            itemsLeft = 3;
            hide = true;
            score = 150;
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

        if (itemType == MapItemType.shop)
        {
            var gun = playerPrefab.GetComponent<Player>().guns[guni];
            var cur = transform.Find("cursor");
            if (cur != null)
            {
                var g2 = (GameObject)Instantiate(gun.gunModel, cur.transform.position, cur.transform.rotation);
                g2.transform.parent = cur;
            }
            if (bullets == 1000 && gun is Gun)
            {
                bullets = (int)(20 / ((Gun)gun).interval);
            }
            text = "Press F to buy" + gunType;
            Parse(ref score, 2);
            Parse(ref autoTake, 3);
            endless = !autoTake;
            hide = autoTake;
            if (autoTake)
                distance = 1;
        }
        if (itemType == MapItemType.teleport)
        {
            endless = true;
            text = "Teleport";
            foreach (var a in this.GetComponentsInChildren<Collider>())
                a.isTrigger = true;
            score = 10;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "Door";
            Parse(ref score, 1);
        }
        
        if (itemType == MapItemType.speed)
        {
            distance = 0;            
            autoTake = false;
            endless = true;
            text = "Speed up";
            try
            {
                Speed.x = float.Parse(param[1]);
                Speed.y = -float.Parse(param[2]);
            }
            catch (Exception e) { Debug.Log(e); }            
        }

        if (itemType == MapItemType.money)
        {
            hide = true;
            autoTake = true;
            distance = 1;
            if (score == 0) score = -20;            
            text = "Take money";
        }
        
        if (itemType == MapItemType.jumper)
        {
            text = "Jumper";
            endless = true;
            distance = 0;
            try
            {
                Jumper.x = -float.Parse(param[1]);
                Jumper.y = float.Parse(param[3]);
                Jumper.z = -float.Parse(param[2]);
            }
            catch { }
        }
        if (itemType == MapItemType.health)
            text = "To buy energy press F";

        
        foreach(var a in g.GetComponentsInChildren<Renderer>().Distinct())
        {
            var go = a.gameObject;
            if(a.collider!=null)
                DestroyImmediate(a.collider);
            go.AddComponent<BoxCollider>();
        }

        base.Init();
    }
    protected override void Awake()
    {
        foreach (var a in gameObject.GetComponentsInChildren<Animation>())
            a.wrapMode = WrapMode.Once;

        if (itemType == MapItemType.shop || itemType == MapItemType.laser || itemType == MapItemType.health || itemType == MapItemType.spotlight)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.material.shader = Shader.Find("Self-Illumin/Diffuse");
        }
        base.Awake();
    }
    protected override void Start()
    {
        if (animation != null && animation.clip.name == "Take 001" && Network.isServer)
            animation.Stop();

        UpdateLightmap(this.GetComponentsInChildren<Renderer>().SelectMany(a => a.materials));
    }
    bool canEnable { get { return (animation == null || animation.clip.name != "Take 001" || !animation.isPlaying) && _localPlayer.Alive; } }


    public void Aim()
    {
        if (lookat)
        {
            TmOn = 1;            
        }
    }
    void Update()
    {
        foreach (var t in transform.GetTransforms())
            if (Vector3.Distance(t.position, _localPlayer.pos) < distance && canEnable)
            {
                TmOn = 1;
                teleport = t;
            }        

        tmCheckOut -= Time.deltaTime;
        if (TmOn > 0)
            TmOn -= Time.deltaTime;
        if (TmOn > 0)
        {
            bool donthavegun = (itemType == MapItemType.shop && _localPlayer.guns[guni].patronsLeft == -1);
            float Score = (donthavegun ? this.score * 3 : this.score);

            if ((Input.GetKeyDown(KeyCode.F) || autoTake) && (_localPlayer.score >= Score || debug))
            {
                LocalCheckOut();
            }

            if ((endless || itemsLeft > 0) && text != "")
            {
                _GameWindow.CenterText.text = text + (Score > 0 ? (", costs " + Score + " Money") : "");

            }
        }
        else if (TmOn < 0)
        {
            _GameWindow.CenterText.text = "";
            TmOn = 0;
        }
    }
    bool isbought { get { return score == 0; } }
    
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
            
        if (c.gameObject == _localPlayer.gameObject && canEnable)
            TmOn = 1;
    }
    public void LocalCheckOut()
    {
        if ((itemsLeft > 0 || endless) && tmCheckOut < 0)
        {
            RPCCheckOut(itemsLeft - 1);
            if (itemType == MapItemType.speed && isbought)
                _localPlayer.rigidbody.AddTorque(new Vector3(Speed.y, 0, Speed.x) / Time.timeScale);

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
                _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass / Time.timeScale);
                GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
                PlaySound(superphys_launch3);
                Destroy(g, 1.6f);
            }

            tmCheckOut = 4;
            _localPlayer.score -= score;
            
            if (itemType == MapItemType.shop)
            {
                _localPlayer.guns[this.guni].patronsLeft += this.bullets;
                _localPlayer.RPCSelectGun(this.guni);
            }
            if (itemType == MapItemType.health)
            {
                _localPlayer.Life += this.bullets;
                if (_localPlayer.Life - 100 > 0)
                {
                    _localPlayer.nitro += _localPlayer.Life - 100;
                    _localPlayer.Life = 100;
                    _localPlayer.RPCSetLife(_localPlayer.Life, -1);
                }
            }
            if (itemType == MapItemType.laser)
            {
                _localPlayer.gun.RPCSetLaser(true);
            }
            if (itemType == MapItemType.spotlight)
                _localPlayer.RPCSetFanarik(true);
            if (itemType == MapItemType.lifeupgrate)
                _localPlayer.RPCSetLifeUpgrate(_localPlayer.lifeUpgrate + 1);
            if (itemType == MapItemType.speedupgrate)
                _localPlayer.RPCSetSpeedUpgrate(_localPlayer.speedUpgrate + 1);
            if (itemType == MapItemType.timebomb)
                _localPlayer.RPCSetHaveTimeWarp(true);
            if (itemType == MapItemType.antigravitation)
                _localPlayer.RPCSetHaveAntiGrav(true);

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


    public void RPCCheckOut(int i) { CallRPC("CheckOut",i); }
    [RPC]
    public void CheckOut(int i)
    {
        itemsLeft = i;
        Debug.Log("check out " + name + " " + i);
        isCheckOutCalled = true;
        if (payonce) { score = 0; endless = true; payonce = false; }
        if (opendoor != null)
            audio.PlayOneShot(opendoor, 10);

        if (hide && itemsLeft == 0)
        {
            this.Show(false);
            if (respawnTm > 0) _TimerA.AddMethod(respawnTm, delegate { this.Show(true); itemsLeft = 1; });
        }

    }
    
    private void Parse(ref float t, int id)
    {
        try
        {
            t = float.Parse(param[id]);
        }
        catch (System.Exception) { }
    }

    private void Parse(ref bool t, int id)
    {
        try
        {
            t = int.Parse(param[id]) == 1;
        }
        catch (System.Exception) { }
    }

    private void Parse(ref int t, int id)
    {
        try
        {
            t = int.Parse(param[id]);
        }
        catch (System.Exception) { }
    }
    private void ParseItemType()
    {
        try
        {
            itemType = (MapItemType)System.Enum.Parse(typeof(MapItemType), param[0].ToLower().Substring(1));
        }
        catch { }
    }
}
