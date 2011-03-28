using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Shared {

    public Vector3 syncVel;
    public Vector3 vel;
    [FindTransform]
    public GameObject model;
    public override void Awake()
    {
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
        _Loader.WriteVar(speed);
        var sn = speed.normalized;                
        if (Mathf.Abs(speed.z) > 1f)
            Fade(run, sn.z);
        else if (speed.x < -1f)
            Fade(sleft, Mathf.Abs(sn.x));
        else if (speed.x > 1f)
            Fade(sright, Mathf.Abs(sn.x));
        else 
            Fade(idle, 1);
        
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

    private void Fade(AnimationState s, float speed)
    {
        an.CrossFade(s.name);
        s.speed = speed;
    }
    public Animation an { get { return model.animation; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    AnimationState sleft { get { return an["sleft"]; } }
    AnimationState sright { get { return an["sright"]; } }
}
  