using System;
using UnityEngine;

public class Cam : bs
{
    [FindTransform(scene=true)]
    public bs cursor;
    public Vector3 fakeCursor;
    public Camera cam;
    public bs player { get { return Game.iplayer; } }
    private Vector3 velocity;
    public void Start()
    {
        if (!Application.isEditor)
            Screen.lockCursor = true;
        cam = Camera.main;
        cam.transform.parent = this.transform;
        cam.transform.position = cam.transform.parent.position;
        cam.transform.rotation = cam.transform.parent.rotation;
        cam.GetComponent<GUILayer>().enabled = true;
    }
    public override void InitValues()
    {        
        base.InitValues();
    }
    public bool disableTips;
    public void Update()
    {       
        if (disableTips != _MenuWindow.Disable_Tips)
        {
            disableTips = _MenuWindow.Disable_Tips;
            if (_MenuWindow.Disable_Tips)
                cam.cullingMask = ~(1 << LayerMask.NameToLayer("Tips"));
            else
                cam.cullingMask = ~0;
        }
        if (Input.GetKeyDown(KeyCode.Tab)) Screen.lockCursor = !Screen.lockCursor;

        Vector2 v = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * _MenuWindow.MouseSensivity;
        //if (v.magnitude < 20)
            cursorpos += v;
    }
    float vel;
    float fake;
    float camoffset= 30;
    public void FixedUpdate()
    {

        camoffset += Input.GetAxis("Mouse ScrollWheel") * -20;
        camoffset = Mathf.Min(Mathf.Max(camoffset, 30), 200);

        fake = Mathf.SmoothDamp(fake, Player.rigidbody.velocity.sqrMagnitude, ref vel, 0.95f);
        var pp = player.pos;
        pos = new Vector3(pp.x, pp.y + 10, -camoffset - Mathf.Sqrt(fake));
        cam.transform.LookAt(fakeCursor);
        fakeCursor = Vector3.SmoothDamp(fakeCursor, cursor.pos, ref velocity, 0.5f);
        if (!Screen.lockCursor) return;       
        
        cursor.pos2 = player.pos2 + cursorpos;
    }
    Vector2 cursorpos;
} 