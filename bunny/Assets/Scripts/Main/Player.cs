using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Player : Shared
{
    
    public float speed = 300f;
    float groundy = -.4f;
    bool scndJump;
    float groundtime;
    public AnimationState idle { get { return an["idle"]; } }
    public AnimationState run { get { return an["run"]; } }
    public AnimationState walk { get { return an["walk"]; } }
    public AnimationState jump { get { return an["jump"]; } } //todo
    public AnimationState fall { get { return an["midair"]; } }
    public AnimationState land { get { return an["landing"]; } }
    public AnimationState hit { get { return an["pawhit1"]; } }
    
    public AnimationState jumphit { get { return an["aerialattack1"]; } }
    public List<Transform> hands = new List<Transform>();
    public List<Transform> legs = new List<Transform>();
    public Transform HitEffectTrail;

    public override void Start()
    {
        base.Start();
        SetupOther();
        SetupLayers();
    }
    private void SetupOther()
    {
        _Game.shareds.Add(this);
        foreach (var a in legs.Union(hands))
            ((Transform)Instantiate(HitEffectTrail, a.transform.position, a.transform.rotation)).parent = a;
    }
    private void SetupLayers()
    {
        fall.wrapMode = WrapMode.Loop;
        idle.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        death.wrapMode = WrapMode.ClampForever;
        jumphit.wrapMode = hit.wrapMode = land.wrapMode = jump.wrapMode = WrapMode.Clamp;
        hit.layer = land.layer = jump.layer = 1;
        jumphit.layer = 2;
    }
    public override void  Update()
    {
        base.Update();
        UpdateAnimations();
        UpdateOther();
        UpdateMove();
        UpdateAtack();
    }
    private void UpdateOther()
    {
        if (life <= 0)
        {
            Die();
            return;
        }

        run.speed = 1.5f;
        land.speed = 1.4f;
        bool jumpbtn = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space);
        if (jumpbtn && (ground || (!scndJump && _Game.powerType == PowerType.doubleJump)))
        {
            if (!ground)
                scndJump = true;
            rigidbody.AddForce(Vector3.up * jumpPower * (_Game.powerType == PowerType.HighJump ? 1.5f : 1));
        }

        foreach (var a in hands)
            foreach (Transform b in a)
                b.gameObject.active = hit.enabled;

        foreach (var a in legs)
            foreach (Transform b in a)
                b.gameObject.active = jumphit.enabled;
        
    }
    private void UpdateAtack()
    {
        if (attackbtn)
        {
            Attack();
        }
    }
    public override void Damage()
    {
        
        
        life--;
        base.Damage();
    }
    public void Attack()
    {
        if (ground)
            an.CrossFade(hit.name);
        else
            an.CrossFade(jumphit.name);

        foreach (Barrel br in trigger.triggers.Where(a => a is Barrel))
            if (br != null)
                br.Hit();
        foreach (Raccoon r in trigger.triggers.Where(a => a is Raccoon))
            r.Damage();

        
    }
    public override void UpdateAnimations()
    {
        //if (isGrounded) groundtime += Time.deltaTime;
        //else
        //    groundtime = 0;
        
        var l = rigidbody.velocity;
        //Debug.Log(l);
        l.y = 0;
        if (l.magnitude > .5f)
            an.CrossFade(run.name);
        else
            an.CrossFade(idle.name);

        if(!ground)
            an.CrossFade(fall.name);

        base.UpdateAnimations();
    }
    private void UpdateMove()
    {
        
        var keydir = _Cam.rot * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        keydir.y = 0;
        keydir = keydir.normalized;
        var atackdir = keydir;
        if (attackbtn)
            atackdir = _Cam.rot * Vector3.forward;
        atackdir.y = 0;

        if (keydir != Vector3.zero || attackbtn)
            rot = Quaternion.LookRotation(atackdir);

        var move = Vector3.zero;
        move += keydir * Time.deltaTime * speed * (ground ? 1 : .6f);
     
        var rv = rigidbody.velocity;

        rv.y = 0;
        if (!land.enabled)
            rigidbody.AddForce((move - rv)*10);
    }
    void OnCollisionExit(Collision col)
    {
        foreach (var a in col.contacts)
            if ((a.point - pos).y < groundy)
                ground = false;
        
    }
    void OnCollisionStay(Collision col)
    {
        foreach (var a in col.contacts)
            if ((a.point - pos).y < groundy)
                ground = true;
    }
    void OnCollisionEnter(Collision col)
    {
        if (Mathf.Abs(col.impactForceSum.y) > 8)
            an.CrossFade(land.name);            

        scndJump = false;
    }
    public bool attackbtn { get { return Input.GetMouseButton(0) && !hit.enabled && !jumphit.enabled; } }
}
