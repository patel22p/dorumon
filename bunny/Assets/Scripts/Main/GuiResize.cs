using System;
using UnityEngine;
//[UnityEngine.ExecuteInEditMode]
public class GuiResize : bs
{
    public Vector3 oldScale;
    public void Start()
    {
        oldScale = transform.localScale;
    }
    public void Update()
    {
        var v = transform.localScale;
        v.x = oldScale.x * (16f / 9f) / ((float)Screen.width / Screen.height);        
        transform.localScale = v;

        name = v.x+" "+ ((float)Screen.width / Screen.height);
    }
}