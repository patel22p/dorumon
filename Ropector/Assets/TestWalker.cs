using System;
using UnityEngine;

[Serializable, RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TestWalker : MonoBehaviour
{
    public bool canJump = true;
    private CapsuleCollider capsule;
    public float gravity = 10f;
    private bool grounded;
    private Vector3 groundVelocity;
    
    public float jumpHeight = 2f;
    public float maxVelocityChange = 10f;
    public float speed = 8f;

    public  void Awake()
    {
        rigidbody.freezeRotation = true;
        rigidbody.useGravity = false;
        capsule = (CapsuleCollider) GetComponent(typeof(CapsuleCollider));
    }

    public  float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt((2 * jumpHeight) * gravity);
    }

    public  void FixedUpdate()
    {
        if (grounded)
        {
            Vector3 v = new Vector3(Input.GetAxis("Horizontal"), (float)0, Input.GetAxis("Vertical"));
            v = (Vector3) (transform.TransformDirection(v) * speed);
            Vector3 velocity = rigidbody.velocity;
            Vector3 force = (v - velocity) + groundVelocity;
            force.x = Mathf.Clamp(force.x, -maxVelocityChange, maxVelocityChange);
            force.z = Mathf.Clamp(force.z, -maxVelocityChange, maxVelocityChange);
            force.y = 0;
            rigidbody.AddForce(force, ForceMode.VelocityChange);
            if (canJump && Input.GetButton("Jump"))
                rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            grounded = false;
        }
        else
        {
            Vector3 v = new Vector3(Input.GetAxis("Horizontal"), (float) 0, Input.GetAxis("Vertical"));
            v = (Vector3)(transform.TransformDirection(v) * 8f);
            rigidbody.AddForce(v, ForceMode.VelocityChange);
        }
        rigidbody.AddForce(new Vector3((float) 0, -gravity * rigidbody.mass, (float) 0));
    }

    public  void Main()
    {
    }

    public  void OnCollisionEnter(Collision col)
    {
        TrackGrounded(col);
    }

    public void OnCollisionStay(Collision col)
    {
        TrackGrounded(col);
    }

    public void TrackGrounded(Collision col)
    {

        for (int i = 0; i < col.contacts.Length; i++)
            if (col.contacts[i].point.y < capsule.bounds.min.y + capsule.radius)
            {
                if (col.rigidbody != null)
                    groundVelocity = col.rigidbody.velocity;
                else
                    groundVelocity = Vector3.zero;
                grounded = true;
            }
    }
}

