using System.Linq;
using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;

public class Raccoon : Shared
{

    //[FindAsset("Nut_1p")]
    //public GameObject nutPrefab;
    //[FindAsset("Berry_5p")]   
    //public GameObject berryPrefab;
    public enum State { walk = 0, run = 1, sneak = 2, crouch = 3, runAway = 4, attack =5} ;
    public State state;

    int[] states = new int[] { 0, 0, 0, 0, 2, 2, 3 };
    TimerA timer = new TimerA();
    Node[] nodes;

    AnimationState sneak { get { return an["sneak"]; } }
    AnimationState crouch { get { return an["crouch"]; } }
    AnimationState getHitA { get { return an["gethitA"]; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState walk { get { return an["walk"]; } }
    AnimationState fall { get { return an["midair"]; } }
    AnimationState land { get { return an["landing"]; } }
    AnimationState run { get { return an["run"]; } }
    
    AnimationState punch { get { return an["punch"]; } }

    Node node;
    Node lastNode;
    Vector3 fakedir;
    int TmJump;
    int TmChange;

    int TmState;
    public void Awake()
    {
        if (disableScripts)
            enabled = false;
    }
    public override void Start()
    {
        _Game.shareds.Add(this);
        base.Start();
        SetupLayers();
        SetupOther();
    }
    private void SetupOther()
    {
        nodes = _MyNodes.transform.Cast<Transform>().Select(a => a.gameObject.GetComponent<Node>()).ToArray();
        NearestNode();
        if (node == null) { enabled = false; Debug.Log("No nodes"); }
        TmChange = Random.Range(1000, 5000);
        TmJump = Random.Range(5000, 6000);
    }
    private void SetupLayers()
    {
        fall.wrapMode = idle.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        sneak.wrapMode = WrapMode.Loop;
        land.wrapMode = WrapMode.Clamp;
        getHitA.wrapMode = WrapMode.Clamp;
        death.wrapMode = WrapMode.ClampForever;
        punch.wrapMode = WrapMode.Clamp;

        death.layer = 2;
        punch.layer = getHitA.layer = land.layer = 1;
    }
    public override void Update()
    {
        base.Update();
        UpdateAnimations();
        //if (debug) state = State.crouch;
        UpdateOther();
        UpdateSelectNode();
        UpdateCheckStuck();
        UpdateMove();
        UpdateSeePlayer();
        UpdateAttack();
        timer.Update();
    }
    public void UpdateAttack()
    {
        if (!punch.enabled)
        {
            foreach (Barrel br in trigger.triggers.Where(a => a is Barrel))
                if (br != null)
                {
                    br.Hit();
                    an.CrossFade(punch.name);
                }

            foreach (Player r in trigger.triggers.Where(a => a is Player && a.enabled != false))
            {
                if(selected)
                    Debug.Log("Punch");
                r.Damage();
                an.CrossFade(punch.name);
            }
        }

    }
    public void UpdateSeePlayer()
    {
        Vector3 v = _Player.pos - pos;
        if (v.magnitude < 5 && Quaternion.Angle(rot, Quaternion.LookRotation(_Player.pos - pos)) < 90)
        {
            RaycastHit h;
            if (Physics.Raycast(new Ray(pos, v), out h, v.magnitude, 1 << LayerMask.NameToLayer("Level")))
            {
            }
            else if(state != State.runAway)
            {
                //if(selected)
                //    Debug.Log("Run Away");
                onDetect();
            }
        }
        if ((!_Player.enabled || v.magnitude > 10) && (state == State.runAway || state == State.attack))
        {
            state = State.walk;
            NearestNode();
        }
    }
    void UpdateOther()
    {
        if (life <= 0)
        {
            Die();
            return;
        }

        if (timer.TimeElapsed(TmState) && state != State.runAway)
        {
            state = (State)states[Random.Range(0, states.Length - 1)];
            if (selected) Debug.Log("state switch" + state);
            TmState = Random.Range(3000, 10000);
        }
    }
    
    private void UpdateCheckStuck()
    {
        if (rigidbody.velocity.magnitude < .5f && !CantMove)
        {
            if (timer.TimeElapsed(TmJump))
            {
                if (selected) Debug.Log("stuck jump");
                Jump();
            }
            if (timer.TimeElapsed(TmChange))
            {
                if (selected) Debug.Log("stuck change node");                
                //node = node.nodes.Random();
                lastNode = node;
                state = State.walk;
                NearestNode();
            }
        }
    }
    private void Jump()
    {
        pos += rot * (Vector3.back + Vector3.up) * .4f;       
        rigidbody.AddForce(Vector3.up * jumpPower);
    }
    public static T Cofient<T>(T[] a, float[] p)
    {
        var sum = p.Sum();
        for (int i = 0; i < p.Length; i++)
            p[i] = p[i] / sum;
        System.Random r = new System.Random();
        double diceRoll = r.NextDouble();
        double cumulative = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            cumulative += p[i];
            if (diceRoll < cumulative)
            {
                return a[i];
                //break;
            }
        }
        return default(T);
    }
    private void UpdateSelectNode()
    {
        if (Vector3.Distance(node.pos, pos) < 1)
        {
            var oldnode = node;
            var nds  = node.nodes.Where(a => a != null && a != lastNode);
            if(Random.value<.1f)
                node = nds.Random();
            else
                node = nds.OrderBy(a => Quaternion.Angle(rot, Quaternion.LookRotation(a.pos - pos))).FirstOrDefault();
   

            if (oldnode.Jump && node.Land)
                Jump();

            if (node == null)
                node = lastNode;
            lastNode = oldnode;
        }
        Debug.DrawLine(pos, node.pos, Color.red);
    }
    private void UpdateMove()
    {
        Vector3 dir = (node.pos - pos);
        if(state == State.attack)
            dir = _Player.pos - pos;
        if (state == State.runAway)
            dir = pos - _Player.pos;

        dir.y = 0;
        dir = dir.normalized;

        fakedir = Vector3.Lerp(fakedir, dir, .02f * rigidbody.velocity.magnitude);
        if (fakedir != Vector3.zero) transform.rotation = Quaternion.LookRotation(fakedir);

        var move = Vector3.zero;
        move += fakedir * Time.deltaTime * 130;
        if (state == State.run || state == State.runAway || state == State.attack && ground) move *= 2;
        if (CantMove) move *= 0;

        var rv = rigidbody.velocity;
        rv.y = 0;
        if (!land.enabled)
            rigidbody.AddForce((move - rv) * 10);
        //controller.Move(move);
    }
    public override void UpdateAnimations()
    {
        
        sneak.speed = walk.speed = Mathf.Max(1, rigidbody.velocity.magnitude);
        var v = rigidbody.velocity;
        //if (selected)
        //    Debug.Log();
        v.y = 0;
        if (state == State.sneak)
            an.CrossFade(sneak.name);
        else if (v.magnitude > 4f)
            an.CrossFade(run.name);
        else if (state == State.crouch)
            an.CrossFade(crouch.name);
        else 
            //if (v.magnitude > 1)
            an.CrossFade(walk.name);        
        
        //else
        //    an.CrossFade(idle.name);
        
        if (!ground)
            an.CrossFade(fall.name);

        base.UpdateAnimations();
    }
    
    public override void Damage()
    {
        base.Damage();
        onDetect();
        an.CrossFade(getHitA.name);
        life--;
    }
    private void NodeUpdate()
    {
        
    }
    private void CreateNut(GameObject prefab)
    {
        GameObject nut = (GameObject)Instantiate(prefab, pos + Vector3.up, Quaternion.identity);
        Rigidbody rig = nut.AddComponent<Rigidbody>();
        rig.constraints = RigidbodyConstraints.FreezeRotation;
        rig.velocity = (UnityEngine.Random.onUnitSphere + Vector3.up) * 4;
    }
    private void onDetect()
    {
        if (nuts + berries > 5)
            state = State.runAway;
        else
            state = State.attack;
    }
    private void NearestNode()
    {
        var order = nodes.Where(a => a.pos.y - pos.y < 1).OrderBy(a => Vector3.Distance(a.pos, pos));
        node = order.FirstOrDefault();
    }
    float groundy = -.1f;
    void OnCollisionExit(Collision col)
    {
        foreach (var a in col.contacts)
            if ((a.point - pos).y < groundy)
                ground = false;

    }
    void OnCollisionStay(Collision col)
    {

        foreach (var a in col.contacts)
        {
            //if(selected)
            //    Debug.Log((a.point - pos).y);
            if ((a.point - pos).y < groundy)
                ground = true;
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (Mathf.Abs(col.impactForceSum.y) > 8)
            an.CrossFade(land.name);        
    }
    bool CantMove
    {
        get { return state == State.crouch || getHitA.enabled; }
    }
}
