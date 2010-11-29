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
    public bool laser;
    public Vector3 Force;
    public ParticleEmitter[] fireEfects;
    [HideInInspector]
    public Light fireLight;
    public AudioClip sound;
    internal Player player;
    public int damage = 60;
    public int probivaemost = 0;
    public float otbrasivanie;
    protected override void Awake()
    {
        player = root.GetComponent<Player>();
        var t = transform.Find("light");
        if (t != null) fireLight = t.GetComponent<Light>();
        base.Awake();
    }
    public override void onShow(bool enabled)
    {
        base.onShow(enabled);
    }
    protected override void Update()
    {
        if(GunPicture!=null && isOwner)
            _GameWindow.gunTexture.texture = GunPicture;
        
        if (isOwner)
            LocalUpdate();

        base.Update();

    }
    protected virtual void LocalUpdate()
    {
        if ((tm -= Time.deltaTime) < 0 && Input.GetMouseButton(0) && lockCursor)
        {
            tm = interval;
            if (patronsleft > 0 || !build)
            {
                patronsleft--;
                RPCShoot();
            }
            else
                PlaySound("noammo");                    
        }
    }
    int cursorid;
    [RPC]
    protected virtual void RPCShoot()
    {
        CallRPC();
        foreach (ParticleEmitter p in fireEfects)
            p.Emit();
        if (fireLight != null)
        {
            fireLight.enabled = true;
            _TimerA.AddMethod(20, delegate
            {
                fireLight.enabled = false;
            });
        }
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
            Patron patron = ((GameObject)Instantiate(patronPrefab, cursor[cursorid].position , rot * Quaternion.Euler(r))).GetComponent<Patron>();
            patron.OwnerID = OwnerID;
            patron.damage = this.damage;
            patron.probivaemost = this.probivaemost;
            if (Force != default(Vector3)) patron.rigidbody.AddForce(this.transform.rotation * Force);
        }        
        
    }
    
    
    
}