using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Player : Shared
{
    [FindTransform]
    public Trigger trigger;
    
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
    public bool scndJump;
    
    public override void Start()
    {
        _Game.shareds.Add(this);
        base.Start();
        
        fall.wrapMode = WrapMode.Loop;
        idle.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        jumphit.wrapMode = hit.wrapMode = land.wrapMode = jump.wrapMode = WrapMode.Clamp;
        
        hit.layer = land.layer = jump.layer = 1;
        jumphit.layer = 2;

        foreach (var a in legs.Union(hands))
            ((Transform)Instantiate(HitEffectTrail, a.transform.position, a.transform.rotation)).parent = a;
    }
    public override void  Update()
    {
        base.Update();
        HitEffect();
        bool attackbtn = UpdateOther();

        UpdateMove(attackbtn);
        UpdateAtack(attackbtn);
    }
    public bool isGrounded;// { get { return ground.colliders.Count > 1; } }
    private bool UpdateOther()
    {

        run.speed = 1.5f;
        land.speed = 1.4f;
        bool attackbtn = Input.GetMouseButton(0) && !hit.enabled;
        bool jumpbtn = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space);


        if (jumpbtn && (isGrounded || (!scndJump && _Game.powerType == PowerType.doubleJump)))
        {
            if (!isGrounded)
                scndJump = true;
            rigidbody.AddForce(Vector3.up * 400 * (_Game.powerType == PowerType.HighJump ? 1.5f : 1));
        }
        return attackbtn;
    }
    private void HitEffect()
    {
        foreach (var a in hands)
            foreach (Transform b in a)
                b.gameObject.active = hit.enabled;

        foreach (var a in legs)
            foreach (Transform b in a)
                b.gameObject.active = jumphit.enabled;
    }
    private void UpdateAtack(bool attackbtn)
    {
        if (attackbtn)
        {
            if (isGrounded)
                an.CrossFade(hit.name);
            else
                an.CrossFade(jumphit.name);

            foreach (Barrel br in trigger.colliders.Where(a => a is Barrel))
                if (br != null)
                    br.Hit();
            foreach (Raccoon r in trigger.colliders.Where(a => a is Raccoon))
                r.Hit();
        }
    }
    float groundtime;
    public override void UpdateAnimations()
    {
        if (isGrounded) groundtime += Time.deltaTime;
        else
            groundtime = 0;
        var l = rigidbody.velocity;
        l.y = 0;
        if (l.magnitude > 1f)
            an.CrossFade(run.name);
        else
            an.CrossFade(idle.name);

        if(groundtime<.05f)
            an.CrossFade(fall.name);

        base.UpdateAnimations();
    }
    private void UpdateMove(bool attackbtn)
    {
        rigidbody.WakeUp();
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
        move += keydir * Time.deltaTime * (isGrounded ? 250f : 100f);
     
        var rv = rigidbody.velocity;

        rv.y = 0;
        if (!land.enabled)
            rigidbody.AddForce((move - rv)*10);
    }
    float groundy = -.4f;
    void OnCollisionExit(Collision col)
    {
        foreach (var a in col.contacts)
            if ((a.point - pos).y < groundy)
                isGrounded = false;
        
    }
    void OnCollisionStay(Collision col)
    {

        foreach (var a in col.contacts)
            if ((a.point - pos).y < groundy)
                isGrounded = true;
    }
    void OnCollisionEnter(Collision col)
    {
        if (Mathf.Abs(col.impactForceSum.y) > 8)
            an.CrossFade(land.name);            

        scndJump = false;
    }
}
