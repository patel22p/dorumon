using System;
using UnityEngine;

public class Shared : bs
{
    
    public int JumpPower = 10;
    public float life = 2;
    public int berries;
    public int nuts;
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