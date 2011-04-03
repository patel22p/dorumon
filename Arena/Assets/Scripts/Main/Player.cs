using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Shared {

    public Vector3 syncVel;
    public Vector3 vel;
    
    public override void Awake()
    {
        if (Check()) return;

        base.Awake();   

        if (networkView.isMine)
            _Game._PlayerOwn = this;
        else
            _Game._PlayerOther = this;
    }
	void Start () {                
        an.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
        sleft.wrapMode = WrapMode.Loop;
        sright.wrapMode = WrapMode.Loop;
        Shoot.layer = 1;
        Shoot.wrapMode = WrapMode.Clamp;
    }
    
    void Update()
    {
        name = "Player" + "+" + GetId();
        if (networkView.isMine)
        {
            vel = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //if (_Cam.secondMode)
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
        
        
        var sn = speed.normalized;
        _Loader.WriteVar("Player vel" + sn);

        //if (Input.GetMouseButtonDown(0))
        //    an.CrossFadeQueued(Shoot.name, 0f, QueueMode.PlayNow);
        float LFlim = .3f;

        if (sn.x < -LFlim)
            Fade(sn.z > -LFlim ? sleft : sright, 1);
        else if (sn.x > LFlim)
            Fade(sn.z > -LFlim ? sright : sleft, 1);
        else if (Mathf.Abs(sn.z) > .1f)
            Fade(run, 1);
        else //if (Mathf.Abs(sn.magnitude) < .1f)
            Fade(idle, 1);
        //else


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

    
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState sleft { get { return an["StrafeLeft"]; } }
    AnimationState sright { get { return an["StrafeRight"]; } }
    AnimationState Shoot { get { return an["Shoot"]; } }
    
}
  