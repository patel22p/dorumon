using UnityEngine;
using System.Collections;

// Destructable object, sprouts items if so is to be desired.
public class Destructible : MonoBehaviour
{
    public GameObject SpawnCollectable;
    public int AmountOfObjects;

    public ParticleEmitter DestructEffect;

    public float ScatterDistance = 4.0f;
    public float DistanceVariaton = 1.0f;
    public float ScatterSpeed = 3.5f;

    void Start()
    {
        if (!collider)
            Debug.LogWarning("Destructable object requires a collider.");
    }

    void Update()
    {
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            Destruct();
        }
    }

    void Destruct()
    {
        for (int i = 0; i < AmountOfObjects; i++)
        {
            GameObject newObj = (GameObject)GameObject.Instantiate(SpawnCollectable);
            newObj.name = SpawnCollectable.name;
            newObj.transform.position = this.transform.position;
            Collectable col = (Collectable)newObj.GetComponent<Collectable>();
            if (col)
            {
                float dist = Random.Range(ScatterDistance - DistanceVariaton, ScatterDistance + DistanceVariaton);
                col.SetScatterDirection(new Vector3(Random.Range(-1.0f,1.0f), 0.0f, Random.Range(-1.0f, 1.0f)).normalized, dist, ScatterSpeed);
            }
        }

        if (DestructEffect)
        {
            ParticleEmitter nEffect = (ParticleEmitter)GameObject.Instantiate(DestructEffect, transform.position, transform.rotation);
            nEffect.name = DestructEffect.name;
        }
        Destroy(this.gameObject);
    }
}