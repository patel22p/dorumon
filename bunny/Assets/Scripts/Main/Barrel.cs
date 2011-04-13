using System;
using UnityEngine;


public class Barrel : bs
{
    public GameObject DestroyEffect;
    public GameObject nutPrefab;
    
    public float SecondsToDestroy = 2;
    public int nutsDropped = 10;
    public void Hit()
    {
        Debug.Log("Destroy");
        
        var br = this;
        if (br.DestroyEffect != null)
            Destroy(Instantiate(br.DestroyEffect, br.pos, br.rot), SecondsToDestroy);
        for (int i = 0; i < nutsDropped; i++)
        {
            GameObject nut = (GameObject)Instantiate(nutPrefab, pos + Vector3.up, Quaternion.identity);
            Rigidbody rig =  nut.AddComponent<Rigidbody>();
            rig.constraints = RigidbodyConstraints.FreezeRotation;
            rig.velocity = (UnityEngine.Random.onUnitSphere + Vector3.up)*4;
        }
        Destroy(this.gameObject);
    }
}