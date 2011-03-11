using System;
using UnityEngine;
public class CheckPoint : bs
{
    public bool checkd;

    void Update()
    {
        if (Vector3.Distance(Player.pos, pos) < 20)
        {
            checkd = true;
            enabled = false;
            renderer.material.color = Color.cyan;
            Game.LastCheckPoint = this;
        }
    }
}