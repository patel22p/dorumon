using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBase : Base
{
    float tm;

    protected Player LocalPlayer { get { return Find<Player>("LocalPlayer"); } }
    protected Vector3 previousPosition;

    protected override void FixedUpdate()
    {
        tm += Time.deltaTime;
        if (tm > 5) Destroy(gameObject);

        if (previousPosition != default(Vector3))
        {
            Vector3 movementThisStep = transform.position - previousPosition;
            RaycastHit hitInfo;

            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementThisStep.magnitude, 1))
                Hit(hitInfo);
        }

        previousPosition = transform.position;

    }

    protected virtual void Hit(RaycastHit raycastHit)
    {

    }

}