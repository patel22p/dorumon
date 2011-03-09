using System;
using UnityEngine;
using doru;
using System.Collections.Generic;

public class Game : bs
{
    public TimerA timer = new TimerA();
    [FindTransform]
    public Base cursor;
    [FindTransform("Player",scene=true)]
    public bs iplayer;
    public List<Score> blues = new List<Score>();
    [FindTransform(scene = true)]
    public Player Player;
    internal List<Car> cars = new List<Car>();

    float tmWall;
    Vector3? oldp;
    GameObject Holder;
    [FindAsset]
    public Material wallMat;
    void Update()
    {
        GameGui.scores.text = Player.scores + "/" + blues.Count;
        UpdateWall();
        timer.Update();
    }
    private GameObject CreateCube()
    {
        var holder = new GameObject();
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.renderer.material = wallMat;
        //cube.renderer.receiveShadows = cube.renderer.castShadows;
        cube.transform.parent = holder.transform;
        cube.transform.localPosition = new Vector3(0, 0, .5f);
        return holder;
    }

    private void UpdateWall()
    {
        tmWall -= Time.deltaTime;

        if (Input.GetKey(KeyCode.B))//create rigidbody first?
        {
            //if (tmWall < 0)
            //Debug.Log("draw");
            if (tmWall < 0 && (oldp == null || Vector3.Distance(cursor.pos, oldp.Value) > 1))
            {
                //Debug.Log("draw");
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
                    if (cubetr.transform.rotation.eulerAngles.y == 0)
                        cubetr.transform.Rotate(new Vector3(0, 90, 0));
                    cubetr.transform.parent = Holder.transform;

                }
                oldp = cursor.transform.position;
                tmWall = .05f;
            }
        }
        else
        {
            if (oldp != null && Holder != null)
            {
                Holder.AddComponent<Rigidbody>();
                Holder.AddComponent<Drawed>();
                Holder.rigidbody.isKinematic = true;
                Holder.rigidbody.mass = .1f;
                Holder.rigidbody.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ);
                foreach (Transform t in Holder.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = LayerMask.NameToLayer("Level");
                Holder = null;
                oldp = null;
            }
        }

        bool active = Input.GetKey(KeyCode.V);
        bool clear = Input.GetKey(KeyCode.C);
        bool cut = Input.GetKey(KeyCode.X);
        if (active || clear || cut)
        {
            Debug.DrawLine(Cam.pos, cursor.pos);
            var r = new Ray(Cam.pos, cursor.pos - Cam.pos);
            RaycastHit h;
            if (Physics.Raycast(r, out h, Vector3.Distance(cursor.pos, Cam.pos), 1 << LayerMask.NameToLayer("Level")))
            {
                var mh = h.transform.GetMonoBehaviorInParrent() as Drawed;
                if (mh != null)
                {
                    Debug.Log("Found");
                    if (clear || cut)
                    {
                        if (mh.GetComponentInChildren<RopeEnd>() != null)
                            Player.EnableRope(false);
                        if (cut)
                            Destroy(h.collider.gameObject);
                        else
                            Destroy(h.transform.gameObject);

                        Debug.Log(h.collider.name);
                    }

                    if (active)
                    {
                        h.rigidbody.isKinematic = false;
                        h.rigidbody.WakeUp();
                    }
                }
            }

        }

    }
}