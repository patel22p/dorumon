using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patron : bs
{    
    private float tm;
    private Vector3 Force; 
    private Vector3 previousPosition;
    internal Gun gun;
    private int? _probivaemost;

    public bool decalhole = true;    
    [FindAsset("Detonator-Base")]
    public GameObject detonator;    
    [FindAsset(overide = true)]
    public GameObject Explosion;
    public bool DestroyOnHit;
    public bool explodeOnDestroy;
    public int detonatorsize = 8;
    public bool granate;
    public bool breakwall;
    public float damageFactor(Object o)
    {
        return _Game.mapSettings.damageFactor * (o is Zombie ? zmDamageFactor : 1) * (o is Player ? plDamageFactor : 1);
    }
    public void Start()
    {
        _Game.patrons.Add(this);
        Force = transform.rotation * new Vector3(0, 0, 300 * BulletForce);
        previousPosition = transform.position;        
    }
    void OnDisable()
    {
        _Game.patrons.Remove(this);
    }
    public override void Init()
    {
        base.Init();
    }
    public override void InitValues()
    {        
        base.InitValues();
    }
    public AnimationCurve SamoNavod;
    protected virtual void Update()
    {
        if (Force != default(Vector3))
            this.transform.position += Force * Time.deltaTime;
        if (granate)
            Force.y += Physics.gravity.y * Time.deltaTime;

        if (gravitate > 0)
            GravitateMagnet();
        if (SamoNavod.length > 0)
            foreach (Destroible p in _Game.players.Union(_Game.zombies.Cast<Destroible>()))
                if (p != null && p.isEnemy(OwnerID))
                    Force += (this.pos - _localPlayer.pos).normalized * Time.deltaTime * this.Force.sqrMagnitude * SamoNavod.Evaluate(Vector3.Distance(this.pos, _localPlayer.pos));

        tm += Time.deltaTime;
        if (tm > timeToDestroy)
        {
            if (explodeOnDestroy)
                Explode(this.transform.position);
            else
                Destroy(gameObject);
        }
        Vector3 movementThisStep = transform.position - previousPosition;
        RaycastHit hitInfo;
        Ray ray = new Ray(previousPosition, movementThisStep);
        if (Physics.Raycast(ray, out hitInfo, movementThisStep.magnitude + 1, granate ? 1 << LayerMask.NameToLayer("Level") : GetMask()))
        {
            if (DestroyOnHit)
                ExplodeOnHit(hitInfo);
            if (granate)
            {
                transform.position = hitInfo.point + (hitInfo.normal * .2f);
                Force = Vector3.zero;                
            }
        }
        previousPosition = transform.position;
    }
    public bool hit;
    private int GetMask()
    {
        int mask = 1 << LayerMask.NameToLayer("HitEnemyOnly") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Glass") | 1 << LayerMask.NameToLayer("Level") | 1 << (_localPlayer.isEnemy(OwnerID) ? LayerMask.NameToLayer("Ally") : LayerMask.NameToLayer("Enemy"));
        return mask;
    }
    private void GravitateMagnet()
    {
        foreach (Patron p in _Game.patrons)
        {
            if (p.Force.magnitude != 0 && !p.granate && p != this)
                p.Force += (pos - p.pos).normalized * gravitate * 500 * Time.deltaTime * p.Force.sqrMagnitude / Vector3.Distance(p.pos, pos) / 2000;
        }

        foreach (var b in _Game.players.Where(p => p != null && p.isEnemy(OwnerID)).Cast<bs>().Union(_Game.boxes.Cast<bs>()).Union(_Game.patrons.Where(a => a.rigidbody != null).Cast<bs>()).Union(_Game.zombies.Cast<bs>()))
            if (b != null && b != this && Vector3.Distance(pos, b.pos) < Radius * 3)
            {
                if (b is Zombie && Vector3.Distance(pos, b.pos) < Radius)
                {
                    var z = ((Zombie)b);    
                    z.ResetSpawnTm();                    
                    z.RPCSetFrozen(true);
                }
                b.rigidbody.AddExplosionForce(-gravitate * 200 * b.rigidbody.mass * fdt, transform.position, 15);
                b.rigidbody.velocity *= .97f;
            }

    }
    protected virtual void ExplodeOnHit(RaycastHit hit)
    {

        var g = hit.collider.gameObject;
        var t = hit.collider.transform;

        transform.position = hit.point + transform.rotation * Vector3.forward;
        bool glass = g.tag == "glass";

        var m = t.GetMonoBehaviorInParrent();        
        if (!explodeOnDestroy)
        {
            var r = hit.rigidbody;
            var rt = transform.rotation;
            _TimerA.AddMethod(delegate
            {
                if (r != null && r.velocity.magnitude < 20)
                    r.AddForceAtPosition(rt * new Vector3(0, 0, ExpForce * hit.rigidbody.mass / hit.collider.bounds.size.magnitude * 1000) * fdt, hit.point);
            });
        }
        Destroible destroible = m as Destroible;
        if (g.layer == LayerMask.NameToLayer("Level") || g.layer == LayerMask.NameToLayer("Glass"))
            _Game.AddDecal(glass ? DecalTypes.glass : (decalhole ? DecalTypes.Hole : DecalTypes.Decal),
                hit.point - rot * Vector3.forward * 0.12f, hit.normal, t);
        
        if (destroible is Zombie && _SettingsWindow.Blood)
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
        {
            _Game.particles[(int)ParticleTypes.particle_metal].Emit(hit.point, transform.rotation);
        }
        if (destroible != null && destroible.isController && destroible.Alive)
        {
            destroible.RPCSetLife(destroible.Life - damage * damageFactor(destroible), OwnerID);
        }
        
        bool staticfield = g.name.ToLower().StartsWith("staticfield");
        if (staticfield)
            Force = Quaternion.LookRotation(hit.point - g.transform.position) * Vector3.forward * Force.magnitude;
        if (!glass && !staticfield)
        {
            probivaemost--;

            if (explodeOnDestroy)
                Explode(hit.point);
        }
        if(probivaemost<0)
            Destroy(gameObject);
    }    
    private void Explode(Vector3 pos)
    {
        Vector3 vector3 = pos - this.transform.rotation * new Vector3(0, 0, 2);
        GameObject o;
        Destroy(o = (GameObject)Instantiate(detonator, vector3, Quaternion.identity), .6f);
        GameObject exp = (GameObject)Instantiate(Explosion, o.transform.position, Quaternion.identity);
        exp.transform.parent = o.transform;
        var e = exp.GetComponent<Explosion>();
        e.exp = ExpForce * 400;
        e.DamageFactor = damage;
        e.radius = Radius;
        e.OwnerID = OwnerID;
        Destroy(gameObject);
        var dt = detonator.GetComponent<Detonator>();
        dt.size = detonatorsize;
        dt.autoCreateForce = false;
    }
    internal float ExpForce { get { return gun.expOttalkivanie; } }
    internal int damage { get { return gun.damage; } }
    internal float Radius { get { return gun.radius; } }
    internal float timeToDestroy { get { return gun.timeToDestroy; } }
    internal float plDamageFactor { get { return gun.plDamageFactor; } }
    internal float zmDamageFactor { get { return gun.zmDamageFactor; } }
    internal float BulletForce { get { return gun.BulletForce; } }
    internal int probivaemost { get { return _probivaemost ?? gun.probivaemost; } set { _probivaemost = value; } }
    internal float gravitate { get { return gun.gravitate; } }
}