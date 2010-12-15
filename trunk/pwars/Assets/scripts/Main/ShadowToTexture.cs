using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowToTexture : Base
{
    public Projector p;
    public RenderTexture t;
    public override void Init()
    {

        var c = camera;
        
        c.farClipPlane = p.farClipPlane;
        //c.fieldOfView = p.fieldOfView;
        
        c.orthographic = p.orthographic;
        c.orthographicSize = p.orthographicSize;
    }
    protected override void Start()
    {
        t = new RenderTexture(256, 256, 100);
        camera.targetTexture = t;
        p.material.SetTexture("_ShadowTex", t);
        //t = new RenderTexture(256, 256, 0);
        Destroy(camera, 1);
    }    
}
