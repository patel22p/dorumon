using UnityEngine;
using System.Collections;
using System.Linq;
using System.Threading;
public class Gun : GunBase
{
    internal float barrelVell;
    int cursorid; Vector3 defPos, defPos2;
    internal float tm;
    public float interval = 1;
    public int howmuch = 1;
    public GameObject patronPrefab;
    public Vector3 random;        
    public Transform barrel;
    public float expOttalkivanie = 1;
    public float vibration=0;
    public AudioClip sound;
    public int damage = 60;
    public int probivaemost = 0;
    public float otbrasivanie;
    public float BulletForce =1;
    public float radius = 1;
    public float soundVolume = 1;
    public float gravitate = 0;
    public float timeToDestroy = 5;
    public float plDamageFactor = 1;
    public float zmDamageFactor = 1;
    [FindAsset("noammo")]
    public AudioClip noammoSound;
    public Light fireLight;
    public override void Init()
    {
        base.Init();
        enabled = false;
        fireLight = root.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Point);
    }
    public bool Its(params GunType[] gts)
    {
        return gts.Contains((GunType)guntype);
    }
    public override void InitValues()
    {
        GunType gt = (GunType)guntype;
        if (Its(GunType.pistol))
        {
            soundVolume = .3f;
        }
        if (Its(GunType.bazoka, GunType.granate, GunType.gravitygranate))
        {
            BulletForce = .15f;
            damage = 1;
            expOttalkivanie = 7;
            radius = 6;
        }
        if (Its(GunType.ak))
        {
            expOttalkivanie = .5f;
        }
        if (Its(GunType.gravitygranate, GunType.granate))
        {
            timeToDestroy = 2;
            BulletForce = .08f;
            radius = 6;
            damage = 2;
            interval = 1;
        }

        if (Its(GunType.gravitygranate)) //garvitationgranate
        {
            timeToDestroy = 5;
            gravitate = 1;
            expOttalkivanie = 20;
        }
        if (Its(GunType.minigun))
        {
            damage = 30;
        }
        if (Its(GunType.railgun))
        {
            expOttalkivanie = 3;
        }

        if (Its(GunType.shotgun))
        {
            probivaemost = 1;
            expOttalkivanie = .3f;
            damage = 6;
        }
        zmDamageFactor = .8f;
    }
    public override void Awake()
    {
        base.Awake();
    }
    public void Start()
    {
        defPos2 = defPos = transform.localPosition;        
    }
    public override void onShow(bool enabled)
    {        
        base.onShow(enabled);
    }
    protected override void Update()
    {
        
        base.Update();
        RandomFactorTm = Mathf.Max(0, RandomFactorTm - Time.deltaTime);
        if (barrel != null)
        {            
            if(barrelVell>.1)
                barrel.rotation = Quaternion.Euler(barrel.rotation.eulerAngles + new Vector3(0, 0, barrelVell));
            barrelVell *= .98f;
        }
        if(_TimerA.TimeElapsed(5000))
            defPos2 = Vector3.Scale(Random.onUnitSphere, new Vector3(1, 1, 3)) / 30;
        defPos = (defPos * 200 + defPos2) / 201;
        transform.localPosition = defPos + transform.localPosition / 2;
        if (isOwner)
            LocalUpdate();

    }    
    protected virtual void LocalUpdate()
    {        
        if ((tm -= Time.deltaTime) < 0 && Input.GetMouseButton(0) && lockCursor)
        {
            tm = interval;
            if (patronsLeft > 0 || debug)
            {
                patronsLeft--;
                RPCShoot();
            }
            else
                PlaySound(noammoSound);                    
        }
    }
    public float RandomFactorTm = 0;
    public void RPCShoot() { CallRPC("Shoot"); }
    [RPC]
    public void Shoot()
    {
        if (sound != null)
            root.audio.PlayOneShot(sound, soundVolume);
        if (player != null)
            player.rigidbody.AddForce(rot * new Vector3(0, 0, -otbrasivanie) * fdt);

        var t = cursor[cursorid].transform;
        _Game.particles[(int)ParticleTypes.fire].Emit(t.position, rot);
        _Game.particles[(int)ParticleTypes.fire1].Emit(t.position, rot);
        _Game.particles[(int)ParticleTypes.patrons].Emit(t.position, rot);
        for (int i = 0; i < howmuch; i++)
        {
            Vector3 r;
            r.x = Random.Range(-random.x, random.x);
            r.y = Random.Range(-random.y, random.y);
            r.z = Random.Range(-random.z, random.z);
            cursorid++;
            if (cursorid >= cursor.Count) cursorid = 0;
            if (fireLight != null && !fireLight.enabled)
            {
                fireLight.enabled = true;
                _TimerA.AddMethod(20, delegate
                {
                    fireLight.enabled = false;
                });
            }
            if (barrel != null) barrelVell += 10;

            var p2 = cursor[cursorid].position;
            Quaternion r2 = rot * Quaternion.Euler(r) * Quaternion.Euler(Random.insideUnitSphere * RandomFactorTm * 2);
            Patron patron = ((GameObject)(Instantiate(patronPrefab, p2 + r2 * Vector3.back * 2, r2))).GetComponent<Patron>();
            patron.gun = this;
            patron.OwnerID = OwnerID;
        }
        RandomFactorTm = Mathf.Min(RandomFactorTm + .2f, 1);
        this.pos -= rot * new Vector3(0, 0, vibration);

    }
}