using UnityEngine;
using System.Collections;
using doru;

public class Player : bs {

    
    float tmRope;
 
    bool ropeEnabled = true;
    public bool scnd;
    public TimerA timer = new TimerA();
    
    [FindTransform(scene = true)]
    public RopeEnd RopeEnd;
    [FindTransform(scene= true)]
    public Transform ClothEnd;
    [FindTransform(scene = true)]
    public Transform ClothStart;
    public InteractiveCloth cloth;
    public Menu menu;
    public int scores;
    void Start()
    {
        rigidbody.maxAngularVelocity = 100;
    }
    void Update()
    {        
        menu.info.text = ("streching:" );
        UpdateRope();
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
    public override void InitValues()
    {
        //Screen.lockCursor = true;
        RopeFactor = 2f;
        ShootFactor = 12f;
        base.InitValues();
    }
    private void UpdatePlayer()
    {
        if (!Screen.lockCursor) return;
        var mv = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        var controller = rigidbody;

        if(rigidbody.velocity.magnitude<4)
            controller.AddForce(mv * rigidbody.mass * 3);
        //if(controller.velocity.magnitude<2)
        //    controller.velocity =
        //controller.MovePosition(transform.position + (mv * Time.deltaTime * 10));
        controller.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 3);
    }
    public float RopeFactor = 1;
    public float ShootFactor = 1;
    
    void UpdateRope()
    {
        tmRope -= Time.deltaTime;
        
        var v = RopeEnd.transform.position - transform.position;
        if (ropeEnabled)
            if (RopeEnd.isFlying)
                RopeEnd.rigidbody.AddForce(v * -1 * ShootFactor);
            else
            {
                this.rigidbody.AddForce(v * RopeFactor);
                var r = RopeEnd.transform.parent.GetComponentInParrent<Rigidbody>();
                if (r != null)
                {
                    r.AddForceAtPosition(-v, RopeEnd.transform.position);
                }
                    
            }


        if (Input.GetKey(KeyCode.Space))
        {
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (RopeEnd.gameObject.active)
        {
            ClothStart.transform.position = transform.position;
            ClothEnd.transform.position = RopeEnd.transform.position;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!ropeEnabled && tmRope < 0)
            {
                
                Debug.Log("RopeEnable");
                
                ClothEnd.transform.position = transform.position + Vector3.up * 1;
                cloth.transform.position = transform.position;
                ClothStart.transform.position = transform.position + Vector3.up * -1;                
                tmRope = 1;
                RopeEnd.oldpos = RopeEnd.transform.position = transform.position;                
                EnableRope(true);                                                
                var dir = Cam.cursor.transform.position - transform.position;
                dir = dir.normalized;
                RopeEnd.rigidbody.velocity = dir * 100;
                
            }
            else if (ropeEnabled) 
            {
                Debug.Log("RopeDisable");
                EnableRope(false);
                
            }
            
        }
    }
    public void EnableRope(bool value)
    {
        RopeEnd.enabled = RopeEnd.gameObject.active = value;       
        ropeEnabled = cloth.gameObject.active = value;
        RopeEnd.oldpos = null;
        if(!value)
            RopeEnd.Disable();
    
    }

    public Base cursor { get { return Cam.cursor; } }
    
} 
