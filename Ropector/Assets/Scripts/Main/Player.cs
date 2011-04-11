using UnityEngine;
using System.Collections;
using doru;

public class Player : bs {

    internal TimerA timer = new TimerA();
    public Wall Trigger;
    public RopeEnd[] ropes = new RopeEnd[2];
    public int scores;
    public override void Awake()
    {
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
        rigidbody.maxAngularVelocity = 30;

    }
    void Update()
    {        
        
        if (Game.prestartTm > 0 && !debug) return;
        UpdateCars();
        UpdatePlayer();
        timer.Update();        
    }
    void UpdateCars()
    {
        foreach (var c in Game.cars)
        {
            if (Vector3.Distance(this.pos, c.pos) < 3 && Input.GetKeyDown(KeyCode.F))
            {
                c.EnterCar(true, this);
                break;
            }
        }
    }
    void FixedUpdate()
    {
        rigidbody.AddForce(new Vector3(0, -10, 0));
    }
    void OnCollisionStay(Collision col)
    {
        //Debug.Log(col.rigidbody.velocity);
        
        //var c = col.gameObject.GetComponent<Animated>();
        
        //if (c != null)
        //{
        //    Debug.Log(c.vel);
        //    rigidbody.transform.position += c.vel ;
        //    this.rigidbody.AddForceAtPosition(c.vel * 100, col.contacts[0].point);

        //}
    }
    private void UpdatePlayer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        if (Input.GetKeyUp(KeyCode.Space))
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        if (!Screen.lockCursor) return;
        
        if (Input.GetMouseButtonDown(0))
            this.ropes[0].MouseDown();

        if (Input.GetMouseButtonUp(0))
            this.ropes[0].MouseUp();

        if (Input.GetMouseButtonDown(1))
            this.ropes[1].MouseDown();
        if (Input.GetMouseButtonUp(1))
            this.ropes[1].MouseUp();
        
        var mv = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        var controller = rigidbody;
        var spf = 1f;
        if (Trigger != null && Trigger.SpeedFactor != 0)
            spf = Trigger.SpeedFactor;
        controller.AddForce(mv * rigidbody.mass * 8); //add force /magnitde
        controller.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 3 * spf);


    }
    public Base cursor { get { return Cam.cursor; } }
} 
