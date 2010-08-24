using UnityEngine;
using System.Collections;
public abstract class GunBase : Base
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
        RaycastHit h = ScreenRay();

        Transform t = cursor;
        if (h.point != default(Vector3))
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

    protected virtual void RPCShoot(Vector3 pos, Quaternion rot) { }

    protected virtual void LocalShoot()
    {
        Transform t = GetRotation();
        RPCShoot(t.position, t.rotation);
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled) return;
        if (isOwner) q = _Cam.transform.rotation;
        stream.Serialize(ref q);
        transform.rotation = q;
    }


    public virtual void EnableGun()
    {
        Show(true);
    }


}