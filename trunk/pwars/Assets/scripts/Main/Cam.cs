using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;
public class Cam : bs
{
    Vector3 oldpos;
    float x = 0.0f;
    float y = 0.0f;
    TimerA ft = new TimerA();
    float damageblurtm;
    float xoffset = 2;
    float yoffset = 3;
    internal float exp;
    public TextMesh LevelText;
    public TextMesh ScoreText;
    [FindTransform]
    public TextMesh Levelcomplete;
    [FindTransform]
    public TextMesh ActionText;
    public GUITexture[] blood = new GUITexture[2];
    public void Hit()
    {
        GUITexture g = blood[Random.Range(0, blood.Length)];
        g.transform.rotation = Random.rotation;
        g.color = new Color(1, 1, 1, 1f);
        damageblurtm = 3;
    }
    public override void Awake()
    {
        
        base.Awake();
    }
    public void Start()
    {
        camera.GetComponent<GUILayer>().enabled = true;
        camera.transform.localPosition= Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        onEffect();
    }
    
    public override void Init()
    {
        camera = GetComponentInChildren<Camera>();
        MotionBlur = GetComponentInChildren<MotionBlur>();
        LevelText = transform.Find("LevelText").GetComponent<TextMesh>();
        ScoreText = transform.Find("ScoreText").GetComponent<TextMesh>();
        Vingetting = (MonoBehaviour)camera.GetComponent("Vignetting");
        DepthOfField = (MonoBehaviour)camera.GetComponent("DepthOfField");
        bloomAndFlares = (MonoBehaviour)camera.GetComponent("BloomAndFlares");
        ambientsmoke = transform.Find("ambientsmoke");
        ssao = GetComponentInChildren<SSAOEffect>();
        contranStretch = GetComponentInChildren<ContrastStretchEffect>();
        base.Init();
    }
    public new Camera camera;
    public Transform ambientsmoke;
    public MotionBlur MotionBlur;
    public MonoBehaviour Vingetting;
    public MonoBehaviour DepthOfField;
    public SSAOEffect ssao;
    public MonoBehaviour bloomAndFlares;
    public ContrastStretchEffect contranStretch;

    public void onEffect()
    {
        ambientsmoke.particleEmitter.emit = _SettingsWindow.AtmoSphere;
        if (ssao.enabled != _SettingsWindow.Sao) { ssao.enabled = _SettingsWindow.Sao; Debug.Log("sao settings" + ssao.enabled); }
        if (MotionBlur.enabled != _SettingsWindow.MotionBlur) { MotionBlur.enabled = _SettingsWindow.MotionBlur; Debug.Log("blur settings" + MotionBlur.enabled); }
        if (bloomAndFlares.enabled != _SettingsWindow.BloomAndFlares) { bloomAndFlares.enabled = _SettingsWindow.BloomAndFlares; Debug.Log("blom and flares" + bloomAndFlares.enabled); }
        if (contranStretch.enabled != _SettingsWindow.Contrast) { contranStretch.enabled = _SettingsWindow.Contrast; Debug.Log("Contrast stretch " + contranStretch.enabled); }
        if (_SettingsWindow.iGraphicQuality != -1 && (QualityLevel)_SettingsWindow.iGraphicQuality != QualitySettings.currentLevel)
        {            
            QualitySettings.currentLevel = (QualityLevel)_SettingsWindow.iGraphicQuality;
            print("graphics quality changed" + QualitySettings.currentLevel);
        }
        if (!_SettingsWindow.Shadows)
        {
            Debug.Log("shadows are dissabled" + _SettingsWindow.Shadows);
            foreach (Light l in GameObject.FindObjectsOfTypeIncludingAssets(typeof(Light)))
                l.shadows = LightShadows.None;
        }
        if (_SettingsWindow.iRenderSettings != -1 && _SettingsWindow.iRenderSettings != (int)RenderingPath.UsePlayerSettings)
        {
            Debug.Log("render type settings chagned" + (RenderingPath)_SettingsWindow.iRenderSettings);
            foreach (Camera c in GameObject.FindObjectsOfTypeIncludingAssets(typeof(Camera)))
                c.renderingPath = (RenderingPath)_SettingsWindow.iRenderSettings;
        }
    }
    
    void FixedUpdate()
    {
        if (ft.TimeElapsed(120))
        {
            MotionBlur.enabled = true;
            MotionBlur.blurAmount = Vector3.Distance(oldpos, transform.position) / 8;
            oldpos = transform.position;
        }
        ft.Update();

        CamUpdate();
    }
    public bool topdown = true;
    void Update()
    {
        if (DebugKey(KeyCode.J))
            Hit();

        damageblurtm -= Time.deltaTime * 4;
        if (damageblurtm > 0)
        {
            DepthOfField.enabled = true;
            damageBlur = damageblurtm;
        }
        else
            DepthOfField.enabled = false;
       
        foreach (GUITexture a in blood)
            if (a.guiTexture.color.a > 0)
                a.guiTexture.color -= new Color(0, 0, 0, Time.deltaTime * 1f);

        if (lockCursor)
        {
            x += Input.GetAxis("Mouse X") * 120 * .02f * _SettingsWindow.MouseX;
            y -= Input.GetAxis("Mouse Y") * 120 * .02f * _SettingsWindow.MouseY;
        }
        if (topdown)
        {
            y = 90;
        }

    }
    void CamUpdate()
    {
        if (_localPlayer == null) return;
        camera.fieldOfView = _SettingsWindow.Fieldof;
        xoffset = _SettingsWindow.Camx + 0.01f;

        yoffset = _SettingsWindow.Camy * (topdown ? 3 : 1) + 0.01f;

        y = ClampAngle(y, -90, 90, 90);
        Quaternion rot2 = Quaternion.Euler(y, x, 0);
        Vector3 pos2 = rot2 * new Vector3(0.0f, 0.0f, -xoffset) + _localPlayer.pos;
        pos2.y += yoffset;
        RaycastHit h;
        Vector3 plpos = _localPlayer.transform.position + Vector3.up * _SettingsWindow.Camy;
        Ray r = new Ray(plpos, pos2 - plpos);
        
        if (Physics.Raycast(r, out h, Vector3.Distance(plpos, pos2), 1 << LayerMask.NameToLayer("Level")))
            pos2 = h.point - r.direction.normalized;
        if (_SettingsWindow.CamSmooth == 0)
            pos = pos2;
        else
        {
            var a = Time.deltaTime * 5 * (2f - _SettingsWindow.CamSmooth);
            pos = ((pos2 * a) + (pos)) / (a + 1);
        }
        rot = rot2;
        camera.transform.localPosition = new Vector3(Random.Range(-exp, exp), Random.Range(-exp, exp), Random.Range(-exp, exp));
        exp -= .1f;
        if (exp < 0) exp = 0;
    }
    public static float ClampAngle(float angle, float min, float max, float clamp)
    {
        if (angle < -clamp)
            angle = -clamp;
        if (angle > clamp)
            angle = clamp;
        return Mathf.Clamp(angle, min, max);
    }
    public float damageBlur
    {
        get { return DepthOfField.GetValue<float>("blurSpread"); }
        set { DepthOfField.SetValue("blurSpread", value); }
    }

}
[System.Serializable]
public class Decal
{
    public string name;
    public float scale = 1;
    public override string ToString()
    {
        return name;
    }
    public GameObject mesh;
    public Material mat;
}