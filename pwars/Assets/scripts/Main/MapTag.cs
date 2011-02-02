using System;
using UnityEngine;
public enum SpawnType { none = -1, zombie, RedSpawn, NoneSpawn, BlueSpawn, none2, ZombieSpawnLocation }
[AddComponentMenu("MapTag")]
public class MapTag : MonoBehaviour
{
    public bool glass;
    public bool disablelight;
    public float lightIntensivity;
    public SpawnType SpawnType;
    public void Init()
    {
        if (glass) tag = "glass";
    }
    public void Awake()
    {

        if (this.light != null)
        {
            this.light.intensity = lightIntensivity;
            if (disablelight) Destroy(this.light);
        }
    }
    
}