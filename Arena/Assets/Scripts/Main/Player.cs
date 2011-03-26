using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : bs {
    [FindTransform]
    public GameObject model;

    public Animation an { get { return model.animation; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    CharacterController controller;
    public override void Awake()
    {
        if (networkView.isMine)
            _Game._Player = this;
        else
            _Game._Player2 = this;
    }
	void Start () {        
        controller = (CharacterController)this.GetComponent(typeof(CharacterController));
        an.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
	}

	void Update () {

        Vector3 mv = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //if (_Cam.secondMode)
        mv = _Cam.oldrot * mv;
        mv += mv * 5;
        var v = _Cursor.pos - _Player.pos;
        v.y = 0;
        if (v != Vector3.zero)
            _Player.rot = Quaternion.LookRotation(v);
        controller.SimpleMove(mv);
        
        Vector3 speed = Quaternion.Inverse(transform.rotation) * controller.velocity;
        var sn = speed.normalized;        
        if (Mathf.Abs(speed.magnitude) > 2f)//fix
            Fade(run, 1);
        else
            Fade(idle, 1);

	}
    private void Fade(AnimationState s, float speed)
    {
        an.CrossFade(s.name);
        s.speed = speed;
    }
}
  