using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;
using System.Linq;
public class Player : Shared {

    public Vector3 syncVel;
    public Vector3 vel;
        
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState sleft { get { return an["sleft2"]; } }
    AnimationState sright { get { return an["sright2"]; } }
    AnimationState runback { get { return an["runback"]; } }    
    AnimationState ShootAnim { get { return an["Shoot"]; } }
    AnimationState punch { get { return an["punch"]; } }
    AnimationState punch2 { get { return an["punch2"]; } }
    AnimationState punch3 { get { return an["punch3"]; } }
    AnimationState death { get { return an["death"]; } }
    AnimationState gesture { get { return an["gesture"]; } }

    public Quaternion rot2;
    public override void Awake()
    {
        if (NotInstance()) return;

        base.Awake();   

        if (networkView.isMine)
            _Game._PlayerOwn = this;
        else
            _Game._PlayerOther = this;

        an.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        sleft.wrapMode = WrapMode.Loop;
        sright.wrapMode = WrapMode.Loop;
        
        punch3.layer = punch2.layer = punch.layer = ShootAnim.layer = 1;
        punch2.wrapMode = punch3.wrapMode = punch.wrapMode = ShootAnim.wrapMode = WrapMode.Clamp;
        death.wrapMode = WrapMode.ClampForever;
        death.layer = 1;
        gesture.wrapMode = WrapMode.Clamp;
        gesture.layer = 1;
    }
    
    bool shoting;
    public TimerA timer = new TimerA();
    public int killed;
    float tmLastCombo;
    void LateUpdate()
    {        
        
        if (Alive)
        {
            UpdateOther();            
            if (networkView.isMine)
                UpdateMine();

            UpdatePhysics();
            timer.Update();
        }
    }

    private void UpdateOther()
    {
        slow -= Time.deltaTime;
        name = "Player" + "+" + GetId();
    }
    Vector3 vel2;
    private void UpdatePhysics()
    {
        //vel2 = Vector3.Lerp(vel, vel2, .88f);
        controller.SimpleMove(vel);
        vel *= .88f;
        rot = Quaternion.Lerp(rot, rot2, .2f);
        if (vel.magnitude > .5f)
            Fade(run);
        else
            Fade(idle);
        _Loader.WriteVar("Player vel" + vel);
    }

    private void UpdateMine()
    {
        if (timer.TimeElapsed(2000)) //regenerate
            life += 1;
        life = Mathf.Min(life, 100);
        if (life <= 0 && !_Game.CantDie)
        {
            networkView.RPC("Die", RPCMode.All);            
        }
        bool stay = punch.enabled || punch2.enabled || punch3.enabled || ShootAnim.enabled || gesture.enabled;
        if (!stay)
            tmLastCombo += Time.deltaTime;
        _Game.GameText.text = "Stage:" + _Game.stage + " Zombies:" + _Game.AliveZombies.Count() + " Killed:" + killed + " Life:" + Mathf.Max(0, life);
        if (!stay)
        {
            vel = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));            
            vel = _Cam.oldrot * vel.normalized;
            vel *= 5;
            if (slow > 0) vel *= .5f;
        }
        else
            vel = Vector3.zero;

        var v = _Cursor.pos - _PlayerOwn.pos;
        v.y = 0;
        if (vel != Vector3.zero)
            rot2 = Quaternion.LookRotation(vel);

        if (Input.GetKeyDown(KeyCode.Space))
            networkView.RPC("Gesture", RPCMode.All);

        if (Input.GetMouseButtonDown(0) && !stay)
        {
            Combo();
            networkView.RPC("Hit", RPCMode.All, combo);
        }
        if (Input.GetMouseButtonDown(1) && !stay)
            networkView.RPC("Shoot", RPCMode.All);
    }
    [RPC]
    void Gesture()
    {
        if (networkView.isMine && _PlayerOther != null && !_PlayerOther.Alive && Vector3.Distance(_PlayerOther.pos, pos) < 3)
            _PlayerOther.networkView.RPC("WakeUp", RPCMode.All);
        Fade(gesture);
    }
    [RPC]
    public void WakeUp()
    {
        Alive = true;
        life = 20;
        SetLayer(LayerMask.NameToLayer("Player"));
    }

    [RPC]
    public void Die()
    {
        Debug.Log("Doe");
        Alive = false;        
        an.CrossFade(death.name);
        SetLayer(LayerMask.NameToLayer("Dead"));
    }
    [FindAsset("patron")]
    public GameObject PatronPrefab;
    [FindTransform("revolver")]
    public Transform GunPos;
    [RPC]
    private void Shoot()
    {
        an.CrossFade(ShootAnim.name);
        rot2 = Quaternion.LookRotation(plLook);
        ShootAnim.speed = 0;
        timer.AddMethod(500, delegate
        {
            ShootAnim.speed = 1.5f;
            if(networkView.isMine)
                networkView.RPC("Shoot2", RPCMode.All);
        });
    }
    float slow;
    [RPC]
    private void Shoot2()
    {
        Instantiate(PatronPrefab, GunPos.transform.position, rot2);
    }
    [RPC]
    private void Damage(int dmg)
    {
        slow = .5f;
        life -= dmg;
    }
    [RPC]
    private void Hit(int combo)
    {
        combo = combo % 4;
        int damage = 25;
        var punch = this.punch;
        if (combo == 2)
        {
            punch = punch2;
            damage = 50;
        }
        if (combo == 3)
        {
            punch = punch3;
            damage = 50;
        }
        if (networkView.isMine)
        {
            rot2 = Quaternion.LookRotation(plLook);
            if (combo == 3)
            {
                timer.AddMethod(1100, delegate
                {
                    foreach (Zombie z in _Game.Zombies.Where(a => a != null && Vector3.Distance(a.pos, pos) < 4))
                        z.networkView.RPC("Damage", RPCMode.All, damage);
                });
            }
            else
            {
                timer.AddMethod(200, delegate
                {
                    foreach (Zombie z in trigger.colliders.Where(a => a is Zombie))
                        z.networkView.RPC("Damage", RPCMode.All, damage);
                });
            }
        }
        an.CrossFade(punch.name);
        punch.speed = 0;
        timer.AddMethod(100, delegate { punch.speed = 1; });
        tmLastCombo = 0;


    }

    private void Combo()
    {        
        if (tmLastCombo < .3f)
            combo++;
        else
            combo = 0;
    }
    int combo;

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            syncPos = pos;
            syncRot = rot2;
            syncVel = controller.velocity;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRot);
        stream.Serialize(ref syncVel);
        if (stream.isReading)
        {
            if (syncPos == Vector3.zero) Debug.Log("Sync ErroR");
            pos = syncPos;
            rot2 = syncRot;
            vel = syncVel;
        }
    }
    public Vector3 plLook
    {
        get
        {
            var v = _Cursor.pos - pos;
            v.y = 0;
            return v;
        }
    }
}
  