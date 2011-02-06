using System;
using UnityEngine;
public enum SpawnType { none = -1, zombie, RedSpawn, NoneSpawn, BlueSpawn, clothcollider, ZombieSpawnLocation,trap }
[AddComponentMenu("MapTag")]
public class MapTag : Base2
{
    public bool glass;
    public bool disablelight;
    public float lightIntensivity;
    public SpawnType SpawnType = SpawnType.none;
    public bool skipResetPos ;
    public void Awake()
    {
        if (this.light != null)
        {
            this.light.intensity = lightIntensivity;
            if (disablelight) Destroy(this.light);
        }
    }
    public int damage = 1000;
    void OnCollisionStay(Collision c)
    {
        if (SpawnType == SpawnType.trap)
        {
            var ipl = c.gameObject.GetComponent<Destroible>();
            if (ipl != null && ipl.Alive && ipl.isController)
                ipl.RPCSetLifeLocal(ipl.Life - damage, -1);
        }
    }
}