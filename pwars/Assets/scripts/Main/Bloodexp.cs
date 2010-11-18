using UnityEngine;
using System.Collections;

public class bloodexp : Base {
    
    float r = 5;
    public Transform blood;
    bool l;
	void Start () {
        _Game.dynamic.Add(this);
        transform.position += new Vector3(Random.Range(-r, r), 0, Random.Range(-r, r)) / 10;
        rigidbody.AddExplosionForce(400, transform.position + new Vector3(Random.Range(-r, r), -10, Random.Range(-r, r)), 50);
	}
    
    //protected override void OnDisable()
    //{
        
    //    _Game.dynamic.Remove(this);        
    //}
    public AudioClip[] gibsound;
    void OnCollisionEnter(Collision collisionInfo)
    {        
        if (collisionInfo.impactForceSum.magnitude >10 )
        {
            //PlayRandSound(gibsound);
        }
        if (!l && Root(collisionInfo.gameObject).name == "Level")
        {
            Transform a = ((Transform)Instantiate(blood, transform.position, Quaternion.identity));
            Destroy(a.gameObject, 10);
            a.parent = _Game.effects;
            l = true;
        }   
    }
    
}
