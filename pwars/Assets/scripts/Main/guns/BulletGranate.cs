using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BulletGranate : BulletBase
{
    public float velocity;
    public float gravitaty = 1;
    public float scalefactor = 10;
    public float radius = 50;
    protected override void Awake()
    {
        onShow(true);
        base.Awake();
    }
    public void StartGranate()
    {
        _TimerA.AddMethod(3000, Bum);
        _Game.dynamic.Add(this);
        enabled = true;
    }
    protected override void FixedUpdate()
    {        
        foreach (var b in _Game.dynamic.Union(_Game.zombies.Cast<Base>()))
        {
            if (b.GetType() == typeof(Box) || b is Zombie)
            {
                b.rigidbody.AddExplosionForce(-gravitaty * scalefactor * b.rigidbody.mass, transform.position, radius);
                b.rigidbody.angularDrag = 30;
                b.rigidbody.velocity *= .97f;
            }
        }
    }
    public void Bum()
    {
        GameObject o;
        Destroy(o = (GameObject)Instantiate(Resources.Load("Detonator/Prefab Examples/Detonator-Sounds"), this.transform.position, Quaternion.identity), 10);
        o.AddComponent<Explosion>().exp = 300;
        o.GetComponent<Detonator>().size = 8f;
        o.GetComponent<Detonator>().autoCreateForce = false;
        o.GetComponent<Explosion>().OwnerID = OwnerID;
        Transform a;
        Destroy((a = (Transform)Instantiate(_Game.decal, this.transform.position, Quaternion.identity)), 10);
        a.parent = _Game.effects;
        _Game.dynamic.Remove(this);
        this.transform.parent = players[OwnerID].GetComponentInChildren<GunGranate>().cursor;
        if (players[OwnerID].selectedgun != 4)
        {
            this.Show(false);
            print(pr);
        }
        this.transform.localPosition = Vector3.zero;
        this.rigidbody.isKinematic = true;
        rigidbody.detectCollisions = false;
        enabled = false;
    }
    
    public override void onShow(bool value)
    {
        enabled = false;
        rigidbody.isKinematic = true;
        rigidbody.detectCollisions = false;
        //base.onShow(value);
        //enabled = false;
    }

}
