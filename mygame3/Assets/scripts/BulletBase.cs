using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBase : Base
{
    float tm;

    
    protected Vector3 previousPosition;
    public Transform decal;
    protected override void OnStart()
    {
        previousPosition = transform.position;
    }
    protected override void OnFixedUpdate()
    {
        tm += Time.deltaTime;
        if (tm > 5) Destroy(gameObject);

        Vector3 movementThisStep = transform.position - previousPosition;
        RaycastHit hitInfo;

        if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementThisStep.magnitude + 1, 1 << 8))
        {
            if (!hitInfo.collider.isTrigger || hitInfo.collider.gameObject.name == "hit")
                Hit(hitInfo);
        }

        previousPosition = transform.position;

    }

    protected virtual void Hit(RaycastHit hit)
    {        
        
        
        
        
    }

}