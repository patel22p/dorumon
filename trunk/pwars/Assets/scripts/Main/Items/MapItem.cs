using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
public enum MapItemType { door, lift, jumper, shop, money, speed, laser, health, trap , spotlight }
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
    public MapItemType itemType = MapItemType.door;
    public GunType gunIndex;
    public int bullets = 1000;
    public string text = "";
    public float TmOn;
    public int respawnTm;
    public float tmJumper;
    public string[] param { get { return name.Split(','); } }
    public float tmCollEnter;
    [LoadPath("wave")]
    public GameObject wavePrefab;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    public override void Init()
    {
        foreach(Transform t in GetComponentsInChildren<Transform>())
            t.gameObject.isStatic = false;
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

        if (itemType == MapItemType.trap)
        {
            text = "Чтобы использовать лавушку нажми F";
        }
        if (itemType == MapItemType.lift)
        {
            text = "Чтобы использовать лифт нажми F";
            payonce = true;
        }

        if (itemType == MapItemType.shop)
        {            
            text = "Нажми F чтобы купить " + (GunType)gunIndex;            
            Parse(ref score, 1);
            Parse(ref autoTake, 2);
            endless = !autoTake;
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
    protected override void Awake()
    {
        
        if (itemType == MapItemType.shop)
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.material.shader = Shader.Find("Self-Illumin/Diffuse");
        base.Awake();
    }
    void Start()
    {
        if(animation!=null)
            animation.Stop();
    }
    void Update()
    {

        if (transform.Cast<Transform>().Union(new[] { transform }).Any(a => Vector3.Distance(a.position, _localPlayer.pos) < distance))
            TmOn = 1;        

        tmCollEnter -= Time.deltaTime;
        tmJumper -= Time.deltaTime;
        if (TmOn > 0)
            TmOn -= Time.deltaTime;

        

        if (TmOn > 0 && (animation == null || !animation.isPlaying))
        {
            JumperUpdate();
            bool donthavegun = (itemType == MapItemType.shop && _localPlayer.guns[(int)gunIndex].patronsLeft != -1);
            int Score = (donthavegun ? this.score * 3 : this.score);
            
            if ((Input.GetKeyDown(KeyCode.F) || autoTake) && (_localPlayer.score >= Score || debug))
            {
                if (itemType == MapItemType.speed && tmCollEnter < 0)
                {
                    _localPlayer.rigidbody.AddTorque(Speed.y, 0, Speed.x);
                    tmCollEnter = 1;
                }

                CheckOut();
            }
            if ((endless || itemsLeft > 0) && text != "")
                _GameWindow.CenterText.text = text + (Score > 0 ? (", нужно заплатить " + Score + " очков") : "");
            
        }
        if (TmOn < 0)
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
                PlaySound(superphys_launch3);
                GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot);
                Destroy(g, 1.6f);
            }
            
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        networkView.RPC("RPCCheckOut", np, itemsLeft);
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
            
        if (c.gameObject == _localPlayer.gameObject)
            TmOn = 1;
    }
    [RPC]
    public void CheckOut()
    {
        if (itemsLeft > 0 || endless)
        {
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
                _localPlayer.guns[_localPlayer.guni].RPCSetLaser(true);
            if (itemType == MapItemType.spotlight)
                _localPlayer.RPCSetFanarik(true);
            RPCCheckOut(itemsLeft);
        }
    }
    [RPC]
    public void RPCCheckOut(int i)
    {
        if(CallRPC(i)) return;
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
            itemType = (MapItemType)System.Enum.Parse(typeof(MapItemType), param[0].ToLower());
        }
        catch { }
    }
}
