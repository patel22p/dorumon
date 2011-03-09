using System;
using UnityEngine;

public class Cam : bs
{
    public Base cursor;
    public Vector3 faceCursor;
    public Base cam;
    public Base player { get { return Game.iplayer; } }
    private Vector3 velocity;
    public void Start()
    {
        if(!Application.isEditor)
        Screen.lockCursor = true;
    }
    
    public void Update()
    {

        if (Input.GetMouseButtonDown(1)) Screen.lockCursor = !Screen.lockCursor;
        if (!Application.isEditor)
            if (Input.GetMouseButtonDown(0) && !Screen.lockCursor) Screen.lockCursor = true;
    }
    public void FixedUpdate()
    {
        var pp = player.pos;
        var pc = cam.pos;
        pos = new Vector3(pp.x, pp.y, pc.z);

        cam.transform.LookAt(faceCursor);
        faceCursor = Vector3.SmoothDamp(faceCursor, cursor.pos, ref velocity, 0.5f);
        if (!Screen.lockCursor) return;
        Vector3 v = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        uasd = true;
        cursorpos += v;
        //Debug.Log(cursorpos);
        cursor.pos = player.pos + cursorpos;
    }
    Vector3 cursorpos;
    public bool uasd;
}