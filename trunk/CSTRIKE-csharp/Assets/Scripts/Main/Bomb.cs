using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Bomb : Bs
{
    public AudioClip pip;
    public AudioClip explode;
    public AudioClip difuse;
    public AudioClip difuseSay;
    public AudioClip bombpl;
    public AudioClip c4armed;
    public AudioClip c4_plant;

    public bool difused;
    public float bombTime = 1;
    
    public void Start()
    {
        _ObsCamera.audio.PlayOneShot(bombpl);
        audio.PlayOneShot(c4armed);
        audio.PlayOneShot(c4_plant);
        _Bomb = this;
    }
    public void Update()
    {
        bombTime -= Time.deltaTime / 60f;
        if (_Game.timer.TimeElapsed(bombTime > .5f ? 1000 : bombTime > .2f ? 500 : 200))
            audio.PlayOneShot(pip);
        if (IsMine)
        {
            if (bombTime < 0)
                CallRPC(Explode, RPCMode.All);
        }
    }
    [RPC]
    private void Explode()
    {
        _ObsCamera.audio.PlayOneShot(explode);
        enabled = false;
    }
    [RPC]
    public void Difuse()
    {
        audio.PlayOneShot(difuse);
        _ObsCamera.audio.PlayOneShot(difuseSay);
        difused = true;
        enabled = false;
    }
}