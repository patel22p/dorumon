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
    
    public virtual void DisableGun()
    {
        Show(false);
    }

    void Start()
    {


    }
    public Quaternion q;    
    protected virtual void FixedUpdate()
    {
        //if(Root(this.gameObject).name == "LocalPlayer")
        //    Debug.Log(enabled);
        UpdateAim();
        this.transform.rotation = q;
    }

    private void UpdateAim()
    {
        if (isOwner) q = _Cam.transform.rotation;
    }

    protected virtual void Update()
    {

        UpdateAim();
        transform.rotation = q;
        if (isOwner)
            LocalUpdate();

    }
    public Transform cursor;
    public Transform GetRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Transform t = cursor;
        if (Physics.Raycast(ray, out h, float.MaxValue, collmask))
            t.LookAt(h.point);
        else
            t.rotation = _Cam.transform.rotation;
        return t;
    }

    public Transform _Patron;
    protected virtual void LocalUpdate()
    {

        if (Time.time - lt > interval && Input.GetMouseButton(0) && Screen.lockCursor)
        {
            if (bullets > 0)
            {
                bullets--;
                lt = Time.time;
                LocalShoot();
            }
            else
            {                
                _LocalPlayer.NextGun(1);
            }
        }
    }

    protected virtual void LocalShoot()
    {
        UpdateAim();
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (isOwner) q = _Cam .transform.rotation;
        stream.Serialize(ref q);
        transform.rotation = q;
    }


    public virtual void EnableGun()
    {
        Show(true);
    }


}