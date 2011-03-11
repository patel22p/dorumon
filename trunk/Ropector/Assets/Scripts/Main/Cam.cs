using System;
using UnityEngine;

public class Cam : bs
{
    [FindTransform(scene=true)]
    public bs cursor;
    public Vector3 fakeCursor;
    public bs cam;
    public bs player { get { return Game.iplayer; } }
    private Vector3 velocity;
    public void Start()
    {
        if(!Application.isEditor)
        Screen.lockCursor = true;
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Screen.lockCursor = !Screen.lockCursor;
        if (!Application.isEditor)
            if (Input.GetMouseButtonDown(0) && !Screen.lockCursor) Screen.lockCursor = true;
    }
    float vel;
    float fake;
    public void FixedUpdate()
    {
        fake = Mathf.SmoothDamp(fake, Player.rigidbody.velocity.sqrMagnitude, ref vel, 0.95f);
        var pp = player.pos;
        pos = new Vector3(pp.x, pp.y, -20 - Mathf.Sqrt(fake));        
        cam.transform.LookAt(fakeCursor);
        fakeCursor = Vector3.SmoothDamp(fakeCursor, cursor.pos, ref velocity, 0.5f);
        if (!Screen.lockCursor) return;
        Vector2 v = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        cursorpos += v;
        cursor.pos2 = player.pos2 + cursorpos;
    }
    Vector2 cursorpos;
}