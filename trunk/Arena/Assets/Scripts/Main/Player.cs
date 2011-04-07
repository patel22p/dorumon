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
    AnimationState Shoot { get { return an["Shoot"]; } }
    AnimationState punch { get { return an["punch"]; } }
    public override void Awake()
    {
        if (NotInstance()) return;
        foreach (var t in upperbody)
            Shoot.AddMixingTransform(t);
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
        punch.layer =  Shoot.layer = 1;
        punch.wrapMode = Shoot.wrapMode = WrapMode.Clamp;
    }
    
	void Start () {                
        
    }
    bool shoting;
    public TimerA timer = new TimerA();
    void LateUpdate()
    {
        name = "Player" + "+" + GetId();

        
        

        if (networkView.isMine)
        {
            vel = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            vel = _Cam.oldrot * vel.normalized;
            vel += vel * 5;
            var v = _Cursor.pos - _PlayerOwn.pos;
            v.y = 0;
            if (v != Vector3.zero)
                _PlayerOwn.rot = Quaternion.LookRotation(v);            
        }
        controller.SimpleMove(vel);
        vel *= .98f;

        Vector3 speed = Quaternion.Inverse(transform.rotation) * controller.velocity;
        if (Input.GetMouseButtonDown(0) && !punch.enabled)
        {
            foreach (Zombie z in trigger.colliders.Where(a => a is Zombie))
            {

            }
            an.CrossFade(punch.name);
            punch.speed = 0;
            timer.AddMethod(100, delegate { punch.speed = 1; });

        }
        if (Input.GetMouseButtonDown(1) && !Shoot.enabled)
        {
            an.CrossFade(Shoot.name);
            Shoot.speed = 0;
            timer.AddMethod(500, delegate { Shoot.speed = 1; });
        }
        
        
        var sn = speed;
        sn.y = 0;
        sn = sn.normalized;
        _Loader.WriteVar("Player vel" + sn);

        if (sn != Vector3.zero)
        {
            float LFlim = .9f;
            var q = Quaternion.LookRotation(sn).eulerAngles;

            if (sn.x < -LFlim)
                Fade(sn.z > -LFlim ? sleft : sright);
            else if (sn.x > LFlim)
                Fade(sn.z > -LFlim ? sright : sleft);
            else
            {
                if (sn.z > 0)
                    Fade(run);
                else
                {
                    Fade(runback);
                    q.y += 180;
                }
                foreach (var a in downbody)
                {
                    var r = a.rotation.eulerAngles;
                    r.y += q.y;
                    a.rotation = Quaternion.Lerp(a.rotation, Quaternion.Euler(r), .8f);
                }
            }
 
        }
        else
            Fade(idle);


        timer.Update();
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
  