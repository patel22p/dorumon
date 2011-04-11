using System;
using UnityEngine;
using doru;
using System.Collections.Generic;

//bs.cs     base class all clases are derived from it, contains static pointers to other class objects
//Game.cs   game logic, check for win , check player fall, handling 
//Menu.cs   menu scene effects
//Loader.cs Menu window handling, loads first, and will be never destroyed.
//Cam.cs    Handles camera
//Wall.cs   Wall settings- ,sticky, bounchy, 
//RopeE.cs  The rope end collider, handles rope collisions to wall, player wall/dynamicObjects interaction
//Score.cs  Blue Bals on scene
//Button.cs Button in game ,it opens door. 
//Car.cs    Car player can enter/exit in it.


public class Game : bs
{
    Vector3 dragpos;
    Rigidbody drb;
    bool isdrag;
    
    
    
    public List<bs> alwaysUpdate = new List<bs>();  
    public TimerA timer = new TimerA();
    [FindTransform(scene = true)] // with this attribute this variable will be assigned automaticly in editor
    public Animation deadAnim;
    [FindTransform]
    public Base cursor;
    [FindTransform("Player", scene = true)]
    public bs iplayer; // player or car pointer(if player is in car)
    public List<Score> blues = new List<Score>();
    float tmWall;
    Vector3? oldp;
    GameObject Holder;
    [FindAsset]
    public Material wallMat;
    [FindAsset]
    public Material woodMat;    
    public float fall;
    public bool Stop;
    public float Wall;
    public float WallSticky;
    public float WallDynamic;
    public float prestartTm = 3;
    public float TimeElapsed; 
    public bool SecondRope;

    public float graphicsCheck;
    public override void Awake()
    {
        Debug.Log("Game Awake");
        if (isLevel(1, 2, 3, 4, 5))
            fall = -7f;
        if (isLevel(3))
            Wall = 100;
        if (isLevel(5))
            WallSticky = 100;
        if (isLevel(9, 10, 11, 12))
            WallSticky = Wall = 1000;
        base.Awake();
    }
    void Start()
    {
        timer.AddMethod(800, delegate
        {
            Screen.lockCursor = true;
            timer.AddMethod(200,delegate
            {
                cursor.pos = Vector3.zero;
            });
        });                
        
    }
    public override void InitValues() // this function called when you press start or pause in editor, usualy used for variables init in editor
    {
        IgnoreAll("Ignore Raycast"); //ignores colision for layer
        IgnoreAll("IgnoreColl");
        IgnoreAll("Button");
        IgnoreAll("Water");
        AddColl("Button", "Player");        
        base.InitValues();
    }
    float TimeSpeed = 1;
    bool enableTimeWarp;
    void Update()
    {        
        timer.Update();
        prestartTm -= Time.deltaTime;

        TimeWarp();

        if (prestartTm > 0 && !debug)
        {
            GameGui.CenterTime.text = (Mathf.Ceil(prestartTm)) + "";
            return;
        }
        else
            GameGui.CenterTime.enabled = false;
        if (Stop) return;

        TimeElapsed += Time.deltaTime;                
        GameGui.time.text = TimeToSTr(TimeElapsed);

        foreach (var a in alwaysUpdate) // calls update function if object enabled or not enabled;
            a.AlwaysUpdate();

        if (Player.pos.y < fall && !Stop) // if player fall then we reload level
        {
            Stop = true;
            Player.gameObject.active = false;
            deadAnim.Play();
            timer.AddMethod(2000, delegate { _Loader.ResetLevel(); });
        }
        
        GameGui.scores.text = Player.scores + "/" + blues.Count;
        PlayerScores();
        
    }

    private void TimeWarp()
    {
        if (enableTimeWarp)
            TimeSpeed = ((TimeSpeed * 5) + .1f) / 6f;
        if (!enableTimeWarp)
            TimeSpeed = ((TimeSpeed * 5) + 1) / 6f;
        //Debug.Log(TimeSpeed);
        Music.audio.pitch = TimeSpeed;
        Time.timeScale = TimeSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("TimeWarp");
            enableTimeWarp = true;
            timer.AddMethod(5000, delegate { enableTimeWarp = false; });
        }
    }

    private void PlayerScores()
    {
        if (Player.scores == blues.Count && !Stop) // if player win we play animation, save scores and load next level
        {
            Stop = true;
            deadAnim.Play();
            var f = PlayerPrefs.GetFloat(Application.loadedLevelName);
            if (TimeElapsed < f || f == 0)
            {
                GameGui.time.text = "New Record:" + TimeToSTr(TimeElapsed);
                PlayerPrefs.SetFloat(Application.loadedLevelName, TimeElapsed);
                _Loader.RefreshRecords();
            }

            timer.AddMethod(2000, delegate { _Loader.NextLevel(); });
            Player.gameObject.active = false;
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
    
}
