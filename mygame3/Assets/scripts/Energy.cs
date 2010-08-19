using UnityEngine;
using System.Collections;
using doru;


public class Energy : Base
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
            player.transform.FindChild("Reload").GetComponent<AudioSource>().Play();
            player.guns[gunIndex].bullets += bullets;
            Hide();
            
            _TimerA.AddMethod(spawnTime, Show);
        }
    }
    
}