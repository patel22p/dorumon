using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patron : Base
{
    public Vector3 Force = new Vector3(0,0,80);    
    public Transform detonator;
    public bool explodeOnHit;
    public bool explodeOnDestroy;
    public int detonatorsize = 8;
    public int addLife = 60;
    public float exp = 500;
    public float radius = 4;
    public bool magnet;
    public bool samonavod;
    public bool breakwall;
    public float lifeTake;
    public AudioClip sound;
    public float timetoexplode =5;
    public float freezetime;
    public Transform decal;
    public float tm;
    protected Vector3 previousPosition;
    protected virtual void Start()
    {        
        previousPosition = transform.position;
        
    }
    protected virtual void FixedUpdate()
    {
        tm += Time.deltaTime;
        if (tm > timetoexplode)
        {
            if (explodeOnDestroy)
                Explode(this.transform.position);
            else
                Destroy(gameObject);
        }
        Vector3 movementThisStep = transform.position - previousPosition;
        if (explodeOnHit)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementThisStep.magnitude + 1, collmask))
            {
                if (!hitInfo.collider.isTrigger || hitInfo.collider.gameObject.name == "hit")
                    ExplodeOnHit(hitInfo);
            }
            previousPosition = transform.position;
        }
    }
    protected virtual void ExplodeOnHit(RaycastHit hit)
    {
        if (breakwall)
        {
            Fragment f = hit.collider.GetComponent<Fragment>();
            if (f != null)
                f.BreakAndDestroy();
        }
        
        if (decal != null && hit.collider.gameObject.isStatic)
        {
            Transform a;
            Destroy((a = (Transform)Instantiate(decal, hit.point, Quaternion.LookRotation(hit.normal))), 10);
            a.parent = _Game.effects;
        }
        if (detonator != null)
        {
            Explode(hit.point);
        }

        Destroy(gameObject);
        IPlayer iplayer = hit.collider.gameObject.transform.root.GetComponent<IPlayer>();
        if ((iplayer as Player != null || iplayer as Zombie != null) && _SettingsWindow.Blood)
            _Game.Emit(_Game.BloodEmitors, _Game.Blood, hit.point, transform.rotation);        
        else
            _Game.Emit(_Game.metalSparkEmiters, _Game.metalSpark, hit.point, transform.rotation);

        if (iplayer != null && iplayer.isController && !iplayer.dead)
        {
            if (iplayer is Player)
                ((Player)iplayer).freezedt = 40;
            iplayer.RPCSetLife(iplayer.Life + addLife, OwnerID);
        }

    }

    private void Explode(Vector3 pos)
    {
        Vector3 vector3 = pos - this.transform.rotation * new Vector3(0, 0, 2);
        GameObject o;
        Destroy(o = (GameObject)Instantiate(detonator, vector3, Quaternion.identity), 10);
        o.GetComponent<Detonator>().size = detonatorsize;
        Explosion e = o.AddComponent<Explosion>();
        e.exp = exp;
        e.damage = addLife;
        o.GetComponent<Explosion>().OwnerID = OwnerID;
        Destroy(gameObject);
        
    }
}