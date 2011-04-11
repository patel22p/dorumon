using UnityEngine;
using System.Collections;

public class Player : bs {
    [FindTransform]
    public GameObject model;
    public Animation an { get { return model.animation; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    void Start()
    {
        idle.wrapMode = run.wrapMode = WrapMode.Loop;
    }
    Vector3 vel;

    [FindTransform(self = true)]
    public CharacterController controller;
    //void FixedUpdate()
    //{
    //    controller.Move(vel);

    //    vel *= .98f;
    //}
    void Update()
    { 
        //var mv = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //mv = _Cam.rot * mv.normalized * 5;

        controller.SimpleMove(Vector3.zero);
        var l = controller.velocity.normalized;
        l.y = 0;
        if (l != Vector3.zero)
        {
            rot = Quaternion.LookRotation(l);
            an.CrossFade(run.name);
            //run.speed = l.magnitude;
        }
        else
            an.CrossFade(idle.name);

        //an["idle"].speed = 2;
        //if (controller.isGrounded)
        //    vel = Vector3.zero;
        if (Input.GetMouseButtonDown(1) && controller.isGrounded)
            vel = Vector3.up * 20f;
        var move = Vector3.zero;
        move += _Cam.rot * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized * 8 * Time.deltaTime;
        move += vel * Time.deltaTime;
        vel += Physics.gravity * Time.deltaTime * 3;
        controller.Move(move);
    }
}
