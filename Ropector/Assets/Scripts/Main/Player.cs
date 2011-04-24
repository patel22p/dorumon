using UnityEngine;
using System.Collections;
using System.Linq;
using doru;
using System.Collections.Generic;

public class Player : bs {

    internal TimerA timer { get { return _Game.timer; } }
    public GameObject model;
    public GameObject deathPrefab;
    public RopeEnd[] ropes = new RopeEnd[2];
    public int scores;
    public int totalscores;
    public override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        _Game.players2.Add(this);
        //foreach (var r in ropes)
        //    _Game.alwaysUpdate.Add(r);
        rigidbody.maxAngularVelocity = 300;
        if (networkView.isMine)
        {
            Debug.Log("set nick"+ _Loader.nick);
            networkView.RPC("SetName", RPCMode.AllBuffered, _Loader.nick);
        }
    }

    internal Vector3 lastpos;
    void OnPlayerConnected(NetworkPlayer player)
    {
        if (networkView.isMine)
            networkView.RPC("SetScores", player, scores, _Loader.totalScores);
    }
    public string nick;
    [RPC]
    void SetName(string name)
    {
        Debug.Log("Set Nick" + name);
        _GameGui.rightup.text += name + " Connected\r\n";
        this.nick = name;
    }
    void Update()
    {
        name = "Player:" + nick + " " + ToString();
        if (_Game.prestartTm > 0 && !debug) return;
        if (networkView.isMine && !fall && !_Game.pause)
        {
            
            UpdateMove();
            UpdateRopes();
            UpdateFall();
        }
        timer.Update();        
    }
    private void UpdateMove()
    {
        
        var mv = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        var v = rigidbody.velocity * .05f;
        foreach (Wall w in collides.Where(a => a is Wall))
            if (w.SpeedTrackFactor > 0)
                rigidbody.AddRelativeTorque(0, 0, -mv.x * 15 * w.SpeedTrackFactor);
        foreach (UpForce w in triggers.Where(a => a is UpForce))
            rigidbody.AddForce(w.forceFactor * 10);

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
                HideRope(i);
        }        
    }

    private void HideRope(int i)
    {
        this.ropes[i].networkView.RPC("Hide", RPCMode.All);
    }
    bool fall;
    void UpdateFall()
    {
        if ((_Player.pos.y + 2) < _Game.water.position.y)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!enabled) return;
        fall = true;
        //fall = _Game.deadAnim.gameObject.active = true;
        //_Game.deadAnim.Play();        
        HideRope(0);
        HideRope(1);
        _Game.timer.AddMethod(2000, delegate { networkView.RPC("ResetPos", RPCMode.All); });
        networkView.RPC("RPCDie", RPCMode.All);
    }
    [RPC]
    private void RPCDie()
    {
        Hide(true);
        GameObject g = (GameObject)Instantiate(deathPrefab, pos, rot);
        Destroy(g, 6);
        foreach (Rigidbody r in g.GetComponentsInChildren<Rigidbody>())
        {
            r.renderer.sharedMaterial = _Player.model.renderer.sharedMaterial;
            r.velocity = rigidbody.velocity;
            r.mass = .1f;
        }
    }
    [RPC]
    private void ResetPos()
    {
        Hide(false);
        if (networkView.isMine)
        {
            _Cam.Reset();
            fall = _Game.deadAnim.gameObject.active = false;
            rigidbody.velocity = Vector3.zero;
            pos = lastpos;
        }
    }

    private void Hide(bool h)
    {
        enabled = !h;
        rigidbody.isKinematic = h;
        model.renderer.enabled = !h;
        model.collider.isTrigger = h;
    }
    
    [RPC]
    public void SetScores(int score,int totalscores)
    {
        this.scores = score;
        this.totalscores = totalscores;
    }
    public override void OnEditorGui()
    {
        if(GUILayout.Button("SendNick"))
            networkView.RPC("SetName", RPCMode.AllBuffered, _Loader.nick);
        base.OnEditorGui();
    }
    public Base cursor { get { return _Cam.cursor; } }

    public List<bs> collides = new List<bs>();

    
    void OnCollisionEnter(Collision c)
    {
        var w = c.gameObject.GetComponent<Wall>();
        if (w != null && w.die)
            Die();
        var bs = c.gameObject.GetComponent<bs>();

        

        if (bs != null && !collides.Contains(bs))
            collides.Add(bs);
    }
    void OnCollisionExit(Collision c)
    {
        var bs = c.gameObject.GetComponent<bs>();
        if (bs != null)
            collides.Remove(bs);
    }
    
    public List<bs> triggers = new List<bs>();
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        var bs = other.gameObject.GetComponent<bs>();
        if (!triggers.Contains(bs) && bs != null)
            triggers.Add(bs);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("exit");
        var bs = other.gameObject.GetComponent<bs>();
        triggers.Remove(bs);
    }
    
} 
