using UnityEngine;
using System.Collections;

public class Cam : bs {

    Camera cam;
    Vector3 cursorpos;
    Vector3 fakeCursor;
    Vector3 velocity;
    float camFade;
    float fCamFade;
    public bool secondMode;
    public Quaternion oldrot;
    public Quaternion frot;
    [FindTransform("CamAnim")]
    public Transform Place;
    [FindTransform("CamAnim")]
    public Animation CamFadeAnim;

    public override void Awake()
    {
        AddToNetwork();
        CamFadeAnim["CamFade"].enabled = false;
    }
    void Start()
    {
        Debug.Log("Cam Start");
        cam = Camera.main;
        cam.animation.Stop();
        cam.transform.parent = Place;
        pos = cam.transform.position;
        //rot = cam.transform.rotation;
        cam.transform.position = Place.position;
        cam.transform.rotation = Place.rotation;        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            secondMode = !secondMode;
        

        if (Screen.lockCursor)
        {
            Vector3 plCur = _Cursor.pos - _PlayerOwn.pos;
            plCur.y = 0;
            if (secondMode)
            {
                frot = Quaternion.LookRotation(plCur);
                oldrot =rot = Quaternion.Lerp(rot, frot, 1f / 15f);
            }
            else
                rot = oldrot;
            
            Vector3 v = rot * new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));            
            cursorpos = Vector3.ClampMagnitude(cursorpos + v, 10);
            _Cursor.pos = _PlayerOwn.pos + cursorpos;
            if (fCamFade == 0)
                fakeCursor = (_PlayerOwn.pos + _Cursor.pos) / 2;                 
            else
                fakeCursor = _PlayerOwn.pos; 
            pos = Vector3.SmoothDamp(pos, fakeCursor, ref velocity, .1f);
        }
        camFade = (Mathf.Lerp(camFade, fCamFade, 1f / 30f));
        //camFade = (camFade * 30 + fCamFade) / 31;

        var st = CamFadeAnim["CamFade"];
        st.time = camFade;
        
        st.enabled = true;
        CamFadeAnim.Sample();
        st.enabled = false;
        
        fCamFade = Mathf.Clamp(fCamFade + Input.GetAxis("Mouse ScrollWheel"), 0, st.length);
        
    }
}
