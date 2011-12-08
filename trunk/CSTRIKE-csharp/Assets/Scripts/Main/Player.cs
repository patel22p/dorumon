using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Player : Shared
{    
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
        //todo shield
        if (IsMine && !bot)
        {
            _Player = this;
            Screen.lockCursor = true;
        }
        
        if(IsMine)
            CallRPC(SetSkin, RPCMode.All, pv.skin);

        InitAnimations();
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
    public void OnPlayerConnected(NetworkPlayer player)
    {
        if (!Network.isServer) return;
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
            vel = syncVel;
            move = syncMove;
        }

        if (move.magnitude > 0 && isGrounded)
        {
            speeadd = Mathf.Max(0f, SpeedAdd.Evaluate(Vector3.Distance(controller.velocity, rot * move)));
            vel += rot * move * speeadd * (Time.time - HitTime < 1 ? .5f : 1);
        }
        if (isGrounded)
            vel *= .83f;
        controller.SimpleMove(vel);

        if (yvel > 0f)
            controller.Move(new Vector3(0, yvel, 0));
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
    }
    
    private void UpdateOther()
    {
        
        if (isGrounded && jump.weight > 0)
            jump.weight *= .86f;

        if (posy < -20)
            CallRPC(Die, RPCMode.All);

        

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
    private void UpdateInput()
    {
        //foreach (var g in guns)
        //    if (Input.GetKeyDown(KeyCode.Alpha0 + g.gunId))
        //    {
        //        CallRPC(SelectGun, RPCMode.All, g.arrayId);
        //        break;
        //    }
        //if (Input.GetAxis("Mouse ScrollWheel") > 0)
        //    NextGun();
        //if (Input.GetAxis("Mouse ScrollWheel") < 0)
        //    PrevGun();
        
        if(Input.GetTouch(0).phase == TouchPhase.Moved)
            print(Input.GetTouch(0).deltaPosition);

        if (Input.GetMouseButtonDown(0) && Screen.lockCursor)
            gun.MouseButtonDown = true;
        if (Input.GetMouseButtonUp(0) && Screen.lockCursor)
            gun.MouseButtonDown = false;

        move = GetMove();
        if (gun == c4 && gun.MouseButtonDown)
            move = Vector3.zero;
        if (_Bomb != null && (_Bomb.pos - pos).magnitude < 2 && _Bomb.enabled)
        {
            if (Input.GetKeyDown(KeyCode.E))
                EPressTime = Time.time;
            if (Input.GetKey(KeyCode.E))
            {
                var t = (Time.time - EPressTime) / 10;
                Debug.Log(t);
                _Hud.SetProgress(t);
                if (t > 1)
                    _Bomb.CallRPC(_Bomb.Difuse, RPCMode.All);
                move = Vector3.zero;
            }
            
        }
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
        if (Input.GetKeyDown(KeyCode.Space) && Screen.lockCursor)
            CallRPC(Jump, RPCMode.All);
        

        if (Input.GetKeyDown(KeyCode.R))
            gun.OnRDown();
    }

    public void PrevGun()
    {
        CallRPC(SelectGun, RPCMode.All, guns.Prev(gun).arrayId);
    }

    public void NextGun()
    {
        CallRPC(SelectGun, RPCMode.All, guns.Next(gun).arrayId);
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
    public override void Die()
    {
        SetPlayerRendererActive(true);
        Destroy(p_gun.gameObject);
        base.Die();
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
    protected void Rotate(float d)
    {
        roty += d;
        model.lroty = Mathf.Clamp(clampAngle(model.lroty - d), left, right);
    }    
    private Vector3 playerNearPos()
    {
        for (int i = 0; i < 10; i++)
        {

            var p = _Player.pos + _Player.move * 2 + ZeroY(Random.onUnitSphere * 2);
            RaycastHit h;
            Debug.DrawLine(hpos, p, Color.red);
            if (Physics.Raycast(new Ray(hpos, p - hpos), out h, Vector3.Distance(hpos, p) + 3, CantSeeThrough))
                //if (Mathf.Abs(h.point.y - _Player.pos.y) < .5f)
                    return p;            
        }
        return Vector3.zero;
    }
    protected virtual void UpdateBot()
    {
        if (CreatedTime + 3 > Time.time)return;
        gun.MouseButtonDown = false;
        if (_Game.timer.TimeElapsed(500) || visibleEnemy == null)
            visibleEnemy = UpdateVisibleEnemy();
        move = Vector3.zero;
        if (enemies.Count() > 0)
        {
            //selectNode
            if (_Game.gameType == GameType.Survival)
            {

                
                if (_Player != null)
                {

                    if (_Player.move.magnitude > 0 && _Game.timer.TimeElapsed(500))
                        nextDist = playerNearPos();

                    if (nextDist != Vector3.zero && ZeroY(nextDist - pos).magnitude > .5f)
                    {
                        Debug.DrawLine(pos, nextDist, Color.green);
                        if (Time.time - EnemySeenTime > 3)
                            botRotation = Quaternion.LookRotation(ZeroY(nextDist - pos)).eulerAngles;

                        move = ZeroYNorm(Quaternion.Inverse(this.camera.transform.rotation) * ZeroY(nextDist - pos));
                    }
                    //else
                    //    move = Vector3.zero;
                }
            }
            else
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
            }

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
            if (_Game.gameType == GameType.TeamDeathMatch)
            {
                Vector3 dir = UpdateBotMoveDir();
                dir = UpdateCheckDir(dir);
                move = ZeroYNorm(Quaternion.Inverse(this.camera.transform.rotation) * dir);
            }

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
}


