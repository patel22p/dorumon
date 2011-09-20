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
    public Team team;

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
    public Transform[] RagDoll;
    public List<Renderer> playerRenders= new List<Renderer>();
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
    public string PlayerName;
    public int Life = 100;
    public int Shield = 100;
    public int PlayerMoney;
    public int PlayerScore;
    public int PlayerDeaths;
    public int PlayerPing;
    public int PlayerFps;
    public int id;
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
    internal bool observing;
    public int skin;

    //todo draw calls
    public override void Awake()
    {
        Debug.Log("Player Awake");
        
        if (IsMine) _Game._Player = this;
        camera.gameObject.active = false;
        IgnoreAll("IgnoreColl");
        if (IsMine || Offline) {
            CallRPC(RpcSetupPlayer, RPCMode.All, _Loader.playerName, Offline ? Random.Range(0, 32) : Network.player.GetHashCode(), (int)_Game.team,_Game.PlayerSkin);
            CallRPC(SetPlayerDeaths, RPCMode.All, _Game.PlayerDeaths);
            CallRPC(SetPlayerScore, RPCMode.All, _Game.PlayerScore);
            CallRPC(SetMoney, RPCMode.All, _Game.PlayerMoney);
        }
 
        gun = GetComponentInChildren<Gun>();
        gun.pl = this;
        if (!IsMine) {
            name = "Player-Remote";
            model.animation.cullingType = AnimationCullingType.BasedOnClipBounds;
        }
        else {
            name = "Player";
            model.animation.cullingType = AnimationCullingType.AlwaysAnimate;
        }
        foreach (var a in GetComponentsInChildren<Rigidbody>())
            a.isKinematic = true;
        if (team == Team.Spectators)
            CallRPC(Disable, RPCMode.All);

        
        
    }

    private void LoadSkin()
    {
        
        var nwModelPrefab = (team == Team.CounterTerrorists ? CTerrorSkins : TerrorSkins)[this.skin];
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
        CallRPC(RpcSetupPlayer, player, PlayerName, Offline ? Random.Range(0, 32) : Network.player.GetHashCode(), (int)_Game.team,_Game.PlayerSkin);
        CallRPC(SetPlayerDeaths, player, PlayerDeaths);
        CallRPC(SetPlayerScore, player, PlayerScore);
        CallRPC(SetMoney, player, PlayerMoney);
        if (dead)
            CallRPC(Disable, player);
        else
            CallRPC(SetLife, player, Life, id);
    }


    private void InitAnimations()
    {
        idle.speed = .5f;
        run.wrapMode = WrapMode.Loop;
        jump.wrapMode = WrapMode.Once;
        

        foreach (var b in blends) {
            b.AddMixingTransform(UpperBone);
            b.layer = 2;
        }

        blends[5].layer = 1;
        jump.layer = 1;
    }
    public void Start()
    {                
        InitAnimations();
        controller = GetComponent<CharacterController>();
    }
    [RPC]
    private void SetMoney(int Money)
    {
        if (IsMine)
            _Game.PlayerMoney = Money;
        this.PlayerMoney = Money;
    }
    [RPC]
    public void SetTeam(int team)
    {
        this.team = (Team)team;
    }
    [RPC]
    private void SetPlayerScore(int score)
    {
        if(IsMine)
            _Game.PlayerScore = score;
        this.PlayerScore = score;
    }
    [RPC]
    private void SetPlayerDeaths(int PlayerDeaths)
    {
        if (IsMine)
            _Game.PlayerDeaths = PlayerDeaths;
        this.PlayerDeaths = PlayerDeaths;
    }
    [RPC]
    private void RpcSetupPlayer(string PlayerName, int PlayerID, int team, int skin)
    {
        this.skin = skin;
        this.id = PlayerID;
        _Game.players[PlayerID] = this; 
        this.PlayerName = PlayerName;
        if (!Offline)
            this.team = (Team)team;
        LoadSkin();
    }
    public void Update()
    {
        if (dead) return;
        //todo add bomb
        UpdateJump();
        UpdateMove();
        UpdateRotation();
        UpdateInput();
        UpdateOther();
    }
    private void UpdateOther()
    {
        if (posy < -20)
            CallRPC(Disable, RPCMode.All);

        if (_Game.team == Team.Spectators || _Player.dead || team == _Player.team)
            MiniMapCursor.renderer.enabled = true;
        else
            MiniMapCursor.renderer.enabled = false;

        if (!observing) {
            SetGunRenderersActive(false);
            SetPlayerRendererActive(true);
        }

        if (IsMine) {
            if (_Game.timer.TimeElapsed(1000))
                CallRPC(SetFPS, RPCMode.All, (int)_Game.timer.GetFps(), (Network.isServer || Offline) ? -1 : Network.GetLastPing(Network.connections[0])); 

            var ray = camera.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            _Hud.PlayerName.text = "";
            foreach (var h in Physics.RaycastAll(ray, 1000, 1 << LayerMask.NameToLayer("Player")))
                if (h.transform.root != this.transform) {
                    var pl = h.transform.root.GetComponent<Player>();
                    if (pl != null && pl.team == team)
                        _Hud.PlayerName.text = pl.team == team ? ("Ally Life:" + pl.Life) : "Enemy" + " " + pl.PlayerName;
                    break;
                }
        }
    }
    public void LateUpdate()
    {
        if (dead) return;
        observing = false;
    }
    public void FixedUpdate()
    {
        if (dead) return;
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
        if (!IsMine) {
            CamRotX = Mathf.LerpAngle(CamRotX, syncRotx, Time.deltaTime * 10);
            var nwroty = Mathf.LerpAngle(roty, syncRoty, Time.deltaTime * 10);
            var d = nwroty - roty;
            Rotate(d);
        }
        Vector3 CamxModely = clampAngles(new Vector3(CamRotX, model.lroty, 0));
        var numpad = new Vector3(
            (Mathf.Clamp(CamxModely.y / -left, -1, 0) + Mathf.Clamp(CamxModely.y / right, 0, 1)),
            -(Mathf.Clamp(CamxModely.x / 45, -1, 0) + Mathf.Clamp(CamxModely.x / 45, 0, 1)));


        foreach (var b in blends) {
            if (gun.shooting)
                b.speed = 1;
            else {
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

        Debug.DrawRay(pos, rot * Vector3.forward);
        Debug.DrawRay(pos, model.rot * Vector3.forward, Color.red);
    }

    

    private void UpdateMove()
    {

        if (vel.magnitude > .1f) {
            var lookRot = Quaternion.LookRotation(vel);
            float deltaAngle = Mathf.DeltaAngle(lookRot.eulerAngles.y, rot.eulerAngles.y);
            if (Mathf.Abs(deltaAngle) > 110) {
                model.rot = Quaternion.LookRotation(vel * -1);
                run.speed = -2f * (vel.magnitude / 5);
            }
            else {
                model.rot = Quaternion.LookRotation(vel);
                run.speed = 2f * (vel.magnitude / 5);
            }

            Fade(run);
            gun.AnimateRun();
        }
        else {
            Fade(idle);
            gun.AnimateIdle();
        }

    }
    private void UpdateInput()
    {
        if (IsMine) {
            if (DebugKey(KeyCode.H))
                CallRPC(SetLife, RPCMode.All, 0, id);
            move = GetMove();
            Vector3 MouseDelta = GetMouse();
            if (_ObsCamera.camMode == ObsCamera.CamMode.thirdPerson2)
            {
                Ray ray = _ObsCamera.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    var hitRot = Quaternion.LookRotation(hit.point - camera.transform.position).eulerAngles;
                    var CamerRot = camera.transform.eulerAngles;                    
                    MouseDelta = new Vector3(Mathf.DeltaAngle(CamerRot.x, hitRot.x), Mathf.DeltaAngle(CamerRot.y, hitRot.y), 0);
                }
            }
            
            CamRotX = Mathf.Clamp(clampAngle(CamRotX) + MouseDelta.x, -85, 85);
            Rotate(MouseDelta.y);
            if (Input.GetKeyDown(KeyCode.Space))
                CallRPC(Jump, RPCMode.All);
        }
    }
    private void Rotate(float d)
    {
        roty += d;
        model.lroty = Mathf.Clamp(clampAngle(model.lroty - d), left, right);
    }
    [RPC]
    private void SetFPS(int fps,int ping)
    {
        PlayerFps = fps;
        PlayerPing = ping;
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
    public void SetLife(int life, int player)
    {
        Life = life;
        HitTime = Time.time;
        audio.PlayOneShot(hitSound.Random(), 3);
        if (Life <= 0) {
            Die(player);
        }
    }
    private void Die(int player)
    {
        Debug.Log("Die" + PlayerName);
        var pl = _Game.players[player];
        audio.PlayOneShot(dieSound.Random(), 6);
        Destroy(model.animation);
        foreach (var a in model.GetComponentsInChildren<Rigidbody>())
            a.isKinematic = false;
        model.SetLayer(LayerMask.NameToLayer("Player"));
        _Game.timer.AddMethod(15000, delegate { _Hud.KillText.text = RemoveFirstLine(_Hud.KillText.text); });
        _Hud.KillText.text += pl.PlayerName + " Killed " + PlayerName + "\r\n";
        model.parent = _Game.Fx;
        Destroy(p_gun.gameObject);
        Destroy(model.gameObject, 5);
        if (IsMine) {
            _ObsCamera.pl = pl;            
            if (pl != this)
                pl.CallRPC(SetPlayerScore, RPCMode.All, pl.PlayerScore + 1);
            CallRPC(SetPlayerDeaths, RPCMode.All, PlayerDeaths + 1);
            _ObsCamera.camMode = ObsCamera.CamMode.thirdPerson;
        }
        Disable();
    }
    [RPC]
    private void Disable()
    {
        Debug.Log("Disable" + name);
        Life = 0;
        SetPlayerRendererActive(true);
        dead = true;        
        foreach (Transform t in transform)
            Destroy(t.gameObject);
        transform.parent = null;
        foreach (var c in this.GetComponents<Component>())
            if (c != this && !(c is Transform) && !(c is NetworkView)) Destroy(c);
        name += " - Dead";
    }
    
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.time - grounded > .6f)
            vel = Vector3.zero;

        if (controller.velocity.y < -8.3f && IsMine)
        {
            CallRPC(SetLife, RPCMode.All, Life + (int) (controller.velocity.y*3), id);
            audio.PlayOneShot(fallSound.Random());
        }
        if (controller.velocity.y < -7) {
            customAnim.Play("Hit");
            HitTime = Time.time;            
        }
    }
    
    public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (dead) return;
        if (stream.isWriting) {
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
    public bool dead;
    
    
    public float CamRotX { get { return Cam.rotx; } set { Cam.rotx = value; } }

    public void SetPlayerRendererActive(bool value)
    {
        if (PlayerRenderersActive != value) {
            PlayerRenderersActive = value;
            foreach (var r in playerRenders)
                r.enabled = value;
            foreach (var r in SkinRenderers)
                r.enabled = value;
        }
    }
    public void SetGunRenderersActive(bool value)
    {
        if (GunRenderersActive != value) {
            GunRenderersActive = value;
            foreach (var r in gunRenders)
                r.enabled = value;
            
        }
    }
    #endregion
    
}

