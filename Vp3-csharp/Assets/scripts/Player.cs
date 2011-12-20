using UnityEngine;
using System.Collections;

public class Player : bs
{
    public float MaxAngular=300;
    public override void Awake()
    {

        _Player = this;
        base.Awake();
    }
    public void Update()
    {
        rigidbody.maxAngularVelocity = MaxAngular;
    }
    
    public void OnCollisionEnter(Collision collision)
    {
        
        if(collision.gameObject.tag =="Jumper")
        {
            print("Hit");
            rigidbody.AddForce(collision.contacts[0].normal * _Game.Jump);
        }
    }
}
