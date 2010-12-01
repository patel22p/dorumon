using UnityEngine;
using System.Collections;

public class Pick : Base
{
    void OnCollisionStay(Collision other)
    {
        //Debug.Log("pick Hit");
        //this.transform.parent = other.transform;
        //Destroy(rigidbody);        
        //Destroy(this);
    }
}
