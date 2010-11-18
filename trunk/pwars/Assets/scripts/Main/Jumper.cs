using UnityEngine;
using System.Collections;

public class Jumper : MonoBehaviour {

    Transform vector;
	void Start () {
        vector = transform.Find("vector");
	}
    public float force = 10;
    void OnTriggerEnter(Collider c)
    {
        print(c);
        Player p  =c.gameObject.GetComponent<Player>() ;
        if (p != null)
        {
            p.rigidbody.AddForce(vector.localPosition.normalized * force);
        }
    }
	// Update is called once per frame
	void Update () {
	
	}
}
