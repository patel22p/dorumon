using UnityEngine;
using System.Collections;

public class Water3Float : MonoBehaviour {

	public Water3 m_Water;
	
	void Start () {
		if(!m_Water) {
			Debug.Log("Please assign a Water patch for bouyancy script on "+gameObject.name+" to work");	
			enabled = false;
		}
	}
	
	void Update () {
		Vector3 pos = m_Water.GetHeightOffsetAt(transform.position);
		Vector3 norm = m_Water.GetNormalAt(transform.position);
		Quaternion normalRotated = Quaternion.identity; 
		normalRotated.SetFromToRotation(Vector3.up, norm);

		// interpolate a little to get smoother floating
		transform.rotation = Quaternion.Slerp(transform.rotation, normalRotated, Time.deltaTime*4.0f);
		transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime*4.0f);
	}
}
