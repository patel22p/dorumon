using System.Linq;
using UnityEngine;
using System.Collections;

public class Player : Bs {
    
    public Bs model;
    public Transform Hands;
    public Transform UpperBone;
    public Cam Cam;
    public Transform CamRnd;
    public GameObject MuzzleFlash;
    public GameObject MuzzleFlash2;
    public GameObject BulletHolePrefab;    
    public GameObject sparks;
    public Texture[] BulletHoleTextures;
    public Texture[] MuzzleFlashTextures;
    private AudioSource aus;
    private CharacterController controller;
    Vector3 move;
    int left = -65, right = 95;
    public float shootTime = .05f;
    public float shootBump = 2;
    float lastShoot;
    
    
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
    public float slowDown = 1;
    
    public float yvel;
    private float grounded;

    public override void Awake()
    {
        Debug.Log("Player Awake");
        if (!IsMine)
        {
            //Cam.Active(false);
            SetLayer(LayerMask.NameToLayer("CamInvisible"), LayerMask.NameToLayer("Default"));
            SetLayer(LayerMask.NameToLayer("EditorInvisible"), LayerMask.NameToLayer("CamInvisible"));
            ActiveCamera(false);
            name = "Player-Remote";
        }
        else
            name = "Player";        
    }
    void Start () {
        InitAnimations();
        aus = GetComponent<AudioSource>(); 
        controller = GetComponent<CharacterController>();        
	}
    private void InitAnimations()
    {
        run.wrapMode = WrapMode.Loop;
        jump.wrapMode = WrapMode.Once;
        handsRun.wrapMode = WrapMode.Loop;
        handsIdle.wrapMode = WrapMode.Loop;
        handsDraw.layer = 1;
        foreach (var a in handsShoot)
        {
            a.layer = 1;
            a.speed = 2;
        }
        handsAn.Play(handsDraw.name);

        foreach (var b in blends)
        {
            b.AddMixingTransform(UpperBone);
            b.layer = 2;
        }
     
        blends[5].layer = 1;        
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
        if (isGrounded)
        {
            if (jump.weight > 0)
                jump.weight *= .86f;            
        }
        slowDown = Mathf.Lerp(slowDown, 1, Time.deltaTime * 2);
    }
    [RPC]
    private void RpcJump()
    {
        customAnim.Play("jumpCustom");
        Fade(jump);
        vel *= 1.2f;
    }
    
