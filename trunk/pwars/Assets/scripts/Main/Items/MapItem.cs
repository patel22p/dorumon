using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
public enum MapItemType { door, lift, jumper, shop, money, speed, laser, health }
public class MapItem : Base
{
    bool canCheckOut { get { return itemType == MapItemType.shop || itemType == MapItemType.door || itemType == MapItemType.lift; } }
    public int score;
    [LoadPath("dooropen")]
    public AudioClip opendoor;
    public bool payonce;
    public bool hide;
    public float JumperMagnet;
    public float JumperRelease;
    public float JumperDistance = 2;
    public float distance = 50;
    public Vector2 JumperMultiplier = new Vector2(1, 1);
    public Vector2 Speed;
    public int itemsLeft = 1;
    public bool endless;
    public MapItemType itemType = MapItemType.door;
    public int gunIndex = 0;
    public int bullets = 1000;
    public string text = "";
    public float onTm;
    public float tmJumperMagnet;
    public float tmJumperRelease;
    string[] param { get { return name.Split(','); } }
    float tmCollEnter;

    public override void Init()
    {
        if (animation != null)
        {
            gameObject.isStatic = false;
            networkView.observed = animation;
            animation.playAutomatically = false;
            gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            animation.animatePhysics = true;            
        }
        if (canCheckOut)
            Parse(ref score, 1);
        if (itemType == MapItemType.lift)
        {
            animation.wrapMode = WrapMode.Once;
            text = "Чтобы использовать лифт нажми B";            
        }

        if (itemType == MapItemType.shop)
        {
            text = "Нажми B чтобы купить " + (GunType)gunIndex + ", нужно " + score + " очков";
            endless = true;
        }
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "чтобы открыть дверь вам надо " + score + " очков, (Нажми B)";
        }

        if (itemType == MapItemType.speed)
        {
            payonce = true;            
            text = "Ускоряет шар";
            Parse(ref Speed.x, 1);
            Parse(ref Speed.y, 2);
        }

        if (itemType == MapItemType.money)
        {
            hide = true;
            text = "Нажми B чтобы взять";
        }
        if (itemType == MapItemType.laser)
            text = "Нажми B чтобы купить для оружия лазерный прицел, цена:" + score;
        if (itemType == MapItemType.jumper)
        {
            text = "Выбери гравипушку и стреляй по етому предмету";
            payonce = true;
        }
        if (itemType == MapItemType.health)
            text = "Чтобы купить енергию нужно " + score + " очков";

        base.Init();
    }
    private void Parse(ref float t, int id)
    {
        try
        {
            t = float.Parse(param[id]);
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
    void OnMouseOver1()
    {
        onTm = 1;
    }
    [LoadPath("wave")]
    public GameObject wavePrefab;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    void Update()
    {
        tmCollEnter -= Time.deltaTime;
        tmJumperMagnet -= Time.deltaTime;
        tmJumperRelease -= Time.deltaTime;
        if (onTm > 0)
            onTm -= Time.deltaTime;
        if (onTm > 0 && (Vector3.Distance(_localPlayer.pos, pos) < distance))
        {
            if (Input.GetKeyDown(KeyCode.B) && _localPlayer.score >= score)
            {
                CheckOut();
            }
            _GameWindow.CenterText.text = text;
            JumperUpdate();
        }
        if (onTm < 0)
        {
            Debug.Log("off");
            _GameWindow.CenterText.text = "";
            onTm = 0;
        }
    }

    private void JumperUpdate()
    {
        tmJumperMagnet -= Time.deltaTime;
        tmJumperRelease -= Time.deltaTime;
        if (itemType == MapItemType.jumper && _localPlayer.selectedgun == (int)GunType.physxgun)
        {
            bool down = Input.GetMouseButtonDown(0) && this.JumperMagnet > 0 && tmJumperMagnet < 0;
            bool up = Input.GetMouseButtonUp(0) && this.JumperRelease > 0 && tmJumperRelease < 0 && Vector3.Distance(pos, _localPlayer.pos) < JumperDistance;
            if (up || down)
            {
                Vector3 v = transform.rotation * Vector3.forward * (down ? this.JumperMagnet : this.JumperRelease);
                v.x *= this.JumperMultiplier.x;
                v.z *= this.JumperMultiplier.x;
                v.y *= this.JumperMultiplier.y;
                PlaySound(superphys_launch3);
                if (up)
                    _localPlayer.rigidbody.AddForce(v * this.JumperRelease * _localPlayer.rigidbody.mass);
                if (down)
                    _localPlayer.rigidbody.AddForce(v * this.JumperMagnet * _localPlayer.rigidbody.mass);
                GameObject g = (GameObject)Instantiate(wavePrefab, _localPlayer.pos, _localPlayer.rot);
                Destroy(g, 1.6f);
            }
        }
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        if (canCheckOut)
            networkView.RPC("CheckOut", np, itemsLeft);
        base.OnPlayerConnected1(np);
    }
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.name == "LocalPlayer" )
        {
            if (itemType == MapItemType.speed && tmCollEnter < 0)
            {
                _localPlayer.rigidbody.AddTorque(Speed.y, 0, Speed.x);
                tmCollEnter = 1;
            }
        }

    }
    public void CheckOut()
    {
        if (itemsLeft > 0 || endless)
        {
            itemsLeft--;            
            if (itemType == MapItemType.shop)
                _localPlayer.guns[this.gunIndex].patronsLeft += this.bullets;
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
            if (itemType == MapItemType.money)
                _localPlayer.score += 10;
            RPCCheckOut(itemsLeft);
        }
    }
    [RPC]
    public void RPCCheckOut(int i)
    {
        CallRPC(i);
        itemsLeft = i;
        if (animation != null)
        {
            animation.wrapMode = animation.wrapMode == WrapMode.PingPong ? WrapMode.Once : WrapMode.PingPong;
            animation.Play();
        }
        if (opendoor != null)
            PlaySound(opendoor, 10);
        if (itemType == MapItemType.money && itemsLeft == 0)
            this.Show(false);
    }

    
}
