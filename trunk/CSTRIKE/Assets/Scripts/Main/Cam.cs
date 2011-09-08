using UnityEngine;
using System.Collections;

public class Cam : Bs {

    public Camera cam;
	void Start () {
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
	}
}
