using System.Linq;
using doru;
using UnityEngine;
using System.Collections;

public class Player : Bs
{
    public GameObject ropePrefab;
    public Collider ropeCollider;
    void Start()
    {

    }

    public float torq = 1;
    public float MaxAngl = 100;
    public float ropeThrowForce = 100;
    public float ropeThrowStiffness=.5f;
    public float ropeEndStiffness=1;
    RopeEnd rope;
    void Update()
    {
        rigidbody.maxAngularVelocity = MaxAngl;
        //if (Input.GetKey(KeyCode.D))
        //    this.rigidbody.AddRelativeTorque(-Vector3.forward * torq);
        //if (Input.GetKey(KeyCode.A))
        //    this.rigidbody.AddRelativeTorque(Vector3.forward * torq);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit h;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out h, 1 << LayerMask.NameToLayer("Level")))
            {
                Vector3 nearest = Vector3.zero;
                foreach(var a in _Game.Path.nodes)
                    foreach (var b in a.Nodes)
                    {
                        var p = a.pos + Vector3.Project(h.point - a.pos, (b.pos - a.pos).normalized);
                        if (Vector3.Distance(p, h.point) < Vector3.Distance(nearest, h.point))
                            nearest = p;
                    }
                print("hit:"+nearest);
                Debug.DrawLine(pos, nearest, Color.blue, 2);
                rope = ((GameObject)Instantiate(ropePrefab, pos, Quaternion.identity)).GetComponentInChildren<RopeEnd>();
                rope.rigidbody.AddForce((nearest - pos).normalized * ropeThrowForce);
                rope.cloth.stretchingStiffness = ropeThrowStiffness;
                rope.stiffness = ropeEndStiffness;
                rope.col = ropeCollider;
                rope.cloth.AttachToCollider(ropeCollider, false, false);
                _Game.obsToRotate.Add(rope.parent);
            }
        {
            _Game.obsToRotate.Remove(rope.parent);
        }
        if (Input.GetMouseButtonUp(0) && rope != null)
            Destroy(rope.parent.gameObject);
        }
    }

}