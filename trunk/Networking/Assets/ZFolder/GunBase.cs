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

    protected override void Start()
    {
        
    }
    Quaternion q;
    protected override void FixedUpdate()
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        this.transform.rotation = q;
    }

    protected override void Update()
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        transform.rotation = q;
        if (isMine)
            LocalUpdate();

    }
    
    public Transform GetRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Transform t = transform.Find("cursor");
        if (Physics.Raycast(ray, out h, float.MaxValue, 1))
            t.LookAt(h.point);
        else
            t.rotation = cam.transform.rotation;
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