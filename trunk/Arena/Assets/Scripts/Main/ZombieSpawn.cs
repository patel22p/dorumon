using System;
using UnityEngine;

public class ZombieSpawn:bs
{
    public override void Awake()
    {
        _Game.ZombieSpawns.Add(this);
    }
    public void Start()
    {

    }
}