using System;
using UnityEngine;
public class CheckPoint : bs
{
    public bool check;

    void Update()
    {
        if (Vector3.Distance(Player.pos, pos) < 5)
        {
            check = true;
            enabled = false;
            renderer.material.color = Color.clear;
            Player.L
        }
    }
}