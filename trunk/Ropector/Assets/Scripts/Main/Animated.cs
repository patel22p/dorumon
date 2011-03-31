using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/Animated")]
public class Animated : bs {

	void Start () {
	
	}
    Vector3 old;
    public Vector3 vel;
    void FixedUpdate()
    {
        if (old != Vector3.zero)
            vel = pos - old;
        old = pos;
    }

	void Update () {
	
	}
}
