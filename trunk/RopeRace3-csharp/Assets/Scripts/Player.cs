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
       
    }
    
}
