using UnityEngine;
using System.Collections;
using System.Linq;
using doru;
using System.Collections.Generic;

public class Player : bs {

    internal TimerA timer { get { return _Game.timer; } }
    public GameObject model;
    public TextMesh NickText;
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
        lastpos = _Game.spawn;
        _Game.players2.Add(this);
        rigidbody.maxAngularVelocity = 300;
        if (!networkView.isMine)
            SetGhost();
        if (networkView.isMine)
            networkView.RPC("SetName", RPCMode.AllBuffered, _Loader.nick);
        else
            foreach (bs a in GameObject.FindObjectsOfType(typeof(bs)))
                a.OnPlayerCon(networkView.owner);
    }

    private void SetGhost()
    {
        model.renderer.material.shader = Shader.Find("Transparent/Diffuse");
        var c = model.renderer.material.color;
        c.a = .8f;
        model.renderer.material.color = c;
    }

    internal Vector3 lastpos;
    public override void OnPlayerCon(NetworkPlayer player)
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
        UpdateOther();
        UpdatePlayerNick();
        
        if (networkView.isMine && !fall && !_Game.pause && _Game.prestartTm < 0)
        {
            UpdateRopes();
            UpdateFall();
        }
        timer.Update();        
    }
    void UpdateOther()
    {
        name = "Player:" + nick;
        if (networkView.isMine && timer.TimeElapsed(1000))
            networkView.RPC("SetPing", RPCMode.All, Network.isServer ? 0 : Network.GetLastPing(Network.connections[0]), _Loader.fps, _Loader.exceptionCount);
    }
    private void UpdatePlayerNick()
    {
        //if (!networkView.isMine || debug)
        //{
        NickText.text = nick;
        NickText.transform.rotation = Quaternion.identity;
        NickText.transform.position = pos + Vector3.up * 2;
        //}
        //else NickText.gameObject.active = false;        
    }
    void FixedUpdate()
    {
        if (networkView.isMine && !fall && !_Game.pause && _Game.prestartTm < 0)
        {
            UpdateMove();
        }
    }
    private void UpdateMove()
    {
        float mv = Input.GetAxis("Horizontal");
        float av = Input.GetAxis("Horizontal");

        
        //foreach (Wall w in collides.Where(a => a is Wall))
        //    if (w.SpeedTrackVell > 0)
        //        rigidbody.AddRelativeTorque(0, 0, -mv * 15 * w.SpeedTrackVell);
        foreach (UpForce w in triggers.Where(a => a is UpForce))
            rigidbody.AddForce(w.forceFactor * 1000 * Time.deltaTime);

        if (mv != 0)
            mv = mv - Mathf.Clamp(rigidbody.velocity.x * .01f, -.9f, .9f);
        if (av != 0)
            av = av + Mathf.Clamp(rigidbody.angularVelocity.z * .1f, -.9f, .9f);
        //if (collides.Count > 0)
            rigidbody.AddRelativeTorque(0, 0, -av * 20);
        //else
            rigidbody.AddForce(new Vector3(mv * 10, 0, 0));
    }
    void UpdateRopes()
    {
        for (int i = 0; i < 2; i++)
        {
            bool down = Input.GetMouseButtonDown(i) && !_MenuGui.enabled;
            bool up = Input.GetMouseButtonUp(i);
            if (!up) up = Input.GetKeyUp(KeyCode.Q) && i == 0;
            if (!up) up = Input.GetKeyUp(KeyCode.E) && i == 1;
            if (!down) down = Input.GetKeyDown(KeyCode.Q) && i == 0;
            if (!down) down = Input.GetKeyDown(KeyCode.E) && i == 1;
            if (down)
                this.ropes[i].networkView.RPC("Throw", RPCMode.All, _Cam.cursor.transform.position - this.pos);

            if (up)
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
            fall = false;            
            pos = lastpos;
        }
    }

    private void Hide(bool h)
    {

        enabled = !h;
        rigidbody.useGravity = !h;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        model.renderer.enabled = !h;
        model.collider.isTrigger = h;
    }
    
    [RPC]
    public void SetScores(int score,int totalscores)
    {
        this.scores = score;
        this.totalscores = totalscores;
    }
    internal int ping,fps,errors;   
    [RPC]
    void SetPing(int ping, int fps, int errors)
    {
        this.ping = ping;
        this.fps = fps;
        this.errors = errors;
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
        //Debug.Log(c.collider.name);
        if (networkView.isMine)
        {
            var w = c.transform.GetComponentInParrent<Wall>();
            if (w != null)
                if (w.die)
                    Die();
            var p = c.transform.GetComponentInParrent<PhysAnim>();
            if (p != null)
            {
                if (p.PlayOnPlayerHit)
                    p.Anim.Play();
            }
        }
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
