using UnityEngine;
using System.Collections;

public class ParticleFollow : bs {

	void Start () {
	
	}
	
	void Update () {
        if (_Player != null)
            transform.position = _Player.pos / 2f;
	}
}
