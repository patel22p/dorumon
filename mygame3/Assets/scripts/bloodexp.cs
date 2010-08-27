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
    public AudioClip[] gibsound;
    void OnCollisionEnter(Collision collisionInfo)
    {        
        if (collisionInfo.impactForceSum.magnitude >10 )
        {
            PlayRandSound(gibsound);
        }
        if (!l && Root(collisionInfo.gameObject).name == "Level")
        {
            Transform a = ((Transform)Instantiate(blood, transform.position, Quaternion.identity));
            Destroy(a.gameObject, 10);
            a.parent = _Spawn.effects;
            l = true;
        }   
    }
    
}
