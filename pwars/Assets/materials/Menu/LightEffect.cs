using UnityEngine;
using System.Collections;

public class LightEffect : MonoBehaviour {

	void Start () {
	
	}
	void FixedUpdate () {
        this.light.intensity += Random.Range(-.02f, .02f);
        this.light.intensity = Mathf.Max(this.light.intensity, .8f);
        this.light.intensity = Mathf.Min(this.light.intensity, 1.5f);
	}
}
