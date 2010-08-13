using UnityEngine;
using System.Collections;

public class bloodexp : Base {
    
    float r = 5;
    public Transform blood;
    bool l;
	void Start () {        
        transform.position += new Vector3(Random.Range(-r, r), 0, Random.Range(-r, r)) / 10;
        rigidbody.AddExplosionForce(40, transform.position + new Vector3(Random.Range(-r, r), -10, Random.Range(-r, r)), 50);
	}
    void OnCollisionEnter(Collision collisionInfo)
    {
        
        if (!l && Root(collisionInfo.gameObject).name == "Level")
        {
            Transform a;
            Destroy(a = (Transform)Instantiate(blood, transform.position, Quaternion.identity),10);
            a.parent = _Spawn.effects;
            l = true;
        }   
    }	
}
