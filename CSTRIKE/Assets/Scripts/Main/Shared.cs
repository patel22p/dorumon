using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Shared : Bs
{
    public Bs model;
    internal int id = -1;
    public int Life = 100;
    public new Camera camera;
    internal Gun gun;
    protected Vector3 move;
    public int left = -65;
    public int right = 95;
    protected Path path;
    protected Node curNode;
    protected float EnemySeenTime;
    protected float nextShootTime;
    
    protected List<Player> LastShooted = new List<Player>();
    protected float NodeOffset;
    public Bs Cam;
    internal CharacterController controller;
    public AnimationCurve SpeedAdd;
    public Vector3 vel;
    public Vector3 syncPos;
    public Vector3 syncVel;
    public Vector3 syncMove;
    public float HitTime;
    public float yvel;
    protected float grounded;
    public float speeadd;
    protected bool syncUpdated;
    public float syncRotx;
    public float syncRoty;
    protected Animation an { get { return model.animation; } }
    protected AnimationState run { get { return an["run"]; } }
    protected AnimationState idle { get { return an["idle"]; } }
    public PlayerView pv { get { return _Game.playerViews[id]; } }
    public float CamRotX { get { return Cam.rotx; } set { Cam.rotx = value; } }
    protected bool isGrounded { get { return Time.time - grounded < .1f; } }


    protected virtual void UpdateBot()
    {
        //note add hold time
        //note add atack nodes when shooting
        
        var enemies = _Game.Players.Where(a => a.pv.team != pv.team)
            .Union(LastShooted.Where(a => a != null))
            .OrderBy(a => Vector3.Distance(pos, a.pos));
        
        var visibleEnemy = enemies.FirstOrDefault(a => !Physics.Raycast(new Ray(pos, a.pos - pos), Vector3.Distance(a.pos, pos), 1 << LayerMask.NameToLayer("Level"))
                                                       && !Physics.Raycast(new Ray(pos, (a.pos - pos) + Vector3.up), Vector3.Distance(a.pos, pos), 1 << LayerMask.NameToLayer("Level")));

        if (enemies.Count() > 0)
        {
            //selectNode
            if (path == null)
            {
                path = _Game.levelEditor.paths.Where(a => Vector3.Distance(pos, a.StartNode.pos) < 20).Random();
                path.walkCount++;                
                curNode = path.nodes.OrderBy(a => Vector3.Distance(pos, a.pos)).FirstOrDefault();
                curNode.walkCount++;
            }

            if (Vector3.Distance(curNode.GetPos(NodeOffset), pos) < 2)
            {
                curNode = curNode.Nodes.OrderBy(a => a.walkCount).FirstOrDefault();
                if (curNode == null)
                {
                    path = null;
                    return;
                }
                NodeOffset = Random.Range((float) ((int) (-curNode.height)), (int) curNode.height);
                curNode.walkCount++;

            }

            Vector3 e = Vector3.zero;
            //rotate
            if (visibleEnemy != null)
            {
                Debug.DrawRay(pos, visibleEnemy.pos - pos, Color.red);
                EnemySeenTime = Time.time;
                e = Quaternion.LookRotation(visibleEnemy.pos - pos).eulerAngles;
            }
            if (Time.time - EnemySeenTime > 3 || visibleEnemy == null)
                e = Quaternion.LookRotation(ZeroY(curNode.GetPos(NodeOffset) - pos)).eulerAngles;
            var CamerRot = this.camera.transform.eulerAngles;
            Vector3 MouseDelta = new Vector3(Mathf.DeltaAngle(CamerRot.x, e.x),
                                             Mathf.DeltaAngle(CamerRot.y, e.y), 0) * Time.deltaTime * 10;

            ////move

            var dir = ZeroY(curNode.GetPos(NodeOffset) - pos);
            //if bot cross
            if (Physics.Raycast(new Ray(pos, dir), 1, 1 << LayerMask.NameToLayer("Player")))
                dir = Quaternion.LookRotation(Vector3.left) * dir;

            Debug.DrawLine(pos, curNode.GetPos(NodeOffset), Color.green);            
            move = ZeroY(Quaternion.Inverse(this.camera.transform.rotation) * dir);

            CamRotX += MouseDelta.x;
            Rotate(MouseDelta.y);

            ////shoot
            if (visibleEnemy != null)
            {
                //note depends distance
                if (Time.time > nextShootTime && !gun.handsReload.enabled)
                {
                    if (Time.time > nextShootTime + .5f)
                        nextShootTime = Time.time + Random.Range(0, 2f);
                    move = Vector3.zero;
                    gun.MouseDown();
                }
            }
        }
        else
            move = Vector3.zero;
    }

    protected void Rotate(float d)
    {
        //todo smooth rotate
        roty += d;
        model.lroty = Mathf.Clamp(clampAngle(model.lroty - d), left, right);
    }

    public virtual void FixedUpdate()
    {
        if (syncUpdated)
        {
            vel = syncVel;
            move = syncMove;
        }

        if (move.magnitude > 0 && isGrounded)
        {            
            speeadd = Mathf.Max(0f, SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move)));
            vel += rot * move * speeadd * (Time.time - HitTime < 1 ? .5f : 1);
        }
        controller.SimpleMove(vel);
        if (isGrounded)
            vel *= .83f;


        if (yvel > 0f)
            controller.Move(new Vector3(0, yvel, 0));
        if (syncUpdated)
            controller.Move(syncPos - pos);
        syncUpdated = false;
    }

    public virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            syncPos = pos;
            syncRotx = CamRotX;
            syncRoty = roty;
            syncVel = vel;
            syncMove = move;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRotx);
        stream.Serialize(ref syncRoty);
        stream.Serialize(ref syncVel);
        stream.Serialize(ref syncMove);
        if (stream.isReading)
            syncUpdated = true;

    }
}