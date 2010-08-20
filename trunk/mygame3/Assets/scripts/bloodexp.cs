using UnityEngine;
using System.Collections;

public class bloodexp : Base {
    
    float r = 5;
    public Transform blood;
    bool l;
	void Start () {
        _Spawn.dynamic.Add(this);
        transform.position += new Vector3(Random.Range(-r, r), 0, Random.Range(-r, r)) / 10;
        rigidbody.AddExplosionForce(400, transform.position + new Vector3(Random.Range(-r, r), -10, Random.Range(-r, r)), 50);
	}
    public override void Dispose()
    {
        _Spawn.dynamic.Remove(this);        
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        AudioSource[] au = GetComponents<AudioSource>();

        if (collisionInfo.impactForceSum.magnitude >10 )
        {

            AudioSource a = au[Random.Range(0, au.Length - 1)];
            a.volume = .7f;
            a.Play();
        }
        if (!l && Root(collisionInfo.gameObject).name == "Level")
        {
            bloodexp a = ((Transform)Instantiate(blood, transform.position, Quaternion.identity)).GetComponent<bloodexp>();
            a.Destroy(10);
            a.transform.parent = _Spawn.effects;
            l = true;
        }   
    }	
}
