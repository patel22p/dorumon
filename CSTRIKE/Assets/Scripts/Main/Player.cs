using UnityEngine;
using System.Collections;

public class Player : Bs {
    
    public Bs model;
    AudioSource aus;
    CharacterController controller;
    public AudioClip[] pl_dirt;
    public Animation an { get { return model.animation; } }
    AnimationState idle { get { return an["idle"]; } }
    AnimationState run { get { return an["run"]; } }
    
    public Transform UpperBone;
    internal Vector3 vel;

	void Start () {
        foreach (var b in blends)
        {
            b.AddMixingTransform(UpperBone);            
            b.layer = 1;
            b.enabled = true;
        }
        InitAnimations();
        aus = GetComponent<AudioSource>(); 
        controller = GetComponent<CharacterController>();        
	}
  
    private void InitAnimations()
    {
        an.wrapMode = WrapMode.Loop;
        run.wrapMode = WrapMode.Loop;
    }
    float clamp(float a)
    {
        if (a > 180) return a - 360;
        return a;
    }
    Vector3 clamp(Vector3 a)
    {
        if (a.x > 180) a.x -= 360;
        if (a.y > 180) a.y -= 360;
        if (a.z > 180) a.z -= 360;
        return a;
    }
	void Update () {        
        controller.SimpleMove(vel);
        if (vel.magnitude > .5f)
            Fade(run);
        else
            Fade(idle);
        
        var mouse = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        if (mouse.magnitude > 0)
            vel = tr.rotation * mouse * 5;
        vel *= .83f;
        if (Screen.lockCursor)
        {
           
            model.lroty -= Input.GetAxis("Mouse X");
            roty += Input.GetAxis("Mouse X"); 
            var a = Quaternion.Angle(rot,model.rot);
            if (a > 90)
                Quaternion.RotateTowards(rot, model.rot, a - 90);

            //model.roty = Mathf.Clamp(clamp(model.roty) - Input.GetAxis("Mouse X"), -90, 90);
            
                //Mathf.Clamp(, -90, 90);
            Debug.Log(model.roty);
            _Cam.rotx -= Input.GetAxis("Mouse Y");            
        }

        var mrot9 = clamp((rot * Quaternion.Inverse(model.rot)).eulerAngles);
        mrot9.y = clamp(roty - model.roty);
        mrot9.x = _Cam.rotx;
        Debug.Log(mrot9);
        mrot9 /= 90;
        Fade(blends[4], Mathf.Clamp(mrot9.y, -1,0 ));
        Fade(blends[6], Mathf.Clamp(mrot9.y, 0, 1));
        Fade(blends[8], Mathf.Clamp(mrot9.x, -1, 0));
        Fade(blends[2], Mathf.Clamp(mrot9.x, 0, 1));                
	}
    
    void WalkSound()
    {
        aus.PlayOneShot(pl_dirt[Random.Range(0, pl_dirt.Length - 1)]);
    }
    public void Fade(AnimationState s)
    {        
        an.CrossFade(s.name);
    }
    public void Fade(AnimationState s,float f)
    {
        s.weight = Mathf.Clamp(Mathf.Abs(f), 0, 1);
        s.enabled = true;
        //an.CrossFade(s.name);
    }
    AnimationState[] m_blends;
    AnimationState[] blends
    {
        get
        {
            if (m_blends == null)
                m_blends = new AnimationState[] { an["ak47_blend1"], an["ak47_blend1"], an["ak47_blend2"], an["ak47_blend3"], an["ak47_blend4"], an["ak47_blend5"], an["ak47_blend6"], an["ak47_blend7"], an["ak47_blend8"], an["ak47_blend9"] };
            return m_blends;
        }
    }
}
