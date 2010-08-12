using UnityEngine;
using System.Collections;

public class Text3D : Base2{

    
    void Start()
    {
        
    }
	
    void Update()
    {

        this.transform.position = transform.parent.position;
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - Find<Cam>().transform.position);
    }
}
