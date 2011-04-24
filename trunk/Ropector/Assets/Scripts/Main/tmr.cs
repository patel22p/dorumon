using UnityEngine;
using System.Collections;

public class tmr : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
            Application.LoadLevel(Application.loadedLevel + 1);
	}
}
