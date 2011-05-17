using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;


public class Shared : bs
{
    public AnimationState death { get { return an["death"]; } }
    public bool ground;
    public int jumpPower = 400;
    public float life = 2;
    public int berries;
    
    public GameObject BloodPrefab;
    public List<Transform> bloodPoints = new List<Transform>();

    public int nuts;
    public virtual void Start()
    {
        
    }
    public virtual void Update()
    {
        
    }
    public virtual void UpdateAnimations()
    {

    }

    
    [FindTransform]
    public GameObject model;
    public Animation an { get { return model.animation; } }

    [FindTransform]
    public Trigger trigger;

    public virtual void Damage()
    {
        foreach (var p in bloodPoints)
            Destroy(Instantiate(BloodPrefab, p.position, p.rotation), .3f);
    }
    public virtual void Die()
    {
        //an.Stop();
        an.CrossFade(death.name);               
        SetLayer(LayerMask.NameToLayer("Dead"));
        _Game.shareds.Remove(this);
        rigidbody.isKinematic = true;
        for (int i = 0; i < nuts; i++)
            CreateNut(_Game.nutPrefab);
        for (int i = 0; i < berries; i++)
            CreateNut(_Game.berryPrefab);
        enabled = false;
        
    }
    private void CreateNut(GameObject prefab)
    {
        GameObject nut = (GameObject)Instantiate(prefab, pos + Vector3.up, Quaternion.identity);
        Rigidbody rig = nut.AddComponent<Rigidbody>();
        rig.constraints = RigidbodyConstraints.FreezeRotation;
        rig.velocity = (UnityEngine.Random.onUnitSphere + Vector3.up) * 4;
    }

    //public virtual void Attack()
    //{
        
    //}
}