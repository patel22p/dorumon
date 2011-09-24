using System.Linq;
using UnityEngine;
using System.Collections;

public class Monster : Shared {
    
    
    
    public void Update()
    {
        UpdateBot();
    }

    protected override void UpdateBot()
    {

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
                NodeOffset = Random.Range((float)((int)(-curNode.height)), (int)curNode.height);
                curNode.walkCount++;

            }
            //rot
            Quaternion r = Quaternion.identity;
            if (visibleEnemy != null)
            {
                Debug.DrawRay(pos, visibleEnemy.pos - pos, Color.red);
                EnemySeenTime = Time.time;
                r = Quaternion.LookRotation(visibleEnemy.pos - pos);
            }
            if (Time.time - EnemySeenTime > 3 || visibleEnemy == null)
                r = Quaternion.LookRotation(ZeroY(curNode.GetPos(NodeOffset) - pos));

            var dir = ZeroY(curNode.GetPos(NodeOffset) - pos);
            if (Physics.Raycast(new Ray(pos, dir), 1, 1 << LayerMask.NameToLayer("Player")))
                dir = Quaternion.LookRotation(Vector3.left) * dir;


            //shoot
            if (visibleEnemy != null)
            {
                if (Time.time > nextShootTime && !gun.handsReload.enabled)
                {
                    if (Time.time > nextShootTime + .5f)
                        nextShootTime = Time.time + Random.Range(0, 2f);
                    move = Vector3.zero;
                    gun.MouseDown();
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        if (syncUpdated)
        {
            vel = syncVel;
            move = syncMove;
        }

        if (move.magnitude > 0 && isGrounded)
            vel += rot * move;

        controller.SimpleMove(vel);

        if (isGrounded)
            vel *= .83f;
        

        if (syncUpdated)
            controller.Move(syncPos - pos);

        syncUpdated = false;
    }

    public override void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            syncPos = pos;
            syncRoty = roty;
            syncVel = vel;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRoty);
        stream.Serialize(ref syncVel);
        if (stream.isReading)
            syncUpdated = true;

    }
}
