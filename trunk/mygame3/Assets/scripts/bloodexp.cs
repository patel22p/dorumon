using UnityEngine;
using System.Collections;

public class bloodexp : Base {
    
    float r = 5;
    public Transform blood;
    bool l;
	void Start () {        
        transform.position += new Vector3(Random.Range(-r, r), 0, Random.Range(-r, r)) / 10;
        rigidbody.AddExplosionForce(400, transform.position + new Vector3(Random.Range(-r, r), -10, Random.Range(-r, r)), 50);
	}
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (!l && Root(collisionInfo.gameObject).name == "Level")
        {
            AudioSource[] au = GetComponents<AudioSource>();
            AudioSource au1 = au[Random.Range(0, au.Length - 1)];
            au1.volume = .7f;
            au1.Play();

            Transform a;
            Destroy(a = (Transform)Instantiate(blood, transform.position, Quaternion.identity),10);
            a.parent = _Spawn.effects;
            l = true;
        }   
    }	
}
