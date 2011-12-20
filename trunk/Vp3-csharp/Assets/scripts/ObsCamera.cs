using UnityEngine;
using System.Collections;

public class ObsCamera : bs
{
    public Projector[] prs;
    public float max = 7;
	public void Update ()
	{
	    RaycastHit h;
        foreach (var pr in prs)
            if (Physics.Raycast(pos + Vector3.up * .1f, pr.transform.forward, out h, max, 1 << LayerMask.NameToLayer("Level")))
                pr.farClipPlane = h.distance;
            else
                pr.farClipPlane = max;

	}
}
