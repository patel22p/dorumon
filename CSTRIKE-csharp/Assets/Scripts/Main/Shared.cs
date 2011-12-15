using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Team { Spectators, Terrorists, CounterTerrorists, Zombies }
public enum PlType { Player, Bot, Monster,Fatty}
public class PlayerView
{
    public PlType plType;
    public int id;
    public int PlayerPing;
    public int PlayerFps;
    public int PlayerMoney = 16000;
    public int PlayerScore;
    public int PlayerDeaths;
    public string PlayerName = "";
    public int skin;
    public Team team;
    public bool bot { get { return plType != PlType.Player; } }
}

public class Shared : Bs
{
    
    public Node curNode;
    public LayerMask CantSeeThrough;
    public Bs model;

    protected Vector3 move;
    public Transform Head;
    public Transform MiniMapCursor;
    protected float EnemySeenTime;
    protected float nextShootTime;
    public Team TeamPath;    
    protected float NodeOffset;
    public Bs Cam;
    internal CharacterController controller;
    public AnimationCurve SpeedAdd;
    public Vector3 vel;
    public Vector3 syncPos;
    //public Vector3 syncVel;
    public Vector3 syncMove;
    //public char syncAnimState;
    internal int id = -1;
    public int Life = 100;
    public int Shield = 100;
    public float HitTime;
    public float yvel;
    protected float grounded;
    public float speeadd;
    protected bool syncUpdated;
    public float syncRotx;
    public float syncRoty;
    public float CreatedTime;
    public bool observing;
    public AudioClip[] dirtSound;
    public AudioClip[] dieSound;
    protected Shared visibleEnemy;
    public AudioClip[] hitSound;
    protected Animation an { get { return model.animation; } }
    protected AnimationState run { get { return an["run"]; } }
    protected AnimationState idle { get { return an["idle"]; } }
    public PlayerView pv { get { return _Game.playerViews[id]; } }
    public float CamRotX { get { return Cam.rotx; } set { Cam.rotx = value; } }
    protected bool isGrounded { get { return Time.time - grounded < .1f; } }
    public PlType PlType { get { return pv.plType; } }

    public override void Awake()
    {        
        CreatedTime = Time.time;
        base.Awake();
    }
    public virtual void Start()
    {

        if (IsMine)
        {
            CallRPC(SetPlType, PhotonTargets.All, (int)PlType);
            CallRPC(SetTeam, PhotonTargets.All, (int)pv.team);            
        }

        foreach (var a in GetComponentsInChildren<Rigidbody>())
            a.isKinematic = true;

        name = (!IsMine ? "Remote" : "Local") + PlType + id;

        controller = GetComponent<CharacterController>();
    }

    
    public virtual void Update()
    {

        if (controller.isGrounded) grounded = Time.time;
        if (_Game.pv.team == Team.Spectators || pv.team == _Game.pv.team)
            MiniMapCursor.renderer.enabled = true;
        else
            MiniMapCursor.renderer.enabled = false;
    }
    protected virtual Shared UpdateVisibleEnemy()
    {
        Shared near = enemies.FirstOrDefault(a => (a.pos - pos).magnitude < 5);
        if (near) return near;
        if (visibleEnemy == null || !PlayerVisible(visibleEnemy.hpos) || visibleEnemy.pv.team == pv.team)
        {
            var e = enemies.Where(a => PlayerVisible(a.hpos) && !(CantSeeBack && Vector3.Angle(transform.forward, a.hpos - hpos) > 90 && Time.time > HitTime + 3));
            return e.FirstOrDefault(a => (a.pos - pos).magnitude < 5) ?? e.Random();
        }
        return visibleEnemy;
    }
    public bool CantSeeBack;
    public bool PlayerVisible(Vector3 a)
    {
        return !Physics.Raycast(new Ray(hpos, a - hpos), Vector3.Distance(a, hpos), CantSeeThrough);
    }
    public Vector3 UpdateBotMoveDir()
    {
        var dir = ZeroYNorm(curNode.GetPos(NodeOffset) - pos);        
        return dir;
    }

    public Vector3 UpdateCheckDir(Vector3 dir)
    {
        if (Physics.RaycastAll(new Ray(cpos, dir), 1, 1 << LayerMask.NameToLayer("Level") | 1 << LayerMask.NameToLayer("Player")).Any(a => a.transform.root != this.transform))
        {
            dir = Quaternion.LookRotation(Time.time % 6 > 3 ? Vector3.left : Vector3.right) * dir;
            Debug.DrawRay(cpos, dir, Color.red);
        }
        return dir;
    }

