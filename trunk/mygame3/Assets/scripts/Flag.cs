using UnityEngine;
using System.Collections;

public class Flag : MonoBehaviour {

    public bool red;
	void Start () {
        renderer.material.color = red ? Color.red : Color.blue;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
