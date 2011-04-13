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

        var v = Place.position - _Player.pos;
        RaycastHit h;
        if (Physics.Raycast(new Ray(_Player.pos, v.normalized), out h, v.magnitude - .5f, ~(1 << LayerMask.NameToLayer("Player"))))
            Camera.main.transform.position = h.point;
        else
            Camera.main.transform.localPosition = Vector3.zero;
	}
  

}
