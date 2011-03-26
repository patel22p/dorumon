using UnityEngine;
using System.Collections;

public class Cam : bs {

    Camera cam;
    //[FindTransform]
    public Transform Place;
    void Start()
    {
        cam = Camera.main;
        cam.transform.parent = Place;
        cam.transform.position = Place.position;
        cam.transform.rotation = Place.rotation;
        cam.GetComponent<GUILayer>().enabled = true;
    }

    Vector3 cursorpos;
    Vector3 fakeCursor;
    Vector3 velocity;
    
    //[FindTransform]
    public Animation CamFadeAnim;
    float camFade;
    float fCamFade;
    public bool secondMode;
    public Quaternion oldrot;
    public Quaternion frot;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            secondMode = !secondMode;
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;

        if (Screen.lockCursor)
        {
            Vector3 plCur = _Cursor.pos - _Player.pos;
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
            _Cursor.pos = _Player.pos + cursorpos;
            if (fCamFade == 0)
                fakeCursor = (_Player.pos + _Cursor.pos) / 2;                 
            else
                fakeCursor = _Player.pos; 
            pos = Vector3.SmoothDamp(pos, fakeCursor, ref velocity, .1f);
        }
        camFade = (Mathf.Lerp(camFade, fCamFade, 1f / 30f));
        //camFade = (camFade * 30 + fCamFade) / 31;

        
        var st = CamFadeAnim["CamAnim"];
        st.time = camFade;
        st.enabled = true;
        CamFadeAnim.Sample();
        st.enabled = false;
        
        fCamFade = Mathf.Clamp(fCamFade + Input.GetAxis("Mouse ScrollWheel"), 0, st.length);
        
    }
}
