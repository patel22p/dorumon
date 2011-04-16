using System;
using UnityEngine;

public class Shaded : bs
{
    

    public virtual void Start()
    {
        
    }
    public virtual void Update()
    {
        AnimationsUpdate();
    }
    public virtual void AnimationsUpdate()
    {

    }

    public Vector3 vel;
    [FindTransform(self = true)]
    public CharacterController controller;
    [FindTransform]
    public GameObject model;
    public Animation an { get { return model.animation; } }
}