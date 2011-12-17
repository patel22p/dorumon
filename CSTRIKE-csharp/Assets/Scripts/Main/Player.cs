using doru;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Player : Shared
{
    private Timer timer = new Timer();
    public GunBase[] guns;
    public GunBase gun;
    public C4 c4;
    private AnimationState jump { get { return an["jump"]; } }
    public Animation customAnim { get { return animation; } }
    public Transform[] TerrorSkins;
    public Transform[] CTerrorSkins;
    public Transform UpperBone;
    public Transform CamRnd;
    public Transform p_gun;
    public GameObject Plane;
    public GameObject sparks;
    public GameObject MuzzleFlash;
    public GameObject MuzzleFlash2;
    public GameObject BloodPrefab;
    public Material[] BulletHoleMaterials;
    public Material[] MuzzleFlashMaterials;
    public Material[] BloodDecals;
    public AudioClip dryfire_pistol;
    public AudioClip dryfire_rifle;
    public AudioClip[] bulletric;
    public AudioClip[] headShootSound;
    public AudioClip[] fallSound;
    public List<Renderer> playerRenders = new List<Renderer>();
    public Renderer[] gunRenders;
    private Renderer[] SkinRenderers = new Renderer[] { };
    

    public int left = -65;
    public int right = 95;
    private Vector3 nextDist;
    private Vector3 botRotation;
    private bool PlayerRenderersActive = true;
    private bool GunRenderersActive = true;

    public override void Awake()
    {
        base.Awake();
        camera.enabled = false;
        for (int i = 0; i < guns.Length; i++)
        {
            var a = guns[i];
            a.arrayId = i;
            a.SetActive(false);
            a.pl = this;
        }
        gun = guns[0];
        gun.SetActive(true);
        

        enabled = false;
        print("Player Awake " + id);                
        
    }
    public override void Start()
    {        
       //note shield
        if (IsMine && !bot)
        {
            _Player = this;
            lockCursor = true;
        }
        
        if(IsMine)
            CallRPC(SetSkin, PhotonTargets.All, pv.skin);

        InitAnimations();
        timer.AddMethod(1000, delegate
        {
            GunRenderersActive = true;
            PlayerRenderersActive = true;
        });
        
        base.Start();
    }
    private void LoadSkin()
    {

        var nwModelPrefab = (pv.team == Team.CounterTerrorists ? CTerrorSkins : TerrorSkins)[this.pv.skin];
        var nwModel = ((Transform)Instantiate(nwModelPrefab, model.pos, model.rot));
        foreach (var p in nwModel.GetComponentsInChildren<Renderer>())
            playerRenders.Add(p);

        List<Transform> last = new List<Transform>();
        foreach (var p in nwModel.transform.Find("Bip01").GetTransforms().Reverse().ToArray())
        {
            var t = model.transform.GetTransforms().FirstOrDefault(b => b.name == p.name);
            last.Add(p);
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
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient) return;
        //_Game.timer.AddMethod(delegate
        //{
        CallRPC(SetPlType, player, (int)PlType);
        CallRPC(SetTeam, player, (int)pv.team);
        CallRPC(SetSkin, player, bot ? Random.Range(0, 3) : pv.skin);
        CallRPC(SetPlayerDeaths, player, pv.PlayerDeaths);
        CallRPC(SetPlayerScore, player, pv.PlayerScore);
        CallRPC(SetMoney, player, pv.PlayerMoney);
        CallRPC(SetLife, player, Life, id);
        CallRPC(SelectGun, player, gun.arrayId);        
        //});
    }
    private void InitAnimations()
    {
        model.animation.cullingType = IsMine ? AnimationCullingType.BasedOnClipBounds : AnimationCullingType.AlwaysAnimate;
        idle.speed = .5f;
        idle.wrapMode = run.wrapMode = WrapMode.Loop;
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
    private void SetSkin(int skin)
    {
        pv.skin = skin;
        LoadSkin();
    }
    public virtual void FixedUpdate()
    {
       
        if (syncUpdated)
        {
            //vel = syncVel;
            move = syncMove;
        }
        //print(isGrounded);
        if (move.magnitude > 0 && isGrounded)
        {
            speeadd = Mathf.Max(0f, SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move)));
            vel += rot * move * speeadd * (Time.time - HitTime < 1 ? .5f : 1);
        }
        if (isGrounded)
        {
            vel *= .83f;
            move *= .90f;
        }
        //print(controller.velocity);
        //if (!Input.GetKey(KeyCode.I))
        controller.SimpleMove(vel);
        //else
        //{

        //    controller.SimpleMove(Vector3.zero);
        //}

        if (yvel > 0f)
        {
            //print(yvel);

            //controller.SimpleMove(new Vector3(0, yvel, 0));
            controller.Move(new Vector3(0, yvel, 0));
        }

        if (syncUpdated)
        {
            if ((syncPos - pos).magnitude > 1)
                pos = syncPos;
            else
                controller.Move(syncPos - pos);
        }

        syncUpdated = false;
    }
    public override void Update()
    {
        base.Update();
        
        UpdateMove();
        UpdateRotation();
        if (IsMine)
            if (bot)
                UpdateBot();
            else
                UpdateInput();
        UpdateOther();
        timer.Update();
    }
    
    private void UpdateOther()
    {
        //todo shop
        //todo door script
        //todo climbing
        //if (IsMine)
        //if (new[] { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S }.Any(a => Input.GetKeyDown(a) || Input.GetKeyUp(a)) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0) || timer.TimeElapsed(1000))
        if (timer.TimeElapsed(1000))
            CallRPC(UpdateSync, PhotonTargets.All, pos.y);

        if (isGrounded && jump.weight > 0)
            jump.weight *= .86f;

        if (posy < -20 && IsMine)
            CallRPC(Die, PhotonTargets.All);

        if (!observing)
        {   
            SetGunRenderersActive(false);
            SetPlayerRendererActive(true);
        }

        if (observing)
        {            
            //bug wroing viewId assigned
            if (_Game.timer.TimeElapsed(1000))
                CallRPC(SetFPS, PhotonTargets.All, (int)_Game.timer.GetFps(), (PhotonNetwork.isMasterClient || Offline) ? -1 : PhotonNetwork.GetPing());

            var ray = camera.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            _Hud.PlayerName.text = "";
            foreach (var h in Physics.RaycastAll(ray, 1000, 1 << LayerMask.NameToLayer("Player")))
                if (h.transform.root != this.transform)
                {
                    var pl = h.transform.root.GetComponent<Player>();
                    if (pl != null && pl.pv.team == pv.team)
                        _Hud.PlayerName.text = pl.pv.team == pv.team ? (pl.pv.PlayerName + "'s Life:" + pl.Life) : "Enemy" + " " + pl.pv.PlayerName;
                    break;
                }
        }
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
            camera.animation.CrossFade("cam_move");
        }
        else
        {
            Fade(idle);
            camera.animation.CrossFade("cam_idle");
        }
    }
    float EPressTime;

    [RPC]
    public void UpdateSync(float pos)
    {
        if (!IsMine)
            controller.Move(new Vector3(0, this.posy - pos, 0));
    }

    
    private void UpdateInput()
    {
        //foreach (var g in guns)
        //    if (Input.GetKeyDown(KeyCode.Alpha0 + g.gunId))
        //    {
        //        CallRPC(SelectGun, PhotonTargets.All, g.arrayId);
        //        break;
        //    }
        //if (Input.GetAxis("Mouse ScrollWheel") > 0)
        //    NextGun();
        //if (Input.GetAxis("Mouse ScrollWheel") < 0)
        //    PrevGun();
        
        
        Vector3 MouseDelta = _Game.GetMouse();


        bool mbDown = Android ? false : Input.GetMouseButton(0);                
        for (int i = 0; i < Input.touchCount; i++)
        {
            var p = Input.GetTouch(i).position;
            if (Input.GetTouch(i).phase == TouchPhase.Began && Vector2.Distance(new Vector2(1, 0), new Vector2(p.x/800, p.y/400)) < .1f)
            {
                mbDown = true;
                MouseDelta = Vector3.zero;
                break;
            }
        }
        if (lockCursor || Android)
            gun.MouseButtonDown = mbDown;

        //if (isGrounded)
        move = _Game.GetMove();
        
        if (gun == c4 && gun.MouseButtonDown)
            move = Vector3.zero;

        //if (_Bomb != null && (_Bomb.pos - pos).magnitude < 2 && _Bomb.enabled)
        //{
        //    if (Input.GetKeyDown(KeyCode.E))
        //        EPressTime = Time.time;
        //    if (Input.GetKey(KeyCode.E))
        //    {
        //        var t = (Time.time - EPressTime) / 10;
        //        Debug.Log(t);
        //        _Hud.SetProgress(t);
        //        if (t > 1)
        //            _Bomb.CallRPC(_Bomb.Difuse, PhotonTargets.All);
        //        move = Vector3.zero;
        //    }
            
        //}
        //if (Input.GetKey(KeyCode.I))
        //{
        //    vel = Vector3.zero;
        //    move = Vector3.zero;
        //}
        if (_ObsCamera.thirdPerson )
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

        if (Input.GetKeyDown(KeyCode.Space) && lockCursor && isGrounded)
            CallRPC(Jump, PhotonTargets.All);
        

        if (Input.GetKeyDown(KeyCode.R))
            gun.OnRDown();

  
    }

    

    public void PrevGun()
    {
        CallRPC(SelectGun, PhotonTargets.All, guns.Prev(gun).arrayId);
    }

    public void NextGun()
    {
        CallRPC(SelectGun, PhotonTargets.All, guns.Next(gun).arrayId);
    }
    [RPC]
    public void SelectGun(int i)
    {
        //Debug.Log(i);
        if (guns[i].gunId == 5 && guns[i].patrons == 0)
            i = 0;        
        if (gun.arrayId != i)
        {
            var g = guns[i];
            foreach (var g2 in guns)
                g2.SetActive(false);
            g.SetActive(true);
            gun = g;
        }
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
    public override void SetTeam(int team)
    {
        base.SetTeam(team);
    }

    [RPC]
    public override void SetPlType(int bot)
    {
        base.SetPlType(bot);
    }
    [RPC]
    public override void SetLife(int life, int player)
    {
        base.SetLife(life, player);
    }
    //public new void SetID(int id)
    //{
    //    print("SetId" + id);
    //    this.id = id;
    //    _Game.shareds[id] = this;
    //    foreach (var a in this.GetComponentsInChildren<Bs>())
    //        a.SendMessage("OnSetID", SendMessageOptions.DontRequireReceiver);
    //}

    [RPC]
    private void SetFPS(int fps, int ping)
    {
        pv.PlayerFps = fps;
        pv.PlayerPing = ping;
    }
    [RPC]
    private void Jump()
    {
        //bug fix jump 
        customAnim.Play("jumpCustom");
        jump.time = 0;
        Fade(jump);
        vel *= 1.2f;
    }
    [RPC]
    public override void Die()
    {
        SetPlayerRendererActive(true);
        Destroy(p_gun.gameObject);
        base.Die();
    }
    [RPC]
    public override void SetPlayerDeaths(int PlayerDeaths)
    {
        base.SetPlayerDeaths(PlayerDeaths);
    }
    

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.time - grounded > .6f)
            vel = Vector3.zero;
        if (IsMine)
        {
            if (controller.velocity.y < -10f && IsMine)
            {
                CallRPC(SetLife, PhotonTargets.All, Life + (int) (controller.velocity.y*3), id);
                audio.PlayOneShot(fallSound.Random());
            }
            if (controller.velocity.y < -7)
            {
                customAnim.Play("Hit");
                HitTime = Time.time;
            }
        }
    }
    protected void Rotate(float d)
    {
        roty += d;
        model.lroty = Mathf.Clamp(clampAngle(model.lroty - d), left, right);
    }    

    protected virtual void UpdateBot()
    {
        if (CreatedTime + 3 > Time.time) return;
        gun.MouseButtonDown = false;
        if (_Game.timer.TimeElapsed(500) || visibleEnemy == null)
            visibleEnemy = UpdateVisibleEnemy();
        move = Vector3.zero;
        if (enemies.Count() > 0)
        {

            UpdateNode();
            if (curNode == null)
            {
                //move = Vector3.zero;
                return;
            }
            if (Time.time - EnemySeenTime > 3)
                botRotation = Quaternion.LookRotation(ZeroY(curNode.GetPos(NodeOffset) - pos)).eulerAngles;
            Debug.DrawLine(pos, curNode.GetPos(NodeOffset), Color.green);

            //rotate
            if (visibleEnemy != null)
            {
                Debug.DrawRay(pos, visibleEnemy.pos - pos, Color.red);
                EnemySeenTime = Time.time;
                botRotation = Quaternion.LookRotation((visibleEnemy.hpos) - hpos).eulerAngles;
            }



            var CamerRot = this.camera.transform.eulerAngles;
            Vector3 MouseDelta = new Vector3(Mathf.DeltaAngle(CamerRot.x, botRotation.x),
                                             Mathf.DeltaAngle(CamerRot.y, botRotation.y), 0) * Time.deltaTime * 10;

            CamRotX += MouseDelta.x;
            Rotate(MouseDelta.y);

            ////move
            Vector3 dir = UpdateBotMoveDir();
            dir = UpdateCheckDir(dir);
            move = ZeroYNorm(Quaternion.Inverse(this.camera.transform.rotation) * dir);

            ////shoot
            if (visibleEnemy != null)
            {
                if (Time.time > nextShootTime)
                {
                    if ((visibleEnemy.pos - pos).magnitude > 8)
                    {
                        //move = Vector3.zero;
                        if (Time.time > nextShootTime + .5f)
                            nextShootTime = Time.time + Random.Range(0, 2f);
                    }
                    gun.MouseButtonDown=true;
                }
            }
        }
        //else
        //    move = Vector3.zero;
        
    }
    public void SetWeight(AnimationState s, float f)
    {
        an.Blend(s.name, f, 0);
    }
    private AnimationState[] m_blends;
    public new Camera camera;

    internal AnimationState[] blends
    {
        get
        {
            if (m_blends == null)
                m_blends = new[] { an["ak47_shoot_blend1"], an["ak47_shoot_blend1"], an["ak47_shoot_blend2"], an["ak47_shoot_blend3"], an["ak47_shoot_blend4"], an["ak47_shoot_blend5"], an["ak47_shoot_blend6"], an["ak47_shoot_blend7"], an["ak47_shoot_blend8"], an["ak47_shoot_blend9"] };
            return m_blends;
        }
    }
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
    private float tsp;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        //byte syncRotXC = byte.MinValue;
        //byte syncRotYC = byte.MinValue;
        //byte syncMoveC = byte.MinValue;
        //short syncposy = 0, syncposz = 0;
        float sv = (short.MaxValue / 2);

        if (stream.isWriting)
        {
            //syncposy = (short)(Mathf.Clamp(pos.x, -100, 100) / 100f * sv);
            //syncposz = (short)(Mathf.Clamp(pos.z, -100, 100) / 100f * sv);
            stream.SendNext((short)(Mathf.Clamp(pos.x, -100, 100) / 100f * sv));
            stream.SendNext((short)(Mathf.Clamp(pos.z, -100, 100) / 100f * sv));

            //syncRotXC = (byte)(CamRotX / 360f * 255f);
            //syncRotYC = (byte)(roty / 360f * 255f);
            stream.SendNext((byte)(CamRotX / 360f * 255f));
            stream.SendNext((byte)(roty / 360f * 255f));

            stream.SendNext(move == Vector3.zero ? (byte)0 : (byte)((Quaternion.LookRotation(move).eulerAngles.y / 360f * 254f) + 1));
        }

        //stream.Serialize(ref  syncposy);
        //stream.Serialize(ref  syncposz);
        //stream.Serialize(ref syncRotXC);
        //stream.Serialize(ref syncRotYC);
        //stream.Serialize(ref syncMoveC);
        if (stream.isReading)
        {

            if (tsp < info.timestamp)
            {
                var x = (short)stream.ReceiveNext();
                var z = (short)stream.ReceiveNext();
                syncPos = new Vector3(x / sv * 100f, posy, z / sv * 100f);
                syncRotx = (byte)stream.ReceiveNext() / 255f * 360f;
                syncRoty = (byte)stream.ReceiveNext() / 255f * 360f;
                var mv = (byte)stream.ReceiveNext();
                if (mv != byte.MinValue)
                {
                    syncMove = Quaternion.Euler(0, (mv - 1) / 254f * 360f, 0) * Vector3.forward;
                    //print(syncMove);
                }
                else
                    syncMove = Vector3.zero;
                syncUpdated = true;
            }
            //else
            //    print("Time Stamp Error");
            tsp = info.timestamp;
        }

    }
    
    
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{

    //    if (stream.isWriting)
    //    {
    //        stream.SendNext(pos);
    //        stream.SendNext(CamRotX);
    //        syncPos = pos;
    //        syncRotx = CamRotX;
    //        syncRoty = roty;
    //        //syncVel = vel;
    //        syncMove = move;
    //    }
    //    stream.Serialize(ref syncPos);
    //    stream.Serialize(ref syncRotx);
    //    stream.Serialize(ref syncRoty);
    //    //stream.Serialize(ref syncVel);
    //    stream.Serialize(ref syncMove);
    //    if (stream.isReading)
    //        syncUpdated = true;

    //}
}


