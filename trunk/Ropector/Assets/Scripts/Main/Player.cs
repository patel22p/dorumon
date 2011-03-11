using UnityEngine;
using System.Collections;
using doru;

public class Player : bs {
         
    public TimerA timer = new TimerA();
    public RopeEnd[] ropes;

    //public Menu GameGui;
    public int scores;
    void Start()
    {
        foreach (var r in ropes)
            Game.alwaysUpdate.Add(r);
        ropes[0].renderer.material.color = Color.blue;
        ropes[1].renderer.material.color = Color.red;
        rigidbody.maxAngularVelocity = 30;
    }
    public override void InitValues()
    {        
        base.InitValues();
        ropes[0] = GameObject.Find("RopeEnd").GetComponent<RopeEnd>();
        ropes[1] = GameObject.Find("RopeEnd2").GetComponent<RopeEnd>();
    }
    void Update()
    {        
        GameGui.info.text = ("streching:" );        
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
        if (!Screen.lockCursor) return;
        if (Input.GetKey(KeyCode.Space))
        {
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (Input.GetMouseButtonDown(0))
            this.ropes[0].MouseClick();
        if (Input.GetMouseButtonDown(1))
            this.ropes[1].MouseClick();
        
        var mv = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        var controller = rigidbody;

        controller.AddForce(mv * rigidbody.mass * 3); //add force /magnitde
        controller.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 3);
    }


    public Base cursor { get { return Cam.cursor; } }
    
} 
