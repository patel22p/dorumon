using System.Linq;
using UnityEngine;
using System.Collections;
using doru;

public class Raccoon : Shaded {

    TimerA timer = new TimerA();
    Node[] nodes;


    AnimationState crouch { get { return an["crouch"]; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState walk { get { return an["walk"]; } }
    AnimationState fll { get { return an["midair"]; } }
    AnimationState land { get { return an["landing"]; } }
    AnimationState run { get { return an["run"]; } }
    public override void Start () {

        base.Start();
        fll.wrapMode = idle.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        land.wrapMode = WrapMode.Clamp;
        land.layer = fll.layer = 1;
        nodes = _MyNodes.transform.Cast<Transform>().Select(a => a.gameObject.GetComponent<Node>()).ToArray();
        var order = nodes.OrderBy(a => Vector3.Distance(a.pos, pos));
        node = order.FirstOrDefault();
        if (node == null) { enabled = false; Debug.Log("No nodes"); }

        TmChange = Random.Range(1000, 5000);
        TmJump = Random.Range(1000, 3000) ;
        //state = State.run;
	}
    Node node;
    Node lastNode;
    Vector3 fakedir;

    int TmJump;
    int TmChange;

    public enum State { walk, run, crouch } ;
    public State state;
    int TmState;
    public override void Update()
    {
        if (timer.TimeElapsed(TmState))
        {
            state = (State)Random.Range(0, 3);
            TmState = Random.Range(3000, 10000);
        }
        base.Update();
        
        if (Vector3.Distance(node.pos, pos) < 1)
        {
            var oldnode = node;
            node = node.nodes.Where(a => a != null && a != lastNode).Random();
            if (oldnode.Jump && node.Land)
                vel = Vector3.up * oldnode.JumpPower;            

            if (node == null)
                node = lastNode;
            lastNode = oldnode;
        }
        if (controller.velocity.magnitude < .5f && state!= State.crouch)
        {
            if (timer.TimeElapsed(TmJump))
                vel = Vector3.up * 7f;
            if (timer.TimeElapsed(TmChange))
            {
                node = node.nodes.Random();
                lastNode = node;
            }
        }

        if (fll.enabled && controller.isGrounded)
            fll.enabled = false;

        if (controller.isGrounded)
            vel *= .86f;


        Debug.DrawRay(node.pos, Vector3.one);
        Vector3 dir = (node.pos - pos);
        dir.y = 0;
        dir = dir.normalized;
         
        fakedir = Vector3.Lerp(fakedir, dir, .02f);
        if (fakedir != Vector3.zero) transform.rotation = Quaternion.LookRotation(fakedir);
        UpdateMove();
        timer.Update();
	}

    private void UpdateMove()
    {
        var move = Vector3.zero;
        move += fakedir * Time.deltaTime * 2;
        if (state == State.run) move *= 3;
        if (state == State.crouch) move *= 0;       

        move += vel * Time.deltaTime;
        vel += Physics.gravity * Time.deltaTime;
        controller.Move(move);
        
    }

    public override void AnimationsUpdate()
    {

        var v = controller.velocity;
        v.y = 0;
        //Debug.Log(v.magnitude);

        walk.speed = controller.velocity.magnitude;
        if (v.magnitude > 5f)
            an.CrossFade(run.name);
        else if (v.magnitude > 0)
            an.CrossFade(walk.name);
        else if (state == State.crouch)
            an.CrossFade(crouch.name);
        else
            an.CrossFade(idle.name);

        if (vel.y > 1f)
            an.CrossFade(fll.name);


        base.AnimationsUpdate();
    }
    private void NodeUpdate()
    {
        
    }
    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Coll Enter");
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!controller.isGrounded)
            if (Mathf.Abs(controller.velocity.y) > 3)
            {
                an.CrossFade(land.name);
                vel = Vector3.zero;
            }
    }
}
