using System.Linq;
using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;

public class Raccoon : Shared
{

    
    
    [FindAsset("Nut_1p")]
    public GameObject nutPrefab;
    [FindAsset("Berry_5p")]   
    public GameObject berryPrefab;
    public enum State { walk = 0, run = 1, sneak = 2, crouch = 3 } ;
    public State state;

    int[] states = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3 };
    TimerA timer = new TimerA();
    Node[] nodes;

    AnimationState sneak { get { return an["sneak"]; } }
    AnimationState crouch { get { return an["crouch"]; } }
    AnimationState getHitA { get { return an["gethitA"]; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState walk { get { return an["walk"]; } }
    AnimationState fll { get { return an["midair"]; } }
    AnimationState land { get { return an["landing"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState death { get { return an["death"]; } }

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
    
    public override void Update()
    {
        base.Update();
        UpdateOther();
        UpdateSelectNode();
        UpdateCheckStuck();
        UpdateMove();
        timer.Update();
    }
    private void SetupOther()
    {
        nodes = _MyNodes.transform.Cast<Transform>().Select(a => a.gameObject.GetComponent<Node>()).ToArray();
        var order = nodes.OrderBy(a => Vector3.Distance(a.pos, pos));
        node = order.FirstOrDefault();
        if (node == null) { enabled = false; Debug.Log("No nodes"); }
        TmChange = Random.Range(1000, 5000);
        TmJump = Random.Range(1000, 3000);
    }
    private void SetupLayers()
    {
        fll.wrapMode = idle.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        sneak.wrapMode = WrapMode.Loop;
        land.wrapMode = WrapMode.Clamp;
        getHitA.wrapMode = WrapMode.Clamp;
        death.wrapMode = WrapMode.ClampForever;

        death.layer = 2;
        getHitA.layer = land.layer = fll.layer = 1;
    }
    void UpdateOther()
    {
        if (life <= 0)
        {
            Die();
            return;
        }

        if (timer.TimeElapsed(TmState))
        {

            state = (State)states[Random.Range(0, states.Length - 1)];
            if (selected) Debug.Log("state switch" + state);
            TmState = Random.Range(3000, 10000);
        }
    }

    private void Die()
    {
        SetLayer(LayerMask.NameToLayer("Dead"));
        _Game.shareds.Remove(this);
        for (int i = 0; i < nuts; i++)
            CreateNut(nutPrefab);
        for (int i = 0; i < berries; i++)
            CreateNut(berryPrefab);
        enabled = false;
        an.CrossFade(death.name);
    }

    private void CreateNut(GameObject prefab)
    {
        GameObject nut = (GameObject)Instantiate(prefab, pos + Vector3.up, Quaternion.identity);
        Rigidbody rig = nut.AddComponent<Rigidbody>();
        rig.constraints = RigidbodyConstraints.FreezeRotation;
        rig.velocity = (UnityEngine.Random.onUnitSphere + Vector3.up) * 4;
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
                node = node.nodes.Random();
                lastNode = node;
            }
        }
    }

    private void Jump()
    {
        rigidbody.AddForce(Vector3.up * JumpPower);
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
        if (Vector3.Distance(node.pos, pos) < .5f)
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
        //if (fll.enabled && controller.isGrounded)
        //    fll.enabled = false;

        //if (controller.isGrounded)
        //    vel *= .86f;

        Vector3 dir = (node.pos - pos);
        dir.y = 0;
        dir = dir.normalized;

        fakedir = Vector3.Lerp(fakedir, dir, .02f * rigidbody.velocity.magnitude);
        if (fakedir != Vector3.zero) transform.rotation = Quaternion.LookRotation(fakedir);

        var move = Vector3.zero;
        move += fakedir * Time.deltaTime * 2;
        if (state == State.run) move *= 3;
        if (CantMove) move *= 0;

        //move += vel * Time.deltaTime;
        //vel += Physics.gravity * Time.deltaTime;
        move.y += rigidbody.velocity.y;

        rigidbody.velocity = move;
        //controller.Move(move);

    }

    public void Hit()
    {
        an.CrossFade(getHitA.name);
        life --;
    }
    
    public override void UpdateAnimations()
    {

        sneak.speed = walk.speed = rigidbody.velocity.magnitude;
        var v = rigidbody.velocity;
        v.y = 0;
        if (state == State.sneak)
            an.CrossFade(sneak.name);
        else if (v.magnitude > 5f)
            an.CrossFade(run.name);
        else if (v.magnitude > 0)
            an.CrossFade(walk.name);        
        else if (state == State.crouch)
            an.CrossFade(crouch.name);
        else
            an.CrossFade(idle.name);

        if (rigidbody.velocity.y > 1f)
            an.CrossFade(fll.name);


        base.UpdateAnimations();
    }
    private void NodeUpdate()
    {
        
    }
    void OnCollisionEnter(Collision col)
    {
        
        if (Mathf.Abs(col.frictionForceSum.y) > 3)
        {
            fll.enabled = false;
            an.CrossFade(land.name);
            //vel = Vector3.zero;
        }
    }
    //void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (!controller.isGrounded)
    //        if (Mathf.Abs(controller.velocity.y) > 3)
    //        {
    //            an.CrossFade(land.name);
    //            //vel = Vector3.zero;
    //        }
    //}
    bool CantMove
    {
        get { return state == State.crouch || getHitA.enabled; }
    }
}
