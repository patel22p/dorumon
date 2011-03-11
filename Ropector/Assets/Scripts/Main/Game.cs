using System;
using UnityEngine;
using doru;
using System.Collections.Generic;


public class Game : bs
{
    internal List<Car> cars = new List<Car>();
    public List<bs> alwaysUpdate = new List<bs>();
    public TimerA timer = new TimerA();
    [FindTransform(scene = true)]
    public Animation deadAnim;
    [FindTransform]
    public Base cursor;
    [FindTransform("Player", scene = true)]
    public bs iplayer;
    public List<Score> blues = new List<Score>();
    float tmWall;
    Vector3? oldp;
    GameObject Holder;
    [FindAsset]
    public Material wallMat;
    public float fall;
    public override void Init()
    {
        IgnoreAll("IgnoreColl");
        IgnoreAll("Button");
        AddColl("Button", "Player");        
        base.Init();
    }
    void Update()
    {
        foreach (var a in alwaysUpdate)
            a.AlwaysUpdate();
        if (Player.pos.y < fall && Player.gameObject.active)
        {
            
            Player.gameObject.active = false;
            deadAnim.Play();
            timer.AddMethod(2000, delegate { LoadLevel(Application.loadedLevelName ); });
        }

        Menu.scores.text = Player.scores + "/" + blues.Count;
        UpdateWall();
        timer.Update();
    }

    public void LoadLevel(string n)
    {
        Debug.Log("loading Level" + n);
        timer.Clear();
        Application.LoadLevel(n);
    }
    public override void InitValues()
    {
        if (isLevel(1))
            fall = -7f;

        base.InitValues();
    }
    private static void AddColl(string a,string b)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(a), LayerMask.NameToLayer(b), false);
    }
    private static void IgnoreAll(string name)
    {
        for (int i = 1; i < 31; i++)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(name), i, true);
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
            //if (tmWall < 0 && (oldp == null || Vector3.Distance(cursor.pos, oldp.Value) > 1))
            if (oldp == null || Vector3.Distance(cursor.pos, oldp.Value) > .2F)
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
                    var e = cubetr.transform.rotation.eulerAngles;
                    if (e == new Vector3(90, 0, 0))
                        cubetr.transform.rotation = Quaternion.Euler(90, 90, 0);
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
                //Combine(Holder);
                Holder.AddComponent<Rigidbody>();
                Holder.AddComponent<Drawed>();
                Holder.rigidbody.isKinematic = true;
                Holder.rigidbody.mass = .1f;
                Holder.rigidbody.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ);
                foreach (Transform t in Holder.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = LayerMask.NameToLayer("Default");
                Holder = null;
                oldp = null;
            }
            oldp = null;
        }

        bool active = Input.GetKey(KeyCode.V);
        bool clear = Input.GetKey(KeyCode.C);
        bool cut = Input.GetKey(KeyCode.X);
        if (active || clear || cut)
        {
            Debug.DrawLine(Cam.pos, cursor.pos);
            var r = new Ray(Cam.pos, cursor.pos - Cam.pos);
            RaycastHit h;
            if (Physics.Raycast(r, out h, Vector3.Distance(cursor.pos, Cam.pos), 1 << LayerMask.NameToLayer("Default")))
            {
                var mh = h.transform.GetMonoBehaviorInParrent() as Drawed;
                if (mh != null)
                {
                    Debug.Log("Found");
                    if (clear || cut)
                    {
                        var rp = mh.GetComponentsInChildren<RopeEnd>();
                        foreach(var a in rp)
                            a.EnableRope(false);
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
    bool isLevel(int id)
    {
        return UnityEditor.EditorApplication.currentScene.EndsWith("1.unity");
    }
}

//public void SaveCheckPoint(CheckPoint c)
//{
//    PlayerPrefs.SetString("CheckPoint", c.pos.x + "," + c.pos.y + "," + c.pos.z);
//}
//public Vector3 LoadCheckPoint()
//{
//    var s = PlayerPrefs.GetString("CheckPoint");
//    if (s == null) return Vector3.zero;
//    var ss =s.Split(',');
//    return new Vector3(int.Parse(ss[0]), int.Parse(ss[1]), int.Parse(ss[2]));
//}