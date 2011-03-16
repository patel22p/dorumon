using UnityEngine;
using System.Collections;
using doru;

public class Player : bs {

    internal TimerA timer = new TimerA();
    public Wall Trigger;
    public RopeEnd[] ropes = new RopeEnd[2];
    //public Menu GameGui;
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
        if (Game.prestartTm > 0 && !_Loader.debug) return;
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
    
    private void UpdatePlayer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        if (Input.GetKeyUp(KeyCode.Space))
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        if (!Screen.lockCursor) return;
        
        if (Input.GetMouseButtonDown(0))
            this.ropes[0].MouseClick();
        if (Input.GetMouseButtonDown(1))
            this.ropes[1].MouseClick();
        
        var mv = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        var controller = rigidbody;
        var spf = 1f;
        if (Trigger != null && Trigger.SpeedFactor != 0)
            spf = Trigger.SpeedFactor;
        controller.AddForce(mv * rigidbody.mass * 3); //add force /magnitde
        controller.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 3 * spf);


    }
    public Base cursor { get { return Cam.cursor; } }
    
} 
