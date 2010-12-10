using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
public enum MapItemType { door, lift, jumper, shop, money, speed, laser, health }
//[RequireComponent(typeof(NetworkView), typeof(AudioListener))]
public class MapItem : Base
{
    public int score;
    //[LoadPath("Swish grainy")]
    public AudioClip opendoor;
    public bool payonce;
    public bool hide;
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

        if (itemType == MapItemType.lift)
        {
            text = "Чтобы использовать лифт нажми B";
            payonce = true;
        }

        if (itemType == MapItemType.shop)
        {
            text = "Нажми B чтобы купить " + (GunType)gunIndex;
            endless = true;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "чтобы открыть дверь нажми B";
            Parse(ref score, 1);
        }
        
        if (itemType == MapItemType.speed)
        {
            distance = 0;
            payonce = true;
            text = "Ускоряет шар";
            Debug.Log("init");
            try
            {
                Vector3 v = ParseRotation(param[1]) * float.Parse(param[2]);
                Speed.x = v.x;
                Speed.y = v.z;
                Debug.Log(Speed);

            }
            catch (Exception e) { Debug.Log(e); }            
        }

        if (itemType == MapItemType.money)
        {
            hide = true;
            text = "Нажми B чтобы взять деньги";
        }
        if (itemType == MapItemType.laser)
            text = "Нажми B чтобы купить для оружия лазерный прицел";
        if (itemType == MapItemType.jumper)
        {
            text = "Выбери гравипушку и стреляй по етому предмету";
            payonce = true;
        }
        if (itemType == MapItemType.health)
            text = "Чтобы купить енергию нажми B";

        
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
            if (itemType == MapItemType.speed && tmCollEnter < 0)
            {
                _localPlayer.rigidbody.AddTorque(Speed.y, 0, Speed.x);
                tmCollEnter = 1;
            }

            if (Input.GetKeyDown(KeyCode.B) && _localPlayer.score >= score)
            {
                CheckOut();
            }
            if (endless || itemsLeft > 0)
                _GameWindow.CenterText.text = text + (score > 0 ? (", нужно заплатить " + score + " очков") : "");
            JumperUpdate();
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
            if (Input.GetMouseButtonDown(0) && tmJumper < 0)
            {
                tmJumper = 1;
                _localPlayer.rigidbody.AddForce(this.Jumper * _localPlayer.rigidbody.mass);
            }
            PlaySound(superphys_launch3);
            GameObject g = (GameObject)Instantiate(wavePrefab, pos, rot * Quaternion.Euler(90, 0, 0));
            Destroy(g, 1.6f);
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        networkView.RPC("RPCCheckOut", np, itemsLeft);
        base.OnPlayerConnected1(np);
    }

    void OnCollisionStay(Collision c)
    {
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
                _localPlayer.guns[(int)this.gunIndex].patronsLeft += this.bullets;
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
                _localPlayer.guns[_localPlayer.guni].laser = true;
            RPCCheckOut(itemsLeft);
        }
    }
    [RPC]
    public void RPCCheckOut(int i)
    {
        CallRPC(i);
        itemsLeft = i;

        if (animation != null && animation.clip != null)
            animation.Play();


        if (payonce) { score = 0; endless = true; }
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
    public static Vector3 ParseRotation(string s)
    {
            switch (s)
            {
                case "x":
                    return (new Vector3(-1, 0, 0));
                case "-x":
                    return (new Vector3(1, 0, 0));
                case "-z":
                    return (new Vector3(0, -1, 0));
                case "z":
                    return (new Vector3(0, 1, 0));
                case "y":
                    return (new Vector3(0, 0, -1));
                case "-y":
                    return (new Vector3(0, 0, 1));

            };
            throw new System.Exception("cannot parse");
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
