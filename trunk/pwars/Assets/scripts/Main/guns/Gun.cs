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
    public Player p;
    protected override void Awake()
    {        
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
                LocalShoot();
            }
            else
            {
                PlaySound("noammo");                    
                //_LocalPlayer.NextGun(1);
            }
        }
    }
    [RPC]
    protected virtual void RPCShoot(Vector3 pos, Quaternion rot)
    {
        CallRPC(pos, rot);
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
        for (int i = 0; i < howmuch; i++)
        {
            
            Vector3 r;
            r.x = Random.Range(-random.x, random.x);
            r.y = Random.Range(-random.y, random.y);
            r.z = Random.Range(-random.z, random.z);
            Patron patron = ((GameObject)Instantiate(patronPrefab, pos, rot * Quaternion.Euler(r))).GetComponent<Patron>();
            patron.OwnerID = OwnerID;
            if (Force != default(Vector3)) patron.rigidbody.AddForce(this.transform.rotation * Force);
        }        
        
    }
    protected virtual void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);
    }
    
    
    
    public Transform GetRotation()
    {
        RaycastHit h = RayCast(collmask,float.MaxValue);

        Transform t = cursor;
        if (h.point != default(Vector3))
            t.LookAt(h.point);
        else
            t.rotation = _Cam.transform.rotation;
        return t;
    }
}