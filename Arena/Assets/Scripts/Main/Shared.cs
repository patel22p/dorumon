using System;
using UnityEngine;
using System.Text.RegularExpressions;

public class Shared : bs
{
    
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

    public void Fade(AnimationState s, float speed)
    {
        an.CrossFade(s.name);
        s.speed = speed;
    }

    public int id { get { return Network.player.GetHashCode(); } }

    
}