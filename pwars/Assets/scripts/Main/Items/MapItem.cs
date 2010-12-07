using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
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
    public float _distance = 5;
    public Vector2 Speed;
    public int itemsLeft = 1;
    public bool endless;
    public MapItemType itemType = MapItemType.door;
    public GunType gunIndex;
    public int bullets = 1000;
    public string text = "";
    public float onTm;
    public int respawnTm;
    public float tmJumper;
    string[] param { get { return name.Split(','); } }
    public float tmCollEnter;
    [LoadPath("wave")]
    public GameObject wavePrefab;
    [LoadPath("superphys_launch3")]
    public AudioClip superphys_launch3;
    public override void Init()
    {
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
            payonce = true;
            text = "Ускоряет шар";
            Parse(ref Speed.x, 1);
            Parse(ref Speed.y, 2);
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

        base.Init();
    }
    protected override void Awake()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.material.shader = Shader.Find("Self-Illumin/Diffuse");
        base.Awake();
    }
    void OnPlayerOver()
    {
        onTm = 1;
    }
    void Update()
    {
        if (Vector3.Distance(pos, _localPlayer.pos) < _distance)
            OnPlayerOver();

        tmCollEnter -= Time.deltaTime;
        tmJumper -= Time.deltaTime;
        if (onTm > 0)
            onTm -= Time.deltaTime;
        if (onTm > 0 && (Vector3.Distance(_localPlayer.pos, pos) < _distance))
        {
            if (Input.GetKeyDown(KeyCode.B) && _localPlayer.score >= score)
            {
                CheckOut();
            }
            if (endless || itemsLeft > 0)
                _GameWindow.CenterText.text = text + (score > 0 ? (", нужно заплатить " + score + " очков") : "");
            JumperUpdate();
        }
        if (onTm < 0)
        {

            _GameWindow.CenterText.text = "";
            onTm = 0;
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
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject == _localPlayer.gameObject)
        {
            if (itemType == MapItemType.speed && tmCollEnter < 0)
            {
                _localPlayer.rigidbody.AddTorque(Speed.y, 0, Speed.x);
                tmCollEnter = 1;
            }
        }

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
            animation["Take 001"].enabled = true;


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
    private void Parse(ref int t, int id)
    {
        try
        {
            t = int.Parse(param[id]);
        }
        catch (System.Exception) { }
    }

}
