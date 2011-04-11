using UnityEngine;
using System.Collections;

public class Cam : bs {

    [FindTransform]
    public Transform Place;
	void Start () {
        var cam = Camera.main;
        cam.transform.parent = Place;
        cam.transform.position = Place.position;
        cam.transform.rotation = Place.rotation;        
	}
	
    float rx, ry;
	void LateUpdate () {
        if (Screen.lockCursor)
        {
            rx += Input.GetAxis("Mouse X");
            ry -= Input.GetAxis("Mouse Y");
        }
	    this.rot = Quaternion.Euler(new Vector2(ry,rx));
        pos = _Player.pos;
	}

}
