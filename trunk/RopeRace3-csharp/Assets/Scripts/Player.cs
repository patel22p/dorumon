using System.Linq;
using doru;
using UnityEngine;
using System.Collections;

public class Player : Bs {

	void Start () {
	
	}
    
    public float torq = 1;
    public float MaxAngl=100;
	void FixedUpdate () {
        rigidbody.maxAngularVelocity = MaxAngl;
        if (Input.GetKey(KeyCode.D))
            this.rigidbody.AddRelativeTorque(-Vector3.forward * torq);
        if (Input.GetKey(KeyCode.A))
            this.rigidbody.AddRelativeTorque(Vector3.forward * torq);
    }
    
}
