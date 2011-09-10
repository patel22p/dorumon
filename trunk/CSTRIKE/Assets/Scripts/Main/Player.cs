using System.Linq;
using UnityEngine;
using System.Collections;

public class Player : Bs {
    
    public Bs model;
    public Transform Hands;
    public Transform UpperBone;
    public Cam Cam;
    private AudioSource aus;
    private CharacterController controller;
    int left = -65, right = 95;
    
    private AudioClip[] pl_dirt;
    private Animation an { get { return model.animation; } }
    private AnimationState idle { get { return an["idle"]; } }
    private AnimationState run { get { return an["run"]; } }
    private AnimationState jump { get { return an["jump"]; } }

    private Animation handsAn { get { return Hands.animation; } }
    private AnimationState handsDraw { get { return handsAn["draw"]; } }
    private AnimationState handsRun { get { return handsAn["v_run"]; } }
    private AnimationState handsIdle { get { return handsAn["v_idle"]; } }
    public Animation customAnim { get { return animation; } }
    public AnimationCurve SpeedAdd;

    private Vector3 vel;
    public float SpeedFactor = 1;
    public float speedFade = .86f;
    public float yvel;
    private float grounded;
    private float maxSpeed;
    public override void Awake()
    {
        Debug.Log("Player Awake");
        if (networkView != null && !networkView.isMine)
            Cam.Active(false);
    }
    void Start () {
        InitAnimations();
        aus = GetComponent<AudioSource>(); 
        controller = GetComponent<CharacterController>();
        maxSpeed = SpeedAdd.keys.Select(a => a.value).Max();
	}
    private void InitAnimations()
    {
        run.wrapMode = WrapMode.Loop;
        jump.wrapMode = WrapMode.Once;
        handsRun.wrapMode = WrapMode.Loop;
        handsIdle.wrapMode = WrapMode.Loop;
        handsDraw.layer = 1;
        handsAn.Play(handsDraw.name);
        foreach (var b in blends)
        {
            b.AddMixingTransform(UpperBone);
            b.layer = 2;
            b.wrapMode = WrapMode.Loop;
            b.enabled = true;
        }
        blends[5].layer = 1;
        blends[5].weight = 1;
        jump.layer = 1;
    }
    private void Update()
    {
        UpdateJump();
        UpdateMove();
        UpdateRotation();
        UpdateInput();
	}
    private void UpdateJump()
    {
        if (controller.isGrounded) grounded = Time.time;
        if (Time.time - grounded < .1f)
        {
            if (jump.weight > 0)
                jump.weight *= .86f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                customAnim.Play("jumpCustom");
                Fade(jump);
            }
        }
    }
    
    private void UpdateRotation()
    {        
        Vector3 CamxModely = clampAngles(new Vector3(Cam.rotx, model.lroty, 0));
        var numpad = new Vector3(
            (Mathf.Clamp(CamxModely.y / -left, -1, 0) + Mathf.Clamp(CamxModely.y / right, 0, 1)),
            -(Mathf.Clamp(CamxModely.x / 45, -1, 0) + Mathf.Clamp(CamxModely.x / 45, 0, 1)));
        
        SetWeight(blends[6], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.left)));        
        SetWeight(blends[4], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.right)));
        SetWeight(blends[2], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.up)));
        SetWeight(blends[8], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.down)));
        SetWeight(blends[1], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.right + Vector3.up))));
        SetWeight(blends[7], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.right + Vector3.down))));
        SetWeight(blends[3], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.left + Vector3.up))));
        SetWeight(blends[9], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.left + Vector3.down))));        

        Debug.DrawRay(pos, rot * Vector3.forward);
        Debug.DrawRay(pos, model.rot * Vector3.forward, Color.red);
    }
    
    private void UpdateInput()
    {
        if (IsMine)
        {
            Vector3 MouseDelta = Screen.lockCursor ? new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) : Vector3.zero;
            Cam.rotx += MouseDelta.x;
            roty += MouseDelta.y;
            model.lroty = Mathf.Clamp(clampAngle(model.lroty - MouseDelta.y), left, right);
        }
    }
    
    private void UpdateMove()
    {

        if (move.magnitude > 0 && vel.magnitude > 0)
        {
            var lookRot = Quaternion.LookRotation(vel);
            float deltaAngle = Mathf.DeltaAngle(lookRot.eulerAngles.y, rot.eulerAngles.y);
            if (Mathf.Abs(deltaAngle) > 110)
            {
                model.rot = Quaternion.LookRotation(vel * -1);
                run.speed = -1.5f * (vel.magnitude / 5);
            }
            else
            {
                model.rot = Quaternion.LookRotation(vel);
                run.speed = 1.5f * (vel.magnitude / 5);
            }

            Fade(run);
            handsAn.CrossFade(handsRun.name);
        }
        else
        {
            Fade(idle);
            handsAn.CrossFade(handsIdle.name);
        }
        
    }
    private void FixedUpdate()
    {
        if (move.magnitude > 0)
        {
            var speeadd = SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move));
            vel += rot * move * speeadd * SpeedFactor;      
        }
        vel *= speedFade;
        controller.SimpleMove(vel);        
        if(yvel>0f)
            controller.Move(new Vector3(0, yvel, 0));
        SpeedFactor = Mathf.Lerp(SpeedFactor, 1, .05f);

    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {        
        if (controller.velocity.y < -5)
        {
            SpeedFactor = .1f;
            //Debug.Log(controller.velocity.y);
        }
        if (controller.velocity.y < -9)
            customAnim.Play("Hit");
    }
    private void WalkSound()
    {
        //if(vel.magnitude>1)        
        //    aus.PlayOneShot(pl_dirt[Random.Range(0, pl_dirt.Length - 1)]);
    }
    public void Fade(AnimationState s)
    {                
        an.CrossFade(s.name);
    }
    public void SetWeight(AnimationState s,float f)
    {
        s.weight = Mathf.Clamp(Mathf.Abs(f), 0, 1);
    }
    AnimationState[] m_blends;
    AnimationState[] blends
    {
        get
        {
            if (m_blends == null)
                m_blends = new AnimationState[] { an["ak47_blend1"], an["ak47_blend1"], an["ak47_blend2"], an["ak47_blend3"], an["ak47_blend4"], an["ak47_blend5"], an["ak47_blend6"], an["ak47_blend7"], an["ak47_blend8"], an["ak47_blend9"] };
            return m_blends;
        }
    }
    Vector3 move { get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized; } }
}
