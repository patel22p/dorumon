using UnityEngine;
using System.Collections;
public class Cam : Base
{


    
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;

    float x = 0.0f;
    float y = 0.0f;
    

    void Awake()
    {
        _Cam = this;
    }
    MotionBlur blur;
    public Camera cam;
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        
        blur = GetComponentInChildren<MotionBlur>();
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }
    
    float blurtime;
    void FixedUpdate()
    {
        blurtime += Time.fixedDeltaTime;
        if (blurtime>.1f && _Loader.fps > 40)
        {
            blurtime -= .1f;
            blur.blurAmount = Vector3.Distance(oldpos, transform.position) / 15;            
            oldpos = transform.position;
        }
    }
    void LateUpdate()
    {

        cam.fieldOfView = _options.fieldof;
        xoffset = _options.xofset;
        yoffset = _options.yofset;
        //transform.Find("pointer").rotation = Quaternion.LookRotation(this.transform.position- Find<Tower>().transform.position);

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) lockCursor = !lockCursor;

        if (lockCursor)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit, 90);
        if (_localiplayer == null || _localiplayer.dead)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection.Normalize();
            transform.rotation = Quaternion.Euler(y, x, 0);
            transform.position += moveDirection * Time.deltaTime * 40;
        }
        else
        {
            //if (_localiplayer is CarController)
            //    x = ClampAngle(x, yMinLimit, yMaxLimit, 30);
            //bool car = _localiplayer is CarController;
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 pos = _localiplayer.CamPos.transform.position;
            Vector3 pos2 = rotation * new Vector3(0.0f, 0.0f, -xoffset) + pos;
            pos2.y += yoffset;
            transform.position = pos2;
            transform.rotation = rotation;
        }

        transform.Find("MainCam").localPosition = new Vector3(Random.Range(-ran, ran), Random.Range(-ran, ran), Random.Range(-ran, ran));
        ran -= .1f;
        if (ran < 0) ran = 0;
    }
    public float xoffset =2;
    public float yoffset=3;
    Vector3 oldpos;
    internal float ran;
    public static float ClampAngle(float angle, float min, float max, float clamp)
    {
        if (angle < -clamp)
            angle = -clamp;
        if (angle > clamp)
            angle = clamp;
        return Mathf.Clamp(angle, min, max);
    }


}