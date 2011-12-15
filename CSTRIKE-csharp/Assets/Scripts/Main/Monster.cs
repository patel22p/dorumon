using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
public class Monster : Shared
{
    public float range = 2.5f;
    protected AnimationState[] attack { get { return new[] { an["attack1"], an["attack2"] }; } }
    private AnimationState sight { get { return an["sight"]; } }
    protected AnimationState walk { get { return an["walk"]; } }
    public AudioClip[] sighSounds;
    public enum AnimState { Run, Walk, Idle }
    public AnimState animState;
    public float runSpeed = 1;
    public float walkSpeed = 1;
    float sighTime;
    Vector3 EnemySeenPos;
    public override void Awake()
    {
        base.Awake();
        enabled = false;
        print("Monster Awake " + id);        
    }
    public override void Start()
    {
       //note set size
       //note custom speed
        InitAnimations();
        base.Start();
    }
    private void InitAnimations()
    {
        foreach (var a in attack)
            a.layer = 1;
        if (sight != null)
            sight.layer = 1;
        run.speed = runSpeed;
        walk.speed = walkSpeed;
        walk.wrapMode = idle.wrapMode =run.wrapMode = WrapMode.Loop;
    }
    public override void Update()
    {
        
        base.Update();
        if (IsMine)
            UpdateBot();
        UpdateOther();
    }
    private void UpdateOther()
    {
        if (sight != null && sighTime < Time.time && IsMine)
        {
            sighTime = Time.time + Random.Range(10, 20);
            CallRPC(Sigh, PhotonTargets.All);
        }
        if (an != null)
        {
            if(animState== AnimState.Idle)
                Fade(idle);
            if (animState == AnimState.Run)
                Fade(run);
            if (animState == AnimState.Walk)
                Fade(walk);
        }
    }
    [RPC]
    private void Sigh()
    {
        an.Blend(sight.name);
        audio.PlayOneShot(sighSounds.Random());
    }
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient) return;
        //_Game.timer.AddMethod(delegate
        //{
        CallRPC(SetPlType, player, (int)PlType);
        CallRPC(SetTeam, player, (int)pv.team);
        CallRPC(SetPlayerDeaths, player, pv.PlayerDeaths);
        CallRPC(SetPlayerScore, player, pv.PlayerScore);
        CallRPC(SetLife, player, Life, id);
        //});
    }
    protected void UpdateBot()
    {
        //if (_Game.timer.TimeElapsed(1000))
        //{
        //    if(oldPos!=Vector3.zero && (pos-oldPos).magnitude<1){}
        //    oldPos = pos;
        //}

        animState = AnimState.Idle;
        if (CreatedTime + 3 > Time.time) return;
        if (_Game.timer.TimeElapsed(3000) || visibleEnemy == null)
        {
            var old = UpdateVisibleEnemy();
            if (old != this.visibleEnemy && old == null)
                curNode = null;
            this.visibleEnemy = old;
        }

        if (enemies.Count() > 0 || !_Game.GameStarted)
        {
            //selectNode
            UpdateNode();
            if (curNode == null)
                return;
            //move
            var dir = UpdateBotMoveDir();
            if (Time.time < EnemySeenTime + 3)
                dir = ZeroYNorm(EnemySeenPos - pos);
            dir = UpdateCheckDir(dir);
            Debug.DrawLine(pos, curNode.GetPos(NodeOffset), Color.green);

            animState = visibleEnemy == null ? AnimState.Walk : AnimState.Run;
            rot=Quaternion.Lerp(rot, Quaternion.LookRotation(dir), Time.deltaTime * 5);

            //atack
            if (visibleEnemy != null)
            {
                EnemySeenTime = Time.time;
                EnemySeenPos = visibleEnemy.pos;
                if (Vector3.Distance(pos, visibleEnemy.pos) < range && !attack.Any(a => a.enabled))
                    CallRPC(PlayAttack, PhotonTargets.All);
            }

        }
                    

        
    }
    [RPC]
    private void PlayAttack()
    {
        an.Blend(attack.Random().name);
    }
    public void Attack()
    {
        if (visibleEnemy != null && Vector3.Distance(pos, visibleEnemy.pos) < range * 1.1f)
        {
            if (visibleEnemy.IsMine)
                visibleEnemy.CallRPC(visibleEnemy.SetLife, PhotonTargets.All, visibleEnemy.Life - 40, id);
        }
    }
    public Transform orgin;
    public Vector3 LastMove;
    public override void LateUpdate()
    {

        var mv = orgin.parent.rotation * (orgin.localPosition - LastMove);
        if (mv.magnitude < .3f)
            controller.SimpleMove(mv / Time.deltaTime);

        LastMove = orgin.localPosition;        
        orgin.localPosition = Vector3.zero;

        base.LateUpdate();
    }
    
    [RPC]
    public override void SetID(int id)
    {
        base.SetID(id);
    }
    [RPC]
    public override void SetPlayerScore(int score)
    {
        base.SetPlayerScore(score);
    }
    [RPC]
    public override void SetPlType(int bot)
    {
        base.SetPlType(bot);
    }
    [RPC]
    public override void SetTeam(int team)
    {
        base.SetTeam(team);
    }
    [RPC]
    public override void SetLife(int life, int player)
    {
        base.SetLife(life, player);
    }
    //public override void SetID(int id)
    //{
    //    throw new NotImplementedException();
    //}

    //public override void SetPlayerScore(int score)
    //{
    //    throw new NotImplementedException();
    //}

    //public override void SetTeam(int team)
    //{
    //    throw new NotImplementedException();
    //}

    //public override void SetPlayerDeaths(int PlayerDeaths)
    //{
    //    throw new NotImplementedException();
    //}
    
    [RPC]
    public override void SetPlayerDeaths(int PlayerDeaths)
    {
        base.SetPlayerDeaths(PlayerDeaths);
    }
    public void FixedUpdate()
    {
        if (syncUpdated)
        {
            if ((syncPos - pos).magnitude > 2)
                pos = syncPos;
            else
                controller.Move(syncPos - pos);
        }

        syncUpdated = false;
    }
    [RPC]
    public override void Die()
    {
        base.Die();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        //if (!enabled) return;
        if (stream.isWriting)
        {
            stream.SendNext(pos);
            stream.SendNext(roty);
            stream.SendNext((byte)animState);
            //syncPos = pos;
            //syncRoty = roty;
            //syncAnimState = (char)animState;
        }
        
        //stream.Serialize(ref syncAnimState);
        //stream.Serialize(ref syncPos);
        //stream.Serialize(ref syncRoty);
        
        if (stream.isReading)
        {
            syncUpdated = true;
            syncPos = (Vector3) stream.ReceiveNext();
            roty = (float)stream.ReceiveNext();
            animState = (AnimState)(byte)stream.ReceiveNext();
            //roty = syncRoty;
            //animState = (AnimState)syncAnimState;
        }

    }
}
