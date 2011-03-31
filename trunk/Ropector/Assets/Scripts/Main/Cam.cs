using System;
using UnityEngine;

public class Cam : bs
{
    
    
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
    
    public bool disableTips;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Screen.lockCursor = !Screen.lockCursor;
     
    }
    float vel;
    float fake;
    float camoffset = 30;
    public void FixedUpdate()
    {

        camoffset += Input.GetAxis("Mouse ScrollWheel") * -20;
        camoffset = Mathf.Min(Mathf.Max(camoffset, 30), 200);

        fake = Mathf.SmoothDamp(fake, Player.rigidbody.velocity.sqrMagnitude, ref vel, 0.95f);
        var pp = player.pos;
        pos = new Vector3(pp.x, pp.y + 10, -camoffset - Mathf.Sqrt(fake));
        cam.transform.LookAt(player.pos);
        
    }
    
}