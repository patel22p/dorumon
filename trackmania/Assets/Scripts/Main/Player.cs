using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : bs {

    public List<GameObject> whells = new List<GameObject>();
    List<WheelCollider> whellColliders = new List<WheelCollider>();
    internal float torq;
    public override void Awake()
    {
        base.Awake();
        

        foreach (GameObject t in whells)
        {
            GameObject wg = new GameObject(t.name + "WhellColl");
            wg.transform.parent = transform;
            wg.transform.position = t.transform.position;
            wg.transform.rotation = t.transform.rotation;

            var w = wg.AddComponent<WheelCollider>();
            w.radius = t.transform.localScale.y / 2;
            w.suspensionSpring = new JointSpring { damper = 50, spring = 55000, targetPosition = 0 };
            w.forwardFriction = new WheelFrictionCurve { asymptoteSlip = 2, asymptoteValue = 10000, extremumSlip = 1, extremumValue = 20000, stiffness = 0.092f };
            w.sidewaysFriction = new WheelFrictionCurve { asymptoteSlip = 2, asymptoteValue = 10000, extremumSlip = 1, extremumValue = 20000, stiffness = 0.022f };
            w.suspensionDistance = 0.2f;
            whellColliders.Add(w);
        }
    }

	void Start () {
	
	}

    void Update()
    {
      
        rigidbody.drag = rigidbody.velocity.magnitude / 250;
        rigidbody.angularDrag = rigidbody.velocity.magnitude / 150;



        if (Input.GetAxis("Vertical") > 0)
            torq = rigidbody.velocity.magnitude + 10;
        else
            torq = 0;

        for (int i = 0; i < whells.Count; i++)
        {


            var wc = whellColliders[i];

            whells[i].transform.rotation = wc.transform.rotation * Quaternion.Euler(whellrot[i], wc.steerAngle, (float)0);
            whellrot[i] += (wc.rpm * (360 / 60)) * Time.deltaTime;

            
            if (Input.GetKey(KeyCode.Space))
                wc.brakeTorque = 60;
            else
                wc.brakeTorque = 0;
            RaycastHit hit;
            Vector3 ccp = wc.transform.position;
            if (Physics.Raycast(ccp, -wc.transform.up, out  hit, wc.suspensionDistance + wc.radius))
                whells[i].transform.position = hit.point + (wc.transform.up * wc.radius);
            else
                whells[i].transform.position = ccp - (wc.transform.up * wc.suspensionDistance);

            wc.motorTorque = torq;
        }

        rigidbody.AddForce(transform.forward * 5000);

        
        this.whellColliders[1].steerAngle = whellColliders[0].steerAngle = 10 * Input.GetAxis("Horizontal");
              
        //whellColliders[0].motorTorque = torq;
    }
    public float[] whellrot = new float[4];
}
