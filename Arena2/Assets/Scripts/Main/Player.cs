using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;
using System.Linq;
public class Player : Shared {

    public Vector3 syncVel;
    public Vector3 vel;
    [FindTransform()]
    public Trigger trigger;
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState sleft { get { return an["sleft2"]; } }
    AnimationState sright { get { return an["sright2"]; } }
    AnimationState runback { get { return an["runback"]; } }    
    AnimationState ShootAnim { get { return an["Shoot"]; } }
    AnimationState punch { get { return an["punch"]; } }
    AnimationState punch2 { get { return an["punch2"]; } }
    AnimationState punch3 { get { return an["punch3"]; } }
    public override void Awake()
    {
        if (NotInstance()) return;
        foreach (var t in upperbody)
            ShootAnim.AddMixingTransform(t);
        foreach (var t in upperbody)
            punch.AddMixingTransform(t);

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
    }
    
    bool shoting;
    public TimerA timer = new TimerA();
    public int killed;
    void LateUpdate()
    {
        name = "Player" + "+" + GetId();
        bool stay = punch.enabled || punch2.enabled || punch3.enabled || ShootAnim.enabled;
        if (networkView.isMine)
        {
            _Game.GameText.text = "Stage:" + _Game.stage + " Zombies:" + _Game.AliveZombies.Count() + " Killed:" + killed;
            if (!stay)
            {
                vel = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                vel = _Cam.oldrot * vel.normalized;
                vel *= 5;
            }
            else
                vel = Vector3.zero;

            var v = _Cursor.pos - _PlayerOwn.pos;
            v.y = 0;
            if (v != Vector3.zero)
                _PlayerOwn.rot = Quaternion.LookRotation(v);

            if (Input.GetMouseButtonDown(0) && !stay)
                networkView.RPC("Punch", RPCMode.All);
            if (Input.GetMouseButtonDown(1) && !stay)
                networkView.RPC("Shoot", RPCMode.All);
        }
        controller.SimpleMove(vel);
        vel *= .98f;
        Vector3 speed = Quaternion.Inverse(transform.rotation) * controller.velocity;
        if (vel.magnitude > .5f)
            Fade(run);
        else
            Fade(idle);
        _Loader.WriteVar("Player vel" + vel);
        
        timer.Update();
    }
    [FindAsset("patron")]
    public GameObject PatronPrefab;
    [FindTransform("revolver")]
    public Transform GunPos;
    [RPC]
    private void Shoot()
    {
        an.CrossFade(ShootAnim.name);
        ShootAnim.speed = 0;
        timer.AddMethod(500, delegate
        {
            ShootAnim.speed = 1.5f;
            Instantiate(PatronPrefab, GunPos.transform.position, rot);
        });
    }

    [RPC]
    private void Punch()
    {
        if (networkView.isMine)
            foreach (Zombie z in trigger.colliders.Where(a => a is Zombie))            
                z.networkView.RPC("Damage", RPCMode.All, 20);
        
        var pn = punch;
        an.CrossFade(pn.name);
        pn.speed = 0;
        timer.AddMethod(100, delegate { pn.speed = 1; });
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            syncPos = pos;
            syncRot = rot;
            syncVel = controller.velocity;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRot);
        stream.Serialize(ref syncVel);
        if (stream.isReading)
        {
            if (syncPos == Vector3.zero) Debug.Log("Sync ErroR");
            pos = syncPos;
            rot = syncRot;
            vel = syncVel;
        }
    }
}
  