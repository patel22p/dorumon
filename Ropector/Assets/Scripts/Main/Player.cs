using UnityEngine;
using System.Collections;
using System.Linq;
using doru;
using System.Collections.Generic;

public class Player : bs {

    internal TimerA timer = new TimerA();
    
    public RopeEnd[] ropes = new RopeEnd[2];
    public int scores;
    public override void Awake()
    {
        ropes[0].renderer.material.color = Color.blue;
        ropes[1].renderer.material.color = Color.red;
        base.Awake();
    }
    void Start()
    {                
        foreach (var r in ropes)
            _Game.alwaysUpdate.Add(r);
        rigidbody.maxAngularVelocity = 300;
    }
    public List<Vector3> positions = new List<Vector3>();

    void Update()
    {
        //_Game.deadAnim.Play();
        name = "Player: " + ToString();
        if (_Game.prestartTm > 0 && !debug) return;
        if (networkView.isMine && !fall)
        {
            if (timer.TimeElapsed(500))
                positions.Add(transform.position);
            if (positions.Count > 10)
                positions.Remove(positions[0]);
            UpdateMove();
            UpdateRopes();
            UpdateFall();
        }
        timer.Update();        
    }
    private void UpdateMove()
    {
        if (!Screen.lockCursor) return;
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        var v = rigidbody.velocity * .05f;
        //rigidbody.AddRelativeTorque(0, 0, -mv.x * rigidbody.mass * 15 );

        if (mv.x != 0)
            mv.x = mv.x + -Mathf.Clamp(v.x, -.9f, .9f);
        
        rigidbody.AddForce(mv * 15);

    }
    void UpdateRopes()
    {
        for (int i = 0; i < 2; i++)
        {
            if (Input.GetMouseButtonDown(i))
                this.ropes[i].networkView.RPC("Throw", RPCMode.All, _Cam.cursor.transform.position - this.pos);

            if (Input.GetMouseButtonUp(i))
                this.ropes[i].networkView.RPC("Hide", RPCMode.All);
        }        
    }
    bool fall;
    void UpdateFall()
    {
        if (_Player.pos.y < _Game.Fall)
        {            
            fall = _Game.deadAnim.gameObject.active = true;
            _Game.deadAnim.Play();
            Debug.Log("Fall");
            timer.AddMethod(2000, delegate { networkView.RPC("ResetPos", RPCMode.All); });
        }
    }
    [RPC]
    private void ResetPos()
    {
        
        fall = _Game.deadAnim.gameObject.active = false;
        rigidbody.velocity = Vector3.zero;
        pos = positions.First();
        Debug.Log(pos);
        
    }
    public Base cursor { get { return _Cam.cursor; } }
} 
