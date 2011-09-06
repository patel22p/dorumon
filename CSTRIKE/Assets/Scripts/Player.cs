using UnityEngine;
using System.Collections;

public class Player : Bs {
    
    public GameObject model;
    public Animation an { get { return model.animation; } }

	void Start () {
        controller = GetComponent<CharacterController>();
        Network.InitializeServer(32, 80, false);
	}
    CharacterController controller;
	void Update () {
        //controller.SimpleMove(Vector3.zero);
        
	}
}
