using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {

    public List<GameObject> whells = new List<GameObject>();
    List<WheelCollider> whellColliders = new List<WheelCollider>(); 

	void Start () {


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

    public float torq;
	void Update () {
        for (int i = 0; i < whells.Count; i++)
        {
            
            var wc = whellColliders[i];
            if (Input.GetKey(KeyCode.S))
                wc.brakeTorque = 60;
            else
                wc.brakeTorque = 0;
            RaycastHit hit;
            Vector3 ccp = wc.transform.position;
            if (Physics.Raycast(ccp, -wc.transform.up, out  hit, wc.suspensionDistance + wc.radius))
                whells[i].transform.position = hit.point + (wc.transform.up * wc.radius);
            else
                whells[i].transform.position = ccp - (wc.transform.up * wc.suspensionDistance);
            
        }
        
        
        if (Input.GetKey(KeyCode.W))
            torq = 30;
        else
            torq = 0;

        var trq = 20000;
        if (Input.GetKey(KeyCode.A))
            rigidbody.AddTorque(0, 0, trq);
        
        if (Input.GetKey(KeyCode.D))
            rigidbody.AddTorque(0, 0, -trq);

        whellColliders[0].motorTorque = torq;
        if(Input.GetKeyDown(KeyCode.Space))
        {            
            transform.Rotate(Vector3.up, 180);            
        }

	}

}