    private void UpdateRotation()
    {        
        Vector3 CamxModely = clampAngles(new Vector3(Cam.rotx, model.lroty, 0));
        var numpad = new Vector3(
            (Mathf.Clamp(CamxModely.y / -left, -1, 0) + Mathf.Clamp(CamxModely.y / right, 0, 1)),
            -(Mathf.Clamp(CamxModely.x / 45, -1, 0) + Mathf.Clamp(CamxModely.x / 45, 0, 1)));
        bool shooting = Time.time - lastShoot < shootTime * 2;
        CamRnd.localRotation = Quaternion.Slerp(CamRnd.localRotation, Quaternion.identity, Time.deltaTime * 5);
        foreach (var b in blends)
        {
            if (shooting)
                b.speed = 1;
            else
            {
                b.speed = 0;
                b.time = 0;
            }
        }
        SetWeight(blends[5], 1);
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
    private void UpdateMove()
    {

        if (vel.magnitude > .1f)
        {
            var lookRot = Quaternion.LookRotation(vel);
            float deltaAngle = Mathf.DeltaAngle(lookRot.eulerAngles.y, rot.eulerAngles.y);
            if (Mathf.Abs(deltaAngle) > 110)
            {
                model.rot = Quaternion.LookRotation(vel * -1);
                run.speed = -2f * (vel.magnitude / 5);
            }
            else
            {
                model.rot = Quaternion.LookRotation(vel);
                run.speed = 2f * (vel.magnitude / 5);
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
    private void UpdateInput()
    {
        if (IsMine)
        {
            move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            Vector3 MouseDelta = Screen.lockCursor ? new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) : Vector3.zero;
            Cam.rotx = Mathf.Clamp(clampAngle(Cam.rotx) + MouseDelta.x, -85, 85);
            if (Input.GetKey(KeyCode.LeftShift))
                Cam.rotx = 0;
            roty += MouseDelta.y;
            model.lroty = Mathf.Clamp(clampAngle(model.lroty - MouseDelta.y), left, right);

            if (Input.GetKeyDown(KeyCode.Space))
                CallRPC(RpcJump, RPCMode.All);

            if (Input.GetMouseButton(0) && Time.time - lastShoot > shootTime)
                CallRPC(RpcShoot, RPCMode.All, Cam.rotx, roty);
        }
    }

    [RPC]
    private void RpcShoot(float rotx,float roty)
    {
        Cam.rotx = rotx;
        this.roty = roty;
        lastShoot = Time.time;
        var a = handsShoot.Random();
        MuzzleFlash.renderer.sharedMaterial.SetTexture("_MainTex", MuzzleFlashTextures.Random());
        MuzzleFlash.animation.Play();
        MuzzleFlash2.GetComponentInChildren<Animation>().Play();
        Ray ray = Cam.cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        CamRnd.localRotation = Quaternion.Euler(CamRnd.localRotation.eulerAngles + Random.insideUnitSphere + Vector3.left * shootBump);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        RaycastHit h;
        if (Physics.Raycast(ray, out h, 1000, 1 << LayerMask.NameToLayer("Level")))
        {
            ((GameObject)Instantiate(sparks, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx; ;
            //BulletHolePrefab.renderer.material.SetTexture("_MainTex", BulletHoleTextures.Random());
            ((GameObject)Instantiate(BulletHolePrefab, h.point + h.normal * .1f, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;
        }
        a.time=0;
        handsAn.Play(a.name, PlayMode.StopSameLayer);        
    }
    public float speeadd;
    public float VelM;
    private void FixedUpdate()
    {
        if (move.magnitude > 0 && isGrounded)
        {
            speeadd = Mathf.Max(0, SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move)));
            VelM = vel.magnitude;
            //Debug.Log(controller.velocity.magnitude);
            //Debug.Log(Vector3.Distance(controller.velocity, rot * move * SpeedAdd.keys.Max(a=>a.value)));
            vel += rot * move * speeadd * slowDown;      
        }
        if(isGrounded)
            vel *= .83f;
        controller.SimpleMove(vel);        
        if(yvel>0f)
            controller.Move(new Vector3(0, yvel, 0));
        
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.time - grounded > .6f)
        {
            //slowDown = .4f;
            vel = Vector3.zero;
        }
        //if (controller.velocity.y < -1)
        //{
        //    slowDown = .1f;
        //    Debug.Log("hit" + controller.velocity.y);
        //}
        if (controller.velocity.y < -7)
        {
            customAnim.Play("Hit");
            slowDown = .1f;
        }
    }
    public Vector3 syncPos;
    public float syncRotx;
    public float syncRoty;
    public Vector3 syncVel;

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            syncPos = pos;
            syncRotx = Cam.rotx;
            syncRoty = roty;            
            syncVel = vel;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRotx);
        stream.Serialize(ref syncRoty);
        stream.Serialize(ref syncVel);
        if (stream.isReading)
        {
            pos = syncPos;
            Cam.rotx = syncRotx;
            roty = syncRoty;
            vel = syncVel;
        }

    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
        an.Blend(s.name, f, 0);        
    }
    AnimationState[] m_blends;
    AnimationState[] blends
    {
        get
        {
            if (m_blends == null)
                m_blends = new AnimationState[] { an["ak47_shoot_blend1"], an["ak47_shoot_blend1"], an["ak47_shoot_blend2"], an["ak47_shoot_blend3"], an["ak47_shoot_blend4"], an["ak47_shoot_blend5"], an["ak47_shoot_blend6"], an["ak47_shoot_blend7"], an["ak47_shoot_blend8"], an["ak47_shoot_blend9"] };
            return m_blends;
        }
    }


    AnimationState[] m_handsShoot;
    AnimationState[] handsShoot
    {
        get
        {
            if (m_handsShoot == null)
                m_handsShoot = new AnimationState[] { handsAn["shoot1"], handsAn["shoot2"], handsAn["shoot3"] };
            return m_handsShoot;
        }
    }


    private void ActiveCamera(bool enable)
    {
        transform.GetComponentInChildren<AudioListener>().enabled = enable;
        transform.GetComponentInChildren<Camera>().enabled = enable;
        transform.GetComponentInChildren<GUILayer>().enabled = enable;
        //transform.GetComponentInChildren<BloomAndLensFlares>().enabled = enable;
    }
    bool isGrounded { get { return Time.time - grounded < .1f; } }
}
