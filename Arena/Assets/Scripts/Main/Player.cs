using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;

public class Player : Shared {

    public Vector3 syncVel;
    public Vector3 vel;
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState sleft { get { return an["sleft"]; } }
    AnimationState sright { get { return an["sright"]; } }
    AnimationState runback { get { return an["runback"]; } }    
    AnimationState Shoot { get { return an["Shoot"]; } }
    public override void Awake()
    {
        if (NotInstance()) return;
        foreach (var t in upperbody)
            Shoot.AddMixingTransform(t);

        base.Awake();   

        if (networkView.isMine)
            _Game._PlayerOwn = this;
        else
            _Game._PlayerOther = this;


        an.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        sleft.wrapMode = WrapMode.Loop;
        sright.wrapMode = WrapMode.Loop;
        Shoot.layer = 1;
        Shoot.wrapMode = WrapMode.Clamp;
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
        
        if (Input.GetMouseButtonDown(0))
        {
            an.CrossFade(Shoot.name);
            Shoot.speed = 0;
            timer.AddMethod(500, delegate { Shoot.speed = 1; });
        }
        
        
        var sn = speed.normalized;
        _Loader.WriteVar("Player vel" + sn);

        if (sn != Vector3.zero)
        {
            float LFlim = .9f;
            var q = Quaternion.LookRotation(sn).eulerAngles;
            Debug.Log(sn);

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
            //if (!(q.y < 90 || q.y > 360 - 90))
            //{
            //    q.y += 180;
            //    Fade(runback);
            //}
            //else
            //    Fade(run);
            
            
            //Debug.Log(q.y);
        }
        else
            Fade(idle);


        //else
        //    if (Mathf.Abs(sn.z) > .1f)
        //    Fade(run, 1);
        //else //if (Mathf.Abs(sn.magnitude) < .1f)
        
        //else

        timer.Update();
    }
    //void LateUpdate()
    //{
    //    //upperbody[0].rotation = Quaternion.identity;
    //}
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
  