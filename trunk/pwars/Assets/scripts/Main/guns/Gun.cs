using UnityEngine;
using System.Collections;
using System.Linq;
using System.Threading;
public class Gun : GunBase
{
    public float interval = 1;
    [HideInInspector]
    public float tm;
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
    public float soundVolume;
    public float timeToDestroy = 5;
    public float plDamageFactor = 1;
    public float zmDamageFactor = 1;
    Vector3 defPos,defPos2;
    [FindAsset("noammo")]
    public AudioClip noammoSound;
    int cursorid;
    public float barrelVell;
    
    public Light fireLight;
    public override void Init()
    {
        base.Init();
        enabled = false;
        fireLight = root.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Point);

    }
    
    public override void InitValues()
    {
        GunType gt = (GunType)guntype;
        if (new[] { GunType.bazoka , GunType.granate , GunType.gravitygranate }.Contains(gt))
        {
            BulletForce = .15f;
            expOttalkivanie = 10;
        }
        if (new[] { GunType.ak}.Contains(gt))
        {
            expOttalkivanie = .5f;
        }
        if (new[] { GunType.gravitygranate, GunType.granate }.Contains(gt))
        {
            timeToDestroy = 2;
            BulletForce = .08f;
            interval = 1;
            
        }
        if (new[] { GunType.gravitygranate }.Contains(gt)) //garvitationgranate
        {
            timeToDestroy = 5;
            expOttalkivanie = 30;
        }      

        if (new[] { GunType.shotgun }.Contains(gt))
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
            patron.OwnerID = OwnerID;
            patron.timeToDestroy = timeToDestroy;
            patron.damage = this.damage;
            patron.ExpForce = expOttalkivanie * 200;
            patron.Force = new Vector3(0, 0, 300 * BulletForce);
            patron.probivaemost = this.probivaemost;
            patron.plDamageFactor = plDamageFactor;
            patron.Radius = radius;
            patron.zmDamageFactor = zmDamageFactor;
        }
        RandomFactorTm = Mathf.Min(RandomFactorTm + .2f, 1);
        this.pos -= rot * new Vector3(0, 0, vibration);

    }
}