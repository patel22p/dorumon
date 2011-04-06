using UnityEngine;
using System.Collections;

public class Cam : bs{

    [FindTransform]
    public bs campos;
    public override void Awake()
    {
        Camera.main.transform.position = campos.pos;
        Camera.main.transform.rotation = campos.rot;
        Camera.main.transform.parent = campos.transform;
    }
	void Start () {
        
	}
    Quaternion rotf;
    Vector3 posf;
	void Update () {
        posf = _Player.pos;
        var e = _Player.rot.eulerAngles;
        e.z =0;
        rotf = Quaternion.Euler(e);
        rot = Quaternion.Lerp(rotf, rot, .5f);
        pos = Vector3.Lerp(posf, pos, .5f);

        //var v = pos - _Player.pos;
        //v.y = 0;
        //rot = Quaternion.LookRotation(v);
	}
}
