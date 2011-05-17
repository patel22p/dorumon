using UnityEngine;
using System.Collections;

public class Cam : bs
{

    [FindTransform]
    public Transform Place;
    void Start()
    {
        var cam = Camera.main;
        cam.transform.parent = Place;
        cam.transform.position = Place.position;
        cam.transform.rotation = Place.rotation;
    }

    float rx, ry;
    //Vector3 fakepos;

    public float camNonSmooth = .90f;
    public float mousedelta = 1;
    public float Smothmousedelta = 2;
    void FixedUpdate()
    {
        mousedelta = Mathf.Clamp(mousedelta + Input.GetAxis("Mouse ScrollWheel"), .4f, 1.5f);
        Smothmousedelta = Mathf.Lerp(Smothmousedelta, mousedelta, .10f);
        transform.localScale = Vector3.one * Smothmousedelta;        
        if (Screen.lockCursor)
        {
            rx += Input.GetAxis("Mouse X");
            ry = Mathf.Clamp(ry - Input.GetAxis("Mouse Y"), -45, 70);
        }
        this.rot = Quaternion.Euler( new Vector2(ry, rx));
        pos = Vector3.Lerp(pos, _Player.pos, .1f);
        var v = Place.position - _Player.pos;
        RaycastHit h;
        if (Physics.Raycast(new Ray(_Player.pos, v.normalized), out h, v.magnitude - .5f, (1 << LayerMask.NameToLayer("Level"))))
            Camera.main.transform.position = h.point;
        else
            Camera.main.transform.localPosition = Vector3.zero;
    }
    //void FixedUpdate()
    //{
    //    this.rot = Quaternion.Euler(new Vector2(ry, rx));
    //    pos = Vector3.Lerp(pos, _Player.pos, .1f);
    //}

}
