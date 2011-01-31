using UnityEngine;
using System.Collections;
public class Text3D : Base2
{
    public bool unrotate;
    void LateUpdate()
    {
        Unrotate();
    }
    private void Unrotate()
    {
        if (unrotate)
            this.transform.rotation = Quaternion.identity;
        else
            this.transform.rotation = Quaternion.LookRotation(this.transform.position - _Cam.transform.position);
    }
    void FixedUpdate()
    {
        Unrotate();
    }
}
