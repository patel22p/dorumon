using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBazoka : BulletBase
{
    public float velocity;
    protected override void FixedUpdate()
    {
        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        base.FixedUpdate();
    }
    protected override void Hit(RaycastHit hit)
    {
        base.Hit(hit);
        Fragment f = hit.collider.GetComponent<Fragment>();
        if (f != null)
            f.BreakAndDestroy();
        if (hit.collider.gameObject.isStatic)
        {
            Transform a;
            Destroy((a = (Transform)Instantiate(_Game.decal, hit.point, Quaternion.LookRotation(hit.normal))), 10);
            a.parent = _Game.effects;
        }
        Vector3 vector3 = hit.point - this.transform.rotation * new Vector3(0, 0, 2);
        GameObject o;
        Destroy(o = (GameObject)Instantiate(Resources.Load("Detonator/Prefab Examples/Detonator-Base"), vector3, Quaternion.identity), 10);
        o.GetComponent<Detonator>().size = 8f;
        o.AddComponent<Explosion>().exp = 1000;
        o.GetComponent<Explosion>().OwnerID = OwnerID;
        Destroy(gameObject);
    }
}
