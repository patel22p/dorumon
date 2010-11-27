using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;


public class Ammo : MapItem
{
    public int gunIndex=0;
    public int bullets = 1000;
    public int spawnTime = 5000;
    public override string title()
    {
        return "Нажми B чтобы купить "+_localPlayer.guns[gunIndex]._Name+", нужно "+score+" очков";
    }
    
    void Start()
    {
        //((Transform)Instantiate(_Game.AmmoModels[gunIndex], this.transform.position, Quaternion.identity)).parent = this.transform; 
    }
    
    public override void CheckOut()
    {
        Player player = _localPlayer;
        if (this.gunIndex == 3)
        {
            player.Life += this.bullets;
            if (player.Life - 100 > 0)
            {
                player.nitro += player.Life - 100;
                player.Life = 100;
                player.RPCSetLife(player.Life, -1);
            }
        }
        else
            player.guns[this.gunIndex].patronsleft += this.bullets;

    }
}