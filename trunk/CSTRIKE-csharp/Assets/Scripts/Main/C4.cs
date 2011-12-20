using System;
using System.Collections;
using System.Linq;
using doru;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class C4 : GunBase
{
    public GameObject BombPrefab;
    protected AnimationState press { get { return an["pressbutton"]; } }
    
    public AudioClip bombdef;
    public override void Start()
    {
        base.Start();
        idle.wrapMode = WrapMode.Loop;
        press.wrapMode = WrapMode.Default;
        idle.layer = press.layer = 1;
        
    }

    public void Update()
    {
        if (patrons == 0) return;
        if (MouseButtonDown && (_Game.BombPlace.position - pl.pos).magnitude < 5)
            an.CrossFade(press.name);
        else
            an.CrossFade(idle.name);
        if (press.time > 0 && MouseButtonDown)
        {
            if (press.time / press.length < 1)
                _Hud.SetProgress(press.time / press.length);
            else
            {
                PhotonNetwork.Instantiate(BombPrefab, pl.pos, Quaternion.identity, (int)Group.Bomb);
                patrons = 0;
                _Player.NextGun();
            }
        }
    }
}