using UnityEngine;
using System.Collections;
public class Text3D : Base2
{
    public bool unrotate;
    
    void Update()
    {
        if(unrotate)
            this.transform.rotation = Quaternion.identity;
        else
            this.transform.rotation = Quaternion.LookRotation(this.transform.position - _Cam.transform.position);
    }



}
