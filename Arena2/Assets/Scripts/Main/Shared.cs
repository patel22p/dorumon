using System;
using UnityEngine;
using System.Text.RegularExpressions;

public class Shared : bs
{
    public bool Alive = true ;
    internal CharacterController controller;
    internal Quaternion syncRot;
    internal Vector3 syncPos;
    internal GameObject model;
    public Animation an { get { return model.animation; } }    
    internal Trigger trigger;
    public Transform[] upperbody;
    public Transform[] downbody;
    public float life = 100;
    public override void Awake()
    {
        trigger = transform.Find("trigger").GetComponent<Trigger>();
        model = transform.Find("model").gameObject;
        controller = this.GetComponent<CharacterController>();
        base.Awake();
    }
    void Start()
    {
    }
    public string GetId()
    {
        return Regex.Match(networkView.viewID.ToString(), @"\d+").Value;
    }
    public void Fade(AnimationState s)
    {
        an.CrossFade(s.name);
    }
}