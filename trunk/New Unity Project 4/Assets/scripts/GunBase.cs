using UnityEngine;
using System.Collections;
public class GunBase : Base
{
    public float interval = 1;
    float lt;
    public int def;
    internal float bullets;
    public void Reset()
    {
        bullets = def;
    }
    Cam cam { get { return Find<Cam>(); } }
    public virtual void DisableGun()
    {        
        Show(false);
    }

    protected override void OnLoaded()
    {
        
    }
    public Quaternion q;
    public bool car;
    protected override void OnFixedUpdate()
    {
        UpdateAim();
        this.transform.rotation = q;
    }

    private void UpdateAim()
    {
        if (isOwner && !car) q = Find<Cam>().transform.rotation;
    }

    protected override void OnUpdate()
    {
        UpdateAim();
        transform.rotation = q;
        if (isOwner)
            LocalUpdate();

    }
    
    public Transform GetRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Transform t = transform.Find("cursor");
        if (!car)
        {
            if (Physics.Raycast(ray, out h, float.MaxValue, 1))
                t.LookAt(h.point);
            else
                t.rotation = cam.transform.rotation;
        }
        return t;
    }
    public Transform _Patron;
    protected virtual void LocalUpdate()
    {     
        if (Time.time - lt > interval && Input.GetMouseButton(0) && Screen.lockCursor && bullets > 0)
        {
            
            bullets--;
            lt = Time.time;
            LocalShoot();
        }
    }

    protected virtual void LocalShoot()
    {
        UpdateAim();
    }
    protected override void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        stream.Serialize(ref q);
        transform.rotation = q;        
    }
    
    
    public virtual void EnableGun()
    {        
        Show(true);
    }

    
}