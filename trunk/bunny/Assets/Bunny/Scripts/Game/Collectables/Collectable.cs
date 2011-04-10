using UnityEngine;
using System.Collections;

public class Collectable : MonoBehaviour
{
    private PlayerController player;

    public ParticleEmitter DestructEffect;

    public int ScoreAmount = 100;
    public float AttractDistance = 2.0f;
    public float Speed = 10.0f;

    // Private
    private bool scatterFromDestructable = false;
    private float scatterSpeed;
    private Vector3 scatterDir;
    private float scatterDistance;
    private float scatterHeight;
    private Vector3 scatterPos = Vector3.zero;

    void Start()
    {
        NPCLister.Instance.Berries.Add(this);
        player = (PlayerController)GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (!collider)
        {
            Debug.LogWarning("No collider set in this object" + this.gameObject.name);
        }
        else if (!collider.isTrigger)
        {
            Debug.LogWarning("Collider not a trigger!!" + this.gameObject.name);
        }
    }

    void Update()
    {
        if (scatterFromDestructable)
        {
            ScatterCalculation();
        }
        else if (Vector3.Distance(player.transform.position + (Vector3.up * 0.5f), this.transform.position) < AttractDistance)
        {
            DragTowardsPlayer();
        }
    }

    void LateUpdate()
    {

    }

    #region Scattering Math and other shit behind it
    public void SetScatterDirection(Vector3 dir, float distance, float speed)
    {
        scatterFromDestructable = true;
        scatterDir = dir;
        scatterDistance = distance;
        scatterSpeed = speed;

        RaycastHit myRay;


        if(Physics.Raycast(transform.position, scatterDir, out myRay, scatterDistance))
        {
            scatterDistance = myRay.distance;
        }

        scatterHeight = scatterDistance;

        if (Physics.Raycast(transform.position + (scatterDir * scatterDistance), Vector3.down, out myRay, Mathf.Infinity))
        {
            scatterPos = myRay.point + (Vector3.up * 0.2f);
        }
    }

    void ScatterCalculation()
    {
        scatterHeight = Mathf.Lerp(scatterHeight, 0.0f, scatterSpeed * Time.deltaTime);
        Vector3 v3y = Vector3.up * scatterHeight;
        transform.position = Vector3.Slerp(transform.position, scatterPos + v3y, scatterSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, scatterPos)<0.1f)
            scatterFromDestructable = false;
    }
    #endregion

    #region Player Attraction
    void DragTowardsPlayer()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, player.transform.position + (Vector3.up * 0.5f), Speed * Time.deltaTime);
    }
    #endregion

    #region collision check
    void OnTriggerStay(Collider col)
    {
        if (col.tag == player.tag)
        {
            player.AddScore(ScoreAmount);
            if (DestructEffect)
            {
                ParticleEmitter nEffect = (ParticleEmitter)GameObject.Instantiate(DestructEffect, transform.position, transform.rotation);
                nEffect.name = DestructEffect.name;
            }
            Destroy(this.gameObject);
        }
    }
    #endregion
}