using UnityEngine;
using System.Collections;

public class Text3D : Base2
{
    void Update()
    {
        
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - _Cam.transform.position);
    }
}
