using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Player : bs
{
    [FindTransform]
    public Trigger trigger;

    [FindTransform]
    public GameObject model;
    public Animation an { get { return model.animation; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState jump { get { return an["jump"]; } } //todo
    AnimationState fll { get { return an["midair"]; } }
    AnimationState land { get { return an["landing"]; } }
    AnimationState hit { get { return an["pawhit1"]; } }
    AnimationState jumphit { get { return an["aerialattack1"]; } }
    
    
    public List<Transform> hands = new List<Transform>();
    public List<Transform> legs = new List<Transform>();
    

    public Transform HitEffectTrail;
    void Start()
    {

        fll.wrapMode = idle.wrapMode = run.wrapMode = WrapMode.Loop;
        jumphit.wrapMode = hit.wrapMode = land.wrapMode = jump.wrapMode = WrapMode.Clamp;
        hit.layer = land.layer = fll.layer = jump.layer = 1;        
        jumphit.layer = 2;

        foreach (var a in legs)
            ((Transform)Instantiate(HitEffectTrail, a.transform.position, a.transform.rotation)).parent = a;

    }
    Vector3 vel;

    [FindTransform(self = true)]
    public CharacterController controller;
    public bool scndJump;
    void Update()
    {
        


        foreach (var a in legs)
            foreach (Transform b in a)
                b.gameObject.active = jumphit.enabled;

        run.speed = 1.5f;
        land.speed = 1.4f;
        bool attackbtn = Input.GetMouseButton(0) && !hit.enabled;
        bool jumpbtn = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space);
        AnimationsUpdate();
        if (scndJump && controller.isGrounded)
            scndJump = false;

        if (controller.isGrounded)
        {
            vel *= .86f;
            if (fll.enabled)
                fll.enabled = false;
        }

        if (jumpbtn && (controller.isGrounded || (!scndJump && _Game.powerType == PowerType.doubleJump)))
        {
            if (!controller.isGrounded)
                scndJump = true;
            vel = Vector3.up * 7f * (_Game.powerType == PowerType.HighJump ? 1.5f : 1);
        }

        MoveUpdate(attackbtn);
        AtackUpdate(attackbtn);
    }

    private void AtackUpdate(bool attackbtn)
    {
        if (attackbtn)
        {
            if (controller.isGrounded)
                an.CrossFade(hit.name);
            else
                an.CrossFade(jumphit.name);

            foreach (Barrel br in trigger.colliders.Where(a => a is Barrel))
                if (br != null)
                    br.Hit();
        }
    }

    private void AnimationsUpdate()
    {
        var l = controller.velocity.normalized;
        l.y = 0;
        if (l != Vector3.zero)
            an.CrossFade(run.name);
        else
            an.CrossFade(idle.name);

        if (vel.y > .5f)
            an.CrossFade(fll.name);
    }

    private void MoveUpdate(bool attackbtn)
    {
        var move = Vector3.zero;
        var keydir = _Cam.rot * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        var atackdir = keydir;
        if (attackbtn)
            atackdir = _Cam.rot * Vector3.forward;
        atackdir.y = keydir.y = 0;

        if (keydir != Vector3.zero || attackbtn)
            rot = Quaternion.LookRotation(atackdir);

        move += keydir * Time.deltaTime * 6;
        move += vel * Time.deltaTime;
        vel += Physics.gravity * Time.deltaTime;
        if (!land.enabled)
            controller.Move(move);
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(controller.velocity.y) > 8)
        {
            an.CrossFade(land.name);
        }
    }

}
