using UnityEngine;
using System.Collections;

// Polymorphism
public class NPCThief : NPC
{
    public GameObject SpawnBerry;
    public float TauntDistance = 4.0f;
    public float TauntInterval = 2.0f;
    public float TauntIntervalRAND = 2.0f;
    private float tInterval = 0.0f;

    public float BerrieSearchInterval = 5.0f;
    private float berrieSearchTimer = 0.0f;
    public float BerrieSearchIntervalRAND = 5.0f;
    private Vector3 randomPointInWorld = Vector3.zero;
    private GameObject currentBerry;
    private float DragDistance = 2.5f;

    public int BerryCount = 0;

    public float IdleTime = 3.0f;
    public float IdleTimeRAND = 3.0f;
    private float idleTimer = 0.0f;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        switch (base.MyState)
        {
            case NPCSTATE.IDLE:
                if (base.myBehaviour == NPCBEHAVIOUR.HOSTILE)
                {
                    if (base.AggroTarget != null)
                    {
                        if (Vector3.Distance(transform.position, base.AggroTarget.transform.position) < TauntDistance)
                        {
                            if (base.SightCheck(base.AggroTarget.transform.position))
                            {
                                if (base.RotateTowards(base.AggroTarget.transform.position))
                                {
                                    if (tInterval <= 0)
                                    {
                                        tInterval = TauntInterval + Random.Range(0.0f, TauntIntervalRAND) + animation["tauntA"].length;
                                        base.AnimateOnce("tauntA");
                                    }
                                    else
                                        tInterval -= Time.deltaTime;
                                }
                            }
                        }
                    }
                }
                // Idle for a while.
                if (idleTimer <= 0)
                {
                    MyState = NPCSTATE.ROAM;
                    randomPointInWorld = Vector3.zero;
                    idleTimer = IdleTime + Random.Range(0.0f, IdleTimeRAND);
                }
                else
                {
                    // SEARCH BERRIES!!! IF THE TIMER IS MET
                    if (berrieSearchTimer <= 0)
                    {
                        float closest = 10000.0f;
                        foreach (Collectable berry in NPCLister.Instance.Berries)
                        {
                            if (SightCheck(berry.gameObject.transform.position))
                            {
                                float dist = Vector3.Distance(berry.gameObject.transform.position, transform.position);
                                if (dist < closest)
                                {
                                    closest = dist;
                                    currentBerry = berry.gameObject;
                                }
                            }
                        }
                        if (currentBerry)
                            base.GoTo(currentBerry.transform.position);
                        berrieSearchTimer = BerrieSearchInterval + Random.RandomRange(0.0f, BerrieSearchIntervalRAND);
                    }
                    else
                        berrieSearchTimer -= Time.deltaTime;

                    idleTimer -= Time.deltaTime;
                }
                break;
            case NPCSTATE.ROAM:
                // ROAM AROUND, LET THE BERRY TIMER RUN!
                if (randomPointInWorld == Vector3.zero)
                {
                    randomPointInWorld = WorldWaypoints.GetRandomNode();
                    if(!SightCheck(randomPointInWorld))
                        base.GoTo(randomPointInWorld);
                }
                else
                {
                        
                    if (!base.Move(randomPointInWorld, 1.0f))
                    {
                        randomPointInWorld = Vector3.zero;
                        MyState = NPCSTATE.IDLE;
                    }
                    else
                        Debug.Log(randomPointInWorld);
                }
                break;
            case NPCSTATE.FOLLOW:
                if (currentBerry)
                {
                    if (Vector3.Distance(transform.position, currentBerry.transform.position) < DragDistance)
                    {
                        currentBerry.transform.position -= (currentBerry.transform.position - transform.position).normalized * Time.deltaTime;
                        if (Vector3.Distance(transform.position, currentBerry.transform.position) < 0.5f)
                        {
                            Destroy(currentBerry);
                            BerryCount++;
                            MyState = NPCSTATE.IDLE;
                            NPCLister.Instance.Berries.Remove((Collectable)currentBerry.GetComponent<Collectable>());
                            currentBerry = null;
                            
                        }
                    }
                }
                break;
        }
        base.Update();
    }

    public override void Death()
    {
        if (BerryCount > 0)
        {
            if (SpawnBerry)
            {
                for (int i = 0; i < BerryCount; i++)
                {
                    Instantiate(SpawnBerry, transform.position, Quaternion.identity);
                }
                BerryCount = 0;
            }
        }
        base.Death();
    }

    public override void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.green;
        Gizmos.DrawCube(randomPointInWorld, Vector3.one);
        */
        base.OnDrawGizmos();
    }
}