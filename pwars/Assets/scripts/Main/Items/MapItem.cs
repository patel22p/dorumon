using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public enum MapItemType { door, lift, jumper, shop, money, speed,laser,health }
public class MapItem : Base
{
    bool canCheckOut { get { return itemType == MapItemType.shop || itemType == MapItemType.door || itemType == MapItemType.lift; } }
    public int score;
    [LoadPath("dooropen")]
    public AudioClip opendoor;
    public float JumperMagnet;
    public float JumperRelease;    
    public float JumperDistance;
    public Vector2 JumperMultiplier = new Vector2(1, 1);
    public Vector3 Speed;
    public int itemsLeft;
    public bool endless = true;
    public MapItemType itemType = MapItemType.door;
    public int gunIndex = 0;
    public int bullets = 1000;    
    public string text = "";
    public float onTm;
    public override void Init()
    {
        gameObject.AddComponent<AudioSource>();
        if (animation != null)
        {
            networkView.observed = animation;
            animation.playAutomatically = false;
        }
        if (itemType == MapItemType.lift)
        {
            animation.wrapMode = WrapMode.PingPong;
            text = "Чтобы использовать лифт нажми B";
        }
        if(itemType!=MapItemType.door)
            this.gameObject.isStatic = false;

            
        if (itemType == MapItemType.shop)
            text = "Нажми B чтобы купить " + (GunType)gunIndex + ", нужно " + score + " очков";
        if (itemType == MapItemType.door)
        {
            endless = false;
            text = "чтобы открыть дверь вам надо " + score + " очков, (Нажми B)";
        }
        
            
        if (itemType == MapItemType.lift)
            text = "Чтобы включить лифт нажми B";
        if (itemType == MapItemType.money)
            text = "Нажми B чтобы взять";

        if (itemType == MapItemType.laser)
            text = "Нажми B чтобы купить для оружия лазерный прицел, цена:" + score;
        if (itemType == MapItemType.jumper)
            text = "Выбери гравипушку и стреляй по етому предмету";
        if (itemType == MapItemType.health)
            text = "Чтобы купить енергию нужно " + score + " очков";


        string[] s = name.Split(',');
        try
        {
            score = int.Parse(s[1]);
        }
        catch { }

        base.Init();
    }
    void OnMouseOver()
    {
        _GameWindow.CenterText.text = text;
        onTm = 1;
    }
    [LoadPath("wave")]
    public GameObject wavePrefab;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    void Update()
    {
        if (onTm > 0)
        {
            tmJumperMagnet -= Time.deltaTime;
            tmJumperRelease -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.B) && _localPlayer.score > score)
                CheckOut();
            if (itemType == MapItemType.jumper && _localPlayer.selectedgun == (int)GunType.physxgun)
            {
                bool down = Input.GetMouseButtonDown(0) && this.JumperMagnet > 0 && tmJumperMagnet < 0;
                bool up = Input.GetMouseButtonUp(0) && this.JumperRelease > 0 && tmJumperRelease < 0;
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
        if (onTm < 0)
        {
            _GameWindow.CenterText.text = "";
            onTm = 0;
        }
    }
    public float tmJumperMagnet;
    public float tmJumperRelease;
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        if (canCheckOut)
            networkView.RPC("CheckOut", np, itemsLeft);
        base.OnPlayerConnected1(np);
    }
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.name == "LocalPlayer")
        {
            if (itemType == MapItemType.speed)
            {
                _localPlayer.rigidbody.AddTorque(Speed);
            }
        }

    }
    public void CheckOut()
    {
        if (itemType > 0 || endless)
        {
            itemsLeft--;            
            if (itemType == MapItemType.shop)
                _localPlayer.guns[this.gunIndex].patronsleft += this.bullets;
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
        CallRPC();
        itemsLeft = i;
        if (animation != null)
            animation.Play();
        if (opendoor != null)
            PlaySound(opendoor, 10);
        if (itemType == MapItemType.money && itemsLeft == 0)
            this.Show(false);
    }

    
}
