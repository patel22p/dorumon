using UnityEngine;
using System.Collections;

public class Lift : bs {

	// Use this for initialization
	void Start () {
        animation.wrapMode = WrapMode.Once;
	}
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null)
            animation.Play();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
