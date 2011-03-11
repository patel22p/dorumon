//using System;
//using UnityEngine;
//public enum SpawnType { none = -1, zombie, RedSpawn, NoneSpawn, BlueSpawn, clothcollider, ZombieSpawnLocation, trap }
//[AddComponentMenu("MapTag")]
//public class MapTag : MonoBehaviour
//{
//    public bool glass;
//    public bool disablelight;
//    public float lightIntensivity;
//    public SpawnType SpawnType = SpawnType.none;
//    public bool skipResetPos;
//    public GameObject forwardAim;
//    public void Awake()
//    {
//        if (this.light != null && lightIntensivity != 0)
//        {
//            this.light.intensity = lightIntensivity;
//            if (disablelight) Destroy(this.light);
//        }
//    }
//    public int damage = 1000;
    
//    void Aim(object o)
//    {
//        if (forwardAim != null)
//            forwardAim.SendMessage("Aim", o, SendMessageOptions.DontRequireReceiver);
//    }
//}