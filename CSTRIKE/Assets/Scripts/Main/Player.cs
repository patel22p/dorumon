using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum Team { Spectators, Terrorists, CounterTerrorists }
public class Player : Bs
{
    
    public new Camera camera;
    internal Gun gun;
    internal CharacterController controller;
    public Transform[] TerrorSkins;
    public Transform[] CTerrorSkins;
    public Bs model;
    public Bs Cam;
    public Transform UpperBone;
    public Transform CamRnd;
    public Transform p_gun;
    public Transform MiniMapCursor;
    public GameObject Plane;
    public GameObject sparks;
    public GameObject MuzzleFlash;
    public GameObject MuzzleFlash2;
    public GameObject BloodPrefab;
    public Material[] BulletHoleMaterials;
    public Material[] MuzzleFlashMaterials;
    public Material[] BloodDecals;
    public AudioClip[] dirtSound;
    public AudioClip[] dieSound;
    public AudioClip[] headShootSound;
    public AudioClip[] hitSound;
    public AudioClip[] fallSound;
    public PlayerView pv { get { return _Game.playerViews[id]; } }
    public Transform[] RagDoll;
    public List<Renderer> playerRenders = new List<Renderer>();
    public Renderer[] gunRenders;
    Renderer[] SkinRenderers = new Renderer[] { };
    private Animation an { get { return model.animation; } }
    private AnimationState idle { get { return an["idle"]; } }
    private AnimationState run { get { return an["run"]; } }
    private AnimationState jump { get { return an["jump"]; } }
    public Animation customAnim { get { return animation; } }
    public AnimationCurve SpeedAdd;
    public Vector3 vel;
    Vector3 move;
    public Vector3 syncPos;
    public Vector3 syncVel;
    public Vector3 syncMove;
    public int left = -65;
    public int right = 95;
    public int Life = 100;
    public int Shield = 100;    
    internal int id = -1;
    
    public float HitTime;
    public float yvel;
    private float grounded;
    public float speeadd;
    public float VelM;
    public float syncRotx;
    public float syncRoty;
    bool syncUpdated;
    private bool PlayerRenderersActive = true;
    private bool GunRenderersActive = true;
    public bool observing;
    public bool imortality;

    public override void Awake()
    {
        enabled = false;
        print("Player Awake " + id);        
        gun = GetComponentInChildren<Gun>();
        gun.pl = this;
    }
    public void OnSetID()
    {
        enabled = true;
    }

    public void Start()
    {
        if (IsMine && !bot) _Player = this;
        IgnoreAll("IgnoreColl");
        if (IsMine)
        {
            CallRPC(SetBot, RPCMode.All, bot);
            CallRPC(SetTeam, RPCMode.All, (int)pv.team);
            CallRPC(SetSkin, RPCMode.All, pv.skin);
        }
        foreach (var a in GetComponentsInChildren<Rigidbody>())
            a.isKinematic = true;

        InitAnimations();
        controller = GetComponent<CharacterController>();
        model.animation.cullingType = IsMine ? AnimationCullingType.BasedOnClipBounds : AnimationCullingType.AlwaysAnimate;
        if (!IsMine)
            name = (bot ? "RemoteBot" : "RemotePlayer") + id;
        else
            name = (bot ? "Bot" : "Player") + id;        
    }


    private void LoadSkin()
    {

        var nwModelPrefab = (pv.team == Team.CounterTerrorists ? CTerrorSkins : TerrorSkins)[this.pv.skin];
        var nwModel = ((Transform)Instantiate(nwModelPrefab, model.pos, model.rot));
        foreach (var a in nwModel.GetComponentsInChildren<Renderer>())
            playerRenders.Add(a);

        List<Transform> last = new List<Transform>();
        foreach (var a in nwModel.transform.Find("Bip01").GetTransforms().Reverse().ToArray())
        {
            var t = model.transform.GetTransforms().FirstOrDefault(b => b.name == a.name);
            last.Add(a);
            if (t != null)
            {
                foreach (var b in last)
                {
                    b.parent = t;
                    b.localRotation = Quaternion.identity;
                    b.localPosition = Vector3.zero;
                }
                last.Clear();
            }
        }
        Destroy(model.transform.Find("smdimport").gameObject);
        nwModel.Find("smdimport").parent = model.transform;
        Destroy(nwModel.gameObject);
    }
    public void OnPlayerConnected(NetworkPlayer player)
    {
        if (!Network.isServer) return;
        //_Game.timer.AddMethod(delegate
        //{
        CallRPC(SetBot, player, bot);
        CallRPC(SetTeam, player, (int)pv.team);
        CallRPC(SetSkin, player, bot || Offline ? Random.Range(0, 3) : pv.skin);
        CallRPC(SetPlayerDeaths, player, pv.PlayerDeaths);
        CallRPC(SetPlayerScore, player, pv.PlayerScore);
        CallRPC(SetMoney, player, pv.PlayerMoney);
        CallRPC(SetLife, player, Life, id);
        //});
    }


