using UnityEngine;
using System.Collections;

public enum PowerupType { NONE, DOUBLE_JUMP, SUPER_JUMP, ONE_HIT_WONDER, INVINCIBILITY, INVISIBILITY, SUPER_BUNNY }

public class Powerup : MonoBehaviour {

	// Powerup type.
	public PowerupType PowerType = PowerupType.DOUBLE_JUMP;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider col)
	{
		if(col.tag=="Player")
		{
			col.GetComponent<PlayerController>().SetPowerUp(PowerType);
			GameObject.Destroy(gameObject);
		}
	}
}
