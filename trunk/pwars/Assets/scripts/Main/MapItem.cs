using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
public enum MapItemType { none, door, lift, jumper, shop, money, speed, laser, health, trap, spotlight }
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
    public GunType gunIndex;
    public string gunIndexStr;
    public int bullets = 1000;
    public string text = "";
    public float TmOn;
    public int respawnTm;
    public float tmJumper;
    public string[] param { get { return name.Split(','); } }
    public float tmCheckOut;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    [LoadPath("wave")]
    public GameObject wavePrefab;
    public override void Init()
    {
        //gunIndex = (GunType)Enum.Parse(typeof(GunType), gunIndexStr);
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.gameObject.isStatic = false;
            t.gameObject.layer = LayerMask.NameToLayer("MapItem");
        }
        gunIndexStr = gunIndex.ToString();        
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
            a.animation.wrapMode = WrapMode.Once;            
            a.animatePhysics = true;
        }
        if (itemType == MapItemType.trap)
        {
            bullets = 20;
            endless = true;
            score = 2;

        }
        if (itemType == MapItemType.spotlight)
        {
            endless = true; score = 1; text = "Чтобы взять фанарик нажми F";
        }
        if (itemType == MapItemType.laser)
        {
            endless = true; score = 5; text = "Чтобы купить лазерный прицел нажми F";
        }

        if (itemType == MapItemType.trap)
        {
            text = "Чтобы использовать лавушку нажми F";
        }
        if (itemType == MapItemType.lift)
        {
            text = "Чтобы использовать лифт нажми F";
            payonce = true;
            distance = 0;
        }

        if (itemType == MapItemType.shop)
        {            
            text = "Нажми F чтобы купить " + (GunType)gunIndex;
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
            text = "Ускоряет шар";
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
            text = "Нажми F чтобы взять деньги";
        }
        if (itemType == MapItemType.laser)
            text = "Нажми F чтобы купить для оружия лазерный прицел";
        if (itemType == MapItemType.jumper)
        {
            text = "Выбери гравипушку и стреляй по етому предмету";
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
            text = "Чтобы купить енергию нажми F";

        
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

        UpdateLightmap(this.GetComponentsInChildren<Renderer>().SelectMany(a => a.materials));
        base.Awake();
    }
    protected override void Start()
    {                
        if(animation!=null)
            animation.Stop();
    }
    bool canEnable { get { return (animation == null || !animation.isPlaying) && _localPlayer.Alive; } }
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
            bool donthavegun = (itemType == MapItemType.shop && _localPlayer.guns[(int)gunIndex].patronsLeft == -1);
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
                _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass);
                GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
                Destroy(g, 1.6f);
            }
            
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        RPCCheckOut(itemsLeft);
        base.OnPlayerConnected1(np);
    }
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
            if (itemType == MapItemType.speed)
            {
                _localPlayer.rigidbody.AddTorque(Speed.y, 0, Speed.x);
            }
            tmCheckOut = 4;
            _localPlayer.score -= score;
            itemsLeft--;
            if (itemType == MapItemType.shop)
            {
                _localPlayer.guns[(int)this.gunIndex].patronsLeft += this.bullets;
                _localPlayer.RPCSelectGun((int)this.gunIndex);
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
            RPCCheckOut(itemsLeft);
        }
    }
    public void RPCCheckOut(int i) { CallRPC("CheckOut",i); }
    [RPC]
    void CheckOut(int i)
    {
        Debug.Log("checkOut+" + itemType);
        itemsLeft = i;
        if (animation != null && animation.clip != null)
            animation.Play();


        if (payonce) { score = 0; endless = true; payonce = false; }
        if (opendoor != null)
            audio.PlayOneShot(opendoor, 10);

        if (hide && itemsLeft == 0)
        {
            this.Show(false);
            if (respawnTm > 0) _TimerA.AddMethod(respawnTm, delegate { this.Show(true); itemsLeft = 1; });
        }

    }
    void Stop()
    {
        if (animation != null && animation.clip != null)
            animation["Take 001"].enabled = false;
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
