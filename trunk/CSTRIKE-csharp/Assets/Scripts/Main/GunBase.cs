using System;
using System.Collections;
using System.Linq;
using doru;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GunBase : Bs
{
    //public bool Pistol;
    public int gunId;
    internal int arrayId;
    internal Player pl;
    internal bool shooting;
    public int patrons = 30;
    public int globalPatrons = 60;
    internal float cursorOffset;
    public Transform Hands;
    public bool MouseButtonDown;
    internal Animation an { get { return Hands.animation; } }
    public AnimationState handsDraw { get { return an["draw"]; } }
    public AnimationState idle { get { return an["idle"]; } }
    public virtual void Start()
    {
        this.transform.localScale = new Vector3(-1, 1, 1);
        handsDraw.layer = 1;        
    }
 
    public void OnEnable()
    {
        MouseButtonDown = false;
        //Debug.Log("OnEnable:" + name);
        an.Play(handsDraw.name);
    }
    public virtual void OnRDown()
    {

    }
}