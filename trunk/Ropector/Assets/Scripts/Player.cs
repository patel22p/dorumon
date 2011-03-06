using UnityEngine;
using System.Collections;
using doru;

public class Player : MonoBehaviour {

    public TimerA timer = new TimerA();
    ClothRope rope;
    public Cam cam;
    RopeEnd ropeEnd;
    public GameObject cursor { get { return cam.cursor; } }
    void Start()
    {
        rope = this.GetComponent<ClothRope>();
        ropeEnd = rope.ropeEnd.GetComponent<RopeEnd>();
    }
    float tmRope;
    float tmWall;
    Vector3? oldp;
    
    public GameObject Holder;
    private static GameObject CreateCube()
    {
        var holder = new GameObject();
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = holder.transform;
        cube.transform.localPosition = new Vector3(0, 0, .5f);
        return holder;
    }
    

    private void UpdateWall()
    {
        tmWall -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
        {
            if (tmWall < 0)
            {
                if (oldp != null)
                {
                    if (Holder == null)
                        Holder = new GameObject();
                    var o = oldp.Value;
                    var cube = CreateCube();
                    var cubetr = cube.transform;
                    Vector3 v = cursor.transform.position - o;
                    cubetr.localScale = new Vector3(5, .1f, v.magnitude);
                    cubetr.position = o;
                    cubetr.LookAt(cursor.transform.position);
                    cubetr.transform.parent = Holder.transform;

                }
                oldp = cursor.transform.position;
                tmWall = .05f;
            }
        }
        else
        {
            if (oldp != null)
            {
                Holder.AddComponent<Rigidbody>();
                Holder.rigidbody.isKinematic = true;
                Holder.rigidbody.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ);
                foreach (Transform t in Holder.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = LayerMask.NameToLayer("Level");
                Holder = null;
                oldp = null;
            }
        }
    }
    void Update()
    {
        UpdateWall();
        UpdateRope();
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0);
        var controller = rigidbody;
        controller.MovePosition(transform.position + mv * Time.deltaTime * 10);
        timer.Update();
    }
    void UpdateRope()
    {
        tmRope -= Time.deltaTime;
        if (!Screen.lockCursor) return;
        rope.iCloth.stretchingStiffness = Mathf.Min(1, rope.iCloth.stretchingStiffness + Time.deltaTime / 5);
        if (Input.GetMouseButtonDown(0) && tmRope < 0)
        {
            tmRope = 2;
            rope.iCloth.stretchingStiffness = .005f;
            ropeEnd.Reset();
            var dir = cam.cursor.transform.position - transform.position;
            dir = dir.normalized;
            ropeEnd.rigidbody.velocity = dir * 100;
            ropeEnd.transform.position = transform.position;
        }
    }
    
}
