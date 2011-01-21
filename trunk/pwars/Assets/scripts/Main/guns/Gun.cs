using UnityEngine;
using System.Collections;
using System.Linq;
public class Gun : GunBase
{
    public float interval = 1;
    [HideInInspector]
    public float tm;
    public int howmuch = 1;
    public GameObject patronPrefab;
    public GameObject towerPrefab;
    public GameObject staticFieldPrefab;
    public Vector3 random;        
    public Transform barrel;
    public float exp = 500;
    public float vibration=0;
    public AudioClip sound;
    public int damage = 60;
    public int probivaemost = 0;
    public float otbrasivanie;
    
    public float bulletForce;
    public float soundVolume;
    
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
        if (soundVolume == 1) soundVolume = .5f;        
        fireLight = root.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Point);         
    }
    
    public override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
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
        if (staticFieldPrefab != null)
        {
            RaycastHit h;
            if (Physics.Raycast(new Ray(cursor[0].position, cursor[0].rotation * Vector3.forward), out h, 1000, 1 << LayerMask.NameToLayer("Level")) && isOwner)
                RPCCreateField(h.point);
        }
        else if (towerPrefab != null)
        {
            Debug.Log("gun set owner" + OwnerID);

            if(isOwner)
                ((GameObject)Network.Instantiate(towerPrefab, cursor[0].position, cursor[0].rotation, (int)GroupNetwork.Tower)).GetComponent<Tower>().RPCSetOwner(OwnerID);
        }
        else
        {
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
                Patron patron = ((GameObject)(Instantiate(patronPrefab, p2 + r2 * Vector3.back, r2))).GetComponent<Patron>();
                patron.OwnerID = OwnerID;
                patron.damage = this.damage;
                patron.ExpForce = exp;
                if (bulletForce != 0) patron.Force = new Vector3(0, 0, bulletForce);
                patron.probivaemost = this.probivaemost;                
            }
        }
        RandomFactorTm = Mathf.Min(RandomFactorTm + .2f, 1);
        this.pos -= rot * new Vector3(0, 0, vibration);

    }
    public void RPCCreateField(Vector3 pos ) { CallRPC("CreateField", pos); }
    [RPC]
    public void CreateField(Vector3 pos)
    {
        GameObject g = (GameObject)Instantiate(staticFieldPrefab, pos, Quaternion.identity);
        _TimerA.AddMethod(25000, delegate { Destroy(g); });
    }        
    
}