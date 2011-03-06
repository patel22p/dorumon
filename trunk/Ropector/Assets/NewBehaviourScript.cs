using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

    public GameObject Left;
    
    public GameObject Right;
    public WheelCollider leftWhell;
    public WheelCollider RightWhell;
	void Start () {


        foreach (GameObject t in new[] { Left, Right })
        {
            
                
            GameObject wg = new GameObject(t.name + "WhellColl");
            wg.transform.parent = transform;
            wg.transform.position = t.transform.position;
            wg.transform.rotation = t.transform.rotation;
            var rg =wg.AddComponent<Rigidbody>();
            //rg.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rg.mass = 5;
            var w = wg.AddComponent<WheelCollider>();
            w.radius = t.transform.localScale.y/2;
            w.suspensionSpring = new JointSpring { damper = 50, spring = 5500, targetPosition = 0 };
            w.forwardFriction = new WheelFrictionCurve { asymptoteSlip = 2, asymptoteValue = 10000, extremumSlip = 1, extremumValue = 20000, stiffness = 0.092f };
            w.sidewaysFriction = new WheelFrictionCurve { asymptoteSlip = 2, asymptoteValue = 10000, extremumSlip = 1, extremumValue = 20000, stiffness = 0.022f };
            w.suspensionDistance = 0.2f;
            if (t == Left)
                leftWhell = w;
            else
                RightWhell = w;

            var jnt = gameObject.AddComponent<FixedJoint>();
            jnt.connectedBody = wg.rigidbody;
            //jnt.axis = Vector3.forward;

            SpringJoint sj = wg.AddComponent<SpringJoint>();
            sj.connectedBody = rigidbody;
            //sj.axis = Vector3.forward;
            //sj.spring = 9999900;
            //sj.anchor = Vector3.up;

            //FixedJoint sj = wg.AddComponent<FixedJoint>();
            //sj.connectedBody = rigidbody;
            //sj.axis = Vector3.forward;
        }                
	}

    public float torq;
	void Update () {
        //transform
        Left.transform.position = leftWhell.transform.position;
        Right.transform.position = RightWhell.transform.position;
     
        //Left.transform.position = leftWhell.transform.position - (Vector3.up * leftWhell.suspensionDistance);
        //springs
        if (Input.GetKey(KeyCode.W))
            torq = 20;
        else
            torq = 0;

        var trq = 3;
        if (Input.GetKeyDown(KeyCode.A))
            rigidbody.AddTorque(0, 0, trq, ForceMode.VelocityChange);
        
        if (Input.GetKeyDown(KeyCode.D))
            rigidbody.AddTorque(0, 0, -trq, ForceMode.VelocityChange);

        leftWhell.motorTorque = torq;
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //var r = transform.rotation.eulerAngles;
            //r.y += 180;
            transform.Rotate(Vector3.up, 180);
            //transform.rotation = Quaternion.Euler(r);
        }

	}

}
