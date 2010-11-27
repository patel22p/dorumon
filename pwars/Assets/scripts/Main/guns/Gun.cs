using UnityEngine;
using System.Collections;
public class Gun : GunBase
{
    public float interval = 1;
    public float tm;
    public int defcount;
    
    
    public Quaternion rotation;
    
    public Transform patronPrefab;
    public Vector3 Random;
    public bool laser;
    public Vector3? Force;
    public ParticleEmitter[] fireEfects;
    public Light fireLight;
    public AudioClip sound;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Reset()
    {
        patronsleft = defcount;
    }
    
    protected virtual void FixedUpdate()
    {
        UpdateAim();
        this.transform.rotation = rotation;
    }
    private void UpdateAim()
    {
        if (isOwner) rotation = _Cam.transform.rotation;
    }
    protected virtual void Update()
    {

        UpdateAim();
        transform.rotation = rotation;
        if (isOwner)
            LocalUpdate();

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
    protected virtual void RPCShoot(Vector3 pos, Quaternion rot)
    {
        CallRPC(pos, rot);
        foreach (ParticleEmitter p in fireEfects)
            p.Emit();
        if (fireLight != null)
        {
            fireLight.enabled = true;
            _TimerA.AddMethod(100, delegate
            {
                fireLight.enabled = false;
            });
        }
        if (sound != null)
            root.audio.PlayOneShot(sound);

        Patron patron = ((GameObject)Instantiate(patronPrefab, pos, rot * Quaternion.Euler(Random))).GetComponent<Patron>();
        patron.OwnerID = OwnerID;
        if (Force != null) patron.rigidbody.AddForce(Force.Value);
        
    }
    protected virtual void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (isOwner) rotation = _Cam.transform.rotation;
        stream.Serialize(ref rotation);
        transform.rotation = rotation;
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