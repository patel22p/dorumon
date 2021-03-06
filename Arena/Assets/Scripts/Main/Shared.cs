﻿using System;
using UnityEngine;
using System.Text.RegularExpressions;

public class Shared : bs
{

    public Transform[] upperbody;
    public Transform[] downbody;
    void Start()
    {
        
    }
    public string GetId()
    {
        return Regex.Match(networkView.viewID.ToString(), @"\d+").Value;
    }
    [FindTransform(self = true)]
    public CharacterController controller;
    public Quaternion syncRot;
    public Vector3 syncPos;

    [FindTransform]
    public GameObject model;
    public Animation an { get { return model.animation; } }

    public void Fade(AnimationState s)
    {
        an.CrossFade(s.name);
    }

    public int id { get { return Network.player.GetHashCode(); } }

    
}