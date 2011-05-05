using UnityEngine;
using System.Collections;

public class Cam : bs2
{
    Transform holder;
    public override void Awake()
    {
        holder = transform.Find("holder").transform;
        var m = Camera.main;
        m.transform.position = holder.position;
        m.transform.rotation = holder.rotation;
        m.transform.parent = holder;
        base.Awake();
    }
    void Start()
    {

    }

    Vector3 posf;
    float camoffset = 1;
    void Update()
    {

        Vector3 v = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        posf += v;
        pos = Vector3.Lerp(pos, posf, .97f);
        Camera.main.orthographicSize = camoffset * 10;
        camoffset = Mathf.Clamp(camoffset - Input.GetAxis("Mouse ScrollWheel"), .1f, 4f);
        this.transform.localScale = Vector3.one * camoffset;
    }
}
