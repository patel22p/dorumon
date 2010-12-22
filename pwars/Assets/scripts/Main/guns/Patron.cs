using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patron : Base
{
    public Vector3 Force = new Vector3(0, 0, 80);
    public GameObject detonator;
    public float detonatorDestroyTime = 4;
    public bool DestroyOnHit;
    public bool explodeOnDestroy;
    public int detonatorsize = 8;
    internal float ExpForce = 2000;
    internal int damage = 60;
    internal int probivaemost = 0;
    public float magnet;
    public float radius = 6;
    public float samonavod;
    public bool breakwall;
    public float timeToDestroy = 5;
    
    public float tm;
    public DecalTypes decal;
    protected Vector3 previousPosition;
    protected override void Start()
    {
        Force = transform.rotation * Force;
        previousPosition = transform.position;
        base.Start();
    }
    public override void Init()
    {
        decal = DecalTypes.Hole;
        base.Init();
    }
    protected virtual void Update()
    {
        tm += Time.deltaTime;
        if (tm > timeToDestroy)
        {
            if (explodeOnDestroy)
                Explode(this.transform.position);
            else
                Destroy(gameObject);
        }        

        if (DestroyOnHit)
        {
            Vector3 movementThisStep = transform.position - previousPosition;
            RaycastHit hitInfo;
            Ray ray = new Ray(previousPosition, movementThisStep);

            if (Physics.Raycast(ray, out hitInfo, movementThisStep.magnitude + 1, _Game.PatronCollMask))
            {
                ExplodeOnHit(hitInfo);
            }
        }
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (Force != default(Vector3))
            this.transform.position += Force * Time.fixedDeltaTime;

        if (magnet > 0)
            Magnet();
    }

    private void Magnet()
    {
        foreach (Patron p in _Game.patrons)
        {
            if (p.Force.magnitude != 0 && p != this)
                p.Force += (pos - p.pos).normalized * magnet * Time.fixedDeltaTime * p.Force.sqrMagnitude / Vector3.Distance(p.pos, pos) / 2000;
        }

        foreach (var b in _Game.boxes.Cast<MonoBehaviour>().Union(_Game.patrons.Where(a=>a.rigidbody!=null).Cast<MonoBehaviour>()).Union(_Game.zombies.Cast<MonoBehaviour>()).Where(b => b != null))
        {
            if (b != this)
            {
                b.rigidbody.AddExplosionForce(-magnet * b.rigidbody.mass, transform.position, 15);
                b.rigidbody.velocity *= .97f;
            }
        }

    }
    protected virtual void ExplodeOnHit(RaycastHit hit)
    {
        transform.position = hit.point + transform.rotation * Vector3.forward;

        //if (breakwall)
        //{
        //    Fragment f = hit.collider.GetComponent<Fragment>();
        //    if (f != null)
        //        f.BreakAndDestroy();
        //}

        if (hit.collider.gameObject.isStatic)
            _Game.AddDecal(hit.collider.gameObject.name.Contains("glass") && decal == DecalTypes.Hole ? DecalTypes.glass : decal,
                hit.point - rot * Vector3.forward * 0.12f, hit.normal, hit.collider.transform);            

        if (explodeOnDestroy)
            Explode(hit.point);

        
        if (!explodeOnDestroy)
        {
            Transform b = hit.collider.gameObject.transform.root;
            if (b.rigidbody != null)
                b.rigidbody.AddForceAtPosition(transform.rotation * new Vector3(0, 0, ExpForce) / Time.timeScale, hit.point);
        }

        Destroible iplayer = hit.collider.gameObject.transform.GetRoot<Destroible>();


        if ((iplayer as Player != null || iplayer as Zombie != null) && _SettingsWindow.Blood)
        {
            _Game.particles[(int)ParticleTypes.BloodSplatters].Emit(hit.point, transform.rotation);
            RaycastHit h;
            if (Physics.Raycast(new Ray(pos, new Vector3(0, -1, 0)), out h, 10, 1 << LayerMask.NameToLayer("Level") | LayerMask.NameToLayer("MapItem")))
            {
                _Game.AddDecal(
                    DecalTypes.Blood,
                    h.point - new Vector3(0, -1, 0) * 0.1f,
                    h.normal, _Game.decals.transform);
            }
        }
        else
            _Game.particles[(int)ParticleTypes.particle_metal].Emit(hit.point, transform.rotation);

        if (iplayer != null && iplayer.isController && !iplayer.dead)
        {            
            iplayer.RPCSetLife(iplayer.Life - damage, OwnerID);
        }
        probivaemost--;
        if(probivaemost<0)
            Destroy(gameObject);
    }

    private void Explode(Vector3 pos)
    {
        Vector3 vector3 = pos - this.transform.rotation * new Vector3(0, 0, 2);
        GameObject o;
        Destroy(o = (GameObject)Instantiate(detonator, vector3, Quaternion.identity), detonatorDestroyTime);
        if (detonator.GetComponent<Detonator>() != null) o.GetComponent<Detonator>().size = detonatorsize;
        Explosion e = o.AddComponent<Explosion>();        
        e.exp = ExpForce;
        e.radius = radius;
        e.damage = damage;        
        e.OwnerID = OwnerID;
        Destroy(gameObject);
    }
}