using UnityEngine;
using System.Collections;

public class Text3D : Base2
{
    void Update()
    {
        
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - Find<Cam>().transform.position);
    }
}
