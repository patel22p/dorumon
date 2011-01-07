using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
public enum MapItemType { none, door, lift, jumper, shop, money, speed, laser, health, trap, spotlight, clock }
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class MapItem : Base
{
    public int score;
    //[LoadPath("Swish grainy")]
    public AudioClip opendoor;
    public bool payonce;
    public bool hide;
    public bool autoTake;
    public Vector3 Jumper = new Vector3(0, 1000, 0);
    public Vector2 Speed;
    public float distance=10;
    public int itemsLeft = 1;
    public bool endless;
    public MapItemType itemType;
    public string gunType;
    public int guni { get { return (int)gunType.Parse<GunType>(); } }    
    public bool isCheckOutCalled;
    public int bullets = 1000;
    public string text = "";
    public float TmOn;
    public int respawnTm;
    public float tmJumper;
    public string[] param { get { return name.Split(','); } }
    public float tmCheckOut;
    [LoadPath("Player")]
    public GameObject playerPrefab;    
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    [LoadPath("wave")]
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
            a.wrapMode = WrapMode.Once;
        }
        if (itemType == MapItemType.trap)
        {
            bullets = 20;
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

            bullets = (int)(20 / ((Gun)gun).interval);
            text = "Press F to buy" + gunType;
            Parse(ref score, 2);
            Parse(ref autoTake, 3);
            endless = !autoTake;
            hide = autoTake;
            if (autoTake)
                distance = 1;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "чтобы открыть дверь нажми F";
            Parse(ref score, 1);
        }
        
        if (itemType == MapItemType.speed)
        {
            distance = 0;            
            autoTake = true;
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
            payonce = true;
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

        
        foreach(var a in g.GetComponentsInChildren<Collider>().Distinct())
        {
            var go = a.gameObject;
            DestroyImmediate(a);
            go.AddComponent<BoxCollider>();
        }

        base.Init();
    }
    public Transform[] trs;
    protected override void Awake()
    {
        trs = GetComponentsInChildren<Transform>().Union(GetComponents<Transform>()).ToArray();
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
    
    void Update()
    {

        foreach (var a in trs)
        {
            if (Vector3.Distance(a.position, _localPlayer.pos) < distance && canEnable)
                TmOn = 1;
        }
        tmCheckOut -= Time.deltaTime;
        tmJumper -= Time.deltaTime;
        if (TmOn > 0)
            TmOn -= Time.deltaTime;
        if (TmOn > 0)
        {

            JumperUpdate();
            bool donthavegun = (itemType == MapItemType.shop && _localPlayer.guns[guni].patronsLeft == -1);
            int Score = (donthavegun ? this.score * 3 : this.score);

            if ((Input.GetKeyDown(KeyCode.F) || autoTake) && (_localPlayer.score >= Score || debug))
            {
                LocalCheckOut();
            }

            if ((endless || itemsLeft > 0) && text != "")
            {
                _GameWindow.CenterText.text = text + (Score > 0 ? (", нужно заплатить " + Score + " очков") : "");

            }
        }
        else if (TmOn < 0)
        {
            _GameWindow.CenterText.text = "";
            TmOn = 0;
        }
    }
    private void JumperUpdate()
    {
        tmJumper -= Time.deltaTime;
        if (itemType == MapItemType.jumper && _localPlayer.selectedgun == (int)GunType.physxgun && score == 0)
        {
            if (Input.GetMouseButtonUp(0) && tmJumper < 0 && (_localPlayer.gun.patronsLeft > 10 || debug))
            {
                tmJumper = 1;
                _localPlayer.gun.patronsLeft -= 10;
                _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass/Time.timeScale);
                GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
                Destroy(g, 1.6f);
            }
            
        }
    }
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
            if (itemType == MapItemType.speed)
            {
                _localPlayer.rigidbody.AddTorque(new Vector3(Speed.y, 0, Speed.x)/Time.timeScale);
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
