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
    public TextMesh LevelText;
    public TextMesh ScoreText;
    protected override void Awake()
    {        
        blur = GetComponentInChildren<MotionBlur>();
        camera = GetComponentInChildren<Camera>();
        LevelText = transform.Find("LevelText").GetComponent<TextMesh>();
        ScoreText = transform.Find("ScoreText").GetComponent<TextMesh>();
        ambientsmoke =transform.Find("ambientsmoke");
        ssao = GetComponentInChildren<SSAOEffect>();
        xSpeed = 120;
        ySpeed = 120;
        yMinLimit = -90;
        yMaxLimit = 90;        
        base.Awake();
        
    }
    public new Camera camera;
    Transform ambientsmoke;
    MotionBlur blur;
    SSAOEffect ssao;
    public MonoBehaviour bloomAndFlares;
    
    public void onEffect()
    {
        
        ambientsmoke.gameObject.active = _SettingsWindow.AtmoSphere;
        ssao.enabled = _SettingsWindow.Sao;
        bloomAndFlares.enabled = _SettingsWindow.BloomAndFlares;
        if (!_SettingsWindow.Shadows)
            foreach (Light l in GameObject.FindObjectsOfType(typeof(Light)))
                l.shadows = LightShadows.None;
        if (_SettingsWindow.iRenderSettings != -1)
            foreach (Camera c in GameObject.FindObjectsOfType(typeof(Camera)))
                c.renderingPath = (RenderingPath)_SettingsWindow.iRenderSettings;
    }
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        onEffect();
    }
    float blurtime;
    void FixedUpdate()
    {
        blurtime += Time.fixedDeltaTime;
        if (blurtime>.1f && _GameWindow.fps > 40 && _SettingsWindow.MotionBlur)
        {
            blurtime -= .1f;
            blur.blurAmount = Vector3.Distance(oldpos, transform.position) / 15;            
            oldpos = transform.position;
        }
    }
    void LateUpdate()
    {        
        camera.fieldOfView = _SettingsWindow.Fieldof;
        xoffset = _SettingsWindow.Camx+0.01f;
        yoffset = _SettingsWindow.Camy + 0.01f;
        //transform.Find("pointer").rotation = Quaternion.LookRotation(this.transform.position- Find<Tower>().transform.position);

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
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 pos = _localiplayer.CamPos.transform.position;
            Vector3 pos2 = rotation * new Vector3(0.0f, 0.0f, -xoffset) + pos;
            pos2.y += yoffset;
            transform.position = pos2;
            transform.rotation = rotation;
        }

        transform.Find("MainCam").localPosition = new Vector3(Random.Range(-exp, exp), Random.Range(-exp, exp), Random.Range(-exp, exp));
        exp -= .1f;
        if (exp < 0) exp = 0;
    }
    public float xoffset =2;
    public float yoffset=3;
    Vector3 oldpos;
    public float exp;
    public static float ClampAngle(float angle, float min, float max, float clamp)
    {
        if (angle < -clamp)
            angle = -clamp;
        if (angle > clamp)
            angle = clamp;
        return Mathf.Clamp(angle, min, max);
    }


}