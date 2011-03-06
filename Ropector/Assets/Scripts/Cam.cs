using System;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public GameObject cursor;
    public Vector3 faceCursor;
    public GameObject cam;
    public GameObject player;
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
        var pp = player.transform.position;
        var pc = cam.transform.position;
        cam.transform.position = new Vector3(pp.x, pp.y, pc.z);

        cam.transform.LookAt(faceCursor);
        faceCursor = Vector3.SmoothDamp(faceCursor, cursor.transform.position, ref velocity, 0.5f);
        if (!Screen.lockCursor) return;        
        Vector3 v= new Vector3(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"),0);
        uasd = true;
        cursor.transform.position+= v;        
    }
    public bool uasd;
}