using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;


public class Ammo : Base
{
    public int gunIndex=0;
    public int bullets = 1000;
    public int spawnTime = 5000;
    void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        Player player = other.GetComponent<Player>();        
        
        if (player != null)
        {
            
            if (gunIndex == 3)
                player.PlaySound("takeHealth", 5);
            else
                player.PlaySound("shotgun_use_01", .6f);

            if (player.isOwner)
            {
                if (gunIndex == 3)
                {                    
                    player.Life += bullets;
                    if (player.Life - 100 > 0)
                    {
                        player.nitro += player.Life - 100;
                        player.Life = 100;
                        player.RPCSetLife(player.Life, -1);
                    }
                }
                else
                    player.guns[gunIndex].bullets += bullets;
            }

            Hide();
            
            _TimerA.AddMethod(spawnTime, Show);
        }
    }
    
}