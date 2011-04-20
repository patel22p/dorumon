using UnityEngine;
using System.Collections;

public class PhysAnimObj : bs
{
    public Transform original;
    public Quaternion oldRot;
    public Quaternion localrot { get { return rot * Quaternion.Inverse(oldRot); } } 
    
    public override void Awake()
    {
        base.Awake();
        oldRot =  this.rot;
    }
}