    public void UpdateNode()
    {
        SearchForAnyNode:
        if (curNode == null)
        {
            curNode = _Game.levelEditor.paths.Where(a => a.team == TeamPath && a.plTypes.Contains(PlType)).SelectMany(a => a.nodes)
                .Where(a => !a.EndNode).Where(a => PlayerVisible(a.pos + Vector3.up * .1f)).Random();
            if (curNode == null) return;
            NodeOffset = Random.Range(-curNode.height, curNode.height);
            curNode.walkCount++;
        }

        if (ZeroY(curNode.GetPos(NodeOffset) - pos).magnitude < .5f)
        {
            curNode = curNode.Nodes.OrderBy(a => a.walkCount).FirstOrDefault();

            if (curNode == null)
                goto SearchForAnyNode;

            if (curNode.EndNode && TeamPath!= Team.Zombies) TeamPath = (TeamPath == Team.Terrorists ? Team.CounterTerrorists : Team.Terrorists);

            NodeOffset = Random.Range(-curNode.height, curNode.height);
            curNode.walkCount++;
        }
    }


    //public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.isWriting)
    //    {
    //        stream.SendNext(pos);
    //        stream.SendNext(CamRotX);
    //        stream.SendNext(roty);
    //        stream.SendNext(move);
    //    }
    //    else
    //    {
    //        syncPos = (Vector3) stream.ReceiveNext();
    //        syncRotx = (float) stream.ReceiveNext();
    //        syncRoty = (float) stream.ReceiveNext();
    //        syncMove = (Vector3) stream.ReceiveNext();

    //        if (stream.isReading)
    //            syncUpdated = true;
    //    }
    //}


    

    public virtual void LateUpdate()
    {
        observing = false;
    }

    public void Fade(AnimationState s)
    {
        an.CrossFade(s.name);
    }

    [RPC]
    public virtual void SetID(int id)
    {
        print("SetId"+id);
        this.id = id;
        _Game.shareds[id] = this;
        foreach (var a in this.GetComponentsInChildren<Bs>())
            a.SendMessage("OnSetID",SendMessageOptions.DontRequireReceiver);
    }
    
    //public new void print(object o)
    //{
    //    Debug.Log(name + ":" + o);
    //}

    [RPC]
    public virtual void SetPlayerScore(int score)
    {
        pv.PlayerScore = score;
    }

    [RPC]
    public virtual void SetTeam(int team)
    {
        TeamPath = pv.team = (Team)team;
    }

    [RPC]
    public virtual void SetPlType(int bot)
    {
        print("SetPlType "+bot);
        this.pv.plType = (PlType)bot;
    }
    
    [RPC]
    public virtual void SetLife(int life, int player)
    {
        if (!enabled) return;
        var pl = _Game.shareds[player];
        if (!_Game.immortal || !isEditor)
            Life = life;
        HitTime = Time.time;
        audio.PlayOneShot(hitSound.Random(), 3);
        if (Life <= 0)
        {          
            if (IsMine)
            {
                if (pl != null)
                    _Game.CallRPC(_Game.RpcKillText, PhotonTargets.All, (pl.pv.PlayerName + " Killed " + pv.PlayerName));
                if (this == _Player && pl is Player)
                    _ObsCamera.pl = (Player)pl;
                if (pl != this)
                    pl.CallRPC(pl.SetPlayerScore, PhotonTargets.All, pl.pv.PlayerScore + 1);
                CallRPC(SetPlayerDeaths, PhotonTargets.All, pv.PlayerDeaths + 1);
            }
            if (IsMine)
                CallRPC(Die, PhotonTargets.All);
        }
    }
    public override void OnEditorGui()
    {
        if (GUILayout.Button("Kill"))
            CallRPC(SetLife, PhotonTargets.All, 0, id);
        base.OnEditorGui();
    }
    [RPC]
    public virtual void Die()
    {
        if (!enabled) return;
        print("Die" + pv.PlayerName);
        audio.PlayOneShot(dieSound.Random(), 6);
        Destroy(model.animation);

        var nm = (Transform)Instantiate(model.transform, model.pos, model.rot);
        foreach (var a in nm.GetComponentsInChildren<SkinnedMeshRenderer>())
            a.updateWhenOffscreen = true;
        nm.SetLayer(LayerMask.NameToLayer("Dead"));
        foreach (var a in nm.GetComponentsInChildren<Rigidbody>())
            a.isKinematic = false;
        nm.parent = _Game.Fx;
        Destroy(nm.gameObject, 10);

        enabled = false;
        if (IsMine)
        {
            //_Game.timer.AddMethod(delegate
            //{
            PhotonNetwork.RemoveRPCs(photonView);
            PhotonNetwork.Destroy(gameObject);
            //});
        }
    }

    public void OnSetID()
    {
        enabled = true;
    }
    public bool bot
    {
        get
        {
            return PlType != PlType.Player;
        }
    }
    public Vector3 hpos { get { return Head.position; } }
    public Vector3 cpos { get { return (Head.position + pos) / 2f; } }
    public IEnumerable<Shared> enemies
    {
        get
        {
            return _Game.Shareds.Where(a => a.pv != null && a.pv.team != pv.team);
        }
    }

    [RPC]
    public virtual void SetPlayerDeaths(int PlayerDeaths)
    {
        pv.PlayerDeaths = PlayerDeaths;
    }
    public void WalkSound()
    {
        if (isGrounded)
            audio.PlayOneShot(dirtSound.Random(), observing ? .5f : 1);
    }
}