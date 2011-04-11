using UnityEngine;
using System.Collections;
using doru;

public class Player : bs {

    internal TimerA timer = new TimerA();
    public Wall Trigger;
    public RopeEnd[] ropes = new RopeEnd[2];
    public int scores;
    public Vector3 veloticy;
    public CharacterController controller;
    public new Rigidbody rigidbody { set { } }
    public override void Awake()
    {
        
        controller = this.GetComponent<CharacterController>();
        ropes[0] = GameObject.Find("RopeEnd").GetComponent<RopeEnd>();
        ropes[1] = GameObject.Find("RopeEnd2").GetComponent<RopeEnd>();
        ropes[0].renderer.material.color = Color.blue;
        ropes[1].renderer.material.color = Color.red;
        base.Awake();
    }
    void Start()
    {
        
        foreach (var r in ropes)
            Game.alwaysUpdate.Add(r);
        //rigidbody.maxAngularVelocity = 30;

    }
    void Update()
    {        
        
        if (Game.prestartTm > 0 && !debug) return;
        UpdatePlayer();
        timer.Update();        
    }
   
    //void FixedUpdate()
    //{
    //    rigidbody.AddForce(new Vector3(0, -10, 0));
    //}

    private void UpdatePlayer()
    {
        
        //if (Input.GetKeyDown(KeyCode.Space))
        //    rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        //if (Input.GetKeyUp(KeyCode.Space))
        //    rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        if (!Screen.lockCursor) return;

        //if (Input.GetMouseButtonDown(0))
        //    this.ropes[0].MouseDown();
        //if (Input.GetMouseButtonUp(0))
        //    this.ropes[0].MouseUp();
        if (Input.GetMouseButtonDown(0))
            if (!ropes[0].ropedown)
                this.ropes[0].MouseDown();
            else
                this.ropes[0].MouseUp();

        if (Input.GetMouseButtonDown(1))
            if (!ropes[1].ropedown)
                this.ropes[1].MouseDown();
            else
                this.ropes[1].MouseUp();
        

        //if (Input.GetMouseButtonDown(1))
        //    this.ropes[1].MouseDown();
        //if (Input.GetMouseButtonUp(1))
        //    this.ropes[1].MouseUp();
        
        
        var controller = this.GetComponent<CharacterController>();
        var spf = 1f;

        if (Trigger != null && Trigger.SpeedFactor != 0)
            spf = Trigger.SpeedFactor;

        if (controller.isGrounded)
            veloticy *= .58f;
        else
            veloticy *= .95f;
        var mv = Vector3.zero;
        //if(controller.isGrounded)
        name = "Player" + veloticy;
        mv += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * 8 * Time.deltaTime;
        mv += veloticy * Time.deltaTime;
        veloticy += Physics.gravity * Time.deltaTime * 3;
        controller.Move(mv);
        //controller.Move(veloticy * Time.deltaTime);
        //controller.SimpleMove(mv * 8);
        //controller.AddForce(mv * rigidbody.mass * 20); //add force /magnitde
        //pos += mv;
        //controller.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 3 * spf);


    }
    public Base cursor { get { return Cam.cursor; } }
} 
