using UnityEngine;
using System.Collections;

public class Cam : Bs {
    internal Camera cam;
    public override void Awake()
    {
        cam = GetComponentInChildren<Camera>();
    }

}
 