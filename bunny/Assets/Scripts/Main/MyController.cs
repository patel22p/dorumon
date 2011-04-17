using UnityEngine;
using System.Collections;

public class Controller : bs{

	void Start () {
	
	}
	
	void Update () {
	    
	}
    void FixedUpdate()
    {
        var keydir = _Cam.rot * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

    }
}
