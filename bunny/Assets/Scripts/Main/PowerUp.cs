using System;
using UnityEngine;
public enum PowerType { doubleJump, HighJump, none = -1 }
public class PowerUp : bs
{
    public PowerType powerType;
    public void Update()
    {
        var dist = Vector3.Distance(_Player.pos, this.pos);

        if (dist < 3)
        {

            _Game.powerType = powerType;
            _Game.powerTime = 20;
            Destroy(this.gameObject);
        }
    }
}