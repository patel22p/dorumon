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
    [FindAsset]
    public Material woodMat;

    public float fall;
    public bool Win;
    public float Wall;
    public float WallSticky;
    public float WallDynamic;
    public float prestartTm = 3;
    public float TimeElapsed; 
    public bool SecondRope;
    public override void Awake()
    {
        Debug.Log("Game Awake");
        if (isLevel(1, 2, 3, 4, 5))
            fall = -7f;
        if (isLevel(3))
            Wall = 100;
        if (isLevel(5))
            WallSticky = 100;
        base.Awake();
    }
    public override void InitValues()
    {
        IgnoreAll("IgnoreColl");
        IgnoreAll("Button");
        IgnoreAll("Water");
        AddColl("Button", "Player");        
        base.Init();
    }
    
    void Update()
    {
        
        timer.Update();
        prestartTm -= Time.deltaTime;

     

        if (prestartTm > 0 && !_Loader.debug)
        {
            GameGui.CenterTime.text = (Mathf.Ceil(prestartTm)) + "";
            return;
        }
        else
            GameGui.CenterTime.enabled = false;
        if (Win) return;
        TimeElapsed += Time.deltaTime;                
        GameGui.time.text = TimeToSTr(TimeElapsed);

        foreach (var a in alwaysUpdate)
            a.AlwaysUpdate();
        if (Player.pos.y < fall && Player.gameObject.active)
        {
            Player.gameObject.active = false;
            deadAnim.Play();
            timer.AddMethod(2000, delegate { LoadLevel(Application.loadedLevelName); });
        }
        if (Player.scores == blues.Count && !Win)
        {
            Win = true;
            deadAnim.Play();
            var f = PlayerPrefs.GetFloat(Application.loadedLevelName);
            if (TimeElapsed < f || f == 0)
            {
                GameGui.time.text = "New Record:" + TimeToSTr(TimeElapsed);
                PlayerPrefs.SetFloat(Application.loadedLevelName, TimeElapsed);
                _Loader.RefreshRecords();
            }
            int nextLevel = int.Parse(Application.loadedLevelName.Substring(0, 1)) + 1;
            timer.AddMethod(2000, delegate { LoadLevel(nextLevel + ""); });
            Player.gameObject.active = false;
        }
        GameGui.scores.text = Player.scores + "/" + blues.Count;
        UpdateWall();
        UpdateEditWall();
    }
    private void UpdateWall()
    {
        tmWall -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Return))
            LoadLevel(Application.loadedLevelName);

        bool build = Input.GetKey(KeyCode.B) && Wall > 0;
        bool buildSticky = Input.GetKey(KeyCode.V) && WallSticky > 0;
        bool buildDynamic = Input.GetKey(KeyCode.N) && WallDynamic > 0;
        if (build || buildSticky || buildDynamic)
        {
            if (oldp == null || Vector3.Distance(cursor.pos, oldp.Value) > .5F)
            {
                if (build) Wall--;
                if (buildSticky) WallSticky--;
                if (buildDynamic) WallDynamic--;
                if (oldp != null)
                {
                    if (Holder == null)
                    {
                        Holder = new GameObject("Wall");
                        var r = Holder.AddComponent<Rigidbody>();
                        r.isKinematic = true;
                        var dr = Holder.AddComponent<Wall>();
                        dr.attachRope = buildSticky || buildDynamic;                        
                        if (buildDynamic)
                            Holder.rigidbody.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ);
                    }
                    var o = oldp.Value;
                    var cube = CreateCube(buildSticky ? woodMat : wallMat);
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
                Combine(Holder);

                foreach (Transform t in Holder.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = LayerMask.NameToLayer("Default");
                Holder = null;
                oldp = null;
            }
            oldp = null;
        }
    }
    Vector3 dragpos;
    Rigidbody drb;
    bool isdrag;
    void UpdateEditWall()
    {

        bool clear = Input.GetKey(KeyCode.C);
        bool cut = Input.GetKey(KeyCode.X);
        //if (clear || cut)
        //{
        
        var vr = cursor.pos - Cam.pos;
        var r = new Ray(Cam.pos, vr);
        RaycastHit h;
        if (Physics.Raycast(r, out h, vr.magnitude, 1 << LayerMask.NameToLayer("Default")))
        {
            var mh = h.transform.GetMonoBehaviorInParrent() as Wall;
            if (mh != null)
            {
                Debug.Log("Found");
                if (clear || cut)
                {
                    var rp = mh.GetComponentsInChildren<RopeEnd>();
                    foreach (var a in rp)
                        a.EnableRope(false);
                    if (cut)
                        Destroy(h.collider.gameObject);
                    else
                        Destroy(h.transform.gameObject);
                }
                if (Input.GetKeyDown(KeyCode.F) && mh.ClickForce !=Vector3.zero)
                {
                    mh.rigidbody.AddForce(mh.ClickForce*mh.rigidbody.mass   );
        
                }
            }
        }

    }
    private static void AddColl(string a, string b)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(a), LayerMask.NameToLayer(b), false);
    }
    private static void IgnoreAll(string name)
    {
        for (int i = 1; i < 31; i++)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(name), i, true);
    }
    private GameObject CreateCube(Material mat)
    {
        var holder = new GameObject();
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.renderer.material = mat;
        //cube.renderer.receiveShadows = cube.renderer.castShadows;
        cube.transform.parent = holder.transform;
        cube.transform.localPosition = new Vector3(0, 0, .5f);
        return holder;
    }
  
    bool isLevel(params int[] id)
    {
        foreach (var i in id)
            if (Application.loadedLevelName == i.ToString()) return true;
        return false;
    }
    public void LoadLevel(string n)
    {
        Debug.Log("loading Level" + n);
        timer.Clear();
        Application.LoadLevel(n);
    }
}
