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
    float tm;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var r = new GameObject();
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var ca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Debug.Log("test");
            c.transform.parent = r.transform;
            ca.transform.parent = r.transform;
            r.AddComponent<Rigidbody>();
            r.transform.position = cursor.transform.position;
        }

        tm -= Time.deltaTime;
        if (!Screen.lockCursor) return;

        rope.iCloth.stretchingStiffness = Mathf.Min(1, rope.iCloth.stretchingStiffness + Time.deltaTime / 5);
        if (Input.GetMouseButtonDown(0) && tm < 0)
        {
            tm = 2;
            rope.iCloth.stretchingStiffness = .005f;
            ropeEnd.Reset();
            var dir = cam.cursor.transform.position - transform.position;
            dir = dir.normalized;
            ropeEnd.rigidbody.velocity = dir * 100;
            ropeEnd.transform.position = transform.position;
        }
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0);
        var controller = rigidbody;
        controller.MovePosition(transform.position + mv *Time.deltaTime*10);
        timer.Update();
    }
}