    private void InitAnimations()
    {
        idle.speed = .5f;
        run.wrapMode = WrapMode.Loop;
        jump.wrapMode = WrapMode.Once;


        foreach (var b in blends)
        {
            b.AddMixingTransform(UpperBone);
            b.layer = 2;
        }

        blends[5].layer = 1;
        jump.layer = 1;
    }
    
    [RPC]
    private void SetMoney(int Money)
    {        
        this.pv.PlayerMoney = Money;
    }
    [RPC]
    public void SetTeam(int team)
    {
        this.pv.team = (Team)team;
    }

    [RPC]
    public void SetID(int id)
    {
        print("SetId"+id);
        this.id = id;
        _Game.players[id] = this;
        foreach (var a in this.GetComponentsInChildren<Bs>())
            a.SendMessage("OnSetID",SendMessageOptions.DontRequireReceiver);
    }

    [RPC]
    private void SetBot(bool bot)
    {
        print("SetBot "+bot);
        this.pv.bot = bot;
    }
 
    [RPC]
    private void SetSkin(int skin)
    {
        pv.skin = skin;
        LoadSkin();
    }        

    public void Update()
    {
        //todo add bomb
        UpdateJump();
        UpdateMove();
        UpdateRotation();
        if (IsMine)
            if (bot)
                UpdateBot();
            else
                UpdateInput();
        UpdateOther();
    }
    private void UpdateOther()
    {
        if (posy < -20)
            CallRPC(Die, RPCMode.All);

        if (_Game.pv.team == Team.Spectators || pv.team == _Game.pv.team)
            MiniMapCursor.renderer.enabled = true;
        else
            MiniMapCursor.renderer.enabled = false;

        if (!observing)
        {
            SetGunRenderersActive(false);
            SetPlayerRendererActive(true);
        }

        if (observing)
        {
            if (_Game.timer.TimeElapsed(1000))
                CallRPC(SetFPS, RPCMode.All, (int)_Game.timer.GetFps(), (Network.isServer || Offline) ? -1 : Network.GetLastPing(Network.connections[0]));

            var ray = camera.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            _Hud.PlayerName.text = "";
            foreach (var h in Physics.RaycastAll(ray, 1000, 1 << LayerMask.NameToLayer("Player")))
                if (h.transform.root != this.transform)
                {
                    var pl = h.transform.root.GetComponent<Player>();
                    if (pl != null && pl.pv.team == pv.team)
                        _Hud.PlayerName.text = pl.pv.team == pv.team ? ("Ally Life:" + pl.Life) : "Enemy" + " " + pl.pv.PlayerName;
                    break;
                }
        }
    }
    public void LateUpdate()
    {
        observing = false;
    }
    public void FixedUpdate()
    {
        if (syncUpdated)
        {
            vel = syncVel;
            move = syncMove;
        }

        if (move.magnitude > 0 && isGrounded)
        {
            speeadd = Mathf.Max(0, SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move)));
            VelM = vel.magnitude;
            vel += rot * move * speeadd * (Time.time - HitTime < 1 ? .5f : 1);
        }
        if (isGrounded)
            vel *= .83f;
        controller.SimpleMove(vel);


        if (yvel > 0f)
            controller.Move(new Vector3(0, yvel, 0));
        if (syncUpdated)
            controller.Move(syncPos - pos);
        syncUpdated = false;
    }
    private void UpdateJump()
    {
        if (controller.isGrounded) grounded = Time.time;
        if (isGrounded && jump.weight > 0)
            jump.weight *= .86f;
    }
    private void UpdateRotation()
    {
        if (!IsMine)
        {
            CamRotX = Mathf.LerpAngle(CamRotX, syncRotx, Time.deltaTime * 10);
            var nwroty = Mathf.LerpAngle(roty, syncRoty, Time.deltaTime * 10);
            var d = nwroty - roty;
            Rotate(d);
        }
        Vector3 CamxModely = clampAngles(new Vector3(CamRotX, model.lroty, 0));
        var numpad = new Vector3(
            (Mathf.Clamp(CamxModely.y / -left, -1, 0) + Mathf.Clamp(CamxModely.y / right, 0, 1)),
            -(Mathf.Clamp(CamxModely.x / 45, -1, 0) + Mathf.Clamp(CamxModely.x / 45, 0, 1)));


        foreach (var b in blends)
        {
            if (gun.shooting)
                b.speed = 1;
            else
            {
                b.speed = 0;
                b.time = 0;
            }
        }
        SetWeight(blends[5], 1);
        SetWeight(blends[6], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.left)));
        SetWeight(blends[4], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.right)));
        SetWeight(blends[2], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.up)));
        SetWeight(blends[8], Mathf.Max(0, 1 - Vector3.Distance(numpad, Vector3.down)));
        SetWeight(blends[1], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.right + Vector3.up))));
        SetWeight(blends[7], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.right + Vector3.down))));
        SetWeight(blends[3], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.left + Vector3.up))));
        SetWeight(blends[9], Mathf.Max(0, 1 - Vector3.Distance(numpad, (Vector3.left + Vector3.down))));
    }
    private void UpdateMove()
    {

        if (vel.magnitude > .1f)
        {
            var lookRot = Quaternion.LookRotation(vel);
            float deltaAngle = Mathf.DeltaAngle(lookRot.eulerAngles.y, rot.eulerAngles.y);
            if (Mathf.Abs(deltaAngle) > 110)
            {
                model.rot = Quaternion.LookRotation(vel * -1);
                run.speed = -2f * (vel.magnitude / 5);
            }
            else
            {
                model.rot = Quaternion.LookRotation(vel);
                run.speed = 2f * (vel.magnitude / 5);
            }

            Fade(run);
            gun.AnimateRun();
        }
        else
        {
            Fade(idle);
            gun.AnimateIdle();
        }

    }
    
    Path path;
    Node curNode;
    float EnemySeenTime;
    float nextShootTime;
    Vector3 e;

    List<Player> LastShooted = new List<Player>();

    private void UpdateBot()
    {

        //note add hold time
        //note add node width
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

            if (Vector3.Distance(curNode.pos, pos) < 2)
            {
                curNode = curNode.Nodes.OrderBy(a => a.walkCount).FirstOrDefault();
                if (curNode == null)
                {
                    path = null;
                    return;
                }
                curNode.walkCount++;
            }
            

            //rotate
            if (visibleEnemy != null)
            {
                Debug.DrawRay(pos, visibleEnemy.pos - pos, Color.red);
                EnemySeenTime = Time.time;
                e = Quaternion.LookRotation(visibleEnemy.pos - pos).eulerAngles;
            }
            if (Time.time - EnemySeenTime > 3)
                e = Quaternion.LookRotation(ZeroY(curNode.pos - pos)).eulerAngles;

            var CamerRot = camera.transform.eulerAngles;
            Vector3 MouseDelta = new Vector3(Mathf.DeltaAngle(CamerRot.x, e.x),
                                             Mathf.DeltaAngle(CamerRot.y, e.y), 0) * Time.deltaTime * 10;

            ////move
          
            var dir = ZeroY(curNode.pos - pos);
            if (Physics.Raycast(new Ray(pos, dir), 1, 1 << LayerMask.NameToLayer("Player")))
            //    stucktime = Time.time;
            //if (stucktime + .5f > Time.time)
                dir = Quaternion.LookRotation(Vector3.left) * dir;
            Debug.DrawRay(pos, dir, Color.green);            
            move = ZeroY(Quaternion.Inverse(camera.transform.rotation) * dir);

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
    private void UpdateInput()
    {

        if (DebugKey(KeyCode.H))
            CallRPC(SetLife, RPCMode.All, 0, id);
        move = GetMove();
        Vector3 MouseDelta = GetMouse();
        if (_ObsCamera.camMode == CamMode.thirdPerson2)
        {
            Ray ray = _ObsCamera.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            RaycastHit hit;
            ray.origin += (pos - _ObsCamera.pos).magnitude * ray.direction.normalized;
            Debug.DrawRay(ray.origin, ray.direction);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                var dir = Quaternion.LookRotation(hit.point - camera.transform.position).eulerAngles;
                var CamerRot = camera.transform.eulerAngles;
                MouseDelta = new Vector3(Mathf.DeltaAngle(CamerRot.x, dir.x),
                                         Mathf.DeltaAngle(CamerRot.y, dir.y), 0);
            } 
        }

        CamRotX = Mathf.Clamp(clampAngle(CamRotX) + MouseDelta.x, -85, 85);
        Rotate(MouseDelta.y);
        if (Input.GetKeyDown(KeyCode.Space))
            CallRPC(Jump, RPCMode.All);
        if (Input.GetMouseButton(0) && Screen.lockCursor)
            gun.MouseDown();

        if (Input.GetKeyDown(KeyCode.R) && !gun.handsReload.enabled && gun.patrons != 30)
            gun.CallRPC(gun.Reload, RPCMode.All);
    }
    private void Rotate(float d)
    {
        //todo smooth rotate
        roty += d;
        model.lroty = Mathf.Clamp(clampAngle(model.lroty - d), left, right);
    }
    [RPC]
    private void SetFPS(int fps, int ping)
    {
        pv.PlayerFps = fps;
        pv.PlayerPing = ping;
    }
    [RPC]
    private void Jump()
    {
        customAnim.Play("jumpCustom");
        jump.time = 0;
        Fade(jump);
        vel *= 1.2f;
    }
    [RPC]
    public void SetPlayerScore(int score)
    {
        pv.PlayerScore = score;
    }

    [RPC]
    public void SetPlayerDeaths(int PlayerDeaths)
    {
        pv.PlayerDeaths = PlayerDeaths;
    }
    [RPC]
    public void SetLife(int life, int player)
    {
        if (!enabled) return;
        var pl = _Game.players[player];
        if (!(imortality && isEditor))
            Life = life;
        HitTime = Time.time;
        audio.PlayOneShot(hitSound.Random(), 3);
        if (Life <= 0)
        {            
            if (pl != null)
                _Hud.KillText.text += pl.pv.PlayerName + " Killed " + pv.PlayerName + "\r\n";
            _Game.timer.AddMethod(15000, delegate { _Hud.KillText.text = RemoveFirstLine(_Hud.KillText.text); });
            if (IsMine)
            {                
                _ObsCamera.pl = pl;
                if (pl != this)
                    pl.CallRPC(pl.SetPlayerScore, RPCMode.All, pl.pv.PlayerScore + 1);
                CallRPC(SetPlayerDeaths, RPCMode.All, pv.PlayerDeaths + 1);
                _ObsCamera.camMode = CamMode.thirdPerson;
            }
            if (IsMine)
                CallRPC(Die, RPCMode.All);
        }

        if (pl != null && bot && pl.pv.team == pv.team && !LastShooted.Contains(pl))
        {
            LastShooted.Add(pl);
        }

    }
    
    [RPC]
    private void Die()
    {
        if (!enabled) return;
        SetPlayerRendererActive(true);
        print("Die" + pv.PlayerName);        
        audio.PlayOneShot(dieSound.Random(), 6);
        Destroy(model.animation);
        foreach (var a in model.GetComponentsInChildren<Rigidbody>())
            a.isKinematic = false;
        model.SetLayer(LayerMask.NameToLayer("Player"));
        model.parent = _Game.Fx;
        enabled = false;
        Destroy(p_gun.gameObject);
        Destroy(model.gameObject, 10);
        if (IsMine)
        {
            _Game.timer.AddMethod(delegate
                                      {
                                          Network.RemoveRPCs(networkView.viewID);
                                          Network.Destroy(gameObject);
                                      });
        }        
    }
    

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.time - grounded > .6f)
            vel = Vector3.zero;
        if (IsMine)
        {
            if (controller.velocity.y < -10f && IsMine)
            {
                CallRPC(SetLife, RPCMode.All, Life + (int) (controller.velocity.y*3), id);
                audio.PlayOneShot(fallSound.Random());
            }
            if (controller.velocity.y < -7)
            {
                customAnim.Play("Hit");
                HitTime = Time.time;
            }
        }
    }

    public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
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

    #region props
    public void WalkSound()
    {
        if (isGrounded)
            audio.PlayOneShot(dirtSound.Random(), observing ? .5f : 1);
    }
    public void Fade(AnimationState s)
    {
        an.CrossFade(s.name);
    }
    public void SetWeight(AnimationState s, float f)
    {
        an.Blend(s.name, f, 0);
    }
    AnimationState[] m_blends;
    internal AnimationState[] blends
    {
        get
        {
            if (m_blends == null)
                m_blends = new[] { an["ak47_shoot_blend1"], an["ak47_shoot_blend1"], an["ak47_shoot_blend2"], an["ak47_shoot_blend3"], an["ak47_shoot_blend4"], an["ak47_shoot_blend5"], an["ak47_shoot_blend6"], an["ak47_shoot_blend7"], an["ak47_shoot_blend8"], an["ak47_shoot_blend9"] };
            return m_blends;
        }
    }

    bool isGrounded { get { return Time.time - grounded < .1f; } }

    public float CamRotX { get { return Cam.rotx; } set { Cam.rotx = value; } }

    public void SetPlayerRendererActive(bool value)
    {
        if (!enabled) return;
        if (PlayerRenderersActive != value)
        {
            PlayerRenderersActive = value;
            foreach (var r in playerRenders)
                r.enabled = value;
            foreach (var r in SkinRenderers)
                r.enabled = value;
        }
    }
    public void SetGunRenderersActive(bool value)
    {
        if (GunRenderersActive != value)
        {
            GunRenderersActive = value;
            foreach (var r in gunRenders)
                r.enabled = value;

        }
    }

    #endregion

    public bool bot { get { return pv.bot; } }
    public new void print(object o)
    {
        Debug.Log(name + ":" + o);
    }
}

