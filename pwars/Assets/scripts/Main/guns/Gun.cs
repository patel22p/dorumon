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
    public Vector3 random;
    public Texture2D GunPicture;    
    public Vector3 Force;
    public Transform barrel;
    public float exp = 500;
    public float vibration=0;
    public AudioClip sound;
    public int damage = 60;
    public int probivaemost = 0;
    public float otbrasivanie;
    public float ves;
    public float bulletForce;
    public float soundVolume = 1;
    Vector3 defPos,defPos2;
    [LoadPath("noammo")]
    public AudioClip noammoSound;
    int cursorid;
    public float barrelVell;
    
    public Light fireLight;
    public override void Init()
    {
        base.Init();
        if (patronsLeft == 0) { patronsLeft = -1; patronsDefaultCount = -1; }
        fireLight = root.GetComponentsInChildren<Light>().FirstOrDefault(a => a.type == LightType.Point); 
        
    }

    private void Bind(string name)
    {
        var p = this.GetComponentsInChildren<Transform>().FirstOrDefault(a => a.name == name);
        if (p != null)
        {
            var po = p.position;
            var ro = p.rotation;
            var s = p.localScale;
            var p2 = GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)).Cast<GameObject>().FirstOrDefault(a => a.name == name);
            var t = ((GameObject)Instantiate(p2, po, ro)).transform;
            t.localScale = s;
            t.parent = p.parent;
            t.name = p2.name;
            DestroyImmediate(p.gameObject);
        }
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {

        defPos2 = defPos = transform.localPosition;        
    }
    public override void onShow(bool enabled)
    {
        if (enabled && player !=null)
            player.rigidbody.mass = player.defmass + ves * player.defmass;
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
        if (GunPicture != null && player !=null && isOwner)
            _GameWindow.gunTexture.texture = GunPicture;

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
    [RPC]
    public void RPCShoot()
    {
        

        if (sound != null)
            root.audio.PlayOneShot(sound, soundVolume);
        if (player != null)
            player.rigidbody.AddForce(rot * new Vector3(0, 0, -otbrasivanie));


        for (int i = 0; i < howmuch; i++)
        {
            Vector3 r;
            r.x = Random.Range(-random.x, random.x);
            r.y = Random.Range(-random.y, random.y);
            r.z = Random.Range(-random.z, random.z);
            cursorid++;
            if (cursorid >= cursor.Count) cursorid = 0;            
            var t = cursor[cursorid].transform;
            _Game.particles[(int)ParticleTypes.fire].Emit(t.position, t.rotation);
            _Game.particles[(int)ParticleTypes.fire1].Emit(t.position, t.rotation);
            _Game.particles[(int)ParticleTypes.patrons].Emit(t.position, t.rotation);

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
            Quaternion r2 = rot * Quaternion.Euler(r) * Quaternion.Euler(Random.insideUnitSphere * RandomFactorTm*2);
            if (towerPrefab != null)
                Network.Instantiate(towerPrefab, p2, cursor[0].rotation, (int)GroupNetwork.Tower);
            else
            {
                Patron patron = ((GameObject)(Instantiate(patronPrefab, p2, r2))).GetComponent<Patron>();
                patron.OwnerID = OwnerID;
                patron.damage = this.damage;
                patron.ExpForce = exp;
                if (bulletForce != 0) patron.Force = new Vector3(0, 0, bulletForce);
                patron.probivaemost = this.probivaemost;
                if (Force != default(Vector3)) patron.rigidbody.AddForce(this.transform.rotation * Force);
            }
        }
        RandomFactorTm = Mathf.Min(RandomFactorTm + .2f, 1);
        this.pos -= rot * new Vector3(0, 0, vibration);
        
    }
    
    
    
}