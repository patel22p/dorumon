using System;
using System.Collections;
using System.Linq;
using doru;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Ak47 : GunBase
{
    public Transform CamRnd { get { return pl.CamRnd; } }
    Timer timer = new Timer();
    
    public AnimationState handsReload { get { return an["reload"]; } }
    public AudioClip ClipOut;
    public AudioClip ClipIn;
    public AudioClip Draw;
    public AudioClip BoltPull;
    public int damage = 14;

    AnimationState[] m_handsShoot;

    internal AnimationState[] handsShoot
    {
        get
        {
            if (m_handsShoot == null)
                m_handsShoot = new[] { an["shoot1"], an["shoot2"], an["shoot3"] };
            return m_handsShoot;
        }
    }
    public AudioClip[] shootSound;
    public ParticleEmitter Capsules;
    public ParticleEmitter Capsules2;
    public float shootBump = 1;
    public float shootCursor = 5;
    public float shootTime = .1f;
    float lastShoot;

    public override void Awake()
    {
        enabled = false;        
        base.Awake();
    }
    public override void Start()
    {
        base.Start();
        m_handsShoot = new[] { an["shoot1"], an["shoot2"], an["shoot3"] };
        handsReload.layer = 1;        
        an.Play(handsDraw.name);
        //fix hand draw
    }
    public void OnSetID()
    {
        enabled = true;
    }
    public void Update()
    {     
        timer.Update();
        shooting = Time.time - lastShoot < shootTime * 2;
        CamRnd.localRotation = Quaternion.Slerp(CamRnd.localRotation, Quaternion.identity, Time.deltaTime * 2);
        cursorOffset = Mathf.MoveTowards(cursorOffset, 0, Time.deltaTime * 20);
        if (MouseButtonDown && IsMine)
        {
            if (Time.time - lastShoot > shootTime)
            {
                if (!handsReload.enabled && !handsDraw.enabled)
                {
                    if (patrons > 0)
                        shoot();
                    else if (globalPatrons > 0)
                        CallRPC(Reload, PhotonTargets.All);
                    //else
                    //{
                        //pl.audio.PlayOneShot(Pistol ? pl.dryfire_pistol : pl.dryfire_rifle);
                    //}

                }
            }
        }
    }

   
    public override void OnRDown()
    {
        if (!this.handsReload.enabled && patrons != 30 && globalPatrons > 0)
            this.CallRPC(Reload, PhotonTargets.All);
    }

    [RPC]
    public void Reload()
    {
        an.Play(handsReload.name);
    }
    public void OnReloaded()
    {
        var take = Mathf.Min(30 - patrons, globalPatrons);
        globalPatrons -= take;
        patrons += take;
    }
    public void shoot()
    {
        lastShoot = Time.time;
        Ray ray = pl.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        CamRnd.localRotation = Quaternion.Euler(CamRnd.localRotation.eulerAngles + (Random.insideUnitSphere + Vector3.left) * shootBump);
        ray.direction +=
            (Random.insideUnitSphere * cursorOffset * 0.005f) +
            (Random.insideUnitSphere * 0.01f * pl.controller.velocity.magnitude) +
            (Time.time - pl.HitTime < 1f ? Random.insideUnitSphere * .01f : Vector3.zero);
        CallRPC(RPCShoot, PhotonTargets.All, ray.origin, ray.direction);
    }
    private IEnumerable<RaycastHit> RaycastAll(Ray ray, int dist, int layer)
    {
        RaycastHit h;
        while (Physics.Raycast(ray, out h, dist, layer))
        {
            ray.origin = h.point - h.normal * .1f;
            yield return h;
        }
    }

    [RPC]
    private void RPCShoot(Vector3 rayOrg, Vector3 rayDir)
    {
        patrons--;
        if (!pl.observing) {
            pl.MuzzleFlash2.GetComponentInChildren<Animation>().Play();
            Capsules2.Emit();
        }
        else {
            pl.MuzzleFlash.renderer.material = pl.MuzzleFlashMaterials.Random();
            pl.MuzzleFlash.animation.Play();
            Capsules.Emit();
        }
        Ray ray = new Ray(rayOrg, rayDir);
        float WallScore=0;
        pl.audio.PlayOneShot(shootSound.Random());
        cursorOffset = Mathf.Min(cursorOffset + shootCursor, 15);
        foreach (var h in RaycastAll(ray, 1000, 1 << LayerMask.NameToLayer("Level") | 1 << LayerMask.NameToLayer("Dead") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("IgnoreColl"))) 
        {
            Shared enemy = h.collider.transform.root.GetComponent<Shared>();
            if (enemy != null)
            {
                if (enemy == pl ) continue;
                if (enemy.pv.team != pl.pv.team)
                {
                    if (_Loader.EnableBlood)
                    {
                        CreateBlood(h);
                        ray = new Ray(h.point, ray.direction + Vector3.down + Random.insideUnitSphere*.4f);
                        RaycastHit h2;
                        if (Physics.Raycast(ray, out h2, 100, 1 << LayerMask.NameToLayer("Level")))
                        {
                            GameObject g =(GameObject)Instantiate(pl.Plane, h2.point + h2.normal*.04f, Quaternion.LookRotation(h2.normal));
                            g.transform.localScale = Vector3.one*18;
                            g.renderer.material = pl.BloodDecals.Random();
                            g.transform.parent = _Game.Fx;
                        }
                    }
                    if (enemy.observing)
                    {
                        var angle = Mathf.DeltaAngle(Quaternion.LookRotation(pl.pos - enemy.pos).eulerAngles.y, enemy.rot.eulerAngles.y);
                        if (angle > 45) _Hud.SetPainRight(1);
                        else if (angle < -45) _Hud.SetPainLeft(1);
                        else _Hud.SetPainUp(1);
                    }
                    if (enemy.IsMine)
                    {                        
                        if (h.collider.name == "Bip01 Head")
                        {
                            //note headShot sound
                            enemy.CallRPC(enemy.SetLife, PhotonTargets.All, 0, pl.id);
                            enemy.audio.PlayOneShot(pl.headShootSound.Random(), 6);
                        }
                        else
                            enemy.CallRPC(enemy.SetLife, PhotonTargets.All, enemy.Life - damage, pl.id);
                    }                    
                }
            }
            else if (h.rigidbody != null)
            {
                CreateBlood(h);                
                h.rigidbody.AddForceAtPosition(ray.direction * 1000, h.point);
            }
            else
            {
                var g2 = (GameObject)Instantiate(pl.sparks, h.point, Quaternion.LookRotation(h.normal));
                g2.transform.parent = _Game.Fx;
                g2.AddComponent<AudioSource>().PlayOneShot(pl.bulletric.Random());
                var g = (GameObject)Instantiate(pl.Plane, h.point + h.normal * .04f, Quaternion.LookRotation(h.normal));
                g.transform.localScale = Vector3.one * 1f;
                g.transform.parent = _Game.Fx;
                g.renderer.material = pl.BulletHoleMaterials.Random();
            }
            WallScore++;
            if(WallScore>1)
                break;
        }
        var a = handsShoot.Random();
        a.time = 0;
        an.Play(a.name, PlayMode.StopSameLayer);
    }
    private void CreateBlood(RaycastHit h)
    {
        if (_Loader.EnableBlood)
            ((GameObject)Instantiate(pl.BloodPrefab, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;
    }
    
    

    #region props
    public void ClipOutSound()
    {
        pl.audio.PlayOneShot(ClipOut, .5f);
    }
    public void ClipInSound()
    {
        pl.audio.PlayOneShot(ClipIn, .5f);
    }
    public void DrawSound()
    {
        pl.audio.PlayOneShot(Draw,.3f);
    }
    public void BoltPullSound()
    {
        pl.audio.PlayOneShot(BoltPull, .5f);
    }

    

    #endregion
}