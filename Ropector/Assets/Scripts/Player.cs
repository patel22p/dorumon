using UnityEngine;
using System.Collections;
using doru;

public class Player : MonoBehaviour {

    
    float tmRope;
    float tmWall;
    Vector3? oldp;
    GameObject Holder;
    bool ropeEnabled = true;

    public TimerA timer = new TimerA();
    public Cam cam;
    public RopeEnd ropeEnd;
    public InteractiveCloth cloth;
    public Menu menu;
    void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        menu.txt.text = ("streching:" + cloth.stretchingStiffness);
        UpdateWall();
        UpdateRope();
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0);
        var controller = rigidbody;
        controller.MovePosition(transform.position + mv * Time.deltaTime * 10);
        timer.Update();
    }
    private void UpdateWall()
    {
        tmWall -= Time.deltaTime;
        if (Input.GetKey(KeyCode.B))
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
    void UpdateRope()
    {
        tmRope -= Time.deltaTime;
        if (!Screen.lockCursor) return;

        if (Input.GetKeyDown(KeyCode.Space))
            cloth.stretchingStiffness = 1;
        //var f = 0f;
        //if (Input.GetKey(KeyCode.Space))
        //    f = 1;
        //if (Input.GetKey(KeyCode.C))
        //    f = -1;
        //cloth.stretchingStiffness = Mathf.Min(1, Mathf.Max(0, cloth.stretchingStiffness + f * Time.deltaTime / 5));

        if (Input.GetMouseButtonDown(0))
        {
            if (!ropeEnabled && tmRope < 0)
            {
                
                Debug.Log("RopeEnable");
                ropeEnd.transform.position = transform.position + Vector3.up * 3;
                cloth.transform.position = transform.position + Vector3.up * 1;
                tmRope = 2;
                Enable(true);
                cloth.stretchingStiffness = .5f;
                var dir = cam.cursor.transform.position - transform.position;
                dir = dir.normalized;
                ropeEnd.rigidbody.velocity = dir * 100;
                
            }
            else if (ropeEnabled) 
            {
                Debug.Log("RopeDisable");
                Enable(false);
                ropeEnd.rigidbody.isKinematic = false;
            }
        }
    }
    private void Enable(bool value)
    {
        ropeEnd.enabled = ropeEnd.gameObject.active = true;
        ropeEnabled = ropeEnd.enabled =ropeEnd.gameObject.active = cloth.gameObject.active = value;
        ropeEnd.oldpos = null;
    
    }
    private static GameObject CreateCube()
    {
        var holder = new GameObject();
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = holder.transform;
        cube.transform.localPosition = new Vector3(0, 0, .5f);
        return holder;
    }
    public GameObject cursor { get { return cam.cursor; } }
    
} 
