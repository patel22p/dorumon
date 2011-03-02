using UnityEngine;
using System.Collections;
using doru;

public class Player : MonoBehaviour {

    public TimerA timer = new TimerA();
    ClothRope rope;
    public Cam cam;
    RopeEnd ropeEnd;
	void Start () {
        rope = this.GetComponent<ClothRope>();
        ropeEnd = rope.ropeEnd.GetComponent<RopeEnd>();
        enable(false);
	}
    
    private void enable(bool e)
    {
        //rope.ropeEnd.active = e;        
        //rope.iCloth.enabled = e;
        //rope.renderer.enabled = e;
        //rope.enabled = e;
    }
    float tm;
    void Update()
    {
        tm -= Time.deltaTime;
        //Input.GetAxis("Vertical")
        if (!Screen.lockCursor) return;

        rope.iCloth.stretchingStiffness = Mathf.Min(1, rope.iCloth.stretchingStiffness + Time.deltaTime / 5);
        if (Input.GetMouseButtonDown(0) && tm < 2)
        {
            rope.iCloth.stretchingStiffness = .005f;
            ropeEnd.katched = false;
            //ropeEnd.active = true;
            ropeEnd.transform.position = transform.position;
            var dir = cam.cursor.transform.position - transform.position;
            dir = dir.normalized;
            ropeEnd.rigidbody.velocity = dir *100;
            enable(true);
            //ropeEnd.rigidbody.velocity 
        }
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0);
        var controller = rigidbody;
        controller.MovePosition(transform.position + mv / 5);
        timer.Update();
    }
}
