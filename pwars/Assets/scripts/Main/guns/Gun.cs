using UnityEngine;
using System.Collections;
public class Gun : GunBase
{
    public float interval = 1;
    [HideInInspector]
    public float tm;
    public int howmuch = 1;
    public GameObject patronPrefab;
    public Vector3 random;
    public Texture2D GunPicture;    
    public Vector3 Force;
    public Transform barrel;
    public float exp;
    public float vibration=0;
    public AudioClip sound;
    internal Player player;
    public int damage = 60;
    public int probivaemost = 0;
    public float otbrasivanie;
    public float ves;
    public float bulletForce;
    Vector3 defPos,defPos2;
    [LoadPath("noammo")]
    public AudioClip noammoSound;
    int cursorid;
    public float barrelVell;
    protected override void Awake()
    {
        defPos2 = defPos = transform.localPosition;
        player = root.GetComponent<Player>();
        base.Awake();
    }
    public override void onShow(bool enabled)
    {

        if (enabled)
            player.rigidbody.mass = player.defmass + ves * player.defmass;
        base.onShow(enabled);
    }
    protected override void Update()
    {
        if(GunPicture!=null && isOwner)
            _GameWindow.gunTexture.texture = GunPicture;

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

        base.Update();

    }
    protected virtual void LocalUpdate()
    {
        
        if ((tm -= Time.deltaTime) < 0 && Input.GetMouseButton(0) && lockCursor)
        {
            tm = interval;
            if (patronsLeft > 0)
            {
                patronsLeft--;
                RPCShoot();
            }
            else
                PlaySound(noammoSound);                    
        }
    }
    [RPC]
    protected virtual void RPCShoot()
    {
        if (sound != null)
            root.audio.PlayOneShot(sound);
        player.rigidbody.AddForce(rot * new Vector3(0, 0, -otbrasivanie));
        for (int i = 0; i < howmuch; i++)
        {
            Vector3 r;
            r.x = Random.Range(-random.x, random.x);
            r.y = Random.Range(-random.y, random.y);
            r.z = Random.Range(-random.z, random.z);
            cursorid++;
            if (cursorid >= cursor.Count) cursorid = 0;
            foreach (var p in cursor[cursorid].GetComponentsInChildren<ParticleEmitter>())
                p.Emit();
            Light fireLight = cursor[cursorid].GetComponentInChildren<Light>();
            if (fireLight != null && !fireLight.enabled)
            {
                fireLight.enabled = true;
                _TimerA.AddMethod(20, delegate
                {
                    fireLight.enabled = false;
                });
            }
            if (barrel != null) barrelVell += 10;

            Patron patron = ((GameObject)Instantiate(patronPrefab, cursor[cursorid].position , rot * Quaternion.Euler(r))).GetComponent<Patron>();
            patron.OwnerID = OwnerID;
            patron.damage = this.damage;
            if (exp != 0) patron.ExpForce = exp;
            if (bulletForce != 0) patron.Force = new Vector3(0, 0, bulletForce);
            patron.probivaemost = this.probivaemost;
            if (Force != default(Vector3)) patron.rigidbody.AddForce(this.transform.rotation * Force);
        }
        this.pos -= rot * new Vector3(0, 0, vibration);
        
    }
    
    
    
}