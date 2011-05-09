using UnityEngine;
using System.Collections;

public class ECam : bs
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

    //Vector3 posf;
    float camoffset = 1;
    void Update()
    {

        Vector3 offset = new Vector3();
        if (Input.GetKey(KeyCode.A))
            offset.x -= Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            offset.x += Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            offset.y += Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            offset.y -= Time.deltaTime;

        pos += offset*5;
        //pos = Vector3.Lerp(pos, posf, .97f);
        Camera.main.orthographicSize = camoffset * 10;
        camoffset = Mathf.Clamp(camoffset - Input.GetAxis("Mouse ScrollWheel"), .1f, 4f);
        this.transform.localScale = Vector3.one * camoffset;
    }
}
